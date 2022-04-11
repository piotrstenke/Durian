// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Determines the behavior of the target <see cref="ISourceGenerator"/> when creating log files.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class LoggingConfigurationAttribute : Attribute
	{
		private string? _validatedDirectory;

		/// <summary>
		/// Determines what to output when a <see cref="SyntaxNode"/> is being logged and no other <see cref="NodeOutput"/> is specified.
		/// </summary>
		public NodeOutput DefaultNodeOutput { get; set; }

		/// <summary>
		/// Determines whether to enable the <see cref="ISourceGenerator"/> can throw <see cref="Exception"/>s. Defaults to <see langword="false"/>
		/// </summary>
		public bool EnableExceptions { get; set; }

		/// <summary>
		/// The directory the source generator logs will be written to. If not specified, <see cref="LoggingConfiguration.DefaultLogDirectory"/> is used instead.
		/// </summary>
		public string? LogDirectory { get; set; }

		/// <summary>
		/// Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="LoggingConfiguration.DefaultLogDirectory"/>. Defaults to <see langword="false"/>.
		/// </summary>
		/// <remarks>If <see cref="RelativeToDefault"/> is set to <see langword="true"/>, value of <see cref="RelativeToGlobal"/> is irrelevant.</remarks>
		public bool RelativeToDefault { get; set; }

		/// <summary>
		/// Determines whether the <see cref="LogDirectory"/> is relative to the global <see cref="LogDirectory"/>. Defaults to <see langword="true"/>.
		/// </summary>
		public bool RelativeToGlobal { get; set; } = true;

		/// <inheritdoc cref="LoggingConfiguration.SupportedLogs"/>
		public GeneratorLogs SupportedLogs { get; set; }

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> supports reporting <see cref="Diagnostic"/>s. Defaults to <see langword="false"/>
		/// </summary>
		public bool SupportsDiagnostics { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggingConfigurationAttribute"/> class.
		/// </summary>
		public LoggingConfigurationAttribute()
		{
		}

		/// <summary>
		/// Gets the full <see cref="LogDirectory"/> and checks if its valid.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// <see cref="LogDirectory"/> of the <see cref="LoggingConfigurationAttribute"/> cannot be empty or whitespace only. -or-
		/// <see cref="LogDirectory"/> must be specified if <see cref="RelativeToDefault"/> is set to <see langword="false"/>. -or-
		/// <see cref="LogDirectory"/> contains one or more of invalid characters defined in the <see cref="Path.GetInvalidPathChars"/>.
		/// </exception>
		public string GetFullDirectoryForAssembly()
		{
			if (_validatedDirectory is not null)
			{
				return _validatedDirectory;
			}

			if (LogDirectory is null)
			{
				if (!RelativeToDefault)
				{
					throw new ArgumentException($"{nameof(LogDirectory)} must be specified if {nameof(RelativeToDefault)} is set to false!");
				}

				return LoggingConfiguration.DefaultLogDirectory;
			}

			if (string.IsNullOrWhiteSpace(LogDirectory))
			{
				throw new ArgumentException($"{nameof(LogDirectory)} of the {nameof(LoggingConfigurationAttribute)} cannot be empty or whitespace only!", nameof(LogDirectory));
			}

			string? dir;

			if (RelativeToDefault)
			{
				if (LogDirectory![0] == '/')
				{
					dir = LoggingConfiguration.DefaultLogDirectory + LogDirectory;
				}
				else
				{
					dir = LoggingConfiguration.DefaultLogDirectory + "/" + LogDirectory;
				}
			}
			else
			{
				dir = LogDirectory;
			}

			// Checks if the directory is valid.
			Path.GetFullPath(dir);

			_validatedDirectory = dir;
			return dir;
		}

		/// <summary>
		/// Gets the full <see cref="LogDirectory"/> and checks if its valid.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// <see cref="LogDirectory"/> of the <see cref="LoggingConfigurationAttribute"/> cannot be empty or white space only. -or-
		/// <paramref name="globalConfiguration"/> must be specified if <see cref="RelativeToGlobal"/> is set to <see langword="true"/> -or-
		/// <see cref="LogDirectory"/> must be specified if both <see cref="RelativeToDefault"/> and <see cref="RelativeToGlobal"/> are set to <see langword="false"/>. -or-
		/// <see cref="LogDirectory"/> contains one or more of invalid characters defined in the <see cref="Path.GetInvalidPathChars"/>.
		/// </exception>
		public string GetFullDirectoryForType(LoggingConfiguration? globalConfiguration)
		{
			if (_validatedDirectory is not null)
			{
				return _validatedDirectory;
			}

			if (LogDirectory is null)
			{
				if (RelativeToGlobal)
				{
					if (globalConfiguration is null)
					{
						throw Exc_GlobalNotSpecified(nameof(globalConfiguration));
					}

					return globalConfiguration!.LogDirectory;
				}
				else if (RelativeToDefault)
				{
					return LoggingConfiguration.DefaultLogDirectory;
				}
				else
				{
					throw new ArgumentException($"{nameof(LogDirectory)} must be specified if both {nameof(RelativeToDefault)} and {nameof(RelativeToGlobal)} are set to false.");
				}
			}

			if (string.IsNullOrWhiteSpace(LogDirectory))
			{
				throw new ArgumentException($"{nameof(LogDirectory)} of the {nameof(LoggingConfigurationAttribute)} cannot be empty or white space only!");
			}

			string? dir;

			if (RelativeToGlobal)
			{
				if (globalConfiguration is null)
				{
					throw Exc_GlobalNotSpecified(nameof(globalConfiguration));
				}

				dir = CombineWithRoot(globalConfiguration.LogDirectory);
			}
			else if (RelativeToDefault)
			{
				dir = CombineWithRoot(LoggingConfiguration.DefaultLogDirectory);
			}
			else
			{
				dir = LogDirectory;
			}

			// Checks if the directory is valid.
			Path.GetFullPath(dir);

			_validatedDirectory = dir;
			return dir;

			static ArgumentException Exc_GlobalNotSpecified(string argName)
			{
				return new ArgumentException($"{argName} must be specified if {nameof(RelativeToGlobal)} is set to true!");
			}
		}

		private string CombineWithRoot(string root)
		{
			if (LogDirectory![0] == '/')
			{
				return root + LogDirectory;
			}
			else
			{
				return root + "/" + LogDirectory;
			}
		}
	}
}
