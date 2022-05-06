// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	public partial class CodeBuilder
	{
		/// <summary>
		/// Writes the specified <paramref name="array"/>.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
		public CodeBuilder Array(IArrayTypeSymbol array)
		{
			ITypeSymbol element = array.ElementType;

			if (element is IArrayTypeSymbol elementArray)
			{
				Queue<IArrayTypeSymbol> childArrays = new();

				while (elementArray is not null)
				{
					childArrays.Enqueue(elementArray);
					element = elementArray.ElementType;
					elementArray = (element as IArrayTypeSymbol)!;
				}

				Type(element);
				WriteArrayBrackets(array);

				while (childArrays.Count > 0)
				{
					elementArray = childArrays.Dequeue();
					ApplyNullable(elementArray);
					WriteArrayBrackets(elementArray);
				}
			}
			else
			{
				Type(element);
				WriteArrayBrackets(array);
			}

			ApplyNullable(array);

			return this;

			void WriteArrayBrackets(IArrayTypeSymbol a)
			{
				int rank = a.Rank;
				TextBuilder.Append('[');

				for (int i = 1; i < rank; i++)
				{
					TextBuilder.Append(',');
				}

				TextBuilder.Append(']');
			}
		}

		public CodeBuilder BeginDeclaration(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			return method.MethodKind switch
			{
				MethodKind.Ordinary or MethodKind.ReducedExtension => BeginMethod(method, methodBody),
				MethodKind.StaticConstructor => BeginStaticConstructor(method, methodBody),
				MethodKind.LocalFunction => BeginLocalFunction(method, methodBody),
				MethodKind.Conversion => BeginConversionOperator(method, methodBody),
				MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator => BeginOperator(method, methodBody),
				MethodKind.ExplicitInterfaceImplementation => BeginExplicitInterfaceImplementation(method, methodBody),
				MethodKind.LambdaMethod => BeginAnonymousFunction(method, methodBody),
				MethodKind.Destructor => BeginDestructor(method, methodBody),
				MethodKind.Constructor => BeginConstructor(method, methodBody),
				_ => throw new ArgumentException($"Method '{method}' does not support declaration", nameof(method))
			};
		}

		public CodeBuilder BeginAnonymousFunction(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{

		}

		public CodeBuilder BeginExplicitInterfaceImplementation(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{

		}

		public CodeBuilder BeginLocal(ILocalSymbol method)
		{
		}

		/// <summary>
		/// Begins declaration of a constructor with a <see langword="this"/> or <see langword="base"/> call.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="initializer">Determines which constructor initializer to use.</param>
		public CodeBuilder BeginConstructorWithInitializer(IMethodSymbol method, ConstructorInitializer initializer = ConstructorInitializer.None)
		{
			Accessibility(method);

			TextBuilder.Append(' ');
			TextBuilder.Append(method.ContainingType.Name);

			ParameterList(method.Parameters);

			switch (initializer)
			{
				case ConstructorInitializer.This:
					TextBuilder.Append(" : this");
					break;

				case ConstructorInitializer.Base:
					TextBuilder.Append(" : base");
					break;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginConstructor(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			Accessibility(method);

			TextBuilder.Append(' ');
			TextBuilder.Append(method.ContainingType.Name);

			ParameterList(method.Parameters);
			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of a local function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginLocalFunction(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			if(method.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			if(method.IsExtern)
			{
				TextBuilder.Append("extern ");
			}

			if(method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if(method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			WriteMethodHead(method, methodBody);
			return this;
		}

		/// <summary>
		/// Begins declaration of a static constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginStaticConstructor(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			TextBuilder.Append("static ");

			if(method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			TextBuilder.Append(method.ContainingType.Name);
			TextBuilder.Append('(');
			TextBuilder.Append(')');

			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of a destructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginDestructor(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			if(method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			TextBuilder.Append('~');
			TextBuilder.Append(method.ContainingType.Name);
			TextBuilder.Append('(');
			TextBuilder.Append(')');

			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of a conversion operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginConversionOperator(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			Accessibility(method);

			TextBuilder.Append(" static ");

			if(method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if(method.Name == "op_Implicit")
			{
				TextBuilder.Append("implicit ");
			}
			else
			{
				TextBuilder.Append("explicit ");
			}

			TextBuilder.Append("operator ");

			Type(method.ReturnType);

			ParameterList(method.Parameters);
			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of an operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginOperator(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			Accessibility(method);

			TextBuilder.Append(" static ");

			if(method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			Type(method.ReturnType);

			TextBuilder.Append(" operator ");

			if(AnalysisUtilities.GetOperatorText(method.Name) is string operatorName)
			{
				TextBuilder.Append(operatorName);
			}

			ParameterList(method.Parameters);
			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginMethod(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			if(method.ReducedFrom is not null)
			{
				method = method.ReducedFrom;
			}

			Accessibility(method);

			TextBuilder.Append(' ');

			if (method.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			if (method.IsExtern)
			{
				TextBuilder.Append("extern");
			}

			if (method.IsAbstract)
			{
				TextBuilder.Append("abstract ");
			}
			else if (method.IsSealed)
			{
				TextBuilder.Append("sealed ");
			}
			else if (method.IsVirtual)
			{
				TextBuilder.Append("virtual ");
			}

			if (method.IsOverride)
			{
				TextBuilder.Append("override ");
			}

			if (method.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			if(method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			if (method.IsPartial(true))
			{
				TextBuilder.Append("partial ");
			}

			WriteMethodHead(method, methodBody);
			return this;
		}

		private void WriteMethodHead(IMethodSymbol method, MethodBody methodBody)
		{
			if (method.ReturnsVoid)
			{
				TextBuilder.Append("void ");
			}
			else
			{
				if (method.ReturnsByRefReadonly)
				{
					TextBuilder.Append("ref readonly ");
				}
				else if (method.ReturnsByRef)
				{
					TextBuilder.Append("ref ");
				}

				Type(method.ReturnType);
				TextBuilder.Append(' ');
			}

			TextBuilder.Append(method.Name);

			if (method.IsGenericMethod)
			{
				TypeParameterList(method.TypeParameters);
			}

			ParameterList(method.Parameters, method.IsVararg);

			if (method.IsGenericMethod)
			{
				ConstraintList(method.TypeParameters);
			}

			BeginMethodBody(methodBody);
		}

		/// <summary>
		/// Begins a method body.
		/// </summary>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public void BeginMethodBody(MethodBody methodBody)
		{
			switch (methodBody)
			{
				case MethodBody.Block:
					TextBuilder.AppendLine();
					BeginScope();
					break;

				case MethodBody.Expression:
					TextBuilder.Append(" => ");
					break;

				default:
					TextBuilder.Append(';');
					TextBuilder.AppendLine();
					break;
			}
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to begin declaration of.</param>
		/// <param name="type">Type of namespace declaration to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="namespace"/> is <see langword="null"/>.</exception>
		public CodeBuilder BeginNamespace(INamespaceSymbol @namespace, NamespaceScope type = NamespaceScope.Default)
		{
			Indent();
			TextBuilder.Append("namespace ");
			TextBuilder.WriteNamespacesOf(@namespace, true);

			if (type == NamespaceScope.File)
			{
				TextBuilder.Append(';');
				TextBuilder.AppendLine();
			}
			else
			{
				TextBuilder.AppendLine();
				BeginScope();
			}

			return this;
		}

		/// <summary>
		/// Writes constraint of the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write the constraint of.</param>
		public CodeBuilder Constraint(ITypeParameterSymbol typeParameter)
		{
			bool hasConstraint = false;

			if (typeParameter.HasReferenceTypeConstraint)
			{
				TextBuilder.Append("class");

				if (typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated)
				{
					TextBuilder.Append('?');
				}

				hasConstraint = true;
			}
			else if (typeParameter.HasUnmanagedTypeConstraint)
			{
				TextBuilder.Append("unmanaged");
				hasConstraint = true;
			}
			else if (typeParameter.HasValueTypeConstraint)
			{
				TextBuilder.Append("struct");
				hasConstraint = true;
			}
			else if (typeParameter.HasNotNullConstraint)
			{
				TextBuilder.Append("notnull");
				hasConstraint = true;
			}

			ImmutableArray<ITypeSymbol> constraintTypes = typeParameter.ConstraintTypes;

			if (constraintTypes.Length > 0)
			{
				EnsureComma(ref hasConstraint);
				ImmutableArray<NullableAnnotation> nullables = typeParameter.ConstraintNullableAnnotations;
				Type(constraintTypes[0], nullables[0]);

				for (int i = 1; i < constraintTypes.Length; i++)
				{
					TextBuilder.Append(',');
					TextBuilder.Append(' ');
					Type(constraintTypes[i], nullables[i]);
				}
			}

			if (typeParameter.HasConstructorConstraint)
			{
				EnsureComma(ref hasConstraint);
				TextBuilder.Append("new()");
			}

			return this;

			void EnsureComma(ref bool hasConstraint)
			{
				if (hasConstraint)
				{
					TextBuilder.Append(',');
					TextBuilder.Append(' ');
					hasConstraint = true;
				}
			}
		}

		/// <summary>
		/// Writes a list of constraints of the specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write the constraints of.</param>
		public CodeBuilder ConstraintList(ITypeParameterSymbol[] typeParameters)
		{
			if (typeParameters.Length == 0)
			{
				return this;
			}

			return ConstraintList(typeParameters.ToImmutableArray());
		}

		/// <summary>
		/// Writes a list of constraints of the specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write the constraints of.</param>
		public CodeBuilder ConstraintList(ImmutableArray<ITypeParameterSymbol> typeParameters)
		{
			foreach (ITypeParameterSymbol typeParameter in typeParameters)
			{
				TextBuilder.Append(" where ");
				TextBuilder.Append(typeParameters[0].Name);
				TextBuilder.Append(':');
				TextBuilder.Append(' ');
				Constraint(typeParameter);
			}

			return this;
		}

		/// <summary>
		/// Writes the <see langword="dynamic"/> type.
		/// </summary>
		/// <param name="type"><see cref="IDynamicTypeSymbol"/> to write.</param>
		public CodeBuilder Dynamic(IDynamicTypeSymbol type)
		{
			TextBuilder.Append("dynamic");
			ApplyNullable(type);

			return this;
		}

		/// <summary>
		/// Writes the <see langword="dynamic"/> type.
		/// </summary>
		/// <param name="annotation"><see cref="NullableAnnotation"/> that should be used.</param>>
		public CodeBuilder Dynamic(NullableAnnotation annotation)
		{
			TextBuilder.Append("dynamic");
			ApplyNullable(annotation);
			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IFunctionPointerTypeSymbol"/> to write.</param>
		public CodeBuilder FunctionPointer(IFunctionPointerTypeSymbol pointer)
		{
			IMethodSymbol signature = pointer.Signature;

			TextBuilder.Append("delegate*");

			if (signature.CallingConvention == SignatureCallingConvention.Unmanaged)
			{
				ImmutableArray<INamedTypeSymbol> callConv = signature.UnmanagedCallingConventionTypes;

				TextBuilder.Append(" unmanaged");

				if (callConv.Length > 0)
				{
					TextBuilder.Append(callConv[0].Name);

					for (int i = 1; i < callConv.Length; i++)
					{
						TextBuilder.Append(", ");
						TextBuilder.Append(callConv[1].Name);
					}
				}
			}

			TextBuilder.Append('<');

			ImmutableArray<IParameterSymbol> parameters = signature.Parameters;

			if (parameters.Length > 0)
			{
				Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					TextBuilder.Append(", ");
					Parameter(parameters[i]);
				}
			}

			Type(signature.ReturnType);

			TextBuilder.Append('>');

			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to write.</param>
		public CodeBuilder Parameter(IParameterSymbol parameter)
		{
			if (parameter.IsDiscard)
			{
				TextBuilder.Append('_');
				return this;
			}

			if (parameter.IsThis)
			{
				TextBuilder.Append("this ");
			}

			if (parameter.IsParams)
			{
				TextBuilder.Append("params ");
			}

			if (parameter.RefKind != RefKind.None)
			{
				switch (parameter.RefKind)
				{
					case RefKind.In:
						TextBuilder.Append("in ");
						break;

					case RefKind.Out:
						TextBuilder.Append("out ");
						break;

					case RefKind.Ref:
						TextBuilder.Append("ref ");
						break;
				}
			}

			Type(parameter.Type);

			if (parameter.HasExplicitDefaultValue)
			{
				TextBuilder.Append('=');
				TextBuilder.Append(' ');

				if (parameter.ExplicitDefaultValue is null)
				{
					TextBuilder.Append("default");
				}
				else
				{
					TextBuilder.Append(parameter.ExplicitDefaultValue.ToString());
				}
			}

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="parameters"/>.
		/// </summary>
		/// <param name="parameters">Collection of <see cref="IParameterSymbol"/> to write.</param>
		/// <param name="isArgList">Determines whether the <see langword="__arglist"/> should be written in the parameter list.</param>
		public CodeBuilder ParameterList(ImmutableArray<IParameterSymbol> parameters, bool isArgList = false)
		{
			TextBuilder.Append('(');

			if (parameters.Length > 0)
			{
				Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					TextBuilder.Append(',');
					TextBuilder.Append(' ');
					Parameter(parameters[i]);
				}

				if(isArgList)
				{
					TextBuilder.Append(',');
					TextBuilder.Append(' ');
					TextBuilder.Append("__arglist");
				}
			}
			else if(isArgList)
			{
				TextBuilder.Append("__arglist");
			}

			TextBuilder.Append(')');

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="parameters"/>.
		/// </summary>
		/// <param name="parameters">Collection of <see cref="IParameterSymbol"/> to write.</param>
		/// <param name="isArgList">Determines whether the <see langword="__arglist"/> should be written in the parameter list.</param>
		public CodeBuilder ParameterList(IParameterSymbol[] parameters, bool isArgList = false)
		{
			if (parameters.Length == 0)
			{
				return this;
			}

			return ParameterList(parameters.ToImmutableArray(), isArgList);
		}

		/// <summary>
		/// Writes the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IPointerTypeSymbol"/> to write.</param>
		public CodeBuilder Pointer(IPointerTypeSymbol pointer)
		{
			Type(pointer.PointedAtType);
			TextBuilder.Append('*');

			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to write.</param>
		public CodeBuilder Type(ITypeSymbol type)
		{
			return Type(type, type.NullableAnnotation);
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to write.</param>
		/// <param name="annotation"><see cref="NullableAnnotation"/> that should be used instead of the <paramref name="type"/>'s actual <see cref="NullableAnnotation"/>.</param>>
		public CodeBuilder Type(ITypeSymbol type, NullableAnnotation annotation)
		{
			switch (type)
			{
				case INamedTypeSymbol named:
					return Type(named, annotation);

				case IArrayTypeSymbol array:
					return Array(array);

				case IDynamicTypeSymbol:
					return Dynamic(annotation);

				case IPointerTypeSymbol pointer:
					return Pointer(pointer);

				case IFunctionPointerTypeSymbol functionPointer:
					return FunctionPointer(functionPointer);

				case ITypeParameterSymbol typeParameter:
					return TypeParameter(typeParameter, annotation);

				default:
					TextBuilder.Append(type.Name);
					ApplyNullable(annotation);
					return this;
			}
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write.</param>
		public CodeBuilder Type(INamedTypeSymbol type)
		{
			return Type(type, type.NullableAnnotation);
		}

		/// <summary>
		/// Writes the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write.</param>
		/// <param name="annotation"><see cref="NullableAnnotation"/> that should be used instead of the <paramref name="type"/>'s actual <see cref="NullableAnnotation"/>.</param>>
		public CodeBuilder Type(INamedTypeSymbol type, NullableAnnotation annotation)
		{
			if (type.IsNullableValueType())
			{
				WriteTypeName(type.TypeArguments[0]);
				TextBuilder.Append('?');
				return this;
			}

			WriteTypeName(type);
			ApplyNullable(annotation);

			return this;

			void WriteTypeName(ITypeSymbol type)
			{
				if (type.SpecialType == SpecialType.None)
				{
					TextBuilder.WriteGenericName(type, GenericSubstitution.TypeArguments);
				}
				else
				{
					TextBuilder.Append(AnalysisUtilities.TypeToKeyword(type.Name));
				}
			}
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Collection of <see cref="ITypeSymbol"/> to write.</param>
		public CodeBuilder TypeArgumentList(ImmutableArray<ITypeSymbol> typeArguments)
		{
			if (typeArguments.Length == 0)
			{
				return this;
			}

			TextBuilder.Append('<');
			Type(typeArguments[0]);

			for (int i = 1; i < typeArguments.Length; i++)
			{
				TextBuilder.Append(' ');
				TextBuilder.Append(',');
				Type(typeArguments[0]);
			}

			TextBuilder.Append('>');

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Collection of <see cref="ITypeSymbol"/> to write.</param>
		public CodeBuilder TypeArgumentList(ITypeSymbol[] typeArguments)
		{
			return TypeArgumentList(typeArguments.ToImmutableArray());
		}

		/// <summary>
		/// Writes the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder TypeParameter(ITypeParameterSymbol typeParameter)
		{
			return TypeParameter(typeParameter, typeParameter.NullableAnnotation);
		}

		/// <summary>
		/// Writes the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write.</param>
		/// <param name="annotation"><see cref="NullableAnnotation"/> that should be used instead of the <paramref name="typeParameter"/>'s actual <see cref="NullableAnnotation"/>.</param>>
		public CodeBuilder TypeParameter(ITypeParameterSymbol typeParameter, NullableAnnotation annotation)
		{
			TextBuilder.Append(typeParameter.Name);
			ApplyNullable(annotation);

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder TypeParameterList(ImmutableArray<ITypeParameterSymbol> typeParameters)
		{
			if (typeParameters.Length == 0)
			{
				return this;
			}

			TextBuilder.Append('<');
			TextBuilder.Append(typeParameters[0].Name);

			for (int i = 1; i < typeParameters.Length; i++)
			{
				TextBuilder.Append(' ');
				TextBuilder.Append(',');
				TextBuilder.Append(typeParameters[i].Name);
			}

			TextBuilder.Append('>');

			return this;
		}

		/// <summary>
		/// Writes all specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		public CodeBuilder TypeParameterList(ITypeParameterSymbol[] typeParameters)
		{
			if (typeParameters.Length == 0)
			{
				return this;
			}

			return TypeParameterList(typeParameters.ToImmutableArray());
		}

		private void ApplyNullable(ITypeSymbol type)
		{
			ApplyNullable(type.NullableAnnotation);
		}

		private void ApplyNullable(NullableAnnotation annotation)
		{
			if (annotation == NullableAnnotation.Annotated)
			{
				TextBuilder.Append('?');
			}
		}

		private void Accessibility(ISymbol symbol)
		{
			if(AnalysisUtilities.GetAccessiblityText(symbol.DeclaredAccessibility) is string keyword)
			{
				TextBuilder.Append(keyword);
			}
		}
	}
}