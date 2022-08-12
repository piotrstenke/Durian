// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.InterfaceTargets.InterfaceTargetsDiagnostics;

namespace Durian.Analysis.InterfaceTargets
{
	/// <summary>
	/// Analyzes types that are marked with the <c>Durian.InterfaceTargetsAttribute</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class InterfaceTargetsAnalyzer : DurianAnalyzer
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind,
			DUR0402_InterfaceCannotBeBaseOfAnotherInterface,
			DUR0403_InterfaceIsNotDirectlyAccessible,
			DUR0404_InvalidConstraint
		);

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context)
		{
			context.RegisterCompilationStartAction(context =>
			{
				INamedTypeSymbol? targetsAttribute = context.Compilation.GetTypeByMetadataName(InterfaceTargetsAttributeProvider.FullName);

				if (targetsAttribute is null)
				{
					return;
				}

				context.RegisterSyntaxNodeAction(context => AnalyzeBaseType(context, targetsAttribute), SyntaxKind.SimpleBaseType);
				context.RegisterSyntaxNodeAction(context => AnalyzeConstraint(context, targetsAttribute), SyntaxKind.TypeParameterConstraintClause);
			});
		}

		private static void AnalyzeBaseType(SyntaxNodeAnalysisContext context, INamedTypeSymbol targetsAttribute)
		{
			if (context.Node is not SimpleBaseTypeSyntax s)
			{
				return;
			}

			if (context.ContainingSymbol is not INamedTypeSymbol symbol || !(symbol.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Interface))
			{
				return;
			}

			if (!TryGetIntfTargets(context.SemanticModel, targetsAttribute, s.Type, out IntfTargets targets))
			{
				return;
			}

			HandleBaseTypeTarget(context, targets, symbol);
		}

		private static void AnalyzeConstraint(SyntaxNodeAnalysisContext context, INamedTypeSymbol targetsAttribute)
		{
			if (context.Node is not TypeParameterConstraintClauseSyntax clause)
			{
				return;
			}

			SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = clause.Constraints;

			for (int i = 0; i < constraints.Count; i++)
			{
				if (constraints[i] is TypeConstraintSyntax type && TryGetIntfTargets(context.SemanticModel, targetsAttribute, type.Type, out IntfTargets targets))
				{
					HandleConstraintTarget(context, type.Type, targets, constraints, i);
					break;
				}
			}
		}

		private static void HandleBaseTypeTarget(SyntaxNodeAnalysisContext context, IntfTargets targets, INamedTypeSymbol intf)
		{
			if (targets == IntfTargets.ReflectionOnly)
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0403_InterfaceIsNotDirectlyAccessible, context.Node.GetLocation(), context.ContainingSymbol, intf));
				return;
			}

			switch (intf.TypeKind)
			{
				case TypeKind.Interface:

					if (!targets.HasFlag(IntfTargets.Interface))
					{
						context.ReportDiagnostic(Diagnostic.Create(DUR0402_InterfaceCannotBeBaseOfAnotherInterface, context.Node.GetLocation(), context.ContainingSymbol, intf));
					}

					break;

				case TypeKind.Struct:

					if (intf.IsRecord)
					{
						if (!targets.HasFlag(IntfTargets.RecordStruct))
						{
							ReportCannotBeImplemented(context, intf, "a record struct");
						}
					}
					else
					{
						if (!targets.HasFlag(IntfTargets.Struct))
						{
							ReportCannotBeImplemented(context, intf, "a struct");
						}
					}

					break;

				case TypeKind.Class:

					if (intf.IsRecord)
					{
						if (!targets.HasFlag(IntfTargets.RecordClass))
						{
							ReportCannotBeImplemented(context, intf, "a record class");
						}
					}
					else
					{
						if (!targets.HasFlag(IntfTargets.Class))
						{
							ReportCannotBeImplemented(context, intf, "a class");
						}
					}

					break;
			}
		}

		private static void HandleConstraintTarget(
			SyntaxNodeAnalysisContext context,
			TypeSyntax type,
			IntfTargets targets,
			SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints,
			int targetIndex
		)
		{
			int start;

			if (HandleNonTypeConstraint(constraints[0], targets, out DiagnosticDescriptor? diag))
			{
				if (diag is not null)
				{
					ReportDiagnostic(diag, constraints[0]);
				}

				start = 1;
			}
			else
			{
				start = 0;
			}

			for (int i = start; i < targetIndex; i++)
			{
				if (constraints[i] is TypeConstraintSyntax typeConstraint && !HandleTypeConstraint(context.SemanticModel, typeConstraint, targets))
				{
					ReportDiagnostic(DUR0404_InvalidConstraint, typeConstraint);
				}
			}

			for (int i = targetIndex + 1; i < constraints.Count; i++)
			{
				if (constraints[i] is TypeConstraintSyntax typeConstraint && !HandleTypeConstraint(context.SemanticModel, typeConstraint, targets))
				{
					ReportDiagnostic(DUR0404_InvalidConstraint, typeConstraint);
				}
			}

			void ReportDiagnostic(DiagnosticDescriptor diag, TypeParameterConstraintSyntax constraint)
			{
				context.ReportDiagnostic(Diagnostic.Create(diag, constraint.GetLocation(), type, constraint));
			}
		}

		private static bool HandleTypeConstraint(
			SemanticModel semanticModel,
			TypeConstraintSyntax constraint,
			IntfTargets targets
		)
		{
			if (semanticModel.GetSymbolInfo(constraint.Type).Symbol is not INamedTypeSymbol target)
			{
				return false;
			}

			switch (target.TypeKind)
			{
				case TypeKind.Class:

					if(targets == IntfTargets.RecordClass)
					{
						return target.IsRecord;
					}

					if(targets == IntfTargets.ReflectionOnly)
					{
						return false;
					}

					return !(targets is IntfTargets.Struct or IntfTargets.RecordStruct);

				case TypeKind.TypeParameter:
					return true;

				default:
					return true;
			}
		}

		private static bool HandleNonTypeConstraint(TypeParameterConstraintSyntax constraint, IntfTargets targets, out DiagnosticDescriptor? diagnostic)
		{
			switch (constraint)
			{
				case ClassOrStructConstraintSyntax @class when @class.IsClass():

					diagnostic = targets.HasFlag(IntfTargets.Class)
						? default
						: DUR0404_InvalidConstraint;

					return true;

				case ClassOrStructConstraintSyntax @struct when @struct.IsStruct():

					diagnostic = targets.HasFlag(IntfTargets.Struct)
						? default
						: DUR0404_InvalidConstraint;

					return true;

				case ClassOrStructConstraintSyntax:
					diagnostic = default;
					return true;

				case TypeConstraintSyntax unmanaged when unmanaged.IsUnmanagedConstraint():

					diagnostic = targets.HasFlag(IntfTargets.Struct)
						? default
						: DUR0404_InvalidConstraint;

					return true;

				case TypeConstraintSyntax notnull when notnull.IsNotNullConstraint():
					diagnostic = default;
					return true;
			}

			diagnostic = default;
			return false;
		}

		private static void ReportCannotBeImplemented(SyntaxNodeAnalysisContext context, INamedTypeSymbol intf, string memberType)
		{
			context.ReportDiagnostic(Diagnostic.Create(DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind, context.Node.GetLocation(), context.ContainingSymbol, intf, memberType));
		}

		private static bool TryGetIntfTargets(
			SemanticModel semanticModel,
			INamedTypeSymbol targetsAttribute,
			SyntaxNode node,
			out IntfTargets targets
		)
		{
			if (semanticModel.GetSymbolInfo(node).Symbol is not INamedTypeSymbol target || target.TypeKind != TypeKind.Interface)
			{
				targets = default;
				return false;
			}

			if (target.GetAttribute(targetsAttribute) is not AttributeData attr || !attr.TryGetConstructorArgumentValue(0, out int value))
			{
				targets = default;
				return false;
			}

			targets = (IntfTargets)value;
			return true;
		}
	}
}
