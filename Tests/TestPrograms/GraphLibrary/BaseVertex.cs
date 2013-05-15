/*
 * 
 * User: Gavin Mead
 * Date: 11/24/2009
 * Time: 4:20 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    /// <summary>
    /// Description of BaseVertex.
    /// </summary>
    internal class BaseVertex<T> : IVertex<T>
    {
        private IVertexId vertexId;
        private T data;
        
        public BaseVertex()
        {
            vertexId = new BaseVertexId();
        }
        
        public IVertexId VertexId {
            get {
                return vertexId;
            }
        }
        
        public T VertexData {
            get {
                return data;
            }
            set {
                data = value;
            }
        }
        
        public override bool Equals(object obj)
        {
            if(obj is BaseVertex<T>)
            {
                BaseVertex<T> o = obj as BaseVertex<T>;
                return this.vertexId.Equals(o.vertexId);
            }
            else 
            {
                return false;
            }
        }
        
        public override int GetHashCode()
        {
            return this.vertexId.GetHashCode();
        }
    }
}
