// ----------------------------------------------------------------------------------
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Fabric.Samples.Common;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Fabric;

namespace CScale.Views.BaseClasses
{
    // This class (coupled with StateProviderBase) contains a basic implementation of a stateful service replica
    // The purpose here is to demonstrate some of the concepts of windows fabric 
    // as well as gotchas and caveats that should be kept in mind
    // 
    // Performance is not one of the primary focuses of this class 
    //
    // At a high level a windows fabric volatile stateful service can be either a primary or a secondary
    // In this sample, the job of the primary is to service client requests 
    // The job of the secondaries is to maintain a copy of the state
    // When the primary dies one of the secondaries is made a new primary
    // And clients can now make requests to this new primary
    //
    // WINDOWS FABRIC STATES 
    // (For more information refer to documentation - this serves as a high level introduction)
    // Windows Fabric will call the following methods on the implementation of IStatefulServiceReplica
    // Initialize: Provide information about the replica
    // Open: The replica should now initialize it's state, create a replicator etc
    // ChangeRole: The replica's role is changing
    // Close: The replica is no longer active. It should not delete persisted state at this time though
    // 
    // ROLES
    // Replicas can be in the following roles:
    // None
    // IdleSecondary: This is a new replica that is being brought up. It should pump operations 
    // from the copy and replication streams and apply them to its state. 
    // ActiveSecondary: The replica is participating in quorum. It should pump operations from the replication stream
    // Primary: The replica is the primary. It should be handling client requests and sending operations for
    // replication and also bringing up new replicas 
    // 
    // PUMPING OPERATIONS
    // There are two streams that have operations in them. A replica should have an operation pump (which is a loop)
    // which is constantly pulling operations out of these streams and applying them to its state 
    // This should be done whenever the replica is a secondary. Operations should be pumped from the stream
    // until a NULL is dequeued which represents the end of the stream. There are two streams: Copy, Replication.
    //
    // Copy Stream: The copy stream is a representation of the state up to a specific sequence number. 
    // The primary generates the copy stream (this is represented in the GetCurrentState and associated methods in the StateProviderBase)
    // The secondary is pulling operations from the copy stream and applying them (ApplyOnSecondary on StateProviderBase)
    // Replication Stream: The replication stream is a representation of current operations that the primary is replicating
    // The secondary is pulling these from the replication stream and applying them (ReplicateAsync on primary and ApplyOnSecondary)
    //
    // PROCESSING CLIENT REQUESTS
    // The primary (in advanced scenarios even secondaries) process client requests. Client requests require taking a lock on some state,
    // figuring out the change that is going to happen to the state, replicating this state to secondaries, waiting for quorum ack and then
    // responding to the client
    // 
    // The rest of this comment is a description of the code below:
    // replicaState - represents the current state
    // queuedChangeRoleTCS and queuedTargetRole - will be explained later
    // inFlightContexts - contains the requests that have been sent for replication
    // isContextCompletionDispatcherRunning - flag to represent whether completed replication operations are currently being applied or not
    //
    // STATE MACHINE
    // Closed: Replica is closed
    // Opened: Replica has been Opened (OpenAsync was called)
    // Secondary: Replica is a secondary. It is pumping operations from the streams and applying them to its state
    // Primary: Replica is a primary. 
    // ChangingRole: Replica is in the process of changing role 
    // Closing: Replica is in the process of closing
    // Faulted: An error has occurred and report fault has been called
    //
    // STATE TRANSISTIONS:
    // Initial state = Closed
    // Initial | Operation | Target State = Description
    // Closed | OpenAsync | Opened = The replica is being opened
    // 
    // Open | ChangeRole (IdleSec) | Secondary = An opened replica has been made a secondary. It is time to pump operations
    // Open | ChangeRole (Primary) | Primary = An opened replica has been made a primary. Get ready to process requests
    //
    // Secondary | ChangeRole (None) | Opened = No-Op. The pump would have terminated because it would have dequeued null
    // Secondary | ChangeRole (ActiveSecondary) | Secondary = No-Op
    // Secondary | ChangeRole (Primary) | Primary = Transition to primary state. The pump would have terminated because it dequeued null
    // Secondary | Failure to apply copy/replication operation | Faulted = Report fault and transition to faulted state and wait for close
    //
    // Primary | ChangeRole (ActiveSec) | ChangingRole(Secondary) = Start pumping operations
    // Primary | ChangeRole (None) | ChangingRole(None)
    // ChaingingRole(target role) | All inflight requests applied | target = From the changing role state we transition to the target state once we detect that all the inflight requests have been applied
    // Primary | Failure to process request | Faulted = We call report fault and ask windows fabric to move us
    // 
    // * | CloseAsync | Closing = Transition to a closing state. Wait for any operations that are inflight to be applied
    // Closing | All inflight requests applied | Closed = Complete the CloseAsync call
    // 
    // REQUEST PROCESSING - in the base class
    // On the primary, when a request comes in we take a lock on our current state
    // Validate whether we can process the state or not (i.e. are we either a primary or a primary processing requests)
    // Create a replication operation instance that represents the data to be sent
    // Call windows fabric to replicate the data
    // Add that to the list of inflight operations
    //
    // Now, windows fabric will assign a sequence number and go and send the data to the secondaries
    // Once a quorum of secondaries have received the request (or acknowledged it) windows fabric will complete the task
    // 
    // On the primary, the base class guarantees that replication operations are applied in order. 
    // It is possible that the completion of the replication operations happens on drifferent threads and thus
    // if operation 1, 2, 3, 4 were queued, the callback for 4 is actually executed before the callback for 1 even though 
    // both were sent over. The code in the ProcessCompletionCallback simply marks the operation as completed
    // It then checks to see if another thread is applying replication operations (isContextCompletionDispatcher = true)
    // If so, it just exits because that thread will then apply this operation as well 
    // If no other thread is running then it checks to see if the first operation in the list of inFlightRequests is completed
    // If not then it exits and when the thread representing that operation's completion is executed it will process this operation as well
    // If it finds that the first operation in the list is indeed complete then it will provide that to the StateProvider to apply it to its state
    // This thread will then continue processing the next operation in the list (if it is complete)
    // If there is no other operation then it means that as a Primary we have processed all the outstanding requests
    // The algorithm above is codified in the ReplicateAsync and the ProcessReplicationOperation and the TryDequeueFirstRequest functions
    // 
    // REPORTING FAULT
    // ReportFault to windows fabric indicates that something is wrong with the service and it should be moved
    //
    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Stateful")]
    public abstract class StatefulServiceReplica : IStatefulServiceReplica, IInternalStatefulServiceReplica
    {
        private static readonly Guid RequestCompletionDispatcherActivityId = new Guid("{B6A29384-7F7C-47DD-B903-ED6068886CF0}");

        protected enum ReplicaState
        {
            Closed,
            Opened,
            Secondary,
            Primary,
            ChangingRole,
            Closing,
            Faulted,
        }

        private ILogger logger;

        // The current state
        private ReplicaState replicaState = ReplicaState.Closed;

        private IInternalStateProvider stateProvider;

        private StatefulServiceInitializationParameters initializationParameters;
        private IStatefulServicePartition partition;
        private IStateReplicator stateReplicator;
        private OperationFetcher operationFetcher;

        private readonly object stateLock = new object();
        private readonly List<ReplicationContext> inFlightContexts = new List<ReplicationContext>();
        private bool isContextCompletionDispatcherRunning = false;
        private long lastCompletedSequenceNumber = 0;

        protected StatefulServiceInitializationParameters InitializationParameters
        {
            get { return this.initializationParameters; }
        }

        protected IStatefulServicePartition Partition
        {
            get { return this.partition; }
        }

        /// <summary>
        /// This is the very first method called by the framework
        /// Provides all the initialization information
        /// No complex processing should be done here
        /// All heavy lifting should be done in Open
        /// </summary>
        /// <param name="initializationParameters"></param>
        void IStatefulServiceReplica.Initialize(StatefulServiceInitializationParameters initializationParameters)
        {
            Utility.Assert(initializationParameters != null, "Windows Fabric gurantees that initializationParameters is never null");

            Guid activityId = Guid.NewGuid();
            this.logger = new Logger(Path.Combine(initializationParameters.CodePackageActivationContext.LogDirectory, string.Format(CultureInfo.InvariantCulture, "{0}_{1}.txt", initializationParameters.PartitionId, initializationParameters.ReplicaId)));
            this.logger.Write(activityId, "Initialize: Name: {0}. Uri: {1}. PartitionId: {2}. ReplicaId: {3}", initializationParameters.ServiceTypeName, initializationParameters.ServiceName, initializationParameters.PartitionId, initializationParameters.ReplicaId);
            this.initializationParameters = initializationParameters;
        }

        /// <summary>
        /// OpenAsync is called when the replica is going to be actually used
        /// </summary>
        Task<IReplicator> IStatefulServiceReplica.OpenAsync(ReplicaOpenMode openMode, IStatefulServicePartition partition, CancellationToken cancellationToken)
        {
            Guid activityId = Guid.NewGuid();
            return TaskUtility.ExecuteSynchronously<IReplicator>(() =>
            {
                lock (this.stateLock)
                {
                    // Store the partition
                    this.logger.Write(activityId, "OpenAsync");
                    this.partition = partition;

                    // Create an implementation of the state provider
                    // In our case we ask the derived class for an implementation
                    this.stateProvider = this.CreateStateProvider(partition);

                    // assign the Internal StatefulServiceReplica on the state provider so that it can replicate
                    // This is just an implementation detail
                    this.stateProvider.Replica = this;

                    // create the replicator
                    // The Windows Fabric Replicator is used to actually replicate the data
                    // The ReplicatorSettings are used for configuring the replicator - here we ask for them from the derived class
                    // When using service groups Replicator Settings are described in the Settings.xml inside the config package.
                    // So when the service is part of a serive group it should use null as its Replicator Setting.
                    FabricReplicator replicator;
                    if (this.Partition is IServiceGroupPartition)
                    {
                        replicator = this.partition.CreateReplicator(this.stateProvider as IStateProvider, null);
                    }
                    else
                    {
                        replicator = this.partition.CreateReplicator(this.stateProvider as IStateProvider, this.CreateReplicatorSettings());
                    }
                    this.stateReplicator = replicator.StateReplicator;

                    // instantiate the operation fetcher - this can be done when changing role to secondary as well
                    this.operationFetcher = new OperationFetcher(this.stateReplicator, this.ApplyCopyOperation, this.ApplyReplicationOperation, this.logger);
                    
                    // Change state
                    this.replicaState = ReplicaState.Opened;
                    this.isContextCompletionDispatcherRunning = false;

                    // Allow the derived class to do any work
                    // Here in this sample we expect derived class to create their listener
                    // This should be asynchronous for performance. In the sample this is sync for readability
                    this.OnOpen(this.partition);

                    return replicator;
                }
            });
        }

        /// <summary>
        /// CloseAsync is called when the replica is going to be closed
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Stateful")]
        Task IStatefulServiceReplica.CloseAsync(CancellationToken cancellationToken)
        {
            Guid activityId = Guid.NewGuid();

            // This is slightly in-efficient but written to use up an additional thread
            // for clarity and readability. Ideally when the queue of inflight requests is drained
            // it should complete this asynchronous task
            return Task.Factory.StartNew(() =>
                {
                    this.logger.Write(activityId, "CloseAsync");

                    lock (this.stateLock)
                    {
                        this.logger.Write(activityId, "SFREplica: Transitioning to closing state");

                        this.replicaState = ReplicaState.Closing;                        
                    }

                    // NOTE: These are done outside the lock
                    this.OnClose();

                    // It is possible that we were the primary (upgrade etc)
                    // in which case wait until all the inflight requests are completed
                    this.WaitForContextCompletionDispatcherToComplete(activityId);

                    lock (this.stateLock)
                    {
                        this.logger.Write(activityId, "SFReplica: Completing close");
                        this.replicaState = ReplicaState.Closed;
                    }
                });
        }

        
        /// <summary>
        /// Abort is called when the replica is being forced closed
        /// </summary>
        /// <returns></returns>
        void IStatefulServiceReplica.Abort()
        {
            Guid activityId = Guid.NewGuid();

            this.logger.Write(activityId, "Abort");

            lock (this.stateLock)
            {
                this.logger.Write(activityId, "SFREplica: Transitioning to closing state during abort");

                this.replicaState = ReplicaState.Closing;
            }

            // NOTE: These are done outside the lock
            this.OnAbort();

            // It is possible that we were the primary (upgrade etc)
            // in which case wait until all the inflight requests are completed
            this.WaitForContextCompletionDispatcherToComplete(activityId);

            lock (this.stateLock)
            {
                this.logger.Write(activityId, "SFReplica: Completing abort");
                this.replicaState = ReplicaState.Closed;
            }
        }

        /// <summary>
        /// ChangeRole is called whenever the replica role is being changed
        /// See the state transition table at the top of this file for more information
        /// </summary>
        /// <param name="newRole"></param>
        /// <returns></returns>
        Task<string> IStatefulServiceReplica.ChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            Guid activityId = Guid.NewGuid();
            return Task.Factory.StartNew<string>(() =>
                {
                    this.logger.Write(activityId, "ChangeRoleAsync {0}", newRole);

                    // One thing to note is that the lock is not taken for the entire duration of the change role operation
                    // The lock is only guarding the class member variables (replicaState etc)
                    // Otherwise it is possible that we try to perform a replication operation while we are changing role etc ... 

                    ReplicaState currentState = ReplicaState.Faulted;
                    ReplicaState finalState = ReplicaState.Faulted;
                    lock (this.stateLock)
                    {
                        this.logger.Write(activityId, "ChangeRoleAsync to {0}. Current State {1}", newRole, this.replicaState);
                        
                        currentState = this.replicaState;
                        this.replicaState = ReplicaState.ChangingRole;
                    }

                    // All other transitions are invalid
                    if (currentState == ReplicaState.Primary)
                    {
                        this.logger.Write(activityId, "SFREplica: ChangeRoleAsync Transitioning out of primary");

                        // We are changing role out of primary
                        // We want to wait for all the inflight operations to be applied (or cancelled)
                        // Consider a scenario where we had replicated operations 1, 2, 3, 4 and all replication had completed
                        // WF can now ask us to change role to secondary but we could be in the process of applying operation 2
                        // So we delay the change role until we are done
                        this.WaitForContextCompletionDispatcherToComplete(activityId);

                        // After this we are no longer primary so tell the derived class to stop listening
                        this.StopListening();

                        if (newRole == ReplicaRole.ActiveSecondary)
                        {
                            // If we have become a secondary start draining the queue again
                            this.logger.Write(activityId, "SFReplica: Draining replication queue on transition to active secondary");
                            this.operationFetcher.DrainReplicationQueue();
                        }

                        finalState = newRole == ReplicaRole.None ? ReplicaState.Opened : ReplicaState.Secondary;
                    }
                    else if (newRole == ReplicaRole.Primary)
                    {
                        this.lastCompletedSequenceNumber = ((IStateProvider)this.stateProvider).GetLastCommittedSequenceNumber();
                        // In the sample, the primary processes requests
                        // It is possible to have secondaries also processing requests also
                        this.StartListening();            

                        finalState = ReplicaState.Primary;
                    }
                    else if (currentState == ReplicaState.Secondary)
                    {
                        // the transition to primary is taken care of above
                        finalState = newRole == ReplicaRole.None ? ReplicaState.Opened : ReplicaState.Secondary;
                    }
                    else if (currentState == ReplicaState.Opened && (newRole == ReplicaRole.IdleSecondary || newRole == ReplicaRole.ActiveSecondary))
                    {
                        finalState = ReplicaState.Secondary;

                        // Start draining the copy queue
                        // When the operation fetcher encounters a null it will automatically start draining the replication queue
                        this.operationFetcher.DrainCopyQueueAndThenReplicationQueue();
                    }                    

                    lock (this.stateLock)
                    {
                        this.replicaState = finalState;
                    }

                    this.logger.Write(activityId, "ChangeRoleAsync transistioned to {0}", finalState);

                    // Complete the change role by returning the end point is returned by the derived class                   
                    return this.GetEndpoint();
                });
        }

        void IInternalStatefulServiceReplica.ReportFault(Guid activityId)
        {
            lock (this.stateLock)
            {
                if (this.replicaState == ReplicaState.Faulted)
                {
                    // already faulted
                    return;
                }

                this.replicaState = ReplicaState.Faulted;

                // Tell WF that something has gone wrong
                // WF will close the service
                // Here, we always report fault permanent - there is also FaultType.Transient
                // The documentation describes what each flag does
                this.partition.ReportFault(FaultType.Permanent);                               
            }
        }

        #region Protected Members

        protected ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }

        /// <summary>
        /// Return ReplicatorSettings over here
        /// It is guaranteed that when this is called InitializationParameters and the Partition are available
        /// </summary>
        protected abstract ReplicatorSettings CreateReplicatorSettings();

        /// <summary>
        /// Create the state provider over here
        /// It is guaranteed that the InitializationParameters are available
        /// </summary>
        protected abstract StateProviderBase CreateStateProvider(IStatefulServicePartition partition);

        /// <summary>
        /// Return the endpoint that you would like the fabric client to resolve over here
        /// </summary>
        protected abstract string GetEndpoint();

        /// <summary>
        /// Called in OpenAsync
        /// The derived class should create it's listener over here and any other initialization
        /// This should be Asynchronous - it is sync here for readability
        /// </summary>
        /// <param name="partition"></param>
        protected virtual void OnOpen(IStatefulServicePartition partition)
        {            
        }

        /// <summary>
        /// Called in Close 
        /// The derived class should cleanup its listener and any other cleanup
        /// This should be Asynchronous - it is sync here for readability
        /// </summary>
        protected virtual void OnClose()
        {
        }

        /// <summary>
        /// Called in Abort 
        /// The derived class should cleanup its listener and any other cleanup
        /// </summary>
        protected virtual void OnAbort()
        {
        }

        /// <summary>
        /// The replica is being made primary
        /// It should start listening for requests
        /// This is just a pattern being followed in the samples - It is not necessary that this be applicable for your service
        /// </summary>
        protected virtual void StartListening()
        {
        }

        /// <summary>
        /// The Replica is no longer primary
        /// It should stop listening for requests
        /// </summary>
        protected virtual void StopListening()
        {
        }

        #endregion

        private void ProcessContextCompletion(Task<long> replicateTask, ReplicationContext context)
        {
            // At this time a replicate task has completed
            // Take a lock on the context (another dispatcher could be running to check if the context is complete or not
            // If that thread wins he will find that the context was not complete and quit it's dispatcher loop and this thread will dispatch the request
            // IF this thread wins it will mark that the context is complete and then the other dispatcher thread will pcik up the context
            if (replicateTask.IsFaulted)
            {
                this.logger.Write(context.ActivityId, "SFReplica: Replication failed with {0}", replicateTask.Exception.InnerException);
                context.MarkReplicationCompleteWithError(replicateTask.Exception.InnerException);
            }
            else
            {
                this.logger.Write(context.ActivityId, "SFReplica: Replication succeeded {0}", replicateTask.Result);
                context.MarkReplicationCompleteWithSuccess(replicateTask.Result);
            }

            this.DispatchCompletedRequest();
        }

        private void DispatchCompletedRequest()
        {
            lock (this.stateLock)
            {
                if (this.isContextCompletionDispatcherRunning)
                {
                    // the dispatcher is already running
                    this.logger.Write(StatefulServiceReplica.RequestCompletionDispatcherActivityId, "SFReplica.DispatchCompletedRequest - context completion dispatcher is running - returning");
                    return;
                }
            }

            // In a loop, process completed replication operations in order of which they were replicated
            for (;;)
            {
                ReplicationContext context;
                if (!this.TryFetchFirstCompletedReplicationOperation(out context))
                {
                    // There is no completed replication operation available to process
                    // The function TryFetch... has already marked the dispatcher running flag to false
                    return;
                }

                // We have a context that we want to complete
                // We have not set isDispatcherRunning to false because we want to 
                // Complete this task on this thread and allow the state provider to 
                // also process completed replication operations in sequence
                if (context.ReplicationException != null)
                {
                    context.TCS.SetException(context.ReplicationException);
                }
                else
                {
                    context.TCS.SetResult(context.Action.ReplicationSequenceNumber);
                }

                // The State Provider has finished applying/reverting the replication operation
                // We can go ahead now and proceed
            }
        }

        private bool TryFetchFirstCompletedReplicationOperation(out ReplicationContext context)
        {
            // Take a lock on the state
            // This is because we are updating the shared inflight requests list as well as looking at the state
            context = null;
            lock (this.stateLock)
            {
                // Either there are no more requests left to process or the first request is not complete
                // We want to apply requests in order of replication 
                // It is possible that the callback for the first operation is stuck on the threadpool
                // and the callback for the second operation is executing 
                // the callback for the second operation will return from here
                // and when the first operation's callback executes it will continue
                if (this.inFlightContexts.Count == 0)
                {
                    this.logger.Write(StatefulServiceReplica.RequestCompletionDispatcherActivityId, "SFReplica: TryFetchFirstCompleted - no further contexts");
                    this.isContextCompletionDispatcherRunning = false;
                    return false;                    
                }
                else if (!this.inFlightContexts[0].IsCompleted())
                {
                    this.logger.Write(StatefulServiceReplica.RequestCompletionDispatcherActivityId, "SFReplica: TryFetchFirstCompleted - context 0 {0} is incomplete.", this.inFlightContexts[0].ActivityId);
                    this.isContextCompletionDispatcherRunning = false;
                    return false;                    
                }

                // remove the first operation
                context = this.inFlightContexts[0];
                this.lastCompletedSequenceNumber = context.ReplicationSequenceNumber;
                this.inFlightContexts.Remove(context);
                this.isContextCompletionDispatcherRunning = true;

                this.logger.Write(StatefulServiceReplica.RequestCompletionDispatcherActivityId, "SFReplica: TryFetchFirstCompleted - processing {0}", context.ActivityId);
                return true;
            }
        }
        
        private Task ApplyCopyOperation(Guid activityId, IOperation operation)
        {
            // Forward the operation to the state provider
            // This can be done in an asynchronous manner as well
            return TaskUtility.ExecuteSynchronously(() => this.stateProvider.ApplyCopyOperation(activityId, operation));            
        }

        private Task ApplyReplicationOperation(Guid activityId, IOperation operation)
        {
            // Forward the operation to the state provider
            // This can be done in an asynchronous manner as well
            return TaskUtility.ExecuteSynchronously(() => this.stateProvider.ApplyReplicationOperation(activityId, operation));            
        }

        private void WaitForContextCompletionDispatcherToComplete(Guid activityId)
        {
            // This is done using polling for readability
            // Ideally you want to do this without burning a thread over here            
            for (; ; )
            {
                this.logger.Write(activityId, "SFREplica: Waiting for queue to drain");
                lock (this.stateLock)
                {
                    // We need a combination of both of these variables
                    // The flag isContextCompletionDispatcherRunning is true when a context is being processed (i.e. applied etc)
                    // However, that flag alone is not enough - consider the following case:
                    // We have items 1, 2, 3, 4 in our queue
                    // They have all been quorum ack'd
                    // However, the callback for item 2 runs first
                    // It sees that item 1 is not complete so it returns immediately 
                    // At this time we are waiting for the callback of item 1 to execute 
                    // but a changerole happens and this function is called
                    // It will see that the context completion dispatcher is not running 
                    // Thus, if we do not have the check for whether the queue is empty or not we may miss the condition
                    if (!this.isContextCompletionDispatcherRunning && this.inFlightContexts.Count == 0)
                    {
                        this.logger.Write(activityId, "SFREplica: Queue is drained");
                        return;
                    }
                }

                Thread.Sleep(50);
            }
        }

        #region Internal stuff

        Task<long> IInternalStatefulServiceReplica.ReplicateAsync(Guid activityId, ActionBase action)
        {
            long sequenceNumber = 0;
            bool success = false;
            var context = new ReplicationContext
            {
                Action = action,
                ActivityId = activityId,
            };

            byte[] dataToSend;
            try
            {
                dataToSend = action.Serialize();
            }
            catch (Exception ex)
            {
                this.Logger.WriteLine(activityId, "SFReplia: Error serializing action");
                this.logger.Write(activityId, "SFReplica: ReplicateAsync threw {0}", ex);
                throw;
            }

            List<byte[]> buffers = new List<byte[]>
                {
                    dataToSend,
                    Encoding.ASCII.GetBytes(activityId.ToString()), // we send the activity id as well so that we can co-relate traces
                };

            OperationData operationData = new OperationData(buffers);

            // Take a lock on the state
            // This is for locking against adding to the list of inflight contexts
            // as well as forcing calls to a change role to wait until this returns
            lock (this.stateLock)
            {
                // Replication can only happen in the context of being primary
                // If we are a primary and there is a change role queued - reject replication requests
                if (this.replicaState != ReplicaState.Primary)
                {
                    this.logger.Write(activityId, "SFReplica: Asked to replicate when not primary");
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Not Primary {0}", this.replicaState));
                }

                // Add this to the set of contexts we are processing
                this.inFlightContexts.Add(context);

                try
                {
                    // Tell Windows Fabric to go and replicate the data
                    Task<long> replicateTask = this.stateReplicator.ReplicateAsync(operationData, CancellationToken.None, out sequenceNumber);
                    context.UpdateSequenceNumber(sequenceNumber);
                    replicateTask.ContinueWith((t) => this.ProcessContextCompletion(t, context));
                    this.logger.Write(activityId, "SFReplica: assigned sequence number {0}", sequenceNumber);
                    success = true;
                }
                catch (Exception ex)
                {
                    this.logger.Write(activityId, "SFReplica: ReplicateAsync threw {0}", ex);
                    throw;
                }
                finally
                {
                    if (!success)
                    {
                        // the call for ReplicateAsync failed
                        // we should undo the add to the list that we did                         
                        this.logger.Write(activityId, "SFReplica: Failed ReplicateAsync - removing from queue");
                        this.inFlightContexts.Remove(context);
                    }
                }
                
                // setup a completion for that as well
                return context.TCS.Task;
            }
        }

        long IInternalStatefulServiceReplica.GetCopyStateSequenceNumber(long upToSequenceNumber)
        {
            // When used in a Service Group GetCopyState we may receive a upToSequenceNumber which the member itself has not replicated.
            // In order to properly copy we need to find the highest LSN less or equal the upToSequenceNumber that the member actually replicated.
            //
            // StatefulServiceReplica maintains a list of pending operations. These track the ReplicationSequenceNumber.
            //
            // When an operation completes it is sequentially dispatched to which will eventually commit it (see StateProviderBase).
            //
            // An operation is either 
            //
            //    committed (lastCommitedSequenceNumber has been updated)
            //    pending (in the inflight list, may or may not have completed but cannot be commited because a previous operation has not yet completed)
            //    completed (not yet committed, removed from the pending list and sequentially dispatched for commit, this is at most one operation)
            //
            // We need to consider the following cases:
            //
            // a) upToSequenceNumber <= StateProviderBase.lastCommitedSequenceNumber
            //
            // That's the easy case. We just copy what we have (see StateProviderBase)
            //
            // b) There are operations pending.
            //
            // We have to check whether any of the operations have a ReplicationSequenceNumber <= upToSequenceNumber. 
            // If that's the case copy has to wait for these operations. 
            // Thus we update the upToSequenceNumber to the highest ReplicationSequenceNumber of those.
            //
            // c) No operations pending with ReplicationSequenceNumber <= upToSequenceNumber
            //
            // This doesn't mean that there is nothing to wait for.
            // There may be one operation that has been removed from the in-flight list but have not yet been committed.
            // lastCompletedSequenceNumber tracks the highest LSN that has been removed from the in-flight.
            // This is the highest LSN the service has ever seen (nothing is pending) at or before upToSequenceNumber.
            // Thus, this is the LSN which has to be committed before copy can proceed.
            
            long lastCommitedSequenceNumber = ((IStateProvider)this.stateProvider).GetLastCommittedSequenceNumber();
            if (upToSequenceNumber <= lastCommitedSequenceNumber)
            {
                return lastCommitedSequenceNumber;
            }

            lock (this.stateLock)
            {
                long maxInflight = long.MinValue;
                foreach (ReplicationContext context in this.inFlightContexts)
                {
                    if (context.ReplicationSequenceNumber <= upToSequenceNumber)
                    {
                        maxInflight = Math.Max(maxInflight, context.ReplicationSequenceNumber);
                    }
                }

                if (maxInflight != long.MinValue)
                {
                    return maxInflight;
                }

                return this.lastCompletedSequenceNumber;
            }
        }

        #endregion

        private class ReplicationContext
        {
            private readonly object lockObj = new object();
            private bool isComplete;

            public ReplicationContext()
            {
                this.TCS = new TaskCompletionSource<long>();
                this.ReplicationSequenceNumber = 0;
            }

            public Guid ActivityId { get; set; }

            public TaskCompletionSource<long> TCS { get; private set; }

            public ActionBase Action { get; set; }

            public Exception ReplicationException { get; private set; }

            public long ReplicationSequenceNumber { get; private set; }

            public void MarkReplicationCompleteWithError(Exception ex)
            {
                lock (this.lockObj)
                {
                    this.ReplicationException = ex;
                    this.isComplete = true;
                }
            }

            public void MarkReplicationCompleteWithSuccess(long sequenceNumber)
            {
                lock (this.lockObj)
                {
                    this.Action.ReplicationSequenceNumber = sequenceNumber;
                    this.isComplete = true;
                }
            }

            public bool IsCompleted()
            {
                lock (this.lockObj)
                {
                    return this.isComplete;
                }
            }

            public void UpdateSequenceNumber(long sequenceNumber)
            {
                this.ReplicationSequenceNumber = sequenceNumber;
            }
        }
    }
}
