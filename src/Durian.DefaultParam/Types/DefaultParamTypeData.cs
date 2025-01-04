using System;
using System.Collections.Generic;
using System.Threading;
using Durian.Analysis.Data.FromSource;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam.Types
{
	/// <summary>
	/// <see cref="TypeData{TDeclaration}"/> that contains additional information needed by the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public class DefaultParamTypeData : TypeData<TypeDeclarationSyntax>, IDefaultParamTarget
	{
		/// <summary>
		/// Contains data that can be passed to a <see cref="DefaultParamTypeData"/>.
		/// </summary>
		public new sealed class Properties : TypeData<TypeDeclarationSyntax>.Properties
		{
			/// <inheritdoc cref="DefaultParamTypeData.Inherit"/>
			public bool Inherit { get; set; }

			/// <inheritdoc cref="DefaultParamTypeData.NewModifierIndices"/>
			public HashSet<int>? NewModifierIndices { get; set; }

			/// <inheritdoc cref="DefaultParamTypeData.TargetNamespace"/>
			public string? TargetNamespace { get; set; }

			/// <inheritdoc cref="DefaultParamTypeData.TypeParameters"/>
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
			public override void Map(TypeData<TypeDeclarationSyntax>.Properties properties)
			{
				base.Map(properties);

				if (properties is Properties props)
				{
					props.Inherit = Inherit;
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
		/// Determines whether the generated members should inherit the original type.
		/// </summary>
		public bool Inherit { get; private set; }

		/// <inheritdoc cref="Delegates.DefaultParamDelegateData.NewModifierIndices"/>
		public HashSet<int>? NewModifierIndices { get; private set; }

		/// <summary>
		/// Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamTypeData"/>.
		/// </summary>
		public new DefaultParamCompilationData ParentCompilation => (DefaultParamCompilationData)base.ParentCompilation;

		/// <summary>
		/// Specifies the namespace where the target member should be generated in.
		/// </summary>
		public string TargetNamespace => _targetNamespace ??= ContainingNamespaces.ToString();

		/// <inheritdoc/>
		public new ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="DefaultParamTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of this <see cref="DefaultParamTypeData"/>.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DefaultParamTypeData(
			TypeDeclarationSyntax declaration,
			DefaultParamCompilationData compilation,
			Properties properties
		) : base(declaration, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new DefaultParamTypeData Clone()
		{
			return (CloneCore() as DefaultParamTypeData)!;
		}

		/// <summary>
		/// Returns a new instance of <see cref="TypeDeclarationBuilder"/> with <see cref="TypeDeclarationBuilder.OriginalDeclaration"/> set to this member's <see cref="TypeData{TDeclaration}.Declaration"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public TypeDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken = default)
		{
			return new TypeDeclarationBuilder(this, cancellationToken);
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
		public override void Map(TypeData<TypeDeclarationSyntax>.Properties properties)
		{
			base.Map(properties);

			if (properties is Properties props)
			{
				props.Inherit = Inherit;
				props.NewModifierIndices = NewModifierIndices;
				props.TargetNamespace = TargetNamespace;
				props.TypeParameters = TypeParameters;
			}
		}

		/// <inheritdoc/>
		protected override MemberData CloneCore()
		{
			return new DefaultParamTypeData(Declaration, ParentCompilation, GetProperties());
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
				Inherit = props.Inherit;
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
