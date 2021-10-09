// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Analyzes expressions that attempt to access members of a <see cref="Type"/> with at least one <c>Durian.FriendClassAttribute</c> specified.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public class FriendClassAccessAnalyzer : DurianAnalyzer<FriendClassCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0302_MemberCannotBeAccessedOutsideOfFriendClass,
			DUR0307_MemberCannotBeAccessedByChildClass,
			DUR0310_MemberCannotBeAccessedByChildClassOfFriend
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassAccessAnalyzer"/> class.
		/// </summary>
		public FriendClassAccessAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, FriendClassCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(
				context => Analyze(context, compilation),
				SyntaxKind.SimpleMemberAccessExpression,
				SyntaxKind.PointerMemberAccessExpression
			);
		}

		/// <inheritdoc/>
		protected override FriendClassCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
		{
			return new FriendClassCompilationData(compilation);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context, FriendClassCompilationData compilation)
		{
			if (context.Node is not MemberAccessExpressionSyntax node ||
				context.ContainingSymbol is null ||
				context.ContainingSymbol.ContainingType is not INamedTypeSymbol currentType
			)
			{
				return;
			}

			ISymbol? symbol = context.SemanticModel.GetSymbolInfo(node).Symbol;

			if (symbol is null ||
				symbol.DeclaredAccessibility != Accessibility.Internal ||
				symbol.ContainingType is not INamedTypeSymbol accessedType
			)
			{
				return;
			}

			if (TryGetInvalidFriendDiagnostic(currentType, accessedType, compilation, out DiagnosticDescriptor? descriptor))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: descriptor,
					location: node.GetLocation(),
					messageArgs: new object[] { context.ContainingSymbol, symbol.Name, currentType }
				));
			}
		}

		private static bool GetConfigurationBoolValue(INamedTypeSymbol accessedType, FriendClassCompilationData compilation, string argumentName)
		{
			if (accessedType.GetAttribute(compilation.FriendClassConfigurationAttribute!) is not AttributeData attr)
			{
				return false;
			}

			return attr.GetNamedArgumentValue<bool>(argumentName);
		}

		private static bool IsChildOfAccessedType(INamedTypeSymbol currentType, INamedTypeSymbol accessedType)
		{
			INamedTypeSymbol? current = currentType;

			while (current is not null)
			{
				if (IsChildOfAccessedType(current))
				{
					return true;
				}

				current = current.ContainingType;
			}

			return false;

			bool IsChildOfAccessedType(INamedTypeSymbol current)
			{
				INamedTypeSymbol? parent = current;

				while ((parent = parent!.BaseType) is not null)
				{
					if (SymbolEqualityComparer.Default.Equals(parent, accessedType))
					{
						return true;
					}
				}

				return false;
			}
		}

		private static bool IsChildOfFriend(
			INamedTypeSymbol currentType,
			List<(AttributeData attr, INamedTypeSymbol friend)> friends,
			out int targetFriendIndex
		)
		{
			INamedTypeSymbol? current = currentType;

			while (current is not null)
			{
				if (IsChildOfFriend(current, out int index))
				{
					targetFriendIndex = index;
					return true;
				}

				current = current.ContainingType;
			}

			targetFriendIndex = default;
			return false;

			bool IsChildOfFriend(INamedTypeSymbol current, out int targetFriendIndex)
			{
				INamedTypeSymbol? parent = current;

				while ((parent = parent!.BaseType) is not null)
				{
					for (int i = 0; i < friends.Count; i++)
					{
						if (SymbolEqualityComparer.Default.Equals(friends[i].friend, parent))
						{
							targetFriendIndex = i;
							return true;
						}
					}
				}

				targetFriendIndex = default;
				return false;
			}
		}

		private static bool IsInnerClassOfFriend(INamedTypeSymbol currentType, List<(AttributeData, INamedTypeSymbol)> friends)
		{
			INamedTypeSymbol? parent = currentType;

			while ((parent = parent!.ContainingType) is not null)
			{
				foreach ((_, INamedTypeSymbol friend) in friends)
				{
					if (SymbolEqualityComparer.Default.Equals(friend, parent))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static bool IsSpecifiedFriend(
			INamedTypeSymbol currentType,
			AttributeData[] attributes,
			[NotNullWhen(false)] out List<(AttributeData attribute, INamedTypeSymbol friend)>? friendList
		)
		{
			List<(AttributeData, INamedTypeSymbol)> friends = new(attributes.Length);

			foreach (AttributeData attr in attributes)
			{
				if (attr.TryGetConstructorArgumentTypeValue(0, out INamedTypeSymbol? friend) && friend is not null)
				{
					if (SymbolEqualityComparer.Default.Equals(friend, currentType))
					{
						friendList = null;
						return true;
					}

					friends.Add((attr, friend));
				}
			}

			if (IsInnerClassOfFriend(currentType, friends))
			{
				friendList = null;
				return true;
			}

			friendList = friends;
			return false;
		}

		private static bool TryGetInvalidFriendDiagnostic(
			INamedTypeSymbol currentType,
			INamedTypeSymbol accessedType,
			FriendClassCompilationData compilation,
			[NotNullWhen(true)] out DiagnosticDescriptor? descriptor
		)
		{
			AttributeData[] attributes = accessedType.GetAttributes(compilation.FriendClassAttribute!).ToArray();

			if (attributes.Length == 0)
			{
				descriptor = null;
				return false;
			}

			if (!IsSpecifiedFriend(currentType, attributes, out List<(AttributeData attribute, INamedTypeSymbol friend)>? friends))
			{
				if (IsChildOfAccessedType(currentType, accessedType))
				{
					if (!GetConfigurationBoolValue(accessedType, compilation, MemberNames.Config_AllowsChildren))
					{
						descriptor = DUR0307_MemberCannotBeAccessedByChildClass;
						return true;
					}
				}
				else if (IsChildOfFriend(currentType, friends, out int targetFriendIndex))
				{
					if (!friends[targetFriendIndex].attribute.GetNamedArgumentValue<bool>(MemberNames.Config_AllowsFriendChildren))
					{
						descriptor = DUR0310_MemberCannotBeAccessedByChildClassOfFriend;
						return true;
					}
				}
				else
				{
					descriptor = DUR0302_MemberCannotBeAccessedOutsideOfFriendClass;
					return true;
				}
			}

			descriptor = null;
			return false;
		}
	}
}
