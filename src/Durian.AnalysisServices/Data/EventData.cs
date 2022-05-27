// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="EventFieldDeclarationSyntax"/> or <see cref="EventDeclarationSyntax"/>.
	/// </summary>
	public class EventData : MemberData
	{
		/// <summary>
		/// Returns the <see cref="Declaration"/> as a <see cref="EventFieldDeclarationSyntax"/>.
		/// </summary>
		public EventFieldDeclarationSyntax? AsField => Declaration as EventFieldDeclarationSyntax;

		/// <summary>
		/// Returns the <see cref="Declaration"/> as a <see cref="EventDeclarationSyntax"/>.
		/// </summary>
		public EventDeclarationSyntax? AsProperty => Declaration as EventDeclarationSyntax;

		/// <summary>
		/// Target <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		public new MemberDeclarationSyntax Declaration => (base.Declaration as MemberDeclarationSyntax)!;

		/// <summary>
		/// Index of this field in the <see cref="MemberData.Declaration"/>. Returns <c>0</c> if the event is defined as a property.
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// <see cref="IEventSymbol"/> associated with the <see cref="EventFieldDeclarationSyntax"/> or <see cref="EventDeclarationSyntax"/>.
		/// </summary>
		public new IEventSymbol Symbol => (base.Symbol as IEventSymbol)!;

		/// <summary>
		/// <see cref="VariableDeclaratorSyntax"/> used to declare this event field. Equivalent to using <c>AsField.Declaration.Variables[Index]</c>.
		/// </summary>
		public VariableDeclaratorSyntax? Variable { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EventData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EventDeclarationSyntax"/> this <see cref="DelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="DelegateData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public EventData(EventDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EventFieldDeclarationSyntax"/> this <see cref="DelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="DelegateData"/>.</param>
		/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> was out of range.
		/// </exception>
		public EventData(EventFieldDeclarationSyntax declaration, ICompilationData compilation, int index = 0)
			: this(declaration, compilation, GetSemanticModel(compilation, declaration), GetVariable(declaration, index))
		{
			Index = index;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventData"/> class.
		/// </summary>
		/// <param name="symbol"><see cref="IEventSymbol"/> this <see cref="EventData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EventData"/>.</param>
		internal EventData(IEventSymbol symbol, ICompilationData compilation) : base(
			GetFieldOrProperty(
				symbol,
				compilation,
				out SemanticModel semanticModel,
				out VariableDeclaratorSyntax? variable,
				out int index),
			compilation,
			symbol,
			semanticModel
		)
		{
			Index = index;
			Variable = variable;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EventFieldDeclarationSyntax"/> this <see cref="EventData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EventData"/>.</param>
		/// <param name="symbol"><see cref="IEventSymbol"/> this <see cref="EventData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="variable"><see cref="VariableDeclaratorSyntax"/> that represents the target variable.</param>
		/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="IEventSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal EventData(
			EventFieldDeclarationSyntax declaration,
			ICompilationData compilation,
			IEventSymbol symbol,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable,
			int index,
			string[]? modifiers = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			modifiers,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
			Index = index;
			Variable = variable;
		}

		/// <inheritdoc cref="EventData(EventFieldDeclarationSyntax, ICompilationData, IEventSymbol, SemanticModel, VariableDeclaratorSyntax, int, string[], IEnumerable{ITypeData}?, IEnumerable{INamespaceSymbol}?, IEnumerable{AttributeData}?)"/>
		protected internal EventData(
			EventDeclarationSyntax declaration,
			ICompilationData compilation,
			IEventSymbol symbol,
			SemanticModel semanticModel,
			string[]? modifiers = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(declaration, compilation, symbol, semanticModel, modifiers, containingTypes, containingNamespaces, attributes)
		{
		}

		private EventData(
			EventFieldDeclarationSyntax declaration,
			ICompilationData compilation,
			IEventSymbol symbol,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable,
			int index
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel
		)
		{
			Variable = variable;
			Index = index;
		}

		private EventData(
			EventFieldDeclarationSyntax declaration,
			ICompilationData compilation,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable
		) : base(
			declaration,
			compilation,
			(semanticModel.GetDeclaredSymbol(variable) as IEventSymbol)!,
			semanticModel
		)
		{
			Variable = variable;
		}

		/// <summary>
		/// Returns a collection of new <see cref="EventData"/>s of all variables defined in the <see cref="MemberData.Declaration"/>.
		/// </summary>
		public IEnumerable<EventData> GetUnderlayingEvents()
		{
			EventFieldDeclarationSyntax? field = AsField;

			if (field is null)
			{
				yield break;
			}

			int index = Index;
			int length = field.Declaration.Variables.Count;

			for (int i = 0; i < length; i++)
			{
				if (i == index)
				{
					yield return this;
					continue;
				}

				VariableDeclaratorSyntax variable = field.Declaration.Variables[i];

				yield return new EventData(
					field,
					ParentCompilation,
					(IEventSymbol)SemanticModel.GetDeclaredSymbol(variable)!,
					SemanticModel,
					variable,
					index
				);
			}
		}

		private static MemberDeclarationSyntax GetFieldOrProperty(
			IEventSymbol symbol,
			ICompilationData compilation,
			out SemanticModel semanticModel,
			out VariableDeclaratorSyntax? variable,
			out int index
		)
		{
			SyntaxNode? node = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

			if (node is VariableDeclaratorSyntax var)
			{
				EventFieldDeclarationSyntax? field = (node.Parent?.Parent as EventFieldDeclarationSyntax)!;

				if (field is null)
				{
					throw Exc_NoSyntaxReference(symbol);
				}

				SeparatedSyntaxList<VariableDeclaratorSyntax> variables = field.Declaration.Variables;
				int length = variables.Count;

				for (int i = 0; i < length; i++)
				{
					if (variables[i].IsEquivalentTo(var))
					{
						index = i;
						semanticModel = compilation.Compilation.GetSemanticModel(field.SyntaxTree);
						variable = var;
						return field;
					}
				}
			}
			else if (node is EventDeclarationSyntax prop)
			{
				semanticModel = compilation.Compilation.GetSemanticModel(node.SyntaxTree);
				index = 0;
				variable = null;
				return prop;
			}

			throw Exc_NoSyntaxReference(symbol);
		}

		private static SemanticModel GetSemanticModel(ICompilationData compilation, EventFieldDeclarationSyntax declaration)
		{
			if (declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
		}

		private static VariableDeclaratorSyntax GetVariable(EventFieldDeclarationSyntax declaration, int index)
		{
			if (index < 0 || index >= declaration.Declaration.Variables.Count)
			{
				throw new IndexOutOfRangeException(nameof(index));
			}

			return declaration.Declaration.Variables[index];
		}
	}
}
