// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using Durian.Analysis.Data.FromSource;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam.Delegates
{
	/// <summary>
	/// <see cref="DelegateData"/> that contains additional information needed by the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public class DefaultParamDelegateData : DelegateData, IDefaultParamTarget
	{
		/// <summary>
		/// Contains data that can be passed to a <see cref="DefaultParamDelegateData"/>.
		/// </summary>
		public new sealed class Properties : DelegateData.Properties
		{
			/// <inheritdoc cref="DefaultParamDelegateData.NewModifierIndices"/>
			public HashSet<int>? NewModifierIndices { get; set; }

			/// <inheritdoc cref="DefaultParamDelegateData.TargetNamespace"/>
			public string? TargetNamespace { get; set; }

			/// <inheritdoc cref="DefaultParamDelegateData.TypeParameters"/>
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
			public override void Map(DelegateData.Properties properties)
			{
				base.Map(properties);

				if (properties is Properties props)
				{
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

		/// <inheritdoc cref="Methods.DefaultParamMethodData.NewModifierIndices"/>
		public HashSet<int>? NewModifierIndices { get; private set; }

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamDelegateData"/>.
		/// </summary>
		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;

		/// <inheritdoc cref="Types.DefaultParamTypeData.TargetNamespace"/>
		public string TargetNamespace => _targetNamespace ??= ContainingNamespaces.ToString();

		/// <inheritdoc/>
		public new ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamDelegateData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> this <see cref="DefaultParamDelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamDelegateData"/>.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DefaultParamDelegateData(
			DelegateDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			Properties properties
		) : base(declaration, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new DefaultParamDelegateData Clone()
		{
			return (CloneCore() as DefaultParamDelegateData)!;
		}

		/// <summary>
		/// Returns a new instance of <see cref="DelegateDeclarationBuilder"/> with <see cref="DelegateDeclarationBuilder.OriginalDeclaration"/> set to this member's <see cref="DelegateData.Declaration"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public DelegateDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken = default)
		{
			return new DelegateDeclarationBuilder(this, cancellationToken);
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
		public override void Map(DelegateData.Properties properties)
		{
			base.Map(properties);

			if (properties is Properties props)
			{
				props.NewModifierIndices = NewModifierIndices;
				props.TargetNamespace = TargetNamespace;
				props.TypeParameters = TypeParameters;
			}
		}

		/// <inheritdoc/>
		protected override MemberData CloneCore()
		{
			return new DefaultParamDelegateData(Declaration, ParentCompilation, GetProperties());
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
