/*
 * 
 * User: Gavin Mead
 * Date: 11/19/2009
 * Time: 11:56 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    
    
    /// <summary>
    /// Description of IEdge.
    /// </summary>
    internal interface IEdge<U>
    {
        IEdgeId EdgeId { get; }
        
        U EdgeData { get; set;}
    }
}
