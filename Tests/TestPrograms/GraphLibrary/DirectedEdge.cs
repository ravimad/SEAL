/*
 * 
 * User: Gavin Mead
 * Date: 11/25/2009
 * Time: 1:35 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    /// <summary>
    /// Description of BaseDirsectedEdge.
    /// </summary>
    internal class DirectedEdge<U> : BaseEdge<U>, IDirectedEdge<U>
    {
        private IVertexId outboundVertex;
        
        public DirectedEdge() : base()
        {
            
        }
        
        public DirectedEdge(IVertexId vertex1, IVertexId vertex2) : base(vertex1,
                                                                             vertex2)
        {
           outboundVertex = vertex1;
        }
        
        public IVertexId OutboundVertex {
            get {
                return outboundVertex;
            }
            set {
                this.outboundVertex = value;
            }
        }
        
        public override bool Equals(object obj)
        {
            if(obj is DirectedEdge<U>)
            {
                return base.Equals(obj as BaseEdge<U>);
            }
            else 
            {
                return false;
            }
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
