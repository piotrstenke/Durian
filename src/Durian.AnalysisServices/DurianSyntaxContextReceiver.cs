using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <inheritdoc cref="IDurianSyntaxContextReceiver"/>
public abstract class DurianSyntaxContextReceiver : IDurianSyntaxContextReceiver
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DurianSyntaxContextReceiver"/> class.
	/// </summary>
	protected DurianSyntaxContextReceiver()
	{
	}

	/// <inheritdoc/>
	public abstract bool IsEmpty();

	/// <inheritdoc/>
	public abstract IEnumerable<SyntaxNode> GetNodes();

	/// <inheritdoc/>
	public abstract bool OnVisitSyntaxNode(in GeneratorSyntaxContext context);

	void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
	{
		OnVisitSyntaxNode(in context);
	}

	void ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode syntaxNode)
	{
		throw new InvalidOperationException($"{nameof(DurianSyntaxContextReceiver)} does not support the '{nameof(ISyntaxReceiver.OnVisitSyntaxNode)}' method");
	}
}
