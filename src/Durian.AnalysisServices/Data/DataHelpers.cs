// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	internal static class DataHelpers
	{
		public static TProps EnsureValidProperties<TProps>(
			FieldDeclarationSyntax declaration,
			ICompilationData compilation,
			MemberData.Properties? properties
		)
			where TProps : MemberData.Properties, IDeclaratorProperties, new()
		{
			TProps target;

			bool isLocalProperties;

			if (properties is null)
			{
				target = new TProps();
				isLocalProperties = true;
			}
			else if (properties is TProps props)
			{
				target = props;
				isLocalProperties = false;
			}
			else
			{
				target = new TProps();
				isLocalProperties = true;
			}

			if (target.Symbol is not null)
			{
				return target;
			}

			VariableDeclaratorSyntax variable = RetrieveVariableFromProperties(declaration, target);
			SemanticModel semanticModel = RetrieveSemanticModelFromProperties(compilation, target, variable);

			if (!isLocalProperties)
			{
				TProps newProperties = new();
				newProperties.Map(target);
				target = newProperties;
			}

			target.Variable = variable;
			target.SemanticModel = semanticModel;

			return target;
		}

		private static SemanticModel RetrieveSemanticModelFromProperties(ICompilationData compilation, MemberData.Properties properties, VariableDeclaratorSyntax variable)
		{
			if (properties.SemanticModel is not null)
			{
				return properties.SemanticModel;
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return compilation.Compilation.GetSemanticModel(variable);
		}

		private static VariableDeclaratorSyntax RetrieveVariableFromProperties(FieldDeclarationSyntax declaration, IDeclaratorProperties properties)
		{
			if (properties.Variable is not null)
			{
				return properties.Variable;
			}

			if (!properties.Index.HasValue)
			{
				throw new ArgumentException("Index must be specified when no Variable is set", nameof(properties));
			}

			if (declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			return declaration.Declaration.Variables[properties.Index.Value];
		}
	}
}
