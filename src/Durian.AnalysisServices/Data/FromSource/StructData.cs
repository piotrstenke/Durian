using System;
using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource
{
	/// <inheritdoc cref="IStructData"/>
	public class StructData : TypeData<StructDeclarationSyntax>, IStructData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="ParameterData"/>.
		/// </summary>
		public new class Properties : TypeData<StructDeclarationSyntax>.Properties
		{
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
			}

			/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

			[Obsolete("Use Map(Properties) instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public sealed override void Map(TypeData<StructDeclarationSyntax>.Properties properties)
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
			protected override MemberData.Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}

			/// <inheritdoc/>
			protected override void FillWithDefaultData()
			{
				BaseTypes = null;
				Virtuality = Analysis.Virtuality.NotVirtual;
				CompilerCondition = null;
			}
		}

		IStructData ISymbolOrMember<INamedTypeSymbol, IStructData>.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="StructData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="StructDeclarationSyntax"/> this <see cref="StructData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="StructData"/>.</param>
		/// <param name="properties"><see cref="TypeData{TDeclaration}.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public StructData(StructDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal StructData(INamedTypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new StructData Clone()
		{
			return (CloneCore() as StructData)!;
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <inheritdoc cref="MemberData.Properties.Map(MemberData.Properties)"/>
		public virtual void Map(Properties properties)
		{
			base.Map(properties);
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(TypeData<StructDeclarationSyntax>.Properties properties)
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
		protected override MemberData CloneCore()
		{
			return new StructData(Declaration, ParentCompilation, GetProperties());
		}

		/// <inheritdoc/>
		protected override MemberData.Properties? GetDefaultProperties()
		{
			return new Properties(true);
		}

		/// <inheritdoc/>
		protected override MemberData.Properties GetPropertiesCore()
		{
			Properties properties = new();
			Map(properties);
			return properties;
		}

		IStructData IStructData.Clone()
		{
			return Clone();
		}
	}
}
