/*
 * 
 * User: Gavin Mead
 * Date: 11/20/2009
 * Time: 1:36 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{  
    
    
    /// <summary>
    /// Description of IEdgeId.
    /// </summary>
    public interface IEdgeId
    {
        IVertexId Vertex1Id { get; set; }

        IVertexId Vertex2Id { get; set; }
        
        String FriendlyName {get; set;}
    }
}
