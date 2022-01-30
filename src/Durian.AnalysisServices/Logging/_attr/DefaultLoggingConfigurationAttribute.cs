﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Determines the default behavior of <see cref="ISourceGenerator"/>s in the current assembly when creating log files.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class DefaultLoggingConfigurationAttribute : Attribute
	{
		private string? _validatedDirectory;

		/// <summary>
		/// Determines whether to enable the <see cref="ISourceGenerator"/> can throw <see cref="Exception"/>s. Defaults to <see langword="false"/>
		/// </summary>
		public bool EnableExceptions { get; set; }

		/// <summary>
		/// Default directory where the generator log files are to be found.
		/// </summary>
		public string? LogDirectory { get; set; }

		/// <summary>
		/// Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="LoggingConfiguration.DefaultLogDirectory"/>. Defaults to <see langword="true"/>.
		/// </summary>
		public bool RelativeToDefault { get; set; }

		/// <summary>
		/// Defaults types of logs for this assembly.
		/// </summary>
		public GeneratorLogs SupportedLogs { get; set; }

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> supports reporting <see cref="Diagnostic"/>s. Defaults to <see langword="false"/>
		/// </summary>
		public bool SupportsDiagnostics { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultLoggingConfigurationAttribute"/> class.
		/// </summary>
		public DefaultLoggingConfigurationAttribute()
		{
		}

		/// <summary>
		/// Gets the full <see cref="LogDirectory"/> and checks if its valid.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// <see cref="LogDirectory"/> of the <see cref="DefaultLoggingConfigurationAttribute"/> cannot be empty or whitespace only. -or-
		/// <see cref="LogDirectory"/> must be specified if <see cref="RelativeToDefault"/> is set to <see langword="false"/>. -or-
		/// <see cref="LogDirectory"/> contains one or more of invalid characters defined in the <see cref="Path.GetInvalidPathChars"/>.
		/// </exception>
		public string GetAndValidateFullLogDirectory()
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
				throw new ArgumentException($"{nameof(LogDirectory)} of the {nameof(DefaultLoggingConfigurationAttribute)} cannot be empty or whitespace only!", nameof(LogDirectory));
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
	}
}