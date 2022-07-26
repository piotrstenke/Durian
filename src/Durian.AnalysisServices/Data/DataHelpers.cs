// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.CompilerServices;
using Durian.Analysis.Data.FromSource;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	internal static class DataHelpers
	{
		public abstract class VariableDataWrapper<TSymbol, TData> : ISymbolOrMember<TSymbol, TData>
			where TSymbol : class, ISymbol
			where TData : class, IMemberData, IVariableDeclarator
		{
			private TSymbol? _symbol;
			private TData? _data;

			public bool HasMember => _data is not null;

			public int Index { get; }

			public TData Member
			{
				get
				{
					if (_data is not null)
					{
						return _data;
					}

					VariableDeclaratorSyntax variable = Variable;

					IVariableDeclaratorProperties props = Parent.GetProperties();
					props.Symbol = Symbol;
					props.Variable = variable;
					props.Index = Index;

					_data = CreateData(props);

					return _data;
				}
			}

			public TData Parent { get; }

			public TSymbol Symbol
			{
				get
				{
					return _symbol ??= (TSymbol)Parent.SemanticModel.GetDeclaredSymbol(Variable)!;
				}
			}

			public VariableDeclaratorSyntax Variable => GetVariableDeclaration().Variables[Index];

			ISymbol ISymbolOrMember.Symbol => Symbol;

			IMemberData ISymbolOrMember.Member => Member;

			protected VariableDataWrapper(TData parent, int index)
			{
				Parent = parent;
				Index = index;
			}

			protected abstract TData CreateData(IVariableDeclaratorProperties props);

			protected abstract VariableDeclarationSyntax GetVariableDeclaration();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DefaultedValue<T> ToDefaultedValue<T>(T? value) where T : class
		{
			return value is null ? default : new(value);
		}

		public static IWritableSymbolContainer<TSymbol, TData>? FromDefaultedOrEmpty<TSymbol, TData>(DefaultedValue<IWritableSymbolContainer<TSymbol, TData>> value)
			where TSymbol : class, ISymbol
			where TData : class, IMemberData
		{
			if(value.IsDefault)
			{
				return null;
			}

			if(value.Value is null)
			{
				return SymbolContainerFactory.EmptyWritable<TSymbol, TData>();
			}

			return value.Value;
		}

		public static ISymbolContainer<TSymbol, TData>? FromDefaultedOrEmpty<TSymbol, TData>(DefaultedValue<ISymbolContainer<TSymbol, TData>> value)
			where TSymbol : class, ISymbol
			where TData : class, IMemberData
		{
			if (value.IsDefault)
			{
				return null;
			}

			if (value.Value is null)
			{
				return SymbolContainerFactory.Empty<TSymbol, TData>();
			}

			return value.Value;
		}

		public static ILeveledSymbolContainer<TSymbol, TData>? FromDefaultedOrEmpty<TSymbol, TData>(DefaultedValue<ILeveledSymbolContainer<TSymbol, TData>> value)
			where TSymbol : class, ISymbol
			where TData : class, IMemberData
		{
			if (value.IsDefault)
			{
				return null;
			}

			if (value.Value is null)
			{
				return SymbolContainerFactory.EmptyLeveled<TSymbol, TData>();
			}

			return value.Value;
		}

		public static TProps EnsureValidDeclaratorProperties<TProps>(
			BaseFieldDeclarationSyntax declaration,
			ICompilationData compilation,
			MemberData.Properties? properties
		) where TProps : MemberData.Properties, IVariableDeclaratorProperties, new()
		{
			if(declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			return EnsureValidDeclaratorProperties<TProps>(declaration.Declaration, compilation, properties);
		}

		public static TProps EnsureValidDeclaratorProperties<TProps>(
			LocalDeclarationStatementSyntax declaration,
			ICompilationData compilation,
			MemberData.Properties? properties
		) where TProps : MemberData.Properties, IVariableDeclaratorProperties, new()
		{
			if (declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			return EnsureValidDeclaratorProperties<TProps>(declaration.Declaration, compilation, properties);
		}

		private static TProps EnsureValidDeclaratorProperties<TProps>(
			VariableDeclarationSyntax declaration,
			ICompilationData compilation,
			MemberData.Properties? properties
		) where TProps : MemberData.Properties, IVariableDeclaratorProperties, new()
		{
			TProps target;

			bool isLocalProperties;

			if (properties is not TProps props)
			{
				target = new TProps();
				target.FillWithDefaultData();
				isLocalProperties = true;
			}
			else
			{
				target = props;
				isLocalProperties = false;
			}

			bool isChanged = false;

			VariableDeclaratorSyntax? variable;
			SemanticModel? semanticModel;
			ISymbol? symbol;

			if (target.Symbol is null)
			{
				variable = RetrieveVariableFromProperties(declaration, target, ref isChanged);
				semanticModel = RetrieveSemanticModelFromProperties(compilation, target, variable, ref isChanged);
				symbol = semanticModel.GetDeclaredSymbol(variable);
			}
			else
			{
				variable = target.Variable;
				semanticModel = target.SemanticModel;
				symbol = target.Symbol;
			}

			if(isChanged)
			{
				if (!isLocalProperties)
				{
					TProps newProperties = new();
					newProperties.Map(target);
					target = newProperties;
				}

				target.Variable = variable;
				target.SemanticModel = semanticModel;
				target.Symbol = symbol;
			}

			return target;
		}

		private static SemanticModel RetrieveSemanticModelFromProperties(ICompilationData compilation, MemberData.Properties properties, VariableDeclaratorSyntax variable, ref bool isChanged)
		{
			if (properties.SemanticModel is not null)
			{
				return properties.SemanticModel;
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			isChanged = true;
			return compilation.Compilation.GetSemanticModel(variable);
		}

		private static VariableDeclaratorSyntax RetrieveVariableFromProperties(VariableDeclarationSyntax declaration, IVariableDeclaratorProperties properties, ref bool isChanged)
		{
			if (properties.Variable is not null)
			{
				return properties.Variable;
			}

			if (declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			int index = properties.Index ?? default;

			isChanged = true;
			return declaration.Variables[index];
		}
	}
}
