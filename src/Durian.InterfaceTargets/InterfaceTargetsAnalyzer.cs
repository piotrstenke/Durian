// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

			if (!TryGetIntfTargets(context.SemanticModel, targetsAttribute, s.Type, out INamedTypeSymbol? intf, out IntfTargets targets))
			{
				return;
			}

			HandleBaseTypeTarget(context, targets, symbol, intf);
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
				if (constraints[i] is TypeConstraintSyntax type && TryGetIntfTargets(context.SemanticModel, targetsAttribute, type.Type, out INamedTypeSymbol? intf, out IntfTargets targets))
				{
					HandleConstraintTarget(context, intf, targets, targetsAttribute, constraints, i);
					break;
				}
			}
		}

		private static void HandleBaseTypeTarget(SyntaxNodeAnalysisContext context, IntfTargets targets, INamedTypeSymbol type, INamedTypeSymbol intf)
		{
			if (targets == IntfTargets.ReflectionOnly)
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0403_InterfaceIsNotDirectlyAccessible, context.Node.GetLocation(), context.ContainingSymbol, intf));
				return;
			}

			switch (type.TypeKind)
			{
				case TypeKind.Interface:

					if (!targets.HasFlag(IntfTargets.Interface))
					{
						context.ReportDiagnostic(Diagnostic.Create(DUR0402_InterfaceCannotBeBaseOfAnotherInterface, context.Node.GetLocation(), context.ContainingSymbol, intf));
					}

					break;

				case TypeKind.Struct:

					if (type.IsRecord)
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

					if (type.IsRecord)
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
			INamedTypeSymbol intf,
			IntfTargets targets,
			INamedTypeSymbol targetsAttribute,
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
				HandleConstraint(i);
			}

			for (int i = targetIndex + 1; i < constraints.Count; i++)
			{
				HandleConstraint(i);
			}

			void HandleConstraint(int i)
			{
				if (constraints[i] is TypeConstraintSyntax typeConstraint && !HandleTypeConstraint(context.SemanticModel, typeConstraint, targets, targetsAttribute))
				{
					ReportDiagnostic(DUR0404_InvalidConstraint, typeConstraint);
				}
			}

			void ReportDiagnostic(DiagnosticDescriptor diag, TypeParameterConstraintSyntax constraint)
			{
				context.ReportDiagnostic(Diagnostic.Create(diag, constraint.Parent!.GetLocation(), context.ContainingSymbol, intf, constraint));
			}
		}

		private static bool HandleInterfaceConstraint(IntfTargets targets, INamedTypeSymbol targetsAttribute, ITypeSymbol target)
		{
			if (!TryGetIntfTargets((target as INamedTypeSymbol)!, targetsAttribute, out IntfTargets otherTargets))
			{
				return false;
			}

			if (otherTargets == IntfTargets.ReflectionOnly)
			{
				return true;
			}

			if (!CheckTarget(IntfTargets.Class))
			{
				return false;
			}

			if (!CheckTarget(IntfTargets.RecordClass))
			{
				return false;
			}

			if (!CheckTarget(IntfTargets.Struct))
			{
				return false;
			}

			if (!CheckTarget(IntfTargets.RecordStruct))
			{
				return false;
			}

			return true;

			bool CheckTarget(IntfTargets target)
			{
				if (targets.HasFlag(target))
				{
					return otherTargets.HasFlag(target) || otherTargets.HasFlag(IntfTargets.Interface);
				}

				return true;
			}
		}

		private static bool HandleNonTypeConstraint(TypeParameterConstraintSyntax constraint, IntfTargets targets, out DiagnosticDescriptor? diagnostic)
		{
			switch (constraint)
			{
				case ClassOrStructConstraintSyntax @class when @class.IsClass():

					if (targets == IntfTargets.ReflectionOnly)
					{
						diagnostic = default;
						return true;
					}

					diagnostic = targets.HasFlag(IntfTargets.Class) || targets.HasFlag(IntfTargets.Interface) || targets.HasFlag(IntfTargets.RecordClass)
						? default
						: DUR0404_InvalidConstraint;

					return true;

				case ClassOrStructConstraintSyntax @struct when @struct.IsStruct():

					if (targets == IntfTargets.ReflectionOnly)
					{
						diagnostic = default;
						return true;
					}

					diagnostic = targets.HasFlag(IntfTargets.Struct) || targets.HasFlag(IntfTargets.Interface) || targets.HasFlag(IntfTargets.RecordStruct)
						? default
						: DUR0404_InvalidConstraint;

					return true;

				case ClassOrStructConstraintSyntax:
					diagnostic = default;
					return true;

				case TypeConstraintSyntax unmanaged when unmanaged.IsUnmanagedConstraint():

					if (targets == IntfTargets.ReflectionOnly)
					{
						diagnostic = default;
						return true;
					}

					diagnostic = targets.HasFlag(IntfTargets.Struct) || targets.HasFlag(IntfTargets.Interface)
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

		private static bool HandleTypeConstraint(
			SemanticModel semanticModel,
			TypeConstraintSyntax constraint,
			IntfTargets targets,
			INamedTypeSymbol targetsAttribute
		)
		{
			if (semanticModel.GetSymbolInfo(constraint.Type).Symbol is not ITypeSymbol target)
			{
				return false;
			}

			switch (target.TypeKind)
			{
				case TypeKind.Class:

					if (targets == IntfTargets.ReflectionOnly)
					{
						return false;
					}

					if (targets.HasFlag(IntfTargets.RecordClass))
					{
						return target.IsRecord;
					}

					return !targets.HasFlag(IntfTargets.Struct) && !targets.HasFlag(IntfTargets.RecordStruct);

				case TypeKind.Interface:

					if (targets == IntfTargets.ReflectionOnly)
					{
						return true;
					}

					return HandleInterfaceConstraint(targets, targetsAttribute, target);

				case TypeKind.TypeParameter:

					ITypeParameterSymbol typeParameter = (target as ITypeParameterSymbol)!;

					if (targets == IntfTargets.ReflectionOnly)
					{
						return CheckTypeParameter(typeParameter);
					}

					return HandleTypeParameterConstraint(targets, typeParameter);

				default:
					return true;
			}

			static bool CheckTypeParameter(ITypeParameterSymbol typeParameter)
			{
				return !typeParameter.ConstraintTypes.Any(t =>
				{
					if (t.TypeKind == TypeKind.Class)
					{
						return true;
					}

					if (t.TypeKind == TypeKind.TypeParameter)
					{
						return !CheckTypeParameter((t as ITypeParameterSymbol)!);
					}

					return false;
				});
			}
		}

		private static bool HandleTypeParameterConstraint(IntfTargets targets, ITypeParameterSymbol typeParameter)
		{
			if (typeParameter.HasReferenceTypeConstraint)
			{
				return targets.HasFlag(IntfTargets.Class) || targets.HasFlag(IntfTargets.RecordClass) || targets.HasFlag(IntfTargets.Interface);
			}

			if (typeParameter.HasValueTypeConstraint)
			{
				return targets.HasFlag(IntfTargets.Struct) || targets.HasFlag(IntfTargets.RecordStruct) || targets.HasFlag(IntfTargets.Interface);
			}

			if (typeParameter.HasUnmanagedTypeConstraint)
			{
				return targets.HasFlag(IntfTargets.Struct) || targets.HasFlag(IntfTargets.Interface);
			}

			ImmutableArray<ITypeSymbol> constraintTypes = typeParameter.ConstraintTypes;

			if (constraintTypes.Length > 0)
			{
				if (!targets.HasFlag(IntfTargets.Class) && !targets.HasFlag(IntfTargets.RecordClass) && !targets.HasFlag(IntfTargets.Interface))
				{
					return false;
				}
			}
			else
			{
				return true;
			}

			foreach (ITypeSymbol type in typeParameter.ConstraintTypes)
			{
				if (type.TypeKind == TypeKind.TypeParameter)
				{
					if (!HandleTypeParameterConstraint(targets, (type as ITypeParameterSymbol)!))
					{
						return false;
					}

					continue;
				}

				if (type.IsRecord)
				{
					if (!targets.HasFlag(IntfTargets.RecordClass) && !targets.HasFlag(IntfTargets.Interface))
					{
						return false;
					}
				}
				else if (!targets.HasFlag(IntfTargets.Class) && !targets.HasFlag(IntfTargets.Interface))
				{
					return false;
				}
			}

			return true;
		}

		private static void ReportCannotBeImplemented(SyntaxNodeAnalysisContext context, INamedTypeSymbol intf, string memberType)
		{
			context.ReportDiagnostic(Diagnostic.Create(DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind, context.Node.GetLocation(), context.ContainingSymbol, intf, memberType));
		}

		private static bool TryGetIntfTargets(
			SemanticModel semanticModel,
			INamedTypeSymbol targetsAttribute,
			SyntaxNode node,
			[NotNullWhen(true)] out INamedTypeSymbol? intf,
			out IntfTargets targets
		)
		{
			if (semanticModel.GetSymbolInfo(node).Symbol is not INamedTypeSymbol target || target.TypeKind != TypeKind.Interface)
			{
				targets = default;
				intf = default;
				return false;
			}

			if (!TryGetIntfTargets(target, targetsAttribute, out targets))
			{
				intf = default;
				return false;
			}

			intf = target;
			return true;
		}

		private static bool TryGetIntfTargets(
			INamedTypeSymbol intf,
			INamedTypeSymbol targetsAttribute,
			out IntfTargets targets
		)
		{
			if (intf.GetAttribute(targetsAttribute) is not AttributeData attr || !attr.TryGetConstructorArgumentValue(0, out int value))
			{
				targets = default;
				return false;
			}

			targets = (IntfTargets)value;
			return true;
		}
	}
}
