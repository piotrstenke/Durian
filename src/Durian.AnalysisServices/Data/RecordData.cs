// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="RecordDeclarationSyntax"/>.
	/// </summary>
	public class RecordData : TypeData<RecordDeclarationSyntax>
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="RecordData"/>.
		/// </summary>
		public new class Properties : TypeData<RecordDeclarationSyntax>.Properties
		{
			/// <inheritdoc cref="RecordData.ParameterList"/>
			public DefaultedValue<ParameterListSyntax> ParameterList { get; set; }

			/// <inheritdoc cref="RecordData.PrimaryConstructor"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, ConstructorData>> PrimaryConstructor { get; set; }

			/// <inheritdoc cref="RecordData.CopyConstructor"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, ConstructorData>> CopyConstructor { get; set; }
		}

		private DefaultedValue<ParameterListSyntax> _parameterList;
		private DefaultedValue<ISymbolOrMember<IMethodSymbol, ConstructorData>> _primaryConstructor;
		private DefaultedValue<ISymbolOrMember<IMethodSymbol, ConstructorData>> _copyConstructor;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="TypeData{TDeclaration}.Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// <see cref="ParameterListSyntax"/> of the record's primary constructor.
		/// </summary>
		public ParameterListSyntax? ParameterList
		{
			get
			{
				if(_parameterList.IsDefault)
				{
					if(Declaration.ParameterList is not null)
					{
						_parameterList = Declaration.ParameterList;
					}
					else
					{
						_parameterList = PartialDeclarations.Select(d => d.ParameterList).FirstOrDefault(d => d is not null);
					}
				}

				return _parameterList.Value;
			}
		}

		/// <summary>
		/// Primary constructor of the record.
		/// </summary>
		public ISymbolOrMember<IMethodSymbol, ConstructorData>? PrimaryConstructor
		{
			get
			{
				if(_primaryConstructor.IsDefault)
				{
					_primaryConstructor = Symbol.GetPrimaryConstructor().ToDataOrSymbolInternal<ConstructorData>(ParentCompilation);
				}

				return _primaryConstructor.Value;
			}
		}

		/// <summary>
		/// Copy constructor of the record.
		/// </summary>
		public ISymbolOrMember<IMethodSymbol, ConstructorData>? CopyConstructor
		{
			get
			{
				if(_copyConstructor.IsDefault)
				{
					_copyConstructor = Symbol.GetSpecialConstructor(SpecialConstructor.Copy).ToDataOrSymbolInternal<ConstructorData>(ParentCompilation);
				}

				return _copyConstructor.Value;
			}
		}

		/// <summary>
		/// Determines whether the record is a <see langword="class"/>.
		/// </summary>
		public bool IsClass => Symbol.IsReferenceType;

		/// <summary>
		/// Determines whether the record is a <see langword="struct"/>.
		/// </summary>
		public bool IsStruct => Symbol.IsValueType;

		/// <summary>
		/// Initializes a new instance of the <see cref="RecordData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="RecordDeclarationSyntax"/> this <see cref="RecordData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="RecordData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public RecordData(RecordDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			if(properties is not null)
			{
				_primaryConstructor = properties.PrimaryConstructor;
				_parameterList = properties.ParameterList;
				_copyConstructor = properties.CopyConstructor;
			}
		}

		internal RecordData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
