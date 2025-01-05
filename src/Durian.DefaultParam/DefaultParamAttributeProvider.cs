namespace Durian.Analysis.DefaultParam;

/// <summary>
/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.DefaultParamAttribute</c> class.
/// </summary>
public sealed class DefaultParamAttributeProvider : SourceTextProvider
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
	/// Name of the 'Type' property.
	/// </summary>
	public const string Type = "Type";

	/// <summary>
	/// Name of the provided type.
	/// </summary>
	public const string TypeName = "DefaultParamAttribute";

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
	/// Applies a default type for the generic parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
	public sealed class {{TypeName}} : Attribute
	{
		/// <summary>
		/// Type that is used as the default type for this generic parameter.
		/// </summary>
		public Type {{Type}} { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="{{TypeName}}"/> class.
		/// </summary>
		/// <param name="type">Type that is used as the default type for this generic parameter.</param>
		public {{TypeName}}(Type type)
		{
			{{Type}} = type;
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
