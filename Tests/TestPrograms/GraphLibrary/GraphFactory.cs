/*
 * 
 * User: Gavin Mead
 * Date: 11/30/2009
 * Time: 3:41 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{


    /// <summary>
    /// Description of GraphFactory.
    /// </summary>
    public static class GraphFactory<T,U>
    {

        public static IGraph<T, U> CreateGraph(GraphTypes graphType)
        {
            return new BaseGraph<T, U>(graphType);
        }
    }
}
