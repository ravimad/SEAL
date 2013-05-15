/*
 * 
 * User: Gavin Mead
 * Date: 11/20/2009
 * Time: 1:33 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
    
    
    /// <summary>
    /// Used as a unique label for a vertex within a graph.
    /// </summary>
    public interface IVertexId
    {
        /// <summary>
        /// An easy to remember name for a given vertex.  This is recommended to be unique,
        /// but not required.
        /// </summary>
        String FriendlyName {get; set;}
    }
}
