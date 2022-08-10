// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	public partial class CodeBuilder
	{
		/// <summary>
		/// Writes name of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to write the name of.</param>
		/// <param name="format">Format of the name.</param>
		public CodeBuilder Name(IMemberData member, SymbolName format = default)
		{
			InitBuilder();

			if (member is INamedTypeSymbol type && KeywordType(type, format != SymbolName.SystemName && Style.UseBuiltInAliases))
			{
				return this;
			}

			switch (format)
			{
				case SymbolName.Default:
					SimpleName_Internal(member.Symbol);
					break;

				case SymbolName.Generic:
					return GenericName(member.Symbol, false, false);

				case SymbolName.VarianceGeneric:
					return GenericName(member.Symbol, false, true);

				case SymbolName.Substituted:
					return GenericName(member.Symbol, true, false);

				case SymbolName.Attribute:
					return AttributeName(member.Symbol);

				case SymbolName.Qualified:
					return QualifiedName(member, true);

				case SymbolName.GlobalQualified:
					return QualifiedName(member, true, true);

				default:
					goto case SymbolName.Default;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="IMemberData"/> to begin declaration of.</param>
		/// <param name="includeParent">Determines whether to include parent namespaces in the declaration.</param>
		public CodeBuilder Namespace(IMemberData @namespace, bool includeParent = true)
		{
			return Namespace(@namespace, Style.NamespaceStyle, includeParent);
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="IMemberData"/> to begin declaration of.</param>
		/// <param name="type">Type of namespace declaration to write.</param>
		/// <param name="includeParent">Determines whether to include parent namespaces in the declaration.</param>
		public CodeBuilder Namespace(IMemberData @namespace, NamespaceStyle type, bool includeParent = true)
		{
			Indent();

			TextBuilder.Append("namespace ");

			if (includeParent)
			{
				if (type == NamespaceStyle.Nested)
				{
					foreach (INamespaceSymbol parent in @namespace.ContainingNamespaces.GetSymbols())
					{
						SimpleName_Internal(parent);
						BeginBlock();
						Indent();
						TextBuilder.Append("namespace ");
					}
				}
				else
				{
					foreach (INamespaceSymbol parent in @namespace.ContainingNamespaces.GetSymbols())
					{
						SimpleName_Internal(parent);
						TextBuilder.Append('.');
					}
				}
			}

			SimpleName_Internal(@namespace.Symbol);

			if (type == NamespaceStyle.File)
			{
				ColonNewLine();
			}
			else
			{
				BeginBlock();
			}

			return this;
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to write the qualified name of.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters.</param>
		public CodeBuilder QualifiedName(IMemberData member, bool useArguments = false)
		{
			InitBuilder();

			foreach (INamespaceSymbol @namespace in member.ContainingNamespaces.GetSymbols())
			{
				SimpleName_Internal(@namespace);
				TextBuilder.Append('.');
			}

			if (member.Symbol.ContainingType is not null)
			{
				if (useArguments)
				{
					foreach (INamedTypeSymbol type in member.ContainingTypes.GetSymbols())
					{
						SimpleName_Internal(type);
						TypeArgumentList(type.TypeArguments);
						TextBuilder.Append('.');
					}
				}
				else
				{
					foreach (INamedTypeSymbol type in member.ContainingTypes.GetSymbols())
					{
						SimpleName_Internal(type);
						TypeParameterList(type.TypeParameters);
						TextBuilder.Append('.');
					}
				}
			}

			return GenericName(member.Symbol, true, false);
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to write the qualified name of.</param>
		///  <param name="useArguments">Determines whether to use type arguments instead of type parameters.</param>
		/// <param name="globalAlias">Determines whether to include the global alias.</param>
		public CodeBuilder QualifiedName(IMemberData member, bool useArguments, bool globalAlias)
		{
			if (globalAlias)
			{
				return QualifiedName(member, "global");
			}

			return QualifiedName(member, useArguments);
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="member"/> using the given <paramref name="alias"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to write the qualified name of.</param>
		/// <param name="alias"><see cref="IAliasSymbol"/> to include in the qualified name.</param>
		public CodeBuilder QualifiedName(IMemberData member, IAliasSymbol alias)
		{
			return QualifiedName(member, alias.Name);
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="member"/> using the given <paramref name="alias"/>.
		/// </summary>
		/// <param name="member"><see cref="ISymbol"/> to write the qualified name of.</param>
		/// <param name="alias">Alias to include in the qualified name.</param>
		public CodeBuilder QualifiedName(IMemberData member, string alias)
		{
			InitBuilder();
			TextBuilder.Append(alias);
			TextBuilder.Append(':');
			TextBuilder.Append(':');

			return QualifiedName(member);
		}
	}
}
