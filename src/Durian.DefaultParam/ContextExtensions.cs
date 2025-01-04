using Durian.Analysis.DefaultParam.Delegates;
using Durian.Analysis.DefaultParam.Methods;
using Durian.Analysis.DefaultParam.Types;
using Durian.Analysis.Filtering;

namespace Durian.Analysis.DefaultParam;

/// <summary>
/// Contains extension methods for <c>DefaultParam</c>-specific <see cref="ISyntaxValidationContext"/>s.
/// </summary>
public static class ContextExtensions
{
	/// <summary>
	/// Returns a reference to the <see cref="TypeParameterContainer"/> contained within the specified <paramref name="context"/>.
	/// </summary>
	/// <param name="context"><see cref="DefaultParamDelegateContext"/> to get the <see cref="TypeParameterContainer"/> of.</param>
	public static ref readonly TypeParameterContainer GetTypeParameters(this in DefaultParamDelegateContext context)
	{
		return ref context._typeParameters;
	}

	/// <summary>
	/// Returns a reference to the <see cref="TypeParameterContainer"/> contained within the specified <paramref name="context"/>.
	/// </summary>
	/// <param name="context"><see cref="DefaultParamMethodContext"/> to get the <see cref="TypeParameterContainer"/> of.</param>
	public static ref readonly TypeParameterContainer GetTypeParameters(this in DefaultParamMethodContext context)
	{
		return ref context._typeParameters;
	}

	/// <summary>
	/// Returns a reference to the <see cref="TypeParameterContainer"/> contained within the specified <paramref name="context"/>.
	/// </summary>
	/// <param name="context"><see cref="DefaultParamDelegateContext"/> to get the <see cref="DefaultParamTypeContext"/> of.</param>
	public static ref readonly TypeParameterContainer GetTypeParameters(this in DefaultParamTypeContext context)
	{
		return ref context._typeParameters;
	}
}
