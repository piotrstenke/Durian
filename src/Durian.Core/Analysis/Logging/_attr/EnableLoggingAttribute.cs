// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Info;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Allows a source generator called by the runtime to create log files.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class EnableLoggingAttribute : Attribute, ILoggingConfigurationAttribute
	{
		private string? _moduleName;
		private DurianModule? _module;

		/// <summary>
		/// Name of the target module.
		/// </summary>
		public string ModuleName
		{
			get
			{
				if (_moduleName is null)
				{
					ModuleIdentity.TryGetName(_module!.Value, out string? moduleName);
					_moduleName = moduleName;
				}

				return _moduleName!;
			}
		}

		/// <summary>
		/// Target <see cref="DurianModule"/>.
		/// </summary>
		[SuppressMessage("Performance", "CA1806:Do not ignore method results")]
		public DurianModule Module
		{
			get
			{
				if (!_module.HasValue)
				{
					ModuleIdentity.TryParse(_moduleName, out DurianModule module);
					_module = module;
				}

				return _module.Value;
			}
		}

		/// <summary>
		/// Determines what to output when a node is being logged and no other <see cref="NodeOutput"/> is specified.
		/// </summary>
		public NodeOutput DefaultNodeOutput { get; set; }

		/// <summary>
		/// Determines whether to enable the source generator can throw <see cref="Exception"/>s. Defaults to <see langword="false"/>
		/// </summary>
		public bool EnableExceptions { get; set; }

		/// <summary>
		/// The directory the source generator logs will be written to. If not specified, '<c>&lt;documents&gt;/Durian/logs</c>' is used instead.
		/// </summary>
		public string? LogDirectory { get; set; }

		/// <summary>
		/// Determines whether the <see cref="LogDirectory"/> is relative to the '<c>&lt;documents&gt;/Durian/logs</c>'. Defaults to <see langword="false"/>.
		/// </summary>
		public bool RelativeToDefault { get; set; }

		/// <summary>
		/// Types of logs this source generator can produce.
		/// </summary>
		public GeneratorLogs SupportedLogs { get; set; }

		/// <summary>
		/// Determines whether the source generator supports reporting diagnostics. Defaults to <see langword="false"/>
		/// </summary>
		public bool SupportsDiagnostics { get; set; }

		bool ILoggingConfigurationAttribute.RelativeToGlobal => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableLoggingAttribute"/> class.
		/// </summary>
		/// <param name="moduleName">Name of the target module.</param>
		public EnableLoggingAttribute(string moduleName)
		{
			_moduleName = moduleName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableLoggingAttribute"/> class.
		/// </summary>
		/// <param name="module">Target <see cref="DurianModule"/>.</param>
		public EnableLoggingAttribute(DurianModule module)
		{
			_module = module;
		}
	}
}
