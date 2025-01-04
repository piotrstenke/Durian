using Durian.Analysis.Data;

namespace Durian.Analysis.SymbolContainers;

/// <summary>
/// Contains extension methods for the <see cref="SymbolContainerBuilder{T}"/> struct.
/// </summary>
public static class SymbolContainerBuilderExtensions
{
	/// <summary>
	/// Includes a custom <see cref="ISymbolNameResolver"/>.
	/// </summary>
	/// <param name="builder"><see cref="SymbolContainerBuilder"/> to include the <paramref name="nameResolver"/> for.</param>
	/// <param name="nameResolver">Custom <see cref="ISymbolNameResolver"/> to include.</param>
	public static ref SymbolContainerBuilder WithNameResolver(this ref SymbolContainerBuilder builder, ISymbolNameResolver? nameResolver)
	{
		builder.SymbolNameResolver = nameResolver;
		return ref builder;
	}

	/// <summary>
	/// Includes a custom <see cref="ISymbolNameResolver"/>.
	/// </summary>
	/// <typeparam name="TContainer">Type of container being built.</typeparam>
	/// <param name="builder"><see cref="SymbolContainerBuilder{T}"/> to include the <paramref name="nameResolver"/> for.</param>
	/// <param name="nameResolver">Custom <see cref="ISymbolNameResolver"/> to include.</param>
	public static ref SymbolContainerBuilder<TContainer> WithNameResolver<TContainer>(this ref SymbolContainerBuilder<TContainer> builder, ISymbolNameResolver? nameResolver)
		where TContainer : ISymbolContainer, IBuilderReceiver<SymbolContainerBuilder>, new()
	{
		builder.SymbolNameResolver = nameResolver;
		return ref builder;
	}

	/// <summary>
	/// Includes a parent <see cref="ICompilationData"/>.
	/// </summary>
	/// <param name="builder"><see cref="SymbolContainerBuilder{T}"/> to include the <paramref name="parentCompilation"/> for.</param>
	/// <param name="parentCompilation"><see cref="ICompilationData"/> to include.</param>
	public static ref SymbolContainerBuilder WithParentCompilation(this ref SymbolContainerBuilder builder, ICompilationData? parentCompilation)
	{
		builder.ParentCompilation = parentCompilation;
		return ref builder;
	}

	/// <summary>
	/// Includes a parent <see cref="ICompilationData"/>.
	/// </summary>
	/// <typeparam name="TContainer">Type of container being built.</typeparam>
	/// <param name="builder"><see cref="SymbolContainerBuilder{T}"/> to include the <paramref name="parentCompilation"/> for.</param>
	/// <param name="parentCompilation"><see cref="ICompilationData"/> to include.</param>
	public static ref SymbolContainerBuilder<TContainer> WithParentCompilation<TContainer>(this ref SymbolContainerBuilder<TContainer> builder, ICompilationData? parentCompilation)
		where TContainer : ISymbolContainer, IBuilderReceiver<SymbolContainerBuilder>, new()
	{
		builder.ParentCompilation = parentCompilation;
		return ref builder;
	}
}
