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
using System.Diagnostics.CodeAnalysis;

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

				if (GenerateDeclarations(type, target, partialDeclarations, currentName, keyword, replacer, in context))
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
			TypeParameterReplacer replacer,
			in GeneratorExecutionContext context
		)
		{
			bool isGenerated = true;
			string partialName = currentName;

			Action<CSharpSyntaxNode, ISymbol, GenerateInheritdoc> generateAction = GetGenerationMethod(type, target, replacer);

			for (int i = 0; i < partialDeclarations.Length; i++)
			{
				TypeDeclarationSyntax partial = partialDeclarations[i];

				SyntaxList<MemberDeclarationSyntax> members = partial.Members;

				if(!members.Any())
				{
					continue;
				}

				CodeBuilder.WriteDeclarationLead(type, GetUsings(target, partial), GeneratorName, Version);
				CodeBuilder.Indent();
				CodeBuilder.BeginDeclation($"partial {keyword} {type.Name}");

				for (int j = 0; j < members.Count; j++)
				{
					CSharpSyntaxNode member = members[j];

					if (TryGetMutlipleMembers(member, type.SemanticModel, out ISymbol? symbol, out List<(ISymbol original, MemberDeclarationSyntax generated)>? fields))
					{
						//GenerateInheritdoc inheridoc = target.Symbol.HasInheritableDocumentation() ? GenerateInheritdoc.Always : GenerateInheritdoc.Never;

						foreach ((ISymbol original, MemberDeclarationSyntax generated) in fields)
						{
							generateAction(generated, original, GenerateInheritdoc.Always);
						}
					}
					else if(symbol is not null)
					{
						generateAction(member, symbol, GenerateInheritdoc.Always);
					}
				}

				CodeBuilder.EndAllScopes();

				AddSourceWithOriginal(type.Declaration, partialName, in context);
				isGenerated = true;
				partialName = currentName + $"_{i + 1}";
			}

			return isGenerated;
		}

		private Action<CSharpSyntaxNode, ISymbol, GenerateInheritdoc> GetGenerationMethod(CopyFromTypeData type, TargetData target, TypeParameterReplacer replacer)
		{
			if (target.Symbol.IsGenericType)
			{
				List<(string identifier, string replacement)> replacements = GetTypeParamterReplacements(target.Symbol);

				if (target.HandleSpecialMembers)
				{
					return (member, symbol, generateInheritdoc) =>
					{
						HandleSpecialMemberTypes(ref member, type, target.Symbol);
						ReplaceAndGenerate(member, symbol, generateInheritdoc);
					};
				}
				else
				{
					return ReplaceAndGenerate;
				}

				void ReplaceAndGenerate(CSharpSyntaxNode replaced, ISymbol symbol, GenerateInheritdoc generateInheritdoc)
				{
					foreach ((string identifier, string replacement) in replacements)
					{
						replacer.Identifier = identifier;
						replacer.Replacement = replacement;

						replaced = (CSharpSyntaxNode)replacer.Visit(replaced);
					}

					WriteGeneratedMember(replaced, symbol, generateInheritdoc);
				}
			}

			if (target.HandleSpecialMembers)
			{
				return (member, symbol, generateInheritdoc) =>
				{
					HandleSpecialMemberTypes(ref member, type, target.Symbol);
					WriteGeneratedMember(member, symbol, generateInheritdoc);
				};
			}

			return WriteGeneratedMember;
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

		private static void HandleSpecialMemberTypes(ref CSharpSyntaxNode member, CopyFromTypeData type, INamedTypeSymbol target)
		{
			switch (member)
			{
				case ConstructorDeclarationSyntax ctor:
					member = ctor.WithIdentifier(SyntaxFactory.Identifier(type.Name).WithTriviaFrom(ctor.Identifier));
					break;

				case DestructorDeclarationSyntax dtor:
					member = dtor.WithIdentifier(SyntaxFactory.Identifier(type.Name).WithTriviaFrom(dtor.Identifier));
					break;

				case ConversionOperatorDeclarationSyntax conversion:

					if(SymbolEqualityComparer.Default.Equals(type.SemanticModel.GetSymbolInfo(conversion.Type).Symbol, target))
					{
						conversion = conversion.WithType(GetNameSyntax(((SimpleNameSyntax)conversion.Type).Identifier));
					}

					member = conversion.WithParameterList(conversion.ParameterList.WithParameters(SyntaxFactory.SeparatedList(UpdateParameters(conversion.ParameterList.Parameters))));
					break;

				case OperatorDeclarationSyntax op:

					if (SymbolEqualityComparer.Default.Equals(type.SemanticModel.GetSymbolInfo(op.ReturnType).Symbol, target))
					{
						op = op.WithReturnType(GetNameSyntax(((SimpleNameSyntax)op.ReturnType).Identifier));
					}

					member = op.WithParameterList(op.ParameterList.WithParameters(SyntaxFactory.SeparatedList(UpdateParameters(op.ParameterList.Parameters))));
					break;
			}

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

		private static bool TryGetMutlipleMembers(
			CSharpSyntaxNode member,
			SemanticModel semanticModel,
			out ISymbol? symbol,
			[NotNullWhen(true)]out List<(ISymbol original, MemberDeclarationSyntax generated)>? members)
		{
			if (member is not BaseFieldDeclarationSyntax field)
			{
				members = default;
				symbol = semanticModel.GetDeclaredSymbol(member);
				return false;
			}

			SeparatedSyntaxList<VariableDeclaratorSyntax> variables = field.Declaration.Variables;

			if (!variables.Any())
			{
				members = default;
				symbol = default;
				return false;
			}

			if (variables.Count == 1)
			{
				members = default;
				symbol = semanticModel.GetDeclaredSymbol(variables[0]);
				return false;
			}

			members = new(variables.Count);

			if(field is EventFieldDeclarationSyntax)
			{
				foreach (VariableDeclaratorSyntax variable in variables)
				{
					if(semanticModel.GetDeclaredSymbol(variable) is not ISymbol s)
					{
						continue;
					}

					members.Add((s, SyntaxFactory.EventFieldDeclaration(
						field.AttributeLists,
						field.Modifiers,
						SyntaxFactory.VariableDeclaration(field.Declaration.Type, SyntaxFactory.SingletonSeparatedList(variable)))));
				}
			}
			else
			{
				foreach (VariableDeclaratorSyntax variable in variables)
				{
					if (semanticModel.GetDeclaredSymbol(variable) is not ISymbol s)
					{
						continue;
					}

					members.Add((s, SyntaxFactory.FieldDeclaration(
						field.AttributeLists,
						field.Modifiers,
						SyntaxFactory.VariableDeclaration(field.Declaration.Type, SyntaxFactory.SingletonSeparatedList(variable)))));
				}
			}

			symbol = default;
			return true;
		}
	}
}
