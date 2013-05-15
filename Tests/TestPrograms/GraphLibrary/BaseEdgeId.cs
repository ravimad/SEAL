/*
 * 
 * User: Gavin Mead
 * Date: 11/21/2009
 * Time: 4:44 PM
 * 
 * 
 */

using System;

namespace GraphLibrary
{

    
    /// <summary>
    /// Description of BaseEdgeId.
    /// </summary>
    internal class BaseEdgeId : IEdgeId
    {
        private Guid edgeId;
        private IVertexId vertex1;
        private IVertexId vertex2;
        private String name;
        
        
        public BaseEdgeId()
        {
            this.edgeId = Guid.NewGuid();
            name="";
        }
        
        /// <summary>
        /// Creates an edgeId and associates the two IVertexIds with it.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <exception cref="ArgumentNullException">If vertex1 or vertex2 is
        /// null.</exception>
        public BaseEdgeId(IVertexId vertex1, IVertexId vertex2)
        {
            
            if(vertex1 == null)
                throw new ArgumentNullException("vertex1 cannot be null");
            if(vertex2 == null)
                throw new ArgumentNullException("vertex2 cannot be null");
            
            this.edgeId = Guid.NewGuid();
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            name="";
            
        }
        
        public String FriendlyName {
            get {
                return name;
            }
            set {
                if(value != null)
                    name = value;
                else {
                    name = "";
                }
            }
        }
    	    	
		public IVertexId Vertex1Id {
			get {
				return vertex1;
			}
            set {
                vertex1 = value;
            }
		}
    	
		public IVertexId Vertex2Id {
			get {
				return vertex2;
			}
            set {
                vertex2 = value;
            }
		}
        
		public override bool Equals(object obj)
		{
		    if(obj is BaseEdgeId)
		    {
		        BaseEdgeId rhs = (BaseEdgeId) obj;
		        
		        return this.edgeId.Equals(rhs.edgeId)
		                && this.vertex1.Equals(rhs.vertex1)
		                && this.vertex2.Equals(rhs.vertex2);
		            
		    }
		    else 
		    {
		        return false;
		    }
		}
		
		public override int GetHashCode()
		{
		    return edgeId.GetHashCode();
		}
    }
}
