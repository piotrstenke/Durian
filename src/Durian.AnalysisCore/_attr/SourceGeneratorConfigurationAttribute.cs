using System;

namespace Durian
{
	/// <summary>
	/// Determines how the source generator should behave.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class SourceGeneratorConfigurationAttribute : Attribute
	{
		/// <summary>
		/// The directory the source generator logs will be written to.
		/// </summary>
		public string LogDirectory { get; set; }

		/// <summary>
		/// Types of logs this source generator can produce.
		/// </summary>
		public GeneratorLogs SupportedLogs { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGeneratorConfigurationAttribute"/> class.
		/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public SourceGeneratorConfigurationAttribute()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
		}

		/// <summary>
		/// Returns a new instance of <see cref="SourceGeneratorLoggingConfiguration"/> based on values of this <see cref="SourceGeneratorConfigurationAttribute"/>.
		/// </summary>
		public SourceGeneratorLoggingConfiguration GetConfiguration()
		{
			return new SourceGeneratorLoggingConfiguration()
			{
				LogDirectory = LogDirectory,
				SupportedLogs = SupportedLogs,
				EnableLogging = true
			};
		}
	}
}
