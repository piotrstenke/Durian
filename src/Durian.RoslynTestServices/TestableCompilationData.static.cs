using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Tests
{
	public partial class TestableCompilationData
	{
		/// <summary>
		/// Creates a new <see cref="TestableCompilationData"/>.
		/// </summary>
		/// <param name="includeDefaultAssemblies">Determines whether to include all the default assemblies returned bu the <see cref="RoslynUtilities.GetBaseReferences()"/> method.</param>
		public static TestableCompilationData Create(bool includeDefaultAssemblies = true)
		{
			return new TestableCompilationData(includeDefaultAssemblies ? RoslynUtilities.CreateBaseCompilation() : null);
		}

		/// <summary>
		/// Creates a new <see cref="TestableCompilationData"/>.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the newly-created <see cref="TestableCompilationData"/>.</param>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the newly-created <see cref="TestableCompilationData"/>.</param>
		public static TestableCompilationData Create(IEnumerable<string>? sources, IEnumerable<Assembly>? assemblies)
		{
			return new TestableCompilationData(RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies?.ToArray()));
		}

		/// <summary>
		/// Creates a new <see cref="TestableCompilationData"/>.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the newly-created <see cref="TestableCompilationData"/>.</param>
		public static TestableCompilationData Create(IEnumerable<string>? sources)
		{
			return new TestableCompilationData(RoslynUtilities.CreateCompilationWithAssemblies(sources));
		}

		/// <summary>
		/// Creates a new <see cref="TestableCompilationData"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the newly-created <see cref="TestableCompilationData"/>.</param>
		public static TestableCompilationData Create(string? source)
		{
			return new TestableCompilationData(RoslynUtilities.CreateCompilationWithAssemblies(source));
		}

		/// <summary>
		/// Creates a new <see cref="TestableCompilationData"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the newly-created <see cref="TestableCompilationData"/>.</param>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the newly-created <see cref="TestableCompilationData"/>.</param>
		public static TestableCompilationData Create(string? source, IEnumerable<Assembly>? assemblies)
		{
			return new TestableCompilationData(RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies?.ToArray()));
		}

		/// <summary>
		/// Creates a new <see cref="TestableCompilationData"/>.
		/// </summary>
		/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used as the base compilation of the newly-created <see cref="TestableCompilationData"/>.</param>
		public static TestableCompilationData Create(CSharpCompilation? compilation)
		{
			return new TestableCompilationData(compilation);
		}
	}
}