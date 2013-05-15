/*
 * 
 * User: Gavin Mead
 * Date: 11/20/2009
 * Time: 2:31 PM
 * 
 * 
 */
using System;

namespace GraphLibrary
{
        
    /// <summary>
    /// Description of IWeightedEdge.
    /// </summary>
    internal interface IWeightedEdge<U> : IEdge<U> where U : IComparable<U>
    {
        void UpdateWeight(U newWeight);
    }
}
