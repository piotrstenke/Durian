using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized
{
	/// <summary>
	/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles local functions.
	/// </summary>
	public sealed class LocalFunctionContainer : IncludedMemberContainerWithoutInner<IMethodSymbol, ILocalFunctionData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalFunctionContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public LocalFunctionContainer(
			ISymbolOrMember<IMethodSymbol, ILocalFunctionData> root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default,
			bool includeRoot = false
		) : base(root, parentCompilation, nameResolver, includeRoot)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalFunctionContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public LocalFunctionContainer(
			ISymbolOrMember<IMethodSymbol, IMethodData> root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default
		) : base(root, parentCompilation, nameResolver)
		{
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<IMethodSymbol, ILocalFunctionData>> All(ISymbolOrMember<IMethodSymbol, ILocalFunctionData> member)
		{
			return GetLocalFunctions(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<IMethodSymbol, ILocalFunctionData>> Direct(ISymbolOrMember<IMethodSymbol, ILocalFunctionData> member)
		{
			return GetLocalFunctions(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<IMethodSymbol, ILocalFunctionData>> ResolveRoot(ISymbolOrMember root)
		{
			return GetLocalFunctions((root as ISymbolOrMember<IMethodSymbol, IMethodData>)!);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
		public new LocalFunctionContainer Reverse()
		{
			return (base.Reverse() as LocalFunctionContainer)!;
		}

		private IEnumerable<ISymbolOrMember<IMethodSymbol, ILocalFunctionData>> GetLocalFunctions(ISymbolOrMember<IMethodSymbol, IMethodData> member)
		{
			return member.Symbol.GetLocalFunctions().Select(s => s.ToDataOrSymbol<ILocalFunctionData>(ParentCompilation));
		}
	}
}
