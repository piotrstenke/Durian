// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.CopyFrom.Types;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SyntaxVisitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using GenerateAction = System.Action<Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode, Microsoft.CodeAnalysis.ISymbol, Durian.Analysis.CopyFrom.CopyFromPassContext, Durian.Analysis.CodeGeneration.GenerateDocumentation>;

namespace Durian.Analysis.CopyFrom
{
	public sealed partial class CopyFromGenerator
	{
		private static void GenerateMembers(
			TargetTypeData target,
			CopyFromPassContext context,
			GenerateAction generateAction,
			SyntaxList<MemberDeclarationSyntax> members,
			SemanticModel semanticModel
		)
		{
			for (int j = 0; j < members.Count; j++)
			{
				MemberDeclarationSyntax member = members[j];

				if (TryGetMutlipleMembers(
					member,
					semanticModel,
					context,
					out ISymbol? symbol,
					out List<(ISymbol original, MemberDeclarationSyntax generated)>? fields)
				)
				{
					GenerateDocumentation inheridoc = target.Symbol.HasInheritableDocumentation() ? GenerateDocumentation.Always : GenerateDocumentation.Never;

					foreach ((ISymbol original, MemberDeclarationSyntax generated) in fields)
					{
						generateAction(generated, original, context, inheridoc);
					}
				}
				else if (symbol is not null)
				{
					member = SkipGeneratorAttributes(member, semanticModel, context);
					generateAction(member, symbol, context, GenerateDocumentation.WhenPossible);
				}
			}
		}

		private static SemanticModel GetCurrentSemanticModel(
			CopyFromTypeData type,
			TypeDeclarationSyntax currentDeclaration,
			ref Dictionary<SyntaxTree, SemanticModel>? semanticModelCache
		)
		{
			if (currentDeclaration.SyntaxTree == type.Declaration.SyntaxTree)
			{
				return type.SemanticModel;
			}

			SemanticModel semanticModel;

			if (semanticModelCache is null)
			{
				semanticModel = type.ParentCompilation.Compilation.GetSemanticModel(currentDeclaration.SyntaxTree);

				semanticModelCache = new()
				{
					[type.Declaration.SyntaxTree] = type.SemanticModel,
					[currentDeclaration.SyntaxTree] = semanticModel
				};
			}
			else if (!semanticModelCache.TryGetValue(currentDeclaration.SyntaxTree, out semanticModel))
			{
				semanticModel = type.ParentCompilation.Compilation.GetSemanticModel(currentDeclaration.SyntaxTree);
				semanticModelCache[currentDeclaration.SyntaxTree] = semanticModel;
			}

			return semanticModel;
		}

		private static TypeDeclarationSyntax[] GetPartialDeclarations(TargetTypeData target)
		{
			if (target.PartialPart is not null)
			{
				return new TypeDeclarationSyntax[] { target.PartialPart };
			}

			return target.Symbol.GetPartialDeclarations<TypeDeclarationSyntax>().ToArray();
		}

		private static List<(string identifier, string replacement)> GetTypeParameterReplacements(INamedTypeSymbol type)
		{
			List<(string, string)> list = new();

			foreach (INamedTypeSymbol parent in type.GetContainingTypes(true, ReturnOrder.Parent))
			{
				if (!parent.IsGenericType)
				{
					break;
				}

				if (parent.IsUnboundGenericType)
				{
					continue;
				}

				ImmutableArray<ITypeParameterSymbol> parameters = parent.TypeParameters;
				ImmutableArray<ITypeSymbol> arguments = parent.TypeArguments;

				for (int i = 0; i < parameters.Length; i++)
				{
					ITypeParameterSymbol parameter = parameters[i];
					ITypeSymbol argument = arguments[i];

					if (parameter.Name != argument.Name && !SymbolEqualityComparer.Default.Equals(parameter, argument))
					{
						list.Add((parameter.Name, argument.GetTypeKeyword() ?? argument.GetGenericName(true)));
					}
				}
			}

			return list;
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
				{
					TypeSyntax? syntax = default;

					if (SymbolEqualityComparer.Default.Equals(type.SemanticModel.GetSymbolInfo(conversion.Type).Symbol, target))
					{
						InitTypeSyntax(ref syntax);
						conversion = conversion.WithType(GetNameSyntax(syntax, ((SimpleNameSyntax)conversion.Type).Identifier));
					}

					member = conversion.WithParameterList(conversion.ParameterList.WithParameters(SyntaxFactory.SeparatedList(UpdateParameters(ref syntax, conversion.ParameterList.Parameters))));
					break;
				}

				case OperatorDeclarationSyntax op:
				{
					TypeSyntax? syntax = default;

					if (SymbolEqualityComparer.Default.Equals(type.SemanticModel.GetSymbolInfo(op.ReturnType).Symbol, target))
					{
						InitTypeSyntax(ref syntax);
						op = op.WithReturnType(GetNameSyntax(syntax, ((SimpleNameSyntax)op.ReturnType).Identifier));
					}

					member = op.WithParameterList(op.ParameterList.WithParameters(SyntaxFactory.SeparatedList(UpdateParameters(ref syntax, op.ParameterList.Parameters))));
					break;
				}
			}

			void InitTypeSyntax([NotNull] ref TypeSyntax? syntax)
			{
				syntax ??= type.Symbol.CreateTypeSyntax();
			}

			List<ParameterSyntax> UpdateParameters(ref TypeSyntax? syntax, SeparatedSyntaxList<ParameterSyntax> parameters)
			{
				List<ParameterSyntax> newParameters = new(parameters.Count);

				for (int i = 0; i < parameters.Count; i++)
				{
					ParameterSyntax parameter = parameters[i];

					if (parameter.Type is not null && SymbolEqualityComparer.Default.Equals(type.SemanticModel.GetSymbolInfo(parameter.Type).Symbol, target))
					{
						InitTypeSyntax(ref syntax);
						parameter = parameter.WithType(GetNameSyntax(syntax, ((SimpleNameSyntax)parameter.Type).Identifier));
					}

					newParameters.Add(parameter);
				}

				return newParameters;
			}

			static SimpleNameSyntax GetNameSyntax(TypeSyntax syntax, SyntaxToken triviaFrom)
			{
				return syntax switch
				{
					IdentifierNameSyntax identifier => identifier.WithIdentifier(identifier.Identifier.WithTriviaFrom(triviaFrom)),
					GenericNameSyntax generic => generic.WithIdentifier(generic.Identifier.WithTriviaFrom(triviaFrom)),
					NullableTypeSyntax nullable => GetNameSyntax(nullable.ElementType, triviaFrom),
					_ => (SimpleNameSyntax)syntax
				};
			}
		}

		private static void SortByOrder(TargetTypeData[] targets)
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
			MemberDeclarationSyntax member,
			SemanticModel semanticModel,
			CopyFromPassContext context,
			out ISymbol? symbol,
			[NotNullWhen(true)] out List<(ISymbol original, MemberDeclarationSyntax generated)>? members
		)
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

			BaseFieldDeclarationSyntax newMember = (BaseFieldDeclarationSyntax)SkipGeneratorAttributes(member, semanticModel, context);

			if (field is EventFieldDeclarationSyntax)
			{
				foreach (VariableDeclaratorSyntax variable in variables)
				{
					if (semanticModel.GetDeclaredSymbol(variable) is not ISymbol s)
					{
						continue;
					}

					members.Add((s, SyntaxFactory.EventFieldDeclaration(
						newMember.AttributeLists,
						newMember.Modifiers,
						SyntaxFactory.VariableDeclaration(newMember.Declaration.Type, SyntaxFactory.SingletonSeparatedList(variable))).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)));
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
						newMember.AttributeLists,
						newMember.Modifiers,
						SyntaxFactory.VariableDeclaration(newMember.Declaration.Type, SyntaxFactory.SingletonSeparatedList(variable))).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed)));
				}
			}

			symbol = default;
			return true;
		}

		private bool GenerateDeclarations(
			CopyFromTypeData type,
			TargetTypeData target,
			TypeDeclarationSyntax[] partialDeclarations,
			string currentName,
			string keyword,
			TypeParameterReplacer replacer,
			CopyFromPassContext context
		)
		{
			bool isGenerated = false;
			string partialName = currentName;

			GenerateAction generateAction = GetGenerationMethod(type, target, replacer);

			Dictionary<SyntaxTree, SemanticModel>? semanticModelCache = default;

			bool hasDocumentation = false;

			for (int i = 0; i < partialDeclarations.Length; i++)
			{
				TypeDeclarationSyntax partial = partialDeclarations[i];

				SyntaxList<MemberDeclarationSyntax> members = partial.Members;

				if (IncludeAdditionalNodes(
					type,
					target,
					partial,
					context,
					keyword,
					ref hasDocumentation,
					semanticModelCache,
					out SemanticModel? semanticModel
				))
				{
					if (members.Any())
					{
						GenerateMembers(target, context, generateAction, members, semanticModel);
					}
				}
				else if (members.Any())
				{
					WriteDeclarationLead(context.CodeBuilder, type, target.Usings);
					context.CodeBuilder.Indent();
					context.CodeBuilder.Declaration(type.Symbol);
					semanticModel = GetCurrentSemanticModel(type, partial, ref semanticModelCache);
					GenerateMembers(target, context, generateAction, members, semanticModel);
				}
				else
				{
					continue;
				}

				context.CodeBuilder.EndAllBlocks();

				AddSourceWithOriginal(type.Declaration, partialName, context);
				isGenerated = true;
				partialName = currentName + $"_{i + 1}";
			}

			return isGenerated;
		}

		private bool GenerateType(CopyFromTypeData type, string hintName, CopyFromPassContext context)
		{
			TargetTypeData[] targets = type.Targets;
			SortByOrder(targets);

			bool generated = false;
			string keyword = type.Declaration.GetKeyword()!;

			string currentName = hintName;

			TypeParameterReplacer replacer = new();

			for (int i = 0; i < targets.Length; i++)
			{
				TargetTypeData target = targets[i];

				TypeDeclarationSyntax[] partialDeclarations = GetPartialDeclarations(target);

				if (partialDeclarations.Length == 0)
				{
					continue;
				}

				currentName = partialDeclarations.Length > 1 ? currentName + "_partial" : currentName;

				if (GenerateDeclarations(type, target, partialDeclarations, currentName, keyword, replacer, context))
				{
					generated = true;
				}

				currentName = hintName + $"_{i + 1}";
			}

			return generated;
		}

		private GenerateAction GetGenerationMethod(CopyFromTypeData type, TargetTypeData target, TypeParameterReplacer replacer)
		{
			if (target.Symbol.IsGenericType)
			{
				List<(string identifier, string replacement)> replacements = GetTypeParameterReplacements(target.Symbol);

				if (target.HandleSpecialMembers)
				{
					return (member, symbol, context, applyInheritdoc) =>
					{
						HandleSpecialMemberTypes(ref member, type, target.Symbol);
						ReplaceAndGenerate(member, symbol, context, applyInheritdoc);
					};
				}
				else
				{
					return ReplaceAndGenerate;
				}

				void ReplaceAndGenerate(CSharpSyntaxNode replaced, ISymbol symbol, CopyFromPassContext context, GenerateDocumentation applyInheritdoc)
				{
					foreach ((string identifier, string replacement) in replacements)
					{
						replacer.Identifier = identifier;
						replacer.Replacement = replacement;

						replaced = (CSharpSyntaxNode)replacer.Visit(replaced);
					}

					WriteGeneratedMember(type, replaced, symbol, context, applyInheritdoc);
				}
			}

			if (target.HandleSpecialMembers)
			{
				return (member, symbol, context, applyInheritdoc) =>
				{
					HandleSpecialMemberTypes(ref member, type, target.Symbol);
					WriteGeneratedMember(type, member, symbol, context, applyInheritdoc);
				};
			}

			return (member, symbol, context, applyInheritdoc) => WriteGeneratedMember(type, member, symbol, context, applyInheritdoc);
		}

		private bool IncludeAdditionalNodes(
			CopyFromTypeData type,
			TargetTypeData target,
			TypeDeclarationSyntax declaration,
			CopyFromPassContext context,
			string keyword,
			ref bool hasDocumentation,
			Dictionary<SyntaxTree, SemanticModel>? semanticModelCache,
			[NotNullWhen(true)] out SemanticModel? semanticModel
		)
		{
			string? name = default;
			bool hasColon = false;
			bool hasLead = false;

			semanticModel = default;

			if (target.AdditionalNodes.HasFlag(AdditionalNodes.Documentation) &&
				!hasDocumentation &&
				declaration.GetLeadingTrivia().Where(t => t.HasStructure).Select(t => t.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault() is DocumentationCommentTriviaSyntax doc
			)
			{
				ApplyLead(ref semanticModel);
				hasDocumentation = true;

				context.CodeBuilder.WriteLine(TryApplyPattern(type, context, doc.ToFullString()));
			}

			if (target.AdditionalNodes.HasFlag(AdditionalNodes.Attributes) && declaration.AttributeLists.Any())
			{
				ApplyLead(ref semanticModel);
				context.CodeBuilder.Write(HandleAttributeText(type, SkipGeneratorAttributes(declaration, semanticModel, context), context));
			}

			if (declaration.BaseList is not null && declaration.BaseList.Types.Any())
			{
				bool? hasBaseType = null;
				SemanticModel? currentModel = default;

				if (target.AdditionalNodes.HasFlag(AdditionalNodes.BaseType))
				{
					currentModel = GetCurrentSemanticModel(type, declaration, ref semanticModelCache);

					TypeSyntax firstType = declaration.BaseList.Types[0].Type;

					if (currentModel.GetTypeInfo(firstType).Type is INamedTypeSymbol t && t.TypeKind == TypeKind.Class)
					{
						hasBaseType = true;

						semanticModel = currentModel;
						ApplyLead(ref semanticModel);

						name += TryApplyPattern(type, context, " : " + firstType.ToString());
						hasColon = true;
					}
				}

				if (target.AdditionalNodes.HasFlag(AdditionalNodes.BaseInterfaces) && HandleBaseInterfaces(ref name, hasBaseType, ref currentModel))
				{
					semanticModel = currentModel;
				}
			}

			if (target.AdditionalNodes.HasFlag(AdditionalNodes.Constraints) && declaration.ConstraintClauses.Any())
			{
				ApplyLead(ref semanticModel);
				name += TryApplyPattern(type, context, string.Join(" ", declaration.ConstraintClauses));
			}

			if (hasLead)
			{
				context.CodeBuilder.Indent();
				context.CodeBuilder.Declaration($"partial {keyword} {name}");
			}

			return hasLead;

#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
			void ApplyLead([NotNull] ref SemanticModel? semanticModel)
			{
				if (!hasLead)
				{
					name = type.Symbol.GetGenericName();
					semanticModel ??= GetCurrentSemanticModel(type, declaration, ref semanticModelCache);
					WriteDeclarationLead(context.CodeBuilder, type, target.Usings);
					hasLead = true;
				}
			}
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.

			bool HandleBaseInterfaces(ref string? name, bool? hasBaseType, ref SemanticModel? currentModel)
			{
				currentModel ??= GetCurrentSemanticModel(type, declaration, ref semanticModelCache);

				TypeSyntax firstType = declaration.BaseList.Types[0].Type;

				bool skipFirst;

				if (hasBaseType == true)
				{
					skipFirst = true;
				}
				else if (!hasBaseType.HasValue)
				{
					skipFirst = currentModel.GetTypeInfo(firstType).Type is not INamedTypeSymbol t || t.TypeKind != TypeKind.Interface;
				}
				else
				{
					skipFirst = false;
				}

				IEnumerable<BaseTypeSyntax> interfaces;

				if (skipFirst)
				{
					if (declaration.BaseList.Types.Count < 2)
					{
						return false;
					}

					interfaces = declaration.BaseList.Types.Skip(1);
				}
				else
				{
					interfaces = declaration.BaseList.Types;
				}

				ApplyLead(ref currentModel);

				if (hasColon)
				{
					name += TryApplyPattern(type, context, string.Join(", ", interfaces));
				}
				else
				{
					name += TryApplyPattern(type, context, " : " + string.Join(", ", interfaces));
				}

				return true;
			}
		}
	}
}
