/*
 * 
 * User: Gavin Mead
 * Date: 11/20/2009
 * Time: 2:30 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    
    
    /// <summary>
    /// Description of IDirectedEdge.
    /// </summary>
    internal interface IDirectedEdge<U> : IEdge<U>
    {
        IVertexId OutboundVertex {get; set;}
    }
}
