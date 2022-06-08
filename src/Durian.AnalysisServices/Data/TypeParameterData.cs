// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="TypeParameterSyntax"/>.
	/// </summary>
	public class TypeParameterData : MemberData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="Properties"/>.
		/// </summary>
		public new class Properties : Properties<ITypeParameterSymbol>
		{
			/// <inheritdoc cref="TypeParameterData.Constraints"/>
			public GenericConstraint? Constraints { get; set; }

			/// <inheritdoc cref="TypeParameterData.ConstraintClause"/>
			public TypeParameterConstraintClauseSyntax? ConstraintClause { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		private GenericConstraint? _constraints;
		private DefaultedValue<TypeParameterConstraintClauseSyntax> _constraintClause;

		/// <summary>
		/// Target <see cref="TypeParameterSyntax"/>.
		/// </summary>
		public new TypeParameterSyntax Declaration => (base.Declaration as TypeParameterSyntax)!;

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new ITypeParameterSymbol Symbol => (base.Symbol as ITypeParameterSymbol)!;

		/// <summary>
		/// Generic constraints applied to this type parameter.
		/// </summary>
		public GenericConstraint Constraints
		{
			get
			{
				return _constraints ??= Symbol.GetConstraints(true);
			}
		}

		/// <summary>
		/// <see cref="TypeParameterConstraintSyntax"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public TypeParameterConstraintClauseSyntax? ConstraintClause
		{
			get
			{
				if(!_constraintClause.IsDefault)
				{
					_constraintClause.SetValue(Declaration.GetConstraintClause());
				}

				return _constraintClause.Value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameterData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeParameterSyntax"/> this <see cref="TypeParameterData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeParameterData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public TypeParameterData(TypeParameterSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			if(properties is not null)
			{
				_constraintClause = properties.ConstraintClause;
				_constraints = properties.Constraints;
			}
		}

		internal TypeParameterData(ITypeParameterSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
