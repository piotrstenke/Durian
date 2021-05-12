using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public abstract class DefaultParamAnalyzerTest<T> : AnalyzerTest<T>, IClassFixture<DefaultParamCompilationFixture> where T : DurianAnalyzer, new()
	{
		public DefaultParamAnalyzerTest(DefaultParamCompilationFixture fixture) : base(fixture.Compilation)
		{
		}

		public new async Task<ImmutableArray<Diagnostic>> RunAnalyzerAsync(string? input, bool addToCompilation = false)
		{
			return await base.RunAnalyzerAsync(input, addToCompilation);
		}

		public new async Task<ImmutableArray<Diagnostic>> RunAnalyzerAsync(T analyzer, string? input, bool addToCompilation = false)
		{
			return await base.RunAnalyzerAsync(analyzer, input, addToCompilation);
		}

		protected sealed override T CreateAnalyzer()
		{
			return new T();
		}
	}
}