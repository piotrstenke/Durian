using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging;

public sealed partial class LoggingConfiguration
{
	private static readonly ConcurrentDictionary<Assembly, LoggingConfiguration> _assemblyConfigurations = new();
	private static readonly ConcurrentDictionary<string, LoggingConfiguration> _dynamicConfigurations = new();

	/// <summary>
	/// Creates a new instance of the <see cref="LoggingConfiguration"/> class with its values set to default.
	/// </summary>
	public static LoggingConfiguration Default { get; } = new()
	{
		_enableLogging = false,
		_enableDiagnostics = false,
		EnableExceptions = false,
		_logDirectory = DefaultLogDirectory,
		SupportedLogs = GeneratorLogs.None,
		_supportsDiagnostics = false,
	};

	/// <summary>
	/// Default directory where the generator log files are placed, which is '<c>&lt;documents&gt;/Durian/logs</c>'.
	/// </summary>
	public static string DefaultLogDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Durian/logs";

	/// <summary>
	/// Determines whether generator logging is globally enabled.
	/// </summary>
	/// <remarks>
	/// Logging becomes globally disabled when an <see cref="DisableLoggingGloballyAttribute"/>
	/// is detected on any assembly in the current <see cref="AppDomain"/>.
	/// </remarks>
	public static bool IsGloballyEnabled { get; } = CheckLoggingIsEnabled();

	/// <summary>
	/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified <paramref name="assembly"/>.
	/// </summary>
	/// <param name="assembly"><see cref="Assembly"/> to get the <see cref="LoggingConfiguration"/> for.</param>
	/// <exception cref="ArgumentException">
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or whitespace only. -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> is set to <see langword="false"/>.
	/// </exception>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration ForAssembly(Assembly assembly)
	{
		if (assembly is null)
		{
			throw new ArgumentNullException(nameof(assembly));
		}

		LoggingConfigurationAttribute? attr = assembly.GetCustomAttribute<LoggingConfigurationAttribute>();

		if (attr is null)
		{
			return Default;
		}

		string fullDir = GetFullDirectoryForAssembly(attr);

		return new LoggingConfiguration()
		{
			LogDirectory = fullDir,
			SupportedLogs = attr.SupportedLogs,
			EnableLogging = IsGloballyEnabled && !Attribute.IsDefined(assembly, typeof(DisableLoggingAttribute)),
			SupportsDiagnostics = attr.SupportsDiagnostics,
			EnableDiagnostics = attr.SupportsDiagnostics,
			EnableExceptions = attr.EnableExceptions,
			DefaultNodeOutput = attr.DefaultNodeOutput == NodeOutput.Default ? NodeOutput.Node : attr.DefaultNodeOutput
		};
	}

	/// <inheritdoc cref="ForGenerator{T}(in GeneratorLogCreationContext, LoggingConfiguration?)"/>
	public static LoggingConfiguration ForGenerator<T>() where T : ISourceGenerator
	{
		return ForGenerator_Internal(typeof(T));
	}

	/// <inheritdoc cref="ForGenerator{T}(in GeneratorLogCreationContext, LoggingConfiguration?)"/>
	public static LoggingConfiguration ForGenerator<T>(in GeneratorLogCreationContext context)
	{
		return ForGenerator<T>(in context, default);
	}

	/// <summary>
	/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</typeparam>
	/// <param name="context"><see cref="GeneratorLogCreationContext"/> to use when creating the <see cref="LoggingConfiguration"/>.</param>
	/// <param name="defaultConfiguration"><see cref="LoggingConfiguration"/> to return if <see cref="GeneratorLogCreationContext.CheckForConfigurationAttribute"/> is <see langword="true"/>.</param>
	/// <exception cref="ArgumentException">
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
	/// <see cref="LoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
	/// </exception>
	public static LoggingConfiguration ForGenerator<T>(in GeneratorLogCreationContext context, LoggingConfiguration? defaultConfiguration)
	{
		LoggingConfiguration config = context.CheckForConfigurationAttribute
			? ForGenerator_Internal(typeof(T))
			: (defaultConfiguration ?? Default);

		config.AcceptContext(in context);

		return config;
	}

	/// <inheritdoc cref="ForGenerator(Type, in GeneratorLogCreationContext, LoggingConfiguration?)"/>
	public static LoggingConfiguration ForGenerator(Type type)
	{
		if (type is null)
		{
			throw new ArgumentNullException(nameof(type));
		}

		CheckTypeIsISourceGenerator(type);
		return ForGenerator_Internal(type);
	}

	/// <summary>
	/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
	/// </summary>
	/// <param name="type"><see cref="Type"/> of <see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</param>
	/// <exception cref="ArgumentException">
	/// <param name="context"><see cref="GeneratorLogCreationContext"/> to use when creating the <see cref="LoggingConfiguration"/>.</param>
	/// <paramref name="type"/> does not implement the <see cref="ISourceGenerator"/> interface. -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
	/// <see cref="LoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
	/// </exception>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration ForGenerator(Type type, in GeneratorLogCreationContext context)
	{
		return ForGenerator(type, in context, default);
	}

	/// <summary>
	/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
	/// </summary>
	/// <param name="type"><see cref="Type"/> of <see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</param>
	/// <exception cref="ArgumentException">
	/// <param name="context"><see cref="GeneratorLogCreationContext"/> to use when creating the <see cref="LoggingConfiguration"/>.</param>
	/// <param name="defaultConfiguration"><see cref="LoggingConfiguration"/> to return if <see cref="GeneratorLogCreationContext.CheckForConfigurationAttribute"/> is <see langword="true"/>.</param>
	/// <paramref name="type"/> does not implement the <see cref="ISourceGenerator"/> interface. -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
	/// <see cref="LoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
	/// </exception>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration ForGenerator(Type type, in GeneratorLogCreationContext context, LoggingConfiguration? defaultConfiguration)
	{
		if (type is null)
		{
			throw new ArgumentNullException(nameof(type));
		}

		CheckTypeIsISourceGenerator(type);

		LoggingConfiguration config = context.CheckForConfigurationAttribute
			? ForGenerator_Internal(type)
			: (defaultConfiguration ?? Default);

		config.AcceptContext(in context);

		return config;
	}

	/// <summary>
	/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</param>
	/// <exception cref="ArgumentException">
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
	/// <see cref="LoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
	/// </exception>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration ForGenerator(ISourceGenerator generator)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		return ForGenerator_Internal(generator.GetType());
	}

	/// <summary>
	/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</param>
	/// <param name="context"><see cref="GeneratorLogCreationContext"/> to use when creating the <see cref="LoggingConfiguration"/>.</param>
	/// <exception cref="ArgumentException">
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
	/// <see cref="LoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
	/// </exception>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration ForGenerator(ISourceGenerator generator, in GeneratorLogCreationContext context)
	{
		return ForGenerator(generator, in context, default);
	}

	/// <summary>
	/// Creates new instance of the <see cref="LoggingConfiguration"/> for the specified type.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> to get the <see cref="LoggingConfiguration"/> for.</param>
	/// <param name="context"><see cref="GeneratorLogCreationContext"/> to use when creating the <see cref="LoggingConfiguration"/>.</param>
	/// <param name="defaultConfiguration"><see cref="LoggingConfiguration"/> to return if <see cref="GeneratorLogCreationContext.CheckForConfigurationAttribute"/> is <see langword="true"/>.</param>
	/// <exception cref="ArgumentException">
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or white space only. -or-
	/// <see cref="LoggingConfigurationAttribute"/> must be specified if <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> is set to <see langword="true"/> -or-
	/// <see cref="LoggingConfigurationAttribute.LogDirectory"/> must be specified if both <see cref="LoggingConfigurationAttribute.RelativeToDefault"/> and <see cref="LoggingConfigurationAttribute.RelativeToGlobal"/> are set to <see langword="false"/>.
	/// </exception>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration ForGenerator(ISourceGenerator generator, in GeneratorLogCreationContext context, LoggingConfiguration? defaultConfiguration)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		LoggingConfiguration config = context.CheckForConfigurationAttribute
			? ForGenerator_Internal(generator.GetType())
			: (defaultConfiguration ?? Default);

		config.AcceptContext(in context);

		return config;
	}

	/// <summary>
	/// Creates a new <see cref="LoggingConfiguration"/> based on data specified in the given <paramref name="attribute"/>.
	/// </summary>
	/// <param name="attribute"><see cref="LoggingConfigurationAttribute"/> to create the <see cref="LoggingConfiguration"/> for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration FromAttribute(LoggingConfigurationAttribute attribute)
	{
		return FromAttribute(attribute as ILoggingConfigurationAttribute);
	}

	/// <summary>
	/// Creates a new <see cref="LoggingConfiguration"/> based on data specified in the given <paramref name="attribute"/>.
	/// </summary>
	/// <param name="attribute"><see cref="EnableLoggingAttribute"/> to create the <see cref="LoggingConfiguration"/> for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration FromAttribute(EnableLoggingAttribute attribute)
	{
		return FromAttribute(attribute as ILoggingConfigurationAttribute);
	}

	/// <summary>
	/// Returns a cached reference to a <see cref="LoggingConfiguration"/> created using an <see cref="EnableLoggingAttribute"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to get the <see cref="LoggingConfiguration"/> of.</param>
	/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value: <paramref name="module"/>. -or- <see cref="LoggingConfiguration"/> not found for the specified <paramref name="module"/>.</exception>
	public static LoggingConfiguration GetDynamic(DurianModule module)
	{
		if (!TryGetDynamic(module, out LoggingConfiguration? configuration))
		{
			throw new ArgumentException($"{nameof(LoggingConfiguration)} not found for module '{module}'", nameof(module));
		}

		return configuration;
	}

	/// <summary>
	/// Returns a cached reference to a <see cref="LoggingConfiguration"/> created using an <see cref="EnableLoggingAttribute"/>.
	/// </summary>
	/// <param name="moduleName">Name of module to get the <see cref="LoggingConfiguration"/> of.</param>
	/// <exception cref="ArgumentException"><paramref name="moduleName"/> cannot be <see langword="null"/> or empty. -or- <see cref="LoggingConfiguration"/> not found for module with name <paramref name="moduleName"/>.</exception>
	public static LoggingConfiguration GetDynamic(string moduleName)
	{
		if (!TryGetDynamic(moduleName, out LoggingConfiguration? configuration))
		{
			throw new ArgumentException($"{nameof(LoggingConfiguration)} not found for module with name '{moduleName}'", nameof(moduleName));
		}

		return configuration;
	}

	/// <summary>
	/// Checks if the specified <paramref name="type"/> has the <see cref="DisableLoggingAttribute"/> applied, either directly or by inheritance.
	/// </summary>
	/// <param name="type"><see cref="Type"/> to perform the check for.</param>
	/// <exception cref="ArgumentException"><paramref name="type"/> does not implement the <see cref="ISourceGenerator"/> interface.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public static bool HasDisableAttribute(Type type)
	{
		if (type is null)
		{
			throw new ArgumentNullException(nameof(type));
		}

		CheckTypeIsISourceGenerator(type);
		return HasDisableAttribute_Internal(type);
	}

	/// <summary>
	/// Checks if the specified <paramref name="assembly"/> has the <see cref="DisableLoggingAttribute"/> applied.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static bool HasDisableAttribute(Assembly assembly)
	{
		if (assembly is null)
		{
			throw new ArgumentNullException(nameof(assembly));
		}

		return !IsEnabledForAssembly_Internal(assembly);
	}

	/// <summary>
	/// Checks if generator logging is enabled for the specified <paramref name="assembly"/>.
	/// </summary>
	/// <remarks>Always returns <see langword="false"/> is <see cref="IsGloballyEnabled"/> is <see langword="false"/>.</remarks>
	/// <param name="assembly"><see cref="Assembly"/> to perform the check for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static bool IsEnabledForAssembly(Assembly assembly)
	{
		if (assembly is null)
		{
			throw new ArgumentNullException(nameof(assembly));
		}

		return IsGloballyEnabled && assembly.GetCustomAttribute(typeof(DisableLoggingAttribute)) is null;
	}

	/// <summary>
	/// Checks if generator logging is enabled for the specified <paramref name="generator"/>.
	/// </summary>
	/// <remarks>Always returns <see langword="false"/> is <see cref="IsGloballyEnabled"/> is <see langword="false"/>.</remarks>
	/// <param name="generator"><see cref="ISourceGenerator"/> to perform the check for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static bool IsEnabledForGenerator(ISourceGenerator generator)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		if (!IsGloballyEnabled)
		{
			return false;
		}

		Type type = generator.GetType();

		if (!IsEnabledForAssembly_Internal(type.Assembly))
		{
			return false;
		}

		return IsEnabledForGenerator_Internal(type, true);
	}

	/// <summary>
	/// Checks if generator logging is enabled for the specified generator <paramref name="type"/>.
	/// </summary>
	/// <remarks>Always returns <see langword="false"/> is <see cref="IsGloballyEnabled"/> is <see langword="false"/>.</remarks>
	/// <param name="type"><see cref="Type"/> of <see cref="ISourceGenerator"/> to perform the check for.</param>
	/// <exception cref="ArgumentException"><paramref name="type"/> does not implement the <see cref="ISourceGenerator"/> interface.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public static bool IsEnabledForGenerator(Type type)
	{
		if (type is null)
		{
			throw new ArgumentNullException(nameof(type));
		}

		CheckTypeIsISourceGenerator(type);

		if (!IsGloballyEnabled)
		{
			return false;
		}

		if (!IsEnabledForAssembly_Internal(type.Assembly))
		{
			return false;
		}

		return IsEnabledForGenerator_Internal(type, true);
	}

	/// <summary>
	/// Checks if generator logging is enabled for the specified generator type.
	/// </summary>
	/// <remarks>Always returns <see langword="false"/> is <see cref="IsGloballyEnabled"/> is <see langword="false"/>.</remarks>
	/// <typeparam name="T">Type of <see cref="ISourceGenerator"/> to perform the check for.</typeparam>
	public static bool IsEnabledForGenerator<T>() where T : ISourceGenerator
	{
		if (!IsGloballyEnabled)
		{
			return false;
		}

		Type type = typeof(T);

		if (!IsEnabledForAssembly_Internal(type.Assembly))
		{
			return false;
		}

		return IsEnabledForGenerator_Internal(type, true);
	}

	/// <summary>
	/// Registers a new dynamic <see cref="LoggingConfiguration"/> for the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to register the <paramref name="configuration"/> for.</param>
	/// <param name="configuration"><see cref="LoggingConfiguration"/> to register.</param>
	/// <param name="replace">Determines whether to replace the existing <see cref="LoggingConfiguration"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value: <paramref name="module"/>.</exception>
	public static bool RegisterDynamic(DurianModule module, LoggingConfiguration configuration, bool replace = true)
	{
		if (configuration is null)
		{
			throw new ArgumentNullException(nameof(configuration));
		}

		string moduleName = ModuleIdentity.GetName(module);
		return RegisterDynamic_Internal(moduleName, configuration, replace);
	}

	/// <summary>
	/// Registers a new dynamic <see cref="LoggingConfiguration"/> for a module with the specified <paramref name="moduleName"/>.
	/// </summary>
	/// <param name="moduleName">Name of module to register the <paramref name="configuration"/> for.</param>
	/// <param name="configuration"><see cref="LoggingConfiguration"/> to register.</param>
	/// <param name="replace">Determines whether to replace the existing <see cref="LoggingConfiguration"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="moduleName"/> cannot be <see langword="null"/> or empty.</exception>
	public static bool RegisterDynamic(string moduleName, LoggingConfiguration configuration, bool replace = true)
	{
		if (configuration is null)
		{
			throw new ArgumentNullException(nameof(configuration));
		}

		if (string.IsNullOrWhiteSpace(moduleName))
		{
			throw new ArgumentException("Value cannot be null or empty", nameof(moduleName));
		}

		return RegisterDynamic_Internal(moduleName, configuration, replace);
	}

	/// <summary>
	/// Removes the cached <see cref="LoggingConfiguration"/> of the specified <paramref name="assembly"/>.
	/// </summary>
	/// <param name="assembly"><see cref="Assembly"/> to remove the cached <see cref="LoggingConfiguration"/> of.</param>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static bool RemoveAssembly(Assembly assembly)
	{
		return RemoveAssembly(assembly, out _);
	}

	/// <summary>
	/// Removes the cached <see cref="LoggingConfiguration"/> of the specified <paramref name="assembly"/>.
	/// </summary>
	/// <param name="assembly"><see cref="Assembly"/> to remove the cached <see cref="LoggingConfiguration"/> of.</param>
	/// <param name="configuration">The removed <see cref="LoggingConfiguration"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static bool RemoveAssembly(Assembly assembly, [NotNullWhen(true)] out LoggingConfiguration? configuration)
	{
		if (assembly is null)
		{
			throw new ArgumentNullException(nameof(assembly));
		}

		return _assemblyConfigurations.TryRemove(assembly, out configuration);
	}

	/// <summary>
	/// Removes the cached dynamic <see cref="LoggingConfiguration"/> of a module with the specified <paramref name="moduleName"/>.
	/// </summary>
	/// <param name="moduleName">Name of module to remove the cached <see cref="LoggingConfiguration"/> of.</param>
	/// <exception cref="ArgumentException"><paramref name="moduleName"/> cannot be <see langword="null"/> or empty.</exception>
	public static bool RemoveDynamic(string moduleName)
	{
		return RemoveDynamic(moduleName, out _);
	}

	/// <summary>
	/// Removes the cached dynamic <see cref="LoggingConfiguration"/> of a module with the specified <paramref name="moduleName"/>.
	/// </summary>
	/// <param name="moduleName">Name of module to remove the cached <see cref="LoggingConfiguration"/> of.</param>
	/// <param name="configuration">The removed <see cref="LoggingConfiguration"/>.</param>
	/// <exception cref="ArgumentException"><paramref name="moduleName"/> cannot be <see langword="null"/> or empty.</exception>
	public static bool RemoveDynamic(string moduleName, [NotNullWhen(true)] out LoggingConfiguration? configuration)
	{
		if (string.IsNullOrWhiteSpace(moduleName))
		{
			throw new ArgumentException("Value cannot be null or empty", nameof(moduleName));
		}

		return _dynamicConfigurations.TryRemove(moduleName, out configuration);
	}

	/// <summary>
	/// Removes the cached dynamic <see cref="LoggingConfiguration"/> of the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to remove the cached <see cref="LoggingConfiguration"/> of.</param>
	public static bool RemoveDynamic(DurianModule module)
	{
		return RemoveDynamic(module, out _);
	}

	/// <summary>
	/// Removes the cached dynamic <see cref="LoggingConfiguration"/> of the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to remove the cached <see cref="LoggingConfiguration"/> of.</param>
	/// <param name="configuration">The removed <see cref="LoggingConfiguration"/>.</param>
	/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value: <paramref name="module"/>.</exception>
	public static bool RemoveDynamic(DurianModule module, [NotNullWhen(true)] out LoggingConfiguration? configuration)
	{
		string moduleName = ModuleIdentity.GetName(module);
		return RemoveDynamic(moduleName, out configuration);
	}

	/// <summary>
	/// Returns a cached reference to the <see cref="LoggingConfiguration"/> for the specified <paramref name="assembly"/>.
	/// </summary>
	/// <param name="assembly"><see cref="Assembly"/> to get the <see cref="LoggingConfiguration"/> for.</param>
	/// <exception cref="ArgumentException"><see cref="LoggingConfigurationAttribute.LogDirectory"/> cannot be empty or whitespace only.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static LoggingConfiguration ResolveAssembly(Assembly assembly)
	{
		if (assembly is null)
		{
			throw new ArgumentNullException(nameof(assembly));
		}

		if (!_assemblyConfigurations.TryGetValue(assembly, out LoggingConfiguration config))
		{
			config = ForAssembly(assembly);
			_assemblyConfigurations.TryAdd(assembly, config);
		}

		return config;
	}

	/// <summary>
	/// Attempts to return a dynamic <see cref="LoggingConfiguration"/> for the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to get the <see cref="LoggingConfiguration"/> of.</param>
	/// <param name="configuration">Returned <see cref="LoggingConfiguration"/>.</param>
	/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value: <paramref name="module"/>.</exception>
	public static bool TryGetDynamic(DurianModule module, [NotNullWhen(true)] out LoggingConfiguration? configuration)
	{
		string moduleName = ModuleIdentity.GetName(module);
		return TryGetDynamic(moduleName, out configuration);
	}

	/// <summary>
	/// Attempts to return a dynamic <see cref="LoggingConfiguration"/> for a module with the specified <paramref name="moduleName"/>.
	/// </summary>
	/// <param name="moduleName">Name of module to get the <see cref="LoggingConfiguration"/> of.</param>
	/// <param name="configuration">Returned <see cref="LoggingConfiguration"/>.</param>
	/// <exception cref="ArgumentException"><paramref name="moduleName"/> cannot be <see langword="null"/> or empty.</exception>
	public static bool TryGetDynamic(string moduleName, [NotNullWhen(true)] out LoggingConfiguration? configuration)
	{
		if (string.IsNullOrWhiteSpace(moduleName))
		{
			throw new ArgumentException("Value cannot be null or empty", nameof(moduleName));
		}

		return _dynamicConfigurations.TryGetValue(moduleName, out configuration);
	}

	internal static LoggingConfiguration FromAttribute(ILoggingConfigurationAttribute attribute)
	{
		if (attribute is null)
		{
			throw new ArgumentNullException(nameof(attribute));
		}

		return new LoggingConfiguration()
		{
			_logDirectory = GetFullDirectoryForType(attribute, null),
			SupportedLogs = attribute.SupportedLogs,
			_enableLogging = IsGloballyEnabled,
			_supportsDiagnostics = attribute.SupportsDiagnostics,
			_enableDiagnostics = attribute.SupportsDiagnostics,
			EnableExceptions = attribute.EnableExceptions,
			DefaultNodeOutput = attribute.DefaultNodeOutput == NodeOutput.Default ? NodeOutput.Node : attribute.DefaultNodeOutput
		};
	}

	internal static ReportDiagnosticTarget GetReportDiagnosticTarget(bool enableLogging, bool enableReport)
	{
		ReportDiagnosticTarget target = ReportDiagnosticTarget.None;

		if (enableLogging)
		{
			target += (int)ReportDiagnosticTarget.Log;
		}

		if (enableReport)
		{
			target += (int)ReportDiagnosticTarget.Report;
		}

		return target;
	}

	private static bool CheckLoggingIsEnabled()
	{
		return !AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetCustomAttribute(typeof(DisableLoggingGloballyAttribute)) is not null);
	}

	private static void CheckTypeIsISourceGenerator(Type type)
	{
		if (!typeof(ISourceGenerator).IsAssignableFrom(type))
		{
			throw new ArgumentException($"Specified type does not implement the {nameof(ISourceGenerator)} interface!");
		}
	}

	private static LoggingConfiguration ForGenerator_Internal(Type type)
	{
		LoggingConfigurationAttribute? attr = type.GetCustomAttribute<LoggingConfigurationAttribute>(true);

		if (attr is null)
		{
			LoggingConfiguration config = ResolveAssembly(type.Assembly);

			if (config.EnableLogging)
			{
				config._enableLogging = IsEnabledForGenerator_Internal(type, false);
			}

			return config;
		}
		else
		{
			return new LoggingConfiguration()
			{
				_logDirectory = GetFullDirectoryForType(attr, ResolveAssembly(type.Assembly)),
				SupportedLogs = attr.SupportedLogs,
				_enableLogging = IsGloballyEnabled && !HasDisableAttribute_Internal(type),
				_supportsDiagnostics = attr.SupportsDiagnostics,
				_enableDiagnostics = attr.SupportsDiagnostics,
				EnableExceptions = attr.EnableExceptions,
				DefaultNodeOutput = attr.DefaultNodeOutput == NodeOutput.Default ? NodeOutput.Node : attr.DefaultNodeOutput
			};
		}
	}

	private static string GetFullDirectoryForAssembly(LoggingConfigurationAttribute attribute)
	{
		if (attribute.LogDirectory is null)
		{
			if (!attribute.RelativeToDefault)
			{
				throw new ArgumentException($"{nameof(LogDirectory)} must be specified if {nameof(attribute.RelativeToDefault)} is set to false!", nameof(attribute));
			}

			return DefaultLogDirectory;
		}

		if (string.IsNullOrWhiteSpace(attribute.LogDirectory))
		{
			throw new ArgumentException($"{nameof(LogDirectory)} of the {nameof(LoggingConfigurationAttribute)} cannot be empty or whitespace only!", nameof(attribute));
		}

		string? dir;

		if (attribute.RelativeToDefault)
		{
			if (attribute.LogDirectory![0] == '/')
			{
				dir = DefaultLogDirectory + attribute.LogDirectory;
			}
			else
			{
				dir = DefaultLogDirectory + "/" + attribute.LogDirectory;
			}
		}
		else
		{
			dir = attribute.LogDirectory;
		}

		// Checks if the directory is valid.
		Path.GetFullPath(dir);

		return dir;
	}

	private static string GetFullDirectoryForType(ILoggingConfigurationAttribute attribute, LoggingConfiguration? globalConfiguration)
	{
		if (attribute.LogDirectory is null)
		{
			if (attribute.RelativeToGlobal)
			{
				if (globalConfiguration is null)
				{
					throw Exc_GlobalNotSpecified(nameof(globalConfiguration));
				}

				return globalConfiguration!.LogDirectory;
			}
			else if (attribute.RelativeToDefault)
			{
				return DefaultLogDirectory;
			}
			else
			{
				throw new ArgumentException($"{nameof(LogDirectory)} must be specified if both {nameof(attribute.RelativeToDefault)} and {nameof(attribute.RelativeToGlobal)} are set to false.", nameof(attribute));
			}
		}

		if (string.IsNullOrWhiteSpace(attribute.LogDirectory))
		{
			throw new ArgumentException($"{nameof(LogDirectory)} of the {nameof(LoggingConfigurationAttribute)} cannot be empty or white space only!", nameof(attribute));
		}

		string? dir;

		if (attribute.RelativeToGlobal)
		{
			if (globalConfiguration is null)
			{
				throw Exc_GlobalNotSpecified(nameof(globalConfiguration));
			}

			dir = CombineWithRoot(globalConfiguration.LogDirectory);
		}
		else if (attribute.RelativeToDefault)
		{
			dir = CombineWithRoot(DefaultLogDirectory);
		}
		else
		{
			dir = attribute.LogDirectory;
		}

		// Checks if the directory is valid.
		Path.GetFullPath(dir);

		return dir;

		static ArgumentException Exc_GlobalNotSpecified(string argName)
		{
			return new ArgumentException($"{argName} must be specified if {nameof(attribute.RelativeToGlobal)} is set to true!", nameof(attribute));
		}

		string CombineWithRoot(string root)
		{
			if (attribute.LogDirectory![0] == '/')
			{
				return root + attribute.LogDirectory;
			}
			else
			{
				return root + "/" + attribute.LogDirectory;
			}
		}
	}

	private static bool HasDisableAttribute_Internal(Type type)
	{
		DisableLoggingAttribute? attr = type.GetCustomAttribute<DisableLoggingAttribute>();

		if (attr is null)
		{
			attr = type.BaseType.GetCustomAttribute<DisableLoggingAttribute>(true);

			if (attr is null || !attr.Inherit)
			{
				return false;
			}
		}

		return true;
	}

	private static bool IsEnabledForAssembly_Internal(Assembly assembly)
	{
		return assembly.GetCustomAttribute(typeof(DisableLoggingAttribute)) is null;
	}

	private static bool IsEnabledForGenerator_Internal(Type type, bool checkForConfigurationAttribute)
	{
		if (HasDisableAttribute_Internal(type))
		{
			return false;
		}

		if (checkForConfigurationAttribute)
		{
			return type.GetCustomAttribute(typeof(LoggingConfigurationAttribute), true) is not null;
		}

		return false;
	}

	private static bool RegisterDynamic_Internal(string moduleName, LoggingConfiguration configuration, bool replace)
	{
		if (replace)
		{
			_dynamicConfigurations[moduleName] = configuration;
			return true;
		}

		return _dynamicConfigurations.TryAdd(moduleName, configuration);
	}
}
