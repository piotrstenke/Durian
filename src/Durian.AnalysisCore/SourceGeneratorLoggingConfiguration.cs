namespace Durian
{
	/// <summary>
	/// Determines how the source generator should behave when logging information.
	/// </summary>
	public sealed class SourceGeneratorLoggingConfiguration
	{
		/// <inheritdoc cref="SourceGeneratorConfigurationAttribute.LogDirectory"/>
		public string LogDirectory { get; init; }

		/// <inheritdoc cref="SourceGeneratorConfigurationAttribute.SupportedLogs"/>
		public GeneratorLogs SupportedLogs { get; init; }

		/// <summary>
		/// Determines whether to enable logging.
		/// </summary>
		public bool EnableLogging { get; init; }

		/// <summary>
		/// Creates a new instance of the <see cref="SourceGeneratorLoggingConfiguration"/> class with its values set to default.
		/// </summary>
		public static SourceGeneratorLoggingConfiguration Default => new();

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGeneratorLoggingConfiguration"/> class.
		/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public SourceGeneratorLoggingConfiguration()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
		}
	}
}
