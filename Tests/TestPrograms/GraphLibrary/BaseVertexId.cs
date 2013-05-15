/*
 * 
 * User: Gavin Mead
 * Date: 11/21/2009
 * Time: 4:13 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{

    
    /// <summary>
    /// BaseVertexId uses System.Guid to generate unique idenifiers for each vertex.
    /// </summary>
    internal class BaseVertexId : IVertexId
    {
        private Guid vertexId;
        private String name;

        public BaseVertexId()
        {
            this.vertexId = Guid.NewGuid();
            name="";
        }
        
        public String FriendlyName {
            get {
                return name;
            }
            set {
                if(value != null)                    
                    value = name;
                else {
                    value = "";
                }
            }
        }
    
        /// <summary>
        /// Examines the vertexId for equality.  Do not mix and match
        /// IVertexId implementations, as that will return a false.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns true, if the vertexId are equal.</returns>
        public override bool Equals(object obj)
        {
            if(obj is BaseVertexId)
            {
            
                BaseVertexId rhs = (BaseVertexId) obj;
                return vertexId.Equals(rhs.vertexId);

            }
            else 
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return vertexId.GetHashCode();
        }
        
    }
}
