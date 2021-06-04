using System.Collections.Generic;
using System.Threading;
using Durian.Generator.Data;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// <see cref="IMemberData"/> that is used to generate new sources by the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public interface IDefaultParamTarget : IMemberData
	{
		/// <summary>
		/// Specifies the namespace where the target member should be generated in.
		/// </summary>
		string TargetNamespace { get; }

		/// <summary>
		/// <see cref="TypeParameterContainer"/> that contains type parameters of this member.
		/// </summary>
		ref readonly TypeParameterContainer TypeParameters { get; }

		/// <summary>
		/// Returns a collection of <see cref="string"/>s representing namespaces used by this member.
		/// </summary>
		IEnumerable<string> GetUsedNamespaces();

		/// <summary>
		/// Returns a new instance of <see cref="IDefaultParamDeclarationBuilder"/> with <see cref="IDefaultParamDeclarationBuilder.OriginalNode"/> set to this member's declaration.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		IDefaultParamDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken = default);
	}
}
