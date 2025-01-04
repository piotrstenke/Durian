using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data;

/// <summary>
/// Encapsulates data associated with a single <see cref="INamedTypeSymbol"/> representing a record.
/// </summary>
public interface IRecordData : ITypeData, ISymbolOrMember<INamedTypeSymbol, IRecordData>
{
	/// <summary>
	/// Copy constructor of the record.
	/// </summary>
	ISymbolOrMember<IMethodSymbol, IConstructorData>? CopyConstructor { get; }

	/// <summary>
	/// Determines whether the record is a <see langword="class"/>.
	/// </summary>
	bool IsClass { get; }

	/// <summary>
	/// Determines whether the record is a <see langword="struct"/>.
	/// </summary>
	bool IsStruct { get; }

	/// <summary>
	/// <see cref="ParameterListSyntax"/> of the record's primary constructor.
	/// </summary>
	ParameterListSyntax? ParameterList { get; }

	/// <summary>
	/// Primary constructor of the record.
	/// </summary>
	ISymbolOrMember<IMethodSymbol, IConstructorData>? PrimaryConstructor { get; }

	/// <summary>
	/// Creates a shallow copy of the current data.
	/// </summary>
	new IRecordData Clone();
}