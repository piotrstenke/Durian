using System.Threading;

namespace Durian.DefaultParam
{
	public interface IDefaultParamFilter : ISyntaxFilterWithDiagnostics
	{
		IDefaultParamDeclarationBuilder GetDeclarationBuilder(IDefaultParamTarget target, CancellationToken cancellationToken = default);
	}
}
