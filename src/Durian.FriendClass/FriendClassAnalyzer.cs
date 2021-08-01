// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Durian.Analysis.Cache;
using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Durian.Analysis.Extensions;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis.Diagnostics;

#endif

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Analyzes classes marked by the <see cref="GenericSpecializationAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public class FriendClassAnalyzer : DurianAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0301_TargetTypeIsOutsideOfAssembly,
			DUR0302_MemberCannotBeAccessedOutsideOfFriendClass,
			DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends,
			DUR0304_DoNotUseApplyToTypeOnNonInternalTypes,
			DUR0305_ValueOfFriendClassCannotAccessTargetType,
			DUR0306_TypeDoesNotDeclareInternalMembers,
			DUR0307_FriendTypeSpecifiedByMultipleAttributes,
			DUR0308_MemberCannotBeAccessedBySubClass,
			DUR0309_TypeCannotBeAccessedByNonFriendType
		);

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context)
		{
			context.RegisterCompilationStartAction(context =>
			{
				if (context.Compilation.GetTypeByMetadataName(typeof(FriendClassAttribute).ToString()) is not INamedTypeSymbol attr)
				{
					return;
				}

				context.RegisterSymbolStartAction(context => BeginAnalysis(context, attr), SymbolKind.NamedType);
			});
		}

		private static List<FriendClass> GetFriendClassesFromAttributes(AttributeData[] attributes)
		{
			List<FriendClass> list = new(attributes.Length);

			foreach (AttributeData attribute in attributes)
			{
				if (!attribute.TryGetConstructorArgumentTypeValue(0, out INamedTypeSymbol? target) || target is null)
				{
					continue;
				}

				if (!attribute.TryGetNamedArgumentValue(nameof(FriendClassAttribute.AllowInherit), out bool allowInherit))
				{
					allowInherit = FriendClass.AllowInheritDefaultValue;
				}

				list.Add(new FriendClass(target, allowInherit));
			}

			return list;
		}

		private static List<FriendClassTarget> GetFriendClassTargets(Compilation compilation, INamedTypeSymbol friendClassAttribute)
		{
		}

		private static bool TryGetFriendClassAttributes(INamedTypeSymbol symbol, INamedTypeSymbol friendClassAttribute, [NotNullWhen(true)] out AttributeData[]? attributes)
		{
			AttributeData[] attrs = symbol.GetAttributes(friendClassAttribute).ToArray();

			if (attrs.Length == 0)
			{
				attributes = null;
				return false;
			}

			attributes = attrs;
			return true;
		}
	}
}
