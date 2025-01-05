namespace Durian.Analysis.GlobalScope;

/// <summary>
/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.GlobalScopeAttribute</c> class.
/// </summary>
public sealed class GlobalScopeAttributeProvider : SourceTextProvider
{
	/// <summary>
	/// Full name of the provided type.
	/// </summary>
	public const string FullName = Namespace + "." + TypeName;

	/// <summary>
	/// Namespace the provided type is located in.
	/// </summary>
	public const string Namespace = DurianStrings.MainNamespace;

	/// <summary>
	/// Name of the provided type.
	/// </summary>
	public const string TypeName = "GlobalScopeAttribute";

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeAttributeProvider"/> class.
	/// </summary>
	public GlobalScopeAttributeProvider()
	{
	}

	/// <inheritdoc/>
	public override string GetFullName()
	{
		return FullName;
	}

	/// <inheritdoc/>
	public override string GetNamespace()
	{
		return Namespace;
	}

	/// <inheritdoc/>
	public override string GetText()
	{
		return
$$"""
using System;

namespace {{Namespace}}
{
	/// <summary>
	/// Specifies that static members of this type should be accessible from the top-level (global) scope.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class {{TypeName}} : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="{{TypeName}}"/> class.
		/// </summary>
		public {{TypeName}}()
		{
		}
	}
}

""";
	}

	/// <inheritdoc/>
	public override string GetTypeName()
	{
		return TypeName;
	}
}
