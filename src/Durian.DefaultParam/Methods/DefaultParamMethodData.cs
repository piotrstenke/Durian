// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using Durian.Analysis.Data.FromSource;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam.Methods
{
	/// <summary>
	/// <see cref="MethodData"/> that contains additional information needed by the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public class DefaultParamMethodData : MethodData, IDefaultParamTarget
	{
		/// <summary>
		/// Contains data that can be passed to a <see cref="DefaultParamMethodData"/>.
		/// </summary>
		public new sealed class Properties : MethodData.Properties
		{
			/// <inheritdoc cref="DefaultParamMethodData.CallInsteadOfCopying"/>
			public bool CallInsteadOfCopying { get; set; }

			/// <inheritdoc cref="DefaultParamMethodData.NewModifierIndices"/>
			public HashSet<int>? NewModifierIndices { get; set; }

			/// <inheritdoc cref="DefaultParamMethodData.TargetNamespace"/>
			public string? TargetNamespace { get; set; }

			/// <inheritdoc cref="DefaultParamMethodData.TypeParameters"/>
			public new TypeParameterContainer TypeParameters { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}

			/// <inheritdoc cref="MemberData.Properties.Clone"/>
			public new Properties Clone()
			{
				return (CloneCore() as Properties)!;
			}

			/// <inheritdoc/>
			public override void Map(MethodData.Properties properties)
			{
				base.Map(properties);

				if (properties is Properties props)
				{
					props.CallInsteadOfCopying = CallInsteadOfCopying;
					props.NewModifierIndices = NewModifierIndices;
					props.TargetNamespace = TargetNamespace;
					props.TypeParameters = TypeParameters;
				}
			}

			/// <inheritdoc/>
			protected override MemberData.Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}
		}

		private TypeParameterContainer _typeParameters;
		private string? _targetNamespace;

		/// <summary>
		/// Determines whether the generated method should call this method instead of copying its contents.
		/// </summary>
		public bool CallInsteadOfCopying { get; private set; }

		/// <summary>
		/// A <see cref="HashSet{T}"/> of indexes of type parameters with the <c>Durian.DefaultParamAttribute</c> applied for whom the <see langword="new"/> modifier should be applied.
		/// </summary>
		public HashSet<int>? NewModifierIndices { get; private set; }

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamMethodData"/>.
		/// </summary>
		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;

		/// <inheritdoc cref="Types.DefaultParamTypeData.TargetNamespace"/>
		public string TargetNamespace => _targetNamespace ??= ContainingNamespaces.ToString();

		/// <inheritdoc/>
		public new ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamMethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="DefaultParamMethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamMethodData"/>.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DefaultParamMethodData(
			MethodDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			Properties properties
		) : base(declaration, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new DefaultParamMethodData Clone()
		{
			return (CloneCore() as DefaultParamMethodData)!;
		}

		/// <summary>
		/// Returns a new instance of <see cref="MethodDeclarationBuilder"/> with <see cref="MethodDeclarationBuilder.OriginalDeclaration"/> set to this member's <see cref="MethodData.Declaration"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public MethodDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken = default)
		{
			return new MethodDeclarationBuilder(this, cancellationToken);
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <inheritdoc/>
		public IEnumerable<string> GetUsedNamespaces(CancellationToken cancellationToken = default)
		{
			return DefaultParamUtilities.GetUsedNamespaces(this, in _typeParameters, cancellationToken);
		}

		/// <inheritdoc/>
		public override void Map(MethodData.Properties properties)
		{
			base.Map(properties);

			if (properties is Properties props)
			{
				props.CallInsteadOfCopying = CallInsteadOfCopying;
				props.NewModifierIndices = NewModifierIndices;
				props.TargetNamespace = TargetNamespace;
				props.TypeParameters = TypeParameters;
			}
		}

		/// <inheritdoc/>
		protected override MemberData CloneCore()
		{
			return new DefaultParamMethodData(Declaration, ParentCompilation, GetProperties());
		}

		/// <inheritdoc/>
		protected override MemberData.Properties GetPropertiesCore()
		{
			Properties properties = new();
			Map(properties);
			return properties;
		}

		/// <inheritdoc/>
		protected override void SetProperties(MemberData.Properties properties)
		{
			base.SetProperties(properties);

			if (properties is Properties props)
			{
				_targetNamespace = props.TargetNamespace;
				_typeParameters = props.TypeParameters;
				CallInsteadOfCopying = props.CallInsteadOfCopying;
				NewModifierIndices = props.NewModifierIndices;
			}
		}

		IDefaultParamDeclarationBuilder IDefaultParamTarget.GetDeclarationBuilder(CancellationToken cancellationToken)
		{
			return GetDeclarationBuilder(cancellationToken);
		}

		IEnumerable<string> IDefaultParamTarget.GetUsedNamespaces()
		{
			return GetUsedNamespaces();
		}
	}
}
