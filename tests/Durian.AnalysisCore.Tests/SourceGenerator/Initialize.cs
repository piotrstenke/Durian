using System.Reflection;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.AnalysisCore.SourceGenerator
{
	public sealed class Initialize
	{
		[Fact]
		public void RegistersSyntaxReceiver()
		{
			DurianSourceGeneratorProxy generator = new();
			GeneratorInitializationContext context = RoslynUtilities.CreateInitializationContext();
			generator.Initialize(context);

			object builder = typeof(GeneratorInitializationContext)
				.GetProperty("InfoBuilder", BindingFlags.Instance | BindingFlags.NonPublic)!
				.GetValue(context)!;

			SyntaxContextReceiverCreator? syntaxReceiverCreator = builder
				.GetType()
				.GetProperty("SyntaxContextReceiverCreator", BindingFlags.Instance | BindingFlags.NonPublic)!
				.GetValue(builder)
				as SyntaxContextReceiverCreator;

			Assert.True(syntaxReceiverCreator is not null);
		}
	}
}