using System;
using Durian.Analysis.Data.FromSource;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.Types
{
	/// <summary>
	/// <see cref="TypeData{TDeclaration}"/> that contains additional information needed by the <see cref="CopyFromGenerator"/>.
	/// </summary>
	public sealed class CopyFromTypeData : TypeData<TypeDeclarationSyntax>, ICopyFromMember
	{
		/// <summary>
		/// Contains data that can be passed to a <see cref="CopyFromTypeData"/>.
		/// </summary>
		public new sealed class Properties : TypeData<TypeDeclarationSyntax>.Properties
		{
			/// <inheritdoc cref="CopyFromTypeData.Dependencies"/>
			public INamedTypeSymbol[]? Dependencies { get; set; }

			/// <inheritdoc cref="CopyFromTypeData.Patterns"/>
			public PatternData[]? Patterns { get; set; }

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
					props.Dependencies = Dependencies;
					props.Patterns = Patterns;
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

		/// <summary>
		/// <see cref="INamedTypeSymbol"/>s generation of this type depends on.
		/// </summary>
		public INamedTypeSymbol[]? Dependencies { get; private set; }

		/// <inheritdoc cref="MemberData.ParentCompilation"/>
		public new CopyFromCompilationData ParentCompilation => (base.ParentCompilation as CopyFromCompilationData)!;

		/// <summary>
		/// A collection of patterns applied to the type using <c>Durian.PatternAttribute</c>.
		/// </summary>
		public PatternData[]? Patterns { get; private set; }

		/// <summary>
		/// A collection of target types.
		/// </summary>
		public TargetTypeData[] Targets { get; }

		ISymbol[]? ICopyFromMember.Dependencies => Dependencies;

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="CopyFromTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="CopyFromCompilationData"/> of this <see cref="CopyFromTypeData"/>.</param>
		/// <param name="targets">A collection of target types.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public CopyFromTypeData(
			TypeDeclarationSyntax declaration,
			CopyFromCompilationData compilation,
			TargetTypeData[] targets,
			Properties? properties = default
		) : base(declaration, compilation, properties)
		{
			Targets = targets;
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new CopyFromTypeData Clone()
		{
			return (CloneCore() as CopyFromTypeData)!;
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <inheritdoc/>
		public override void Map(TypeData<TypeDeclarationSyntax>.Properties properties)
		{
			base.Map(properties);

			if (properties is Properties props)
			{
				props.Dependencies = Dependencies;
				props.Patterns = Patterns;
			}
		}

		/// <inheritdoc/>
		protected override MemberData CloneCore()
		{
			return new CopyFromTypeData(Declaration, ParentCompilation, Targets, GetProperties());
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
				Patterns = props.Patterns;
				Dependencies = props.Dependencies;
			}
		}
	}
}
