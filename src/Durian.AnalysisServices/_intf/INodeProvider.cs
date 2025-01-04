using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <summary>
/// Provides a method that returns an array of <see cref="SyntaxNode"/>s.
/// </summary>
public interface INodeProvider
{
	/// <summary>
	/// Returns a collection of <see cref="SyntaxNode"/>s.
	/// </summary>
	IEnumerable<SyntaxNode> GetNodes();
}

/// <summary>
/// Provides a method that returns an array of <see cref="SyntaxNode"/>s of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Type of <see cref="SyntaxNode"/> this <see cref="INodeProvider{T}"/> can return.</typeparam>
public interface INodeProvider<out T> : INodeProvider where T : SyntaxNode
{
	/// <summary>
	/// Returns a collection of <see cref="SyntaxNode"/>s of type <typeparamref name="T"/>.
	/// </summary>
	new IEnumerable<T> GetNodes();
}
