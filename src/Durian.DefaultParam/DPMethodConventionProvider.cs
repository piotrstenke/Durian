namespace Durian.Analysis.DefaultParam;

/// <summary>
/// <see cref="ISourceTextProvider"/> that creates syntax tree of the <c>Durian.Configuration.DPMethodConvention</c> enum.
/// </summary>
public sealed class DPMethodConventionProvider : SourceTextProvider
{
	/// <summary>
	/// Name of the 'Call' field.
	/// </summary>
	public const string Call = nameof(MethodConvention.Call);

	/// <summary>
	/// Name of the 'Copy' field.
	/// </summary>
	public const string Copy = nameof(MethodConvention.Copy);

	/// <summary>
	/// Name of the 'Default' field.
	/// </summary>
	public const string Default = nameof(MethodConvention.Default);

	/// <summary>
	/// Full name of the provided type.
	/// </summary>
	public const string FullName = Namespace + "." + TypeName;

	/// <summary>
	/// Namespace the provided type is located in.
	/// </summary>
	public const string Namespace = DurianStrings.ConfigurationNamespace;

	/// <summary>
	/// Name of the provided type.
	/// </summary>
	public const string TypeName = "DPMethodConvention";

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
namespace {{Namespace}}
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> method is generated.
	/// </summary>
	public enum {{TypeName}}
	{
		/// <summary>
		/// Uses default convention, which is <see cref="{{Call}}"/>.
		/// </summary>
		{{Default}} = {{Call}},

		/// <summary>
		/// Calls the method.
		/// </summary>
		{{Call}} = 0,

		/// <summary>
		/// Copies contents of the method.
		/// </summary>
		{{Copy}} = 1
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
