using System;
using Durian.Data;
using Durian.Logging;

namespace Durian
{
	/// <inheritdoc cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class SourceGenerator<TCompilationData, TSyntaxReceiver> : SourceGenerator<TCompilationData, TSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
		where TCompilationData : class, ICompilationData
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
		/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver}"/> instance if logging is supported.</param>
		/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableDiagnostics"/> to <see langword="true"/> if <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SupportsDiagnostics"/> is <see langword="true"/>.</param>
		protected SourceGenerator(bool checkForConfigurationAttribute, bool enableLoggingIfSupported = true, bool enableDiagnosticsIfSupported = true) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
		/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver}"/> instance if logging is supported.</param>
		/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableDiagnostics"/> to <see langword="true"/> if <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SupportsDiagnostics"/> is <see langword="true"/>.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <exception cref="ArgumentNullException"><paramref name="fileNameProvider"/> is <see langword="null"/>.</exception>
		protected SourceGenerator(bool checkForConfigurationAttribute, bool enableLoggingIfSupported, bool enableDiagnosticsIfSupported, IFileNameProvider fileNameProvider) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported, fileNameProvider)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected SourceGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <exception cref="ArgumentNullException"><paramref name="fileNameProvider"/> is <see langword="null"/>.</exception>
		protected SourceGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IFileNameProvider fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}
