﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that validates the filtrated nodes.
	/// </summary>
	/// <typeparam name="TData">Type of <see cref="IMemberData"/> this <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/> returns.</typeparam>
	/// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/> uses.</typeparam>
	/// <typeparam name="TSyntaxReceiver">Type of <see cref="IDurianSyntaxReceiver"/> this <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/> uses.</typeparam>
	/// <typeparam name="TGenerator">Type of <see cref="IDurianGenerator"/> that is the target of this <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/>.</typeparam>
	/// <typeparam name="TSyntax">Type of <see cref="CSharpSyntaxNode"/> this <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/> uses.</typeparam>
	/// <typeparam name="TSymbol">Type of <see cref="ISyntaxFilter"/> this <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/> uses.</typeparam>
	public abstract class SyntaxFilterValidator<TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol> : GeneratorSyntaxFilter<TData, TCompilation, TSyntaxReceiver, TGenerator>, INodeProvider<TSyntax>, INodeValidator<TData>
		where TData : IMemberData
		where TCompilation : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TGenerator : IDurianGenerator
		where TSyntax : CSharpSyntaxNode
		where TSymbol : ISymbol
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected SyntaxFilterValidator(TGenerator generator) : base(generator)
		{
		}

		/// <summary>
		/// Returns a collection of <typeparamref name="TSyntax"/>es collected by the <see cref="GeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}.Generator"/>'s <see cref="IDurianSyntaxReceiver"/> that can be filtrated by this filter.
		/// </summary>
		public virtual IEnumerable<TSyntax> GetCandidateNodes()
		{
			return Generator.SyntaxReceiver?.GetNodes().OfType<TSyntax>() ?? Array.Empty<TSyntax>();
		}

		/// <inheritdoc cref="INodeValidator{T}.GetValidationData(CSharpSyntaxNode, ICompilationData, out SemanticModel?, out ISymbol?, CancellationToken)"/>
		public abstract bool GetValidationData(
			TSyntax node,
			TCompilation compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out TSymbol? symbol,
			CancellationToken cancellationToken = default
		);

		/// <inheritdoc cref="INodeValidator{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, out T, CancellationToken)"/>
		public virtual bool ValidateAndCreate(
			TSyntax node,
			TCompilation compilation,
			[NotNullWhen(true)] out TData? data,
			CancellationToken cancellationToken = default
		)
		{
			if(!GetValidationData(node, compilation, out SemanticModel? semanticModel, out TSymbol? symbol, cancellationToken))
			{
				data = default;
				return false;
			}

			return ValidateAndCreate(node, compilation, semanticModel, symbol, out data, cancellationToken);
		}

		/// <inheritdoc cref="INodeValidator{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T, CancellationToken)"/>
		public abstract bool ValidateAndCreate(
			TSyntax node,
			TCompilation compilation,
			SemanticModel semanticModel,
			TSymbol symbol,
			[NotNullWhen(true)] out TData? data,
			CancellationToken cancellationToken = default
		);

		IEnumerable<TSyntax> INodeProvider<TSyntax>.GetNodes()
		{
			return GetCandidateNodes();
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			return GetCandidateNodes();
		}

		bool INodeValidator<TData>.GetValidationData(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken
		)
		{
			if(node is not TSyntax syntax || compilation is not TCompilation c)
			{
				semanticModel = default;
				symbol = default;
				return false;
			}

			bool isValid = GetValidationData(
				syntax,
				c,
				out semanticModel,
				out TSymbol? s,
				cancellationToken
			);

			symbol = s;
			return isValid;
		}

		bool INodeValidator<TData>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out TData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not TSyntax syntax || compilation is not TCompilation c)
			{
				data = default;
				return false;
			}

			return ValidateAndCreate(syntax, c, out data, cancellationToken);
		}

		bool INodeValidator<TData>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out TData? data,
			CancellationToken cancellationToken
		)
		{
			if (node is not TSyntax syntax || symbol is not TSymbol s || compilation is not TCompilation c)
			{
				data = default;
				return false;
			}

			return ValidateAndCreate(
				syntax,
				c,
				semanticModel,
				s,
				out data,
				cancellationToken
			);
		}

		/// <summary>
		/// <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/> that reports diagnostics during filtration.
		/// </summary>
		public new abstract class WithDiagnostics : SyntaxFilterValidator<TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol>, INodeValidatorWithDiagnostics<TData>, IGeneratorSyntaxFilterWithDiagnostics
		{
			/// <inheritdoc/>
			public IHintNameProvider HintNameProvider { get; }

			/// <inheritdoc/>
			public virtual FilterMode Mode
			{
				get
				{
					if (Generator is ILoggableGenerator loggable)
					{
						return loggable.LoggingConfiguration.CurrentFilterMode;
					}

					return FilterMode.None;
				}
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithDiagnostics"/> class.
			/// </summary>
			/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
			/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
			protected WithDiagnostics(TGenerator generator) : this(generator, new SymbolNameToFile())
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="WithDiagnostics"/> class.
			/// </summary>
			/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
			/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
			/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
			protected WithDiagnostics(TGenerator generator, IHintNameProvider hintNameProvider) : base(generator)
			{
				if (hintNameProvider is null)
				{
					throw new ArgumentNullException(nameof(hintNameProvider));
				}

				HintNameProvider = hintNameProvider;
			}

			/// <inheritdoc cref="ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData, IDurianSyntaxReceiver, IDiagnosticReceiver, CancellationToken)"/>
			public abstract IEnumerable<TData> Filtrate(
				TCompilation compilation,
				TSyntaxReceiver syntaxReceiver,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			);

			/// <inheritdoc cref="ISyntaxFilterWithDiagnostics.Filtrate(ICompilationData, IEnumerable{CSharpSyntaxNode}, IDiagnosticReceiver, CancellationToken)"/>
			public abstract IEnumerable<TData> Filtrate(TCompilation compilation,
				IEnumerable<CSharpSyntaxNode> collectedNodes,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			);

			/// <inheritdoc cref="INodeValidatorWithDiagnostics{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, out T, IDiagnosticReceiver, CancellationToken)"/>
			public virtual bool ValidateAndCreate(
				TSyntax node,
				TCompilation compilation,
				[NotNullWhen(true)] out TData? data,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			)
			{
				if (!GetValidationData(node, compilation, out SemanticModel? semanticModel, out TSymbol? symbol, cancellationToken))
				{
					data = default;
					return false;
				}

				return ValidateAndCreate(node, compilation, semanticModel, symbol, out data, diagnosticReceiver, cancellationToken);
			}

			/// <inheritdoc cref="INodeValidatorWithDiagnostics{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T, IDiagnosticReceiver, CancellationToken)"/>
			public abstract bool ValidateAndCreate(
				TSyntax node,
				TCompilation compilation,
				SemanticModel semanticModel,
				TSymbol symbol,
				[NotNullWhen(true)] out TData? data,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken = default
			);

			/// <inheritdoc/>
			/// <exception cref="InvalidOperationException">Target <see cref="GeneratorSyntaxFilter{TData, TCompilation, TSyntaxReceiver, TGenerator}.Generator"/> is not initialized.</exception>
			public override IEnumerator<TData> GetEnumerator()
			{
				if(Generator.TargetCompilation is null)
				{
					throw new InvalidOperationException("Target generator is not initialized!");
				}

				return Mode switch
				{
					FilterMode.Diagnostics => new FilterEnumeratorWithDiagnostics<TData>(this, Generator.TargetCompilation, this, GetDiagnosticReceiver()),
					FilterMode.Logs => new LoggableFilterEnumerator<TData>(this, Generator.TargetCompilation, this, GetLogReceiver(false), HintNameProvider),
					FilterMode.Both => new LoggableFilterEnumerator<TData>(this, Generator.TargetCompilation, this, GetLogReceiver(true), HintNameProvider),
					_ => new FilterEnumerator<TData>(this, Generator.TargetCompilation, this)
				};
			}

			/// <summary>
			/// Returns a new <see cref="INodeDiagnosticReceiver"/> if it couldn't be created automatically.
			/// </summary>
			/// <param name="includeDiagnostics">Determines whether diagnostics should be reported separately from log files.</param>
			protected virtual INodeDiagnosticReceiver GetNodeDiagnosticReceiverFallback(bool includeDiagnostics)
			{
				return new DiagnosticReceiver.Empty();
			}

			private IDiagnosticReceiver GetDiagnosticReceiver()
			{
				if(Generator is DurianGeneratorBase durian)
				{
					return durian.DiagnosticReceiver!;
				}

				return DiagnosticReceiver.Factory.SourceGenerator();
			}

			private INodeDiagnosticReceiver GetLogReceiver(bool includeDiagnostics)
			{
				if(includeDiagnostics)
				{
					if(Generator is ILoggableGenerator loggable)
					{
						return LoggableDiagnosticReceiver.Factory.SourceGenerator(loggable);
					}
				}
				else
				{
					if (Generator is DurianGeneratorBase durian)
					{
						return durian.LogReceiver;
					}

					if (Generator is ILoggableGenerator loggable)
					{
						return LoggableDiagnosticReceiver.Factory.Basic(loggable);
					}
				}

				return GetNodeDiagnosticReceiverFallback(includeDiagnostics);
			}

			IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(
				ICompilationData compilation,
				IDurianSyntaxReceiver syntaxReceiver,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				return Filtrate((TCompilation)compilation, (TSyntaxReceiver)syntaxReceiver, diagnosticReceiver, cancellationToken).Cast<IMemberData>();
			}

			IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(
				ICompilationData compilation,
				IEnumerable<CSharpSyntaxNode> collectedNodes,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				return Filtrate((TCompilation)compilation, collectedNodes, diagnosticReceiver, cancellationToken).Cast<IMemberData>();
			}

			bool INodeValidatorWithDiagnostics<TData>.ValidateAndCreate(
				CSharpSyntaxNode node,
				ICompilationData compilation,
				[NotNullWhen(true)] out TData? data,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				if (node is not TSyntax syntax || compilation is not TCompilation c)
				{
					data = default;
					return false;
				}

				return ValidateAndCreate(
					syntax,
					c,
					out data,
					diagnosticReceiver,
					cancellationToken
				);
			}

			bool INodeValidatorWithDiagnostics<TData>.ValidateAndCreate(
				CSharpSyntaxNode node,
				ICompilationData compilation,
				SemanticModel semanticModel,
				ISymbol symbol,
				[NotNullWhen(true)] out TData? data,
				IDiagnosticReceiver diagnosticReceiver,
				CancellationToken cancellationToken
			)
			{
				if (node is not TSyntax syntax || symbol is not TSymbol s || compilation is not TCompilation c)
				{
					data = default;
					return false;
				}

				return ValidateAndCreate(
					syntax,
					c,
					semanticModel,
					s,
					out data,
					diagnosticReceiver,
					cancellationToken
				);
			}
		}
	}

	/// <inheritdoc cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/>
	public abstract class SyntaxFilterValidator<TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax> : SyntaxFilterValidator<TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, ISymbol>
		where TData : IMemberData
		where TCompilation : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TGenerator : IDurianGenerator
		where TSyntax : CSharpSyntaxNode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected SyntaxFilterValidator(TGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/>
	public abstract class SyntaxFilterValidator<TData, TCompilation, TSyntaxReceiver, TGenerator> : SyntaxFilterValidator<TData, TCompilation, TSyntaxReceiver, TGenerator, CSharpSyntaxNode, ISymbol>
		where TData : IMemberData
		where TCompilation : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TGenerator : IDurianGenerator
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected SyntaxFilterValidator(TGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/>
	public abstract class SyntaxFilterValidator<TData, TCompilation, TSyntaxReceiver> : SyntaxFilterValidator<TData, TCompilation, TSyntaxReceiver, IDurianGenerator, CSharpSyntaxNode, ISymbol>
		where TData : IMemberData
		where TCompilation : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/>
	public abstract class SyntaxFilterValidator<TData, TCompilation> : SyntaxFilterValidator<TData, TCompilation, IDurianSyntaxReceiver, IDurianGenerator, CSharpSyntaxNode, ISymbol>
		where TData : IMemberData
		where TCompilation : ICompilationData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxFilterValidator{TData, TCompilation}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/>
	public abstract class SyntaxFilterValidator<TData> : SyntaxFilterValidator<TData, ICompilationData, IDurianSyntaxReceiver, IDurianGenerator, CSharpSyntaxNode, ISymbol>
		where TData : IMemberData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxFilterValidator{TData}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}

	/// <inheritdoc cref="SyntaxFilterValidator{TData, TCompilation, TSyntaxReceiver, TGenerator, TSyntax, TSymbol}"/>
	public abstract class SyntaxFilterValidator : SyntaxFilterValidator<IMemberData, ICompilationData, IDurianSyntaxReceiver, IDurianGenerator, CSharpSyntaxNode, ISymbol>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxFilterValidator"/> class.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		protected SyntaxFilterValidator(IDurianGenerator generator) : base(generator)
		{
		}
	}
}