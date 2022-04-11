// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Analyzes members marked with either <c>Durian.CopyFromTypeAttribute</c> or <c>Durian.CopyFromMethodAttribute</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class CopyFromAnalyzer : DurianAnalyzer<CopyFromCompilationData>
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0201_ContainingTypeMustBePartial,
			DUR0202_MemberMustBePartial,
			DUR0203_MemberCannotBeResolved,
			DUR0204_WrongTargetMemberKind,
			DUR0205_ImplementationNotAccessible,
			DUR0206_EquivalentTarget,
			DUR0207_MemberCannotCopyFromItselfOrItsParent,
			DUR0208_MemberConflict,
			DUR0209_CannotCopyFromMethodWithoutImplementation,
			DUR0210_InvalidMethodKind,
			DUR0211_MethodAlreadyHasImplementation,
			DUR0212_TargetDoesNotHaveReturnType,
			DUR0213_TargetCannotHaveReturnType,
			DUR0214_InvalidPatternAttributeSpecified,
			DUR0215_RedundantPatternAttribute,
			DUR0216_EquivalentPatternAttribute,
			DUR0217_TypeParameterIsNotValid,
			DUR0218_UnknownPartialPartName,
			DUR0219_PatternOnDifferentDeclaration,
			DUR0220_UsingAlreadySpecified
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromAnalyzer"/> class.
		/// </summary>
		public CopyFromAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, CopyFromCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(c => AnalyzeAttributeSyntax(c, compilation), SyntaxKind.Attribute);
		}

		/// <inheritdoc/>
		protected override CopyFromCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
		{
			return new CopyFromCompilationData(compilation);
		}

		private static void AnalyzeAttributeSyntax(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation)
		{
			if (context.Node is not AttributeSyntax attr || attr.ArgumentList is null || attr.Parent?.Parent is not CSharpSyntaxNode member)
			{
				return;
			}

			if (member is
				not MemberDeclarationSyntax and
				not AccessorDeclarationSyntax and
				not LocalFunctionStatementSyntax and
				not LambdaExpressionSyntax
			)
			{
				return;
			}

			if (context.SemanticModel.GetSymbolInfo(attr).Symbol is not IMethodSymbol attributeSymbol)
			{
				return;
			}

			if (SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilation.CopyFromTypeAttribute))
			{
				AnalyzeTypeAttribute(context, compilation, attr, member);
			}
			else if (SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilation.CopyFromMethodAttribute))
			{
				AnalyzeMethodAttribute(context, compilation, member);
			}
			else if (SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilation.PatternAttribute))
			{
				AnalyzePatternAttribute(context, compilation, attr, member);
			}
		}

		private static TargetData CreateTargetData(AttributeData attribute, INamedTypeSymbol target)
		{
			return CreateTargetData(attribute, target, default, default);
		}

		private static TargetData CreateTargetData(AttributeData attribute, INamedTypeSymbol target, TypeDeclarationSyntax? partialPart, string? partialPartName)
		{
			int order = GetOrder(attribute);
			string[] usings = GetUsings(attribute);
			bool copyUsings = ShouldCopyUsings(attribute);
			bool handleSpecialMembers = ShouldHandleSpecialMembers(attribute);

			return new(target, order, partialPart, partialPartName, copyUsings, usings, handleSpecialMembers);
		}

		private static AttributeData[] GetAttributes(ImmutableArray<AttributeData> attributes, INamedTypeSymbol attrSymbol)
		{
			return attributes.Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol)).ToArray();
		}

		private static int GetOrder(AttributeData attribute)
		{
			return attribute.GetNamedArgumentValue<int>(CopyFromTypeAttributeProvider.Order);
		}

		private static bool HasValidTypeArguments(
			ImmutableArray<ITypeParameterSymbol> typeParameters,
			ImmutableArray<ITypeSymbol> typeArguments,
			out List<ITypeSymbol>? invalidArguments
		)
		{
			if (typeParameters.Length == 0)
			{
				invalidArguments = default;
				return true;
			}

			if (typeParameters.Length != typeArguments.Length)
			{
				invalidArguments = default;
				return false;
			}

			List<ITypeSymbol> invalid = new(typeArguments.Length);

			for (int i = 0; i < typeParameters.Length; i++)
			{
				ITypeSymbol arg = typeArguments[i];
				ITypeParameterSymbol param = typeParameters[i];

				if (arg.Name != param.Name && !arg.IsValidForTypeParameter(param))
				{
					invalid.Add(arg);
				}
			}

			if (invalid.Count > 0)
			{
				invalidArguments = invalid;
				return false;
			}

			invalidArguments = default;
			return true;
		}
	}
}
