// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Durian.Analysis.Extensions;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.SyntaxVisitors;
using System.Collections.Immutable;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Main class of the <c>CopyFrom</c> module. Generates source code of members marked with the <c>Durian.CopyFromAttribute</c>.
	/// </summary>
	[Generator(LanguageNames.CSharp)]
	[LoggingConfiguration(
		SupportedLogs = GeneratorLogs.All,
		LogDirectory = "CopyFrom",
		SupportsDiagnostics = true,
		RelativeToGlobal = true,
		EnableExceptions = true,
		DefaultNodeOutput = NodeOutput.SyntaxTree)]
	public sealed class CopyFromGenerator : CachedGenerator<ICopyFromMember, CopyFromCompilationData, CopyFromSyntaxReceiver, ICopyFromFilter>
	{
		private const int _numStaticTrees = 4;

		private FilterContainer<ICopyFromFilter>? _filters;

		/// <summary>
		/// Name of this source generator.
		/// </summary>
		public static string GeneratorName => "CopyFrom";

		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public static string Version => "1.0.0";

		/// <inheritdoc/>
		public override int NumStaticTrees => _numStaticTrees;

		/// <inheritdoc cref="CopyFromGenerator(in ConstructionContext, IHintNameProvider?)"/>
		public CopyFromGenerator()
		{
		}

		/// <inheritdoc cref="CopyFromGenerator(in ConstructionContext, IHintNameProvider?)"/>
		public CopyFromGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public CopyFromGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="CopyFromGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		public CopyFromGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public CopyFromGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceTextProvider"/> used by this generator to create initial sources.
		/// </summary>
		public static IEnumerable<ISourceTextProvider> GetSourceProviders()
		{
			return new ISourceTextProvider[_numStaticTrees - 1]
			{
				new CopyFromTypeAttributeProvider(),
				new CopyFromMethodAttributeProvider(),
				new PatternAttributeProvider(),
			};
		}

		/// <inheritdoc/>
		public override CopyFromCompilationData? CreateCompilationData(CSharpCompilation compilation)
		{
			return new CopyFromCompilationData(compilation);
		}

		/// <inheritdoc/>
		public override CopyFromSyntaxReceiver CreateSyntaxReceiver()
		{
			return new CopyFromSyntaxReceiver();
		}

		/// <summary>
		/// Returns a <see cref="FilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="fileNameProvider">Creates name for the generated files.</param>
		public override FilterContainer<ICopyFromFilter> GetFilters(IHintNameProvider fileNameProvider)
		{
			if (_filters is null)
			{
				FilterContainer<ICopyFromFilter> list = new();

				list.RegisterFilterGroup("Methods", new CopyFromMethodFilter(this, fileNameProvider));
				list.RegisterFilterGroup("Types", new CopyFromTypeFilter(this, fileNameProvider));

				_filters = list;
			}

			return _filters;
		}

		/// <inheritdoc/>
		public override string? GetGeneratorName()
		{
			return GeneratorName;
		}

		/// <inheritdoc/>
		public override string? GetGeneratorVersion()
		{
			return Version;
		}

		/// <inheritdoc/>
		public override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return GetSourceProviders();
		}

		/// <inheritdoc/>
		public override DurianModule[] GetRequiredModules()
		{
			return new DurianModule[]
			{
				DurianModule.CopyFrom
			};
		}

		/// <inheritdoc/>
		protected override bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context)
		{
			if (member is not ICopyFromMember target)
			{
				return false;
			}

			if(target is CopyFromTypeData type)
			{
				return GenerateType(type, hintName, in context);
			}

			return false;
		}

		private bool GenerateType(CopyFromTypeData type, string hintName, in GeneratorExecutionContext context)
		{
			TargetData[] targets = type.Targets;
			SortByOrder(targets);

			bool generated = false;
			string keyword = type.Declaration.GetKeyword();

			string currentName = hintName;

			TypeParameterReplacer replacer = new();

			for (int i = 0; i < targets.Length; i++)
			{
				TargetData target = targets[i];

				TypeDeclarationSyntax[] partialDeclarations = GetPartialDeclarations(target);

				if (partialDeclarations.Length == 0)
				{
					continue;
				}

				currentName = partialDeclarations.Length > 1 ? currentName + "_partial" : currentName;

				if (target.Symbol.IsGenericType)
				{
					if (GenerateGenericDeclarations(type, target, partialDeclarations, currentName, keyword, replacer, in context))
					{
						generated = true;
					}
				}
				else if (GenerateDeclarations(type, target, partialDeclarations, currentName, keyword, in context))
				{
					generated = true;
				}

				currentName = hintName + $"_{i + 1}";
			}

			return generated;
		}

		private bool GenerateDeclarations(
			CopyFromTypeData type,
			TargetData target,
			TypeDeclarationSyntax[] partialDeclarations,
			string currentName,
			string keyword,
			in GeneratorExecutionContext context
		)
		{
			bool generated = true;
			string partialName = currentName;

			for (int i = 0; i < partialDeclarations.Length; i++)
			{
				TypeDeclarationSyntax partial = partialDeclarations[i];

				CodeBuilder.WriteDeclarationLead(type, GetUsings(target, partial), GeneratorName, Version);
				CodeBuilder.Indent();
				CodeBuilder.BeginDeclation($"partial {keyword} {type.Name}");

				if(target.HandleSpecialMembers)
				{
					foreach (MemberDeclarationSyntax member in partial.Members)
					{
						if (GetMemberSymbol(member, type.SemanticModel) is not ISymbol symbol)
						{
							continue;
						}

						MemberDeclarationSyntax newMember = HandleSpecialMemberTypes(type, target.Symbol, member);
						WriteGeneratedMember(newMember, symbol);
					}
				}
				else
				{
					foreach (MemberDeclarationSyntax member in partial.Members)
					{
						if (GetMemberSymbol(member, type.SemanticModel) is not ISymbol symbol)
						{
							continue;
						}

						WriteGeneratedMember(member, symbol);
					}
				}

				CodeBuilder.EndAllScopes();

				AddSourceWithOriginal(type.Declaration, partialName, in context);
				generated = true;
				partialName = currentName + $"_{i + 1}";
			}

			return generated;
		}

		private bool GenerateGenericDeclarations(
			CopyFromTypeData type,
			TargetData target,
			TypeDeclarationSyntax[] partialDeclarations,
			string currentName,
			string keyword,
			TypeParameterReplacer replacer,
			in GeneratorExecutionContext context
		)
		{
			bool generated = true;
			string partialName = currentName;

			List<(string identifier, string replacement)> replacements = GetTypeParamterReplacements(target.Symbol);

			for (int i = 0; i < partialDeclarations.Length; i++)
			{
				TypeDeclarationSyntax partial = partialDeclarations[i];

				CodeBuilder.WriteDeclarationLead(type, GetUsings(target, partial), GeneratorName, Version);
				CodeBuilder.Indent();
				CodeBuilder.BeginDeclation($"partial {keyword} {type.Name}");

				if(target.HandleSpecialMembers)
				{
					foreach (MemberDeclarationSyntax member in partial.Members)
					{
						if (GetMemberSymbol(member, type.SemanticModel) is not ISymbol symbol)
						{
							continue;
						}

						CSharpSyntaxNode replaced = HandleSpecialMemberTypes(type, target.Symbol, member);
						ReplaceAndGenerate(replaced, symbol);
					}
				}
				else
				{
					foreach (MemberDeclarationSyntax member in partial.Members)
					{
						if (GetMemberSymbol(member, type.SemanticModel) is not ISymbol symbol)
						{
							continue;
						}

						ReplaceAndGenerate(member, symbol);
					}
				}

				CodeBuilder.EndAllScopes();

				AddSourceWithOriginal(type.Declaration, partialName, in context);
				generated = true;
				partialName = currentName + $"_{i + 1}";
			}

			return generated;

			void ReplaceAndGenerate(CSharpSyntaxNode replaced, ISymbol symbol)
			{
				foreach ((string identifier, string replacement) in replacements)
				{
					replacer.Identifier = identifier;
					replacer.Replacement = replacement;

					replaced = (CSharpSyntaxNode)replacer.Visit(replaced);
				}

				WriteGeneratedMember(replaced, symbol);
			}
		}

		private static ISymbol? GetMemberSymbol(MemberDeclarationSyntax member, SemanticModel semanticModel)
		{
			if(member is BaseFieldDeclarationSyntax field)
			{
				return semanticModel.GetDeclaredSymbol(field.Declaration);
			}

			return semanticModel.GetDeclaredSymbol(member);
		}

		private static TypeDeclarationSyntax[] GetPartialDeclarations(TargetData target)
		{
			if(target.PartialPart is not null)
			{
				return new TypeDeclarationSyntax[] { target.PartialPart };
			}

			return target.Symbol.GetPartialDeclarations<TypeDeclarationSyntax>().ToArray();
		}

		private static List<(string identifier, string replacement)> GetTypeParamterReplacements(INamedTypeSymbol type)
		{
			ImmutableArray<ITypeParameterSymbol> parameters = type.TypeParameters;
			ImmutableArray<ITypeSymbol> arguments = type.TypeArguments;

			List<(string, string)> list = new(parameters.Length);

			for (int i = 0; i < parameters.Length; i++)
			{
				ITypeParameterSymbol parameter = parameters[i];
				ITypeSymbol argument = arguments[i];

				if (!SymbolEqualityComparer.Default.Equals(parameter, argument))
				{
					list.Add((parameter.Name, argument.GetGenericName(GenericSubstitution.Arguments)));
				}
			}

			return list;
		}

		private static IEnumerable<string> GetUsings(TargetData target, TypeDeclarationSyntax partialDeclaration)
		{
			List<string> usings = new(24);
			HashSet<string> set = new();

			if(target.CopyUsings && partialDeclaration.FirstAncestorOrSelf<CompilationUnitSyntax>() is CompilationUnitSyntax root)
			{
				usings.AddRange(root.Usings.Select(u => u.Name.ToString()).Where(u => set.Add(u)));
			}

			if(target.Usings is not null && target.Usings.Length > 0)
			{
				usings.AddRange(target.Usings);
			}

			return usings;
		}

		private static MemberDeclarationSyntax HandleSpecialMemberTypes(CopyFromTypeData type, INamedTypeSymbol target, MemberDeclarationSyntax member)
		{
			switch (member)
			{
				case ConstructorDeclarationSyntax ctor:
					return ctor.WithIdentifier(SyntaxFactory.Identifier(type.Name).WithTriviaFrom(ctor.Identifier));

				case DestructorDeclarationSyntax dtor:
					return dtor.WithIdentifier(SyntaxFactory.Identifier(type.Name).WithTriviaFrom(dtor.Identifier));

				case ConversionOperatorDeclarationSyntax conversion:

					if(SymbolEqualityComparer.Default.Equals(type.SemanticModel.GetSymbolInfo(conversion.Type).Symbol, target))
					{
						conversion = conversion.WithType(GetNameSyntax(((SimpleNameSyntax)conversion.Type).Identifier));
					}

					return conversion.WithParameterList(conversion.ParameterList.WithParameters(SyntaxFactory.SeparatedList(UpdateParameters(conversion.ParameterList.Parameters))));

				case OperatorDeclarationSyntax op:

					if (SymbolEqualityComparer.Default.Equals(type.SemanticModel.GetSymbolInfo(op.ReturnType).Symbol, target))
					{
						op = op.WithReturnType(GetNameSyntax(((SimpleNameSyntax)op.ReturnType).Identifier));
					}

					return op.WithParameterList(op.ParameterList.WithParameters(SyntaxFactory.SeparatedList(UpdateParameters(op.ParameterList.Parameters))));
			}

			return member;

			List<ParameterSyntax> UpdateParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
			{
				List<ParameterSyntax> newParameters = new(parameters.Count);

				for (int i = 0; i < parameters.Count; i++)
				{
					ParameterSyntax parameter = parameters[i];

					if (parameter.Type is not null && SymbolEqualityComparer.Default.Equals(type.SemanticModel.GetSymbolInfo(parameter.Type).Symbol, target))
					{
						parameter = parameter.WithType(GetNameSyntax(((SimpleNameSyntax)parameter.Type).Identifier));
					}

					newParameters.Add(parameter);
				}

				return newParameters;
			}

			SimpleNameSyntax GetNameSyntax(SyntaxToken triviaFrom)
			{
				TypeSyntax syntax = type.Symbol.CreateTypeSyntax();

				return syntax switch
				{
					IdentifierNameSyntax identifier => identifier.WithIdentifier(identifier.Identifier.WithTriviaFrom(triviaFrom)),
					GenericNameSyntax generic => generic.WithIdentifier(generic.Identifier.WithTriviaFrom(triviaFrom)),
					_ => (SimpleNameSyntax)syntax
				};
			}
		}

		private static void SortByOrder(TargetData[] targets)
		{
			if (targets.Length == 1)
			{
				return;
			}

			Array.Sort(targets, (left, right) =>
			{
				if (left.Order > right.Order)
				{
					return 1;
				}
				else if (left.Order < right.Order)
				{
					return -1;
				}
				else
				{
					return 0;
				}
			});
		}
	}
}
