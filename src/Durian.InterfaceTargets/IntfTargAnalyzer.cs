// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.InterfaceTargets.IntfTargDiagnostics;

namespace Durian.Analysis.InterfaceTargets
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzes types that are marked with the <c>Durian.InterfaceTargetsAttribute</c>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

	public class IntfTargAnalyzer : DurianAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind,
			DUR0402_InterfaceCannotBeBaseOfAnotherInterface,
			DUR0403_InterfaceIsNotDirectlyAccessible
		);

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context)
		{
			context.RegisterCompilationStartAction(context =>
			{
				INamedTypeSymbol? targetsAttribute = context.Compilation.GetTypeByMetadataName(MemberNames.InterfaceTargetsAttribute);

				if (targetsAttribute is null)
				{
					return;
				}

				context.RegisterSyntaxNodeAction(context => Analyze(context, targetsAttribute), SyntaxKind.SimpleBaseType);
			});
		}

		private static void Analyze(SyntaxNodeAnalysisContext context, INamedTypeSymbol targetsAttribute)
		{
			if (context.Node is not SimpleBaseTypeSyntax s)
			{
				return;
			}

			if (context.ContainingSymbol is not INamedTypeSymbol symbol || !(symbol.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Interface))
			{
				return;
			}

			if (context.SemanticModel.GetSymbolInfo(s.Type).Symbol is not INamedTypeSymbol target || target.TypeKind != TypeKind.Interface)
			{
				return;
			}

			if (target.GetAttribute(targetsAttribute) is not AttributeData attr || !attr.TryGetConstructorArgumentValue(0, out int value))
			{
				return;
			}

			IntfTargets targets = (IntfTargets)value;

			HandleTargets(context, targets, symbol);
		}

		private static void HandleTargets(SyntaxNodeAnalysisContext context, IntfTargets targets, INamedTypeSymbol intf)
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

		private static void ReportCannotBeImplemented(SyntaxNodeAnalysisContext context, INamedTypeSymbol intf, string memberType)
		{
			context.ReportDiagnostic(Diagnostic.Create(DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind, context.Node.GetLocation(), context.ContainingSymbol, intf, memberType));
		}
	}
}
