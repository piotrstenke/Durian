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
					foreach (INamespaceSymbol parent in @namespace.GetContainingNamespaces())
					{
						SimpleName_Internal(parent);
						BeginBlock();
						Indent();
						TextBuilder.Append("namespace ");
					}
				}
				else
				{
					foreach (INamespaceSymbol parent in @namespace.GetContainingNamespaces())
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
		public CodeBuilder QualifiedName(IMemberData member)
		{
			InitBuilder();

			foreach (INamespaceSymbol @namespace in member.GetContainingNamespaces())
			{
				SimpleName_Internal(@namespace);
				TextBuilder.Append('.');
			}

			if (member.Symbol.ContainingType is not null)
			{
				foreach (INamedTypeSymbol type in member.GetContainingTypes())
				{
					GenericName(type, true, false);
					TextBuilder.Append('.');
				}
			}

			return GenericName(member.Symbol, true, false);
		}

		/// <summary>
		/// Writes fully qualified name of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to write the qualified name of.</param>
		/// <param name="globalAlias">Determines whether to include the global alias.</param>
		public CodeBuilder QualifiedName(IMemberData member, bool globalAlias)
		{
			if (globalAlias)
			{
				return QualifiedName(member, "global");
			}

			return QualifiedName(member);
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
