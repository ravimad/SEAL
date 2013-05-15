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
	/// Use this enum to specify the type of graph to create.  To create a weighted graph
	/// Bitwise OR the Undirected or Directed with WithWeight
	/// </summary>
	/// <example>
	/// <code>
	///     GraphType gType = GraphType.Undirected; //Specifies an unweighted undirected graph.
	///     GraphType gType2 = GraphType.Undirected | GraphType.WithWeight; //Specified a weighted undirected graph.
	/// </code>
	/// </example>
	[Flags]
	public enum GraphTypes {
	    /// <summary>
	    /// Use for an undirected graph.
	    /// </summary>
	    Undirected = 1,
	    /// <summary>
	    /// Use for a directed graph.
	    /// </summary>
	    Directed = 2,
	    /// <summary>
	    /// Use in combination with one of the above to create a weighted graph.
	    /// </summary>
	    WithWeight = 4
	}
}
