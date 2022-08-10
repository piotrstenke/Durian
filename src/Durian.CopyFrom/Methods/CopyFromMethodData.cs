// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Data.FromSource;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.Methods
{
	/// <summary>
	/// <see cref="MethodData"/> that contains additional information needed by the <see cref="CopyFromGenerator"/>.
	/// </summary>
	public sealed class CopyFromMethodData : MethodData, ICopyFromMember
	{
		/// <summary>
		/// Contains data that can be passed to a <see cref="CopyFromMethodData"/>.
		/// </summary>
		public new sealed class Properties : MethodData.Properties
		{
			/// <inheritdoc cref="CopyFromMethodData.Dependencies"/>
			public IMethodSymbol[]? Dependencies { get; set; }

			/// <inheritdoc cref="CopyFromMethodData.Patterns"/>
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
			public override void Map(MethodData.Properties properties)
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
		/// <see cref="IMethodSymbol"/>s generation of this type depends on.
		/// </summary>
		public IMethodSymbol[]? Dependencies { get; private set; }

		/// <inheritdoc cref="MemberData.ParentCompilation"/>
		public new CopyFromCompilationData ParentCompilation => (base.ParentCompilation as CopyFromCompilationData)!;

		/// <summary>
		/// A collection of patterns applied to the method using <c>Durian.PatternAttribute</c>.
		/// </summary>
		public PatternData[]? Patterns { get; private set; }

		/// <summary>
		/// Target method.
		/// </summary>
		public TargetMethodData Target { get; }

		ISymbol[]? ICopyFromMember.Dependencies => Dependencies;

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromMethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="CopyFromMethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="CopyFromCompilationData"/> of this <see cref="CopyFromMethodData"/>.</param>
		/// <param name="target">Target method.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public CopyFromMethodData(
			MethodDeclarationSyntax declaration,
			CopyFromCompilationData compilation,
			TargetMethodData target,
			Properties? properties = default
		) : base(declaration, compilation, properties)
		{
			Target = target;
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new CopyFromMethodData Clone()
		{
			return (CloneCore() as CopyFromMethodData)!;
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <inheritdoc/>
		public override void Map(MethodData.Properties properties)
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
			return new CopyFromMethodData(Declaration, ParentCompilation, Target, GetProperties());
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
