// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="FieldDeclarationSyntax"/>.
	/// </summary>
	public class FieldData : MemberData, IFieldData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="FieldData"/>.
		/// </summary>
		public new class Properties : Properties<IFieldSymbol>
		{
			/// <inheritdoc cref="FieldData.Index"/>
			public int Index { get; set; }

			/// <inheritdoc cref="FieldData.Variable"/>
			public VariableDeclaratorSyntax? Variable { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			/// <param name="fillWithDefault">Determines whether to fill the current properties with default data.</param>
			public Properties(bool fillWithDefault) : base(fillWithDefault)
			{
			}

			/// <inheritdoc cref="MemberData.Properties.Clone"/>
			public new Properties Clone()
			{
				return (CloneCore() as Properties)!;
			}

			/// <inheritdoc cref="MemberData.Properties.Map(MemberData.Properties)"/>
			public virtual void Map(Properties properties)
			{
				base.Map(properties);
				properties.Index = Index;
				properties.Variable = Variable;
			}

			/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
			[Obsolete("Use Map(Properties) instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public sealed override void Map(Properties<IFieldSymbol> properties)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			{
				if (properties is Properties props)
				{
					Map(props);
				}
				else
				{
					base.Map(properties);
				}
			}

			/// <inheritdoc/>
			protected override void FillWithDefaultData()
			{
				IsPartial = false;
				Virtuality = Analysis.Virtuality.NotVirtual;
			}

			/// <inheritdoc/>
			protected override MemberData.Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}
		}

		/// <summary>
		/// Target <see cref="FieldDeclarationSyntax"/>.
		/// </summary>
		public new FieldDeclarationSyntax Declaration => (base.Declaration as FieldDeclarationSyntax)!;

		/// <summary>
		/// Index of this field in the <see cref="Declaration"/>.
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// <see cref="IFieldSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IFieldSymbol Symbol => (base.Symbol as IFieldSymbol)!;

		/// <summary>
		/// <see cref="VariableDeclaratorSyntax"/> used to declare this field. Equivalent to using <c>Declaration.Declaration.Variables[Index]</c>.
		/// </summary>
		public VariableDeclaratorSyntax Variable { get; private set; }

		IFieldData ISymbolOrMember<IFieldSymbol, IFieldData>.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="FieldData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="FieldDeclarationSyntax"/> this <see cref="FieldData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="FieldData"/>.</param>
		/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> was out of range.
		/// </exception>
		public FieldData(FieldDeclarationSyntax declaration, ICompilationData compilation, int index) : this(
			declaration,
			compilation,
			GetSemanticModel(declaration, compilation, null),
			declaration.Declaration.GetVariable(index)
		)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="PropertyDeclarationSyntax"/> this <see cref="PropertyData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="PropertyData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <see cref="Properties.Index"/> was out of range.
		/// </exception>
		public FieldData(FieldDeclarationSyntax declaration, ICompilationData compilation, Properties? properties) : this(
			declaration,
			compilation,
			properties?.Index ?? default
		)
		{
		}

		internal FieldData(IFieldSymbol symbol, ICompilationData compilation, Properties? properties = default) : this(symbol, compilation, properties as MemberData.Properties)
		{
		}


		internal FieldData(IFieldSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(
			GetFieldDeclarationFromSymbol(
				symbol,
				compilation,
				properties as Properties,
				out SemanticModel semanticModel,
				out VariableDeclaratorSyntax var,
				out int index,
				out bool usedPropertySemanticModel
			),
			compilation,
			isFullProperty ? properties : new Properties(true)
			{ 
				SemanticModel = semanticModel,
				Variable = var,
				Index = index
			}
		)
		{
			Variable = var;
			Index = index;
		}

		private FieldData(
			FieldDeclarationSyntax declaration,
			ICompilationData compilation,
			IFieldSymbol symbol,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable,
			int index
		) : base(declaration, compilation, symbol, semanticModel)
		{
			Variable = variable;
			Index = index;
		}

		private FieldData(
			FieldDeclarationSyntax declaration,
			ICompilationData compilation,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable
		) : base(
			declaration,
			compilation,
			(semanticModel.GetDeclaredSymbol(variable) as IFieldSymbol)!,
			semanticModel)
		{
			Variable = variable;
		}

		/// <summary>
		/// Returns a collection of new <see cref="FieldData"/>s of all variables defined in the <see cref="Declaration"/>.
		/// </summary>
		public IEnumerable<FieldData> GetUnderlayingFields()
		{
			int index = Index;
			int length = Declaration.Declaration.Variables.Count;

			for (int i = 0; i < index; i++)
			{
				yield return GetData(i);
			}

			for (int i = index + 1; i < length; i++)
			{
				yield return GetData(i);
			}

			FieldData GetData(int index)
			{
				VariableDeclaratorSyntax variable = Declaration.Declaration.Variables[index];

				return new FieldData(
					Declaration,
					ParentCompilation,
					(IFieldSymbol)SemanticModel.GetDeclaredSymbol(variable)!,
					SemanticModel,
					variable,
					index
				);
			}
		}

		IEnumerable<IFieldData> IFieldData.GetUnderlayingFields()
		{
			return GetUnderlayingFields();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static SemanticModel GetSemanticModel(CSharpSyntaxNode declaration, ICompilationData compilation, MemberData.Properties? properties)
		{
			if(properties?.SemanticModel is SemanticModel semanticModel)
			{
				return semanticModel;
			}

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

		private static FieldDeclarationSyntax GetFieldDeclarationFromSymbol(
			IFieldSymbol symbol,
			ICompilationData compilation,
			Properties? properties,
			out SemanticModel semanticModel,
			out VariableDeclaratorSyntax variable,
			out int index,
			out bool usedPropertySemanticModel
		)
		{
			if(properties is not null)
			{
				variable = properties.Variable ?? GetVariable();
				FieldDeclarationSyntax field = GetDeclaration(variable);

				if(properties.SemanticModel is null)
				{
					semanticModel = compilation.Compilation.GetSemanticModel(variable.SyntaxTree);
					usedPropertySemanticModel = false;
				}
				else
				{
					semanticModel = properties.SemanticModel;
					usedPropertySemanticModel = true;
				}

				index = GetIndex(variable, field);

				return field;
			}
			else
			{
				variable = GetVariable();
				FieldDeclarationSyntax field = GetDeclaration(variable);
				semanticModel = compilation.Compilation.GetSemanticModel(variable.SyntaxTree);
				index = GetIndex(variable, field);
				usedPropertySemanticModel = false;

				return field;
			}

			VariableDeclaratorSyntax GetVariable()
			{
				if(symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not VariableDeclaratorSyntax decl)
				{
					throw Exc_NoSyntaxReference(symbol);
				}

				return decl;
			}

			FieldDeclarationSyntax GetDeclaration(VariableDeclaratorSyntax variable)
			{
				if(variable?.Parent?.Parent is not FieldDeclarationSyntax field)
				{
					throw Exc_NoSyntaxReference(symbol);
				}

				return field;
			}

			int GetIndex(VariableDeclaratorSyntax variable, FieldDeclarationSyntax field)
			{
				SeparatedSyntaxList<VariableDeclaratorSyntax> variables = field.Declaration.Variables;

				for (int i = 0; i < variables.Count; i++)
				{
					if (variables[i].IsEquivalentTo(variable))
					{
						return i;
					}
				}

				throw Exc_NoSyntaxReference(symbol);
			}
		}
	}
}
