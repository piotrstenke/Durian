using System.Diagnostics;
using System.Linq;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian
{
	/// <inheritdoc cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class SourceGenerator
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
		: SourceGenerator<ICompilationData, IDurianSyntaxReceiver, ISyntaxFilterWithDiagnostics>
#else
		: SourceGenerator<ICompilationData, IDurianSyntaxReceiver, ISyntaxFilter>
#endif
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator"/> class.
		/// </summary>
		protected SourceGenerator()
		{
		}
	}

	/// <inheritdoc cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class SourceGenerator<TCompilationData>
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
		: SourceGenerator<TCompilationData, IDurianSyntaxReceiver, ISyntaxFilterWithDiagnostics>
#else
		: SourceGenerator<TCompilationData, IDurianSyntaxReceiver, ISyntaxFilter>
#endif
		where TCompilationData : class, ICompilationData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData}"/> class.
		/// </summary>
		protected SourceGenerator()
		{
		}
	}

	/// <inheritdoc cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class SourceGenerator<TCompilationData, TSyntaxReceiver>
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
		: SourceGenerator<TCompilationData, TSyntaxReceiver, ISyntaxFilterWithDiagnostics>
#else
		: SourceGenerator<TCompilationData, TSyntaxReceiver, ISyntaxFilter>
#endif
		where TCompilationData : class, ICompilationData
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		protected SourceGenerator()
		{
		}
	}

	/// <summary>
	/// Abstract implementation of the <see cref="IDurianSourceGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	/// <typeparam name="TCompilationData">User-defined type of <see cref="ICompilationData"/> this <see cref="IDurianSourceGenerator"/> operates on.</typeparam>
	/// <typeparam name="TSyntaxReceiver">User-defined type of <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="CSharpSyntaxNode"/>s to perform the generation on.</typeparam>
	/// <typeparam name="TFilter">User-defined type of <see cref="ISyntaxFilter"/> that decides what <see cref="CSharpSyntaxNode"/>s collected by the <see cref="SyntaxReceiver"/> are valid for generation.</typeparam>
	[DebuggerDisplay("{GetGeneratorName()}, {GetVersion()}")]
	public abstract class SourceGenerator<TCompilationData, TSyntaxReceiver, TFilter> : IDurianSourceGenerator
		where TCompilationData : class, ICompilationData
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
		where TFilter : ISyntaxFilterWithDiagnostics
#else
		where TFilter : ISyntaxFilter
#endif
	{
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS

#pragma warning disable IDE0032 // Use auto property
		private bool _enableDiagnostics;
#pragma warning restore IDE0032 // Use auto property

		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that is used to report diagnostics.
		/// </summary>
		protected ReadonlyDiagnosticReceiver<GeneratorExecutionContext> DiagnosticReceiver { get; }
#endif
		/// <inheritdoc cref="IDurianSourceGenerator.TargetCompilation"/>
		public TCompilationData TargetCompilation { get; private set; }

		/// <inheritdoc cref="IDurianSourceGenerator.SyntaxReceiver"/>
		public TSyntaxReceiver SyntaxReceiver { get; private set; }

		/// <inheritdoc/>
		public CSharpParseOptions ParseOptions { get; private set; }

		/// <inheritdoc/>
		public bool SupportsDiagnostics
		{
			get
			{
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
				return true;
#else
				return false;
#endif
			}
		}

		/// <inheritdoc/>
		public bool EnableDiagnostics
		{
			get
			{
#pragma warning disable IDE0027 // Use expression body for accessors

#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
				return _enableDiagnostics;
#else
				return false;
#endif

#pragma warning restore IDE0027 // Use expression body for accessors
			}
			set
			{
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS

#pragma warning disable IDE0027 // Use expression body for accessors
				_enableDiagnostics = value;
#pragma warning restore IDE0027 // Use expression body for accessors

#endif
			}
		}

		ICompilationData IDurianSourceGenerator.TargetCompilation => TargetCompilation;
		IDurianSyntaxReceiver IDurianSourceGenerator.SyntaxReceiver => SyntaxReceiver;
		string IDurianSourceGenerator.GeneratorName => GetGeneratorName();
		string IDurianSourceGenerator.Version => GetVersion();

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		protected SourceGenerator()
		{
			TargetCompilation = null!;
			SyntaxReceiver = null!;
			ParseOptions = null!;

#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
			DiagnosticReceiver = DiagnosticReceiverFactory.SourceGenerator();
#endif
		}

		/// <summary>
		/// Initializes the source generator.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public virtual void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(CreateSyntaxReceiver);
		}

		/// <summary>
		/// Begins the generation.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public void Execute(in GeneratorExecutionContext context)
		{
			if (context.SyntaxReceiver is not TSyntaxReceiver receiver || !ValidateSyntaxReceiver(receiver))
			{
				return;
			}

			if (context.Compilation is not CSharpCompilation currentCompilation)
			{
				return;
			}

			TCompilationData? data = CreateCompilationData(currentCompilation);

			if (data is null || data.HasErrors)
			{
				return;
			}

			TargetCompilation = data;
			ParseOptions = context.ParseOptions as CSharpParseOptions ?? CSharpParseOptions.Default;
			SyntaxReceiver = receiver;

			BeforeFiltration(in context);
			Filtrate(in context);
		}

		private void Filtrate(in GeneratorExecutionContext context)
		{
			FilterList<TFilter>? filters = GetFilters(in context);

			if (filters is null || filters.NumGroups == 0)
			{
				return;
			}

			CodeBuilder builder = new(this);

			foreach (TFilter[] filterGroup in filters)
			{
				int length = filterGroup.Length;
				IMemberData[][] data = new IMemberData[length][];

#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
				if (_enableDiagnostics)
				{
					for (int i = 0; i < length; i++)
					{
						data[i] = filterGroup[i].Filtrate(DiagnosticReceiver, TargetCompilation, SyntaxReceiver, context.CancellationToken).ToArray();
					}
				}
				else
				{
#endif
					for (int i = 0; i < length; i++)
					{
						data[i] = filterGroup[i].Filtrate(TargetCompilation, SyntaxReceiver, context.CancellationToken).ToArray();
					}

#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
				}
#endif

				for (int i = 0; i < length; i++)
				{
					foreach (IMemberData d in data[i])
					{
						Generate(d, filterGroup[i], builder, in context);
					}
				}
			}
		}

		void ISourceGenerator.Execute(GeneratorExecutionContext context)
		{
			Execute(in context);
		}

		IDurianSyntaxReceiver IDurianSourceGenerator.CreateSyntaxReceiver()
		{
			return CreateSyntaxReceiver();
		}

		/// <summary>
		/// Creates a new <see cref="IDurianSyntaxReceiver"/> to be used during the current generation pass.
		/// </summary>
		public abstract TSyntaxReceiver CreateSyntaxReceiver();

		/// <summary>
		/// Returns version of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		protected virtual string GetVersion()
		{
			return "1.0.0";
		}

		/// <summary>
		/// Returns name of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		protected virtual string GetGeneratorName()
		{
			return nameof(SourceGenerator);
		}

		/// <summary>
		/// Returns a list of <see cref="ISyntaxFilter"/>s to be used during the current generation pass.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected abstract FilterList<TFilter>? GetFilters(in GeneratorExecutionContext context);

		/// <summary>
		/// Validates the <paramref name="syntaxReceiver"/>.
		/// </summary>
		/// <param name="syntaxReceiver"><typeparamref name="TSyntaxReceiver"/> to validate.</param>
		protected virtual bool ValidateSyntaxReceiver(TSyntaxReceiver syntaxReceiver)
		{
			return !syntaxReceiver.IsEmpty();
		}

		/// <summary>
		/// Method called before node filtration is performed.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void BeforeFiltration(in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Actually begins the generator execution.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to generate the source for.</param>
		/// <param name="filter"><see cref="ISyntaxFilter"/> that collected the target <paramref name="member"/>.</param>
		/// <param name="builder"><see cref="CodeBuilder"/> that should be used to generate the source code.</param>
		/// <param name="context">The <see cref="GeneratorExecutionContext"/> to add source to.</param>
		protected abstract void Generate(IMemberData member, TFilter filter, CodeBuilder builder, in GeneratorExecutionContext context);

		/// <summary>
		/// Creates new instance of <see cref="ICompilationData"/>.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		protected abstract TCompilationData? CreateCompilationData(CSharpCompilation compilation);
	}
}
