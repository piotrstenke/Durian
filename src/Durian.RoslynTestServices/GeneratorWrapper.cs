using System;
using Durian.Logging;

namespace Durian.Tests
{
	/// <summary>
	/// A wrapper class for <see cref="ILoggableSourceGenerator"/> that holds name of the current test.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class GeneratorWrapper<T> where T : ILoggableSourceGenerator
	{
		private static readonly GeneratorLoggingConfiguration _defaultConfig = GeneratorLoggingConfiguration.CreateConfigurationForGenerator<T>();
		private string? _testName;
		private GeneratorLoggingConfiguration _config = _defaultConfig;
		private readonly Func<GeneratorLoggingConfiguration, T> _ctor;

		/// <summary>
		/// Name of the current test.
		/// </summary>
		public string? TestName
		{
			get => _testName;
			set
			{
				if (value is null)
				{
					if (_testName is null)
					{
						return;
					}

					_config = _defaultConfig;
					_testName = null;
				}
				else if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(TestName)} cannot be empty or white space only!");
				}
				else
				{
					_config = _defaultConfig with { LogDirectory = _defaultConfig.LogDirectory + $"/{TestName}" };
					_testName = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorWrapper{T}"/> class.
		/// </summary>
		/// <param name="ctor">A function that creates a new <see cref="ILoggableSourceGenerator"/> based on the specified <see cref="GeneratorLoggingConfiguration"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="ctor"/> is <see langword="null"/>.</exception>
		public GeneratorWrapper(Func<GeneratorLoggingConfiguration, T> ctor)
		{
			if(ctor is null)
			{
				throw new ArgumentNullException(nameof(ctor));
			}

			_ctor = ctor;
		}

		/// <summary>
		/// Creates a new instance of the specified generator type.
		/// </summary>
		public T CreateGenerator()
		{
			return _ctor(_config);
		}
	}
}
