namespace Durian.Analysis;

/// <summary>
/// Provides text and hint name of a syntax tree.
/// </summary>
public interface ISourceTextProvider
{
	/// <summary>
	/// Returns full name of the syntax tree, e.g. a fully-qualified type name.
	/// </summary>
	string GetFullName();

	/// <summary>
	/// Returns the name of the syntax tree.
	/// </summary>
	string GetHintName();

	/// <summary>
	/// Returns name of the namespace the type represented by the syntax tree is part of.
	/// </summary>
	string GetNamespace();

	/// <summary>
	/// Returns the text of the syntax tree.
	/// </summary>
	string GetText();

	/// <summary>
	/// Returns name of the type the syntax tree represents.
	/// </summary>
	string GetTypeName();
}
