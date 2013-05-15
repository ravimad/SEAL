/*
 * 
 * User: Gavin Mead
 * Date: 11/24/2009
 * Time: 3:56 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    /// <summary>
    /// Description of BaseEdge.
    /// </summary>
    internal class BaseEdge<U> : IEdge<U>
    {
        private U data;
        private IEdgeId edgeId;
        
        public BaseEdge()
        {
            edgeId = new BaseEdgeId();
        }
        
        public BaseEdge(IVertexId vertexId1, IVertexId vertexId2)
        {
            if(vertexId1 == null || vertexId2 == null)
            {
                throw new ArgumentNullException("vertexId1 or vertexId2 is null");
            }
            this.edgeId = new BaseEdgeId(vertexId1, vertexId2);
        }
        
        public IEdgeId EdgeId {
            get {
                return this.edgeId;
            }
            set {
                if(value is BaseEdgeId)
                {
                    this.edgeId = value;
                }
                else {
                    throw new InvalidCastException("value must be a BaseEdgeId");
                }
            }
        }
        
        public U EdgeData {
            get {
                return data;
            }
            set {
                data = value;
            }
        }
        
        public override bool Equals(object obj)
        {
            if(obj is BaseEdge<U>)
            {
                BaseEdge<U> o = obj as BaseEdge<U>;
                return this.edgeId.Equals(o.edgeId);
            }
            else 
            {
                return false;
            }
        }
        
        public override int GetHashCode()
        {
            return this.edgeId.GetHashCode();
        }
    }
}
