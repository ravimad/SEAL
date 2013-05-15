/*
 * 
 * User: Gavin Mead
 * Date: 11/19/2009
 * Time: 11:57 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    
    
    /// <summary>
    /// Description of IVertex.
    /// </summary>
    internal interface IVertex<T>
    {
        IVertexId VertexId { get; }
        
        T VertexData { get; set;}
    }
}
