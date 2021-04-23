using System;
using System.Diagnostics;
using System.IO;

namespace Durian.Configuration
{
	/// <summary>
	/// Globally determines how the source generator should behave when logging information.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	[Conditional("ENABLE_GENERATOR_LOGS")]
	public sealed class GlobalGeneratorLoggingConfigurationAttribute : Attribute
	{
		private string? _logDirectory;
		private readonly bool _relativeToDefault;

		/// <summary>
		/// Default directory where the generator log files are to be found.
		/// </summary>
		public string? LogDirectory
		{
			get => _logDirectory;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(LogDirectory)} cannot be null or empty");
				}

				string? dir;

				if (_relativeToDefault)
				{
					if (value![0] == '/')
					{
						dir = DurianStrings.DefaultLogDirectory + value;
					}
					else
					{
						dir = DurianStrings.DefaultLogDirectory + "/" + value;
					}
				}
				else
				{
					dir = value;
				}

				Directory.CreateDirectory(dir);
				_logDirectory = dir;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GlobalGeneratorLoggingConfigurationAttribute"/> class.
		/// </summary>
		/// <param name="relativeToDefault">Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="DurianStrings.DefaultLogDirectory"/>.</param>
		public GlobalGeneratorLoggingConfigurationAttribute(bool relativeToDefault)
		{
			_relativeToDefault = relativeToDefault;
		}
	}
}
