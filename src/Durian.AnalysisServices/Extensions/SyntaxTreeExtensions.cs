using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions;

/// <summary>
/// Contains various extension methods for the <see cref="SyntaxTree"/> class.
/// </summary>
public static class SyntaxTreeExtensions
{
	/// <summary>
	/// Returns first node of type <typeparamref name="TNode"/> present in the specified <paramref name="syntaxTree"/>.
	/// </summary>
	/// <typeparam name="TNode">Type of node to return.</typeparam>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to get the nodes of.</param>
	/// <param name="descendIntoChildren">Function that determines if the search descends into a node's children.</param>
	/// <param name="descendIntoTrivia">Determines if nodes that are part of structured trivia are included in the list.</param>
	public static TNode? GetNode<TNode>(this SyntaxTree syntaxTree, Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia = false) where TNode : SyntaxNode
	{
		return syntaxTree.GetNodes<TNode>(descendIntoChildren, descendIntoTrivia).FirstOrDefault();
	}

	/// <summary>
	/// Returns first node of type <typeparamref name="TNode"/> present in the specified <paramref name="syntaxTree"/>.
	/// </summary>
	/// <typeparam name="TNode">Type of node to return.</typeparam>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to get the nodes of.</param>
	public static TNode? GetNode<TNode>(this SyntaxTree syntaxTree) where TNode : SyntaxNode
	{
		return syntaxTree.GetNodes<TNode>().FirstOrDefault();
	}

	/// <summary>
	/// Returns all nodes present in the specified <paramref name="syntaxTree"/>.
	/// </summary>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to get the nodes of.</param>
	public static IEnumerable<SyntaxNode> GetNodes(this SyntaxTree syntaxTree)
	{
		return syntaxTree.GetRoot().DescendantNodesAndSelf();
	}

	/// <summary>
	/// Returns all nodes present in the specified <paramref name="syntaxTree"/>.
	/// </summary>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to get the nodes of.</param>
	/// <param name="descendIntoChildren">Function that determines if the search descends into a node's children.</param>
	/// <param name="descendIntoTrivia">Determines if nodes that are part of structured trivia are included in the list.</param>
	public static IEnumerable<SyntaxNode> GetNodes(this SyntaxTree syntaxTree, Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia = false)
	{
		return syntaxTree.GetRoot().DescendantNodesAndSelf(descendIntoChildren, descendIntoTrivia);
	}

	/// <summary>
	/// Returns all nodes of type <typeparamref name="TNode"/> present in the specified <paramref name="syntaxTree"/>.
	/// </summary>
	/// <typeparam name="TNode">Type of nodes to return.</typeparam>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to get the nodes of.</param>
	public static IEnumerable<TNode> GetNodes<TNode>(this SyntaxTree syntaxTree) where TNode : SyntaxNode
	{
		return syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TNode>();
	}

	/// <summary>
	/// Returns all nodes of type <typeparamref name="TNode"/> present in the specified <paramref name="syntaxTree"/>.
	/// </summary>
	/// <typeparam name="TNode">Type of nodes to return.</typeparam>
	/// <param name="syntaxTree"><see cref="SyntaxTree"/> to get the nodes of.</param>
	/// <param name="descendIntoChildren">Function that determines if the search descends into a node's children.</param>
	/// <param name="descendIntoTrivia">Determines if nodes that are part of structured trivia are included in the list.</param>
	public static IEnumerable<TNode> GetNodes<TNode>(this SyntaxTree syntaxTree, Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia = false) where TNode : SyntaxNode
	{
		return syntaxTree.GetRoot().DescendantNodesAndSelf(descendIntoChildren, descendIntoTrivia).OfType<TNode>();
	}
}
