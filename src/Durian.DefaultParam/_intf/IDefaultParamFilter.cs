using System.Threading;

namespace Durian.DefaultParam
{
	public interface IDefaultParamFilter : ISyntaxFilterWithDiagnostics
	{
		IDefaultParamTargetWrapper GetWrapper(IDefaultParamTarget target, CancellationToken cancellationToken = default);
	}
}
