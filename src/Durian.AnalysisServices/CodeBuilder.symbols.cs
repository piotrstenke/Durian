// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	public partial class CodeBuilder
	{
		/// <summary>
		/// Writes accessibility modifier of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the accessibility modifier of.</param>
		/// <param name="skipDefault">Determines whether to skip the accessibility modifier if it's default in the current context (see <see cref="SymbolFacts.HasDefaultAccessibility(ISymbol)"/> for more details).</param>
		public CodeBuilder Accessibility(ISymbol symbol, bool skipDefault = true)
		{
			InitBuilder();

			if(skipDefault && symbol.HasDefaultAccessibility())
			{
				return this;
			}

			return Accessibility(symbol.DeclaredAccessibility);
		}

		/// <summary>
		/// Writes an accessibility modifier.
		/// </summary>
		/// <param name="accessibility">Accessibility modifier to write.</param>
		public CodeBuilder Accessibility(Accessibility accessibility)
		{
			InitBuilder();

			if (accessibility.GetText() is string keyword)
			{
				TextBuilder.Append(keyword);
				Space();
			}

			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="array"/>.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to write.</param>
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
					Nullability(elementArray);
					WriteArrayBrackets(elementArray);
				}
			}
			else
			{
				Type(element);
				WriteArrayBrackets(array);
			}

			Nullability(array);

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

		/// <summary>
		/// Writes a list of base types of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write the base type list of.</param>
		public CodeBuilder BaseTypeList(INamedTypeSymbol type)
		{
			InitBuilder();

			ImmutableArray<INamedTypeSymbol> interfaces = type.Interfaces;

			bool hasBegun = false;

			if (type.HasExplicitBaseType())
			{
				Space();
				TextBuilder.Append(':');
				Space();

				Name(type.BaseType!, SymbolName.Substituted);
				hasBegun = true;
			}

			if(interfaces.Length == 0)
			{
				return this;
			}

			if(hasBegun)
			{
				CommaSpace();
			}
			else
			{
				Space();
				TextBuilder.Append(':');
				Space();
			}

			Name(interfaces[0], SymbolName.Substituted);

			for (int i = 1; i < interfaces.Length; i++)
			{
				CommaSpace();
				Name(interfaces[i], SymbolName.Substituted);
			}

			return this;
		}

		/// <summary>
		/// Writes a list of base <paramref name="types"/>.
		/// </summary>
		/// <param name="types">Collection of <see cref="INamedTypeSymbol"/> to write.</param>
		public CodeBuilder BaseTypeList(IEnumerable<INamedTypeSymbol> types)
		{
			return BaseTypeList(types.ToImmutableArray());
		}

		/// <summary>
		/// Writes a list of base <paramref name="types"/>.
		/// </summary>
		/// <param name="types">Collection of <see cref="INamedTypeSymbol"/> to write.</param>
		public CodeBuilder BaseTypeList(ImmutableArray<INamedTypeSymbol> types)
		{
			InitBuilder();

			if (types.Length == 0)
			{
				return this;
			}

			TextBuilder.Append(':');
			Space();

			Name(types[0], SymbolName.Substituted);

			for (int i = 1; i < types.Length; i++)
			{
				CommaSpace();
				Name(types[i], SymbolName.Substituted);
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of an anonymous function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="functionBody">Determines what kind of anonymous function body to begin.</param>
		/// <param name="explicitType">Determines whether to apply explicit return type.</param>
		public CodeBuilder BeginAnonymousFunction(IMethodSymbol method, AnonymousFunctionBody functionBody = AnonymousFunctionBody.Expression, bool explicitType = false)
		{
			InitBuilder();

			if (method.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			if (functionBody == AnonymousFunctionBody.Method)
			{
				TextBuilder.Append("delegate");
			}
			else
			{
				if (explicitType)
				{
					ReturnType(method);
				}

				ImmutableArray<IParameterSymbol> parameters = method.Parameters;

				if (parameters.Length == 0)
				{
					TextBuilder.Append('(');
					TextBuilder.Append(')');
				}
				else if (parameters.Length == 1)
				{
					SimpleName(method.Parameters[0]);
				}
				else
				{
					TextBuilder.Append('(');
					SimpleName(method.Parameters[0]);

					for (int i = 1; i < parameters.Length; i++)
					{
						CommaSpace();
						SimpleName(method.Parameters[i]);
					}

					TextBuilder.Append(')');
				}
			}

			switch (functionBody)
			{
				case AnonymousFunctionBody.Block:
					Space();
					TextBuilder.Append('=');
					TextBuilder.Append('>');
					Space();
					NewLine();
					BeginBlock();
					break;

				case AnonymousFunctionBody.Expression:
					Space();
					TextBuilder.Append('=');
					TextBuilder.Append('>');
					Space();
					break;

				case AnonymousFunctionBody.Method:
					BeginBlock();
					break;

				default:
					TextBuilder.Append(';');
					break;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a class.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder BeginClass(INamedTypeSymbol type)
		{
			Accessibility(type);

			if (type.IsStatic)
			{
				TextBuilder.Append("static ");
			}
			else if (type.IsAbstract)
			{
				TextBuilder.Append("abstract ");
			}
			else if (type.IsSealed)
			{
				TextBuilder.Append("sealed ");
			}

			if (type.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if (type.IsPartial())
			{
				TextBuilder.Append("partial ");
			}

			TextBuilder.Append("class ");

			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters);
				BaseTypeList(type);
				ConstraintList(type.TypeParameters);
			}
			else
			{
				BaseTypeList(type);
			}

			NewLine();
			return BeginBlock();
		}

		/// <summary>
		/// Begins declaration of a constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginConstructor(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			Accessibility(method);

			if (method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			SimpleName(method.ContainingType);

			ParameterList(method);
			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of a constructor with a <see langword="this"/> or <see langword="base"/> call.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="initializer">Determines which constructor initializer to use.</param>
		public CodeBuilder BeginConstructorWithInitializer(IMethodSymbol method, ConstructorInitializer initializer = ConstructorInitializer.None)
		{
			Accessibility(method);

			if (method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			SimpleName(method.ContainingType);

			ParameterList(method);

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
		/// Begins declaration of a conversion operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginConversionOperator(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			Accessibility(method);

			TextBuilder.Append("static ");

			if (method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if (method.Name == WellKnownMemberNames.ImplicitConversionName)
			{
				TextBuilder.Append("implicit ");
			}
			else
			{
				TextBuilder.Append("explicit ");
			}

			TextBuilder.Append("operator ");

			Type(method.ReturnType);

			ParameterList(method);
			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of a method of any valid kind.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		/// <exception cref="ArgumentException"><paramref name="method"/> is of kind does not support declarations.</exception>
		public CodeBuilder BeginDeclaration(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			return method.MethodKind switch
			{
				MethodKind.Ordinary => BeginMethod(method, methodBody),
				MethodKind.ReducedExtension => BeginMethod(method.ReducedFrom!, methodBody),
				MethodKind.StaticConstructor => BeginStaticConstructor(method, methodBody),
				MethodKind.LocalFunction => BeginLocalFunction(method, methodBody),
				MethodKind.Conversion => BeginConversionOperator(method, methodBody),
				MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator => BeginOperator(method, methodBody),
				MethodKind.ExplicitInterfaceImplementation => BeginExplicitInterfaceImplementation(method, methodBody),
				MethodKind.LambdaMethod => BeginAnonymousFunction(method, methodBody.AsAnonymousFunction()),
				MethodKind.Destructor => BeginDestructor(method, methodBody),
				MethodKind.Constructor => BeginConstructor(method, methodBody),
				_ => throw new ArgumentException($"Method '{method}' is of kind that does not support declarations", nameof(method))
			};
		}

		/// <summary>
		/// Begins declaration of a type of any valid kind.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		/// <exception cref="ArgumentException"><paramref name="type"/> is of kind does not support declarations.</exception>
		public CodeBuilder BeginDeclaration(INamedTypeSymbol type)
		{
			if (type.IsRecord)
			{
				return BeginRecord(type);
			}

			return type.TypeKind switch
			{
				TypeKind.Class => BeginClass(type),
				TypeKind.Struct => BeginStruct(type),
				TypeKind.Enum => BeginEnum(type),
				TypeKind.Delegate => Delegate(type),
				TypeKind.Interface => BeginInterface(type),
				_ => throw new ArgumentException($"Type '{type}' is of kind that does not support declarations", nameof(type))
			};
		}

		/// <summary>
		/// Begins declaration of a destructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginDestructor(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			InitBuilder();

			if (method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			TextBuilder.Append('~');
			SimpleName(method.ContainingType);
			TextBuilder.Append('(');
			TextBuilder.Append(')');

			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of an enum.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder BeginEnum(INamedTypeSymbol type)
		{
			Accessibility(type);
			TextBuilder.Append("enum ");
			SimpleName(type);

			if(type.EnumUnderlyingType is not null && type.EnumUnderlyingType.SpecialType == SpecialType.System_Int32)
			{
				Space();
				TextBuilder.Append(':');
				Space();

				TextBuilder.Append(type.EnumUnderlyingType.SpecialType.GetKeyword()!);
			}

			NewLine();
			BeginBlock();

			return this;
		}

		/// <summary>
		/// Begins declaration of an explicit interface method implementation.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginExplicitInterfaceImplementation(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			InitBuilder();

			if (method.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			if (method.IsExtern)
			{
				TextBuilder.Append("extern ");
			}

			if (method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			if (method.ExplicitInterfaceImplementations.Length > 0)
			{
				Type(method.ExplicitInterfaceImplementations[0].ContainingType);
			}

			WriteMethodHead(method, methodBody);
			return this;
		}

		/// <summary>
		/// Begins an attribute list.
		/// </summary>
		/// <param name="type">Type of first attribute in the list.</param>
		public CodeBuilder BeginAttributeList(INamedTypeSymbol type)
		{
			TextBuilder.Append('[');
			return Name(type);
		}

		/// <summary>
		/// Begins declaration of an interface.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder BeginInterface(INamedTypeSymbol type)
		{
			Accessibility(type);

			if(type.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if(type.IsPartial())
			{
				TextBuilder.Append("partial ");
			}

			TextBuilder.Append("interface ");
			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters, true);
				BaseTypeList(type.Interfaces);
				ConstraintList(type.TypeParameters);
			}
			else
			{
				BaseTypeList(type.Interfaces);
			}

			NewLine();
			return BeginBlock();
		}

		/// <summary>
		/// Begins declaration of a local function.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginLocalFunction(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			InitBuilder();

			if (method.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			if (method.IsExtern)
			{
				TextBuilder.Append("extern ");
			}

			if (method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			WriteMethodHead(method, methodBody);
			return this;
		}

		/// <summary>
		/// Begins declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginMethod(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			Accessibility(method);

			if (method.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			if (method.IsExtern)
			{
				TextBuilder.Append("extern ");
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

			if (method.IsUnsafe())
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

		/// <summary>
		/// Begins a method body.
		/// </summary>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginMethodBody(MethodBody methodBody)
		{
			InitBuilder();

			switch (methodBody)
			{
				case MethodBody.Block:
					NewLine();
					BeginBlock();
					break;

				case MethodBody.Expression:
					TextBuilder.Append(" => ");
					break;

				default:
					TextBuilder.Append(';');
					NewLine();
					break;
			}

			return this;
		}

		/// <summary>
		/// Begins declaration of a <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to begin declaration of.</param>
		/// <param name="includeParent">Determines whether to include parent namespaces in the declaration.</param>
		/// <param name="type">Type of namespace declaration to write.</param>
		public CodeBuilder BeginNamespace(INamespaceSymbol @namespace, bool includeParent = true, NamespaceScope type = NamespaceScope.Default)
		{
			InitBuilder();

			TextBuilder.Append("namespace ");

			if(includeParent && @namespace.ContainingNamespace is not null)
			{
				foreach (INamespaceSymbol parent in @namespace.ContainingNamespace.GetContainingNamespaces())
				{
					SimpleName(parent);
					TextBuilder.Append('.');
				}

				SimpleName(@namespace.ContainingNamespace);
			}

			switch (type)
			{
				case NamespaceScope.Default:
					TextBuilder.Append('.');
					SimpleName(@namespace);
					NewLine();
					BeginBlock();
					break;

				case NamespaceScope.File:
					TextBuilder.Append('.');
					SimpleName(@namespace);
					TextBuilder.Append(';');
					NewLine();
					break;

				case NamespaceScope.Nested:
					NewLine();
					BeginBlock();
					Indent();
					SimpleName(@namespace);
					NewLine();
					BeginBlock();
					break;

				default:
					goto case NamespaceScope.Default;
			}

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

			TextBuilder.Append("static ");

			if (method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			Type(method.ReturnType);

			TextBuilder.Append(" operator ");

			if (AnalysisUtilities.GetOperatorText(method.Name) is string operatorName)
			{
				TextBuilder.Append(operatorName);
			}

			ParameterList(method);
			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of a record.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		/// <param name="explicitClass">Determines whether to write the <see langword="class"/> keyword if this <paramref name="type"/> is a reference type.</param>
		public CodeBuilder BeginRecord(INamedTypeSymbol type, bool explicitClass = false)
		{
			Accessibility(type);

			if(type.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			if (type.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if (type.IsPartial())
			{
				TextBuilder.Append("partial ");
			}

			TextBuilder.Append("record ");

			if(type.IsValueType)
			{
				TextBuilder.Append("struct ");
			}
			else if(explicitClass)
			{
				TextBuilder.Append("class ");
			}

			SimpleName(type);
			type.constructor
			if(type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters);
			}
		}

		/// <summary>
		/// Begins declaration of a static constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to begin the declaration of.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		public CodeBuilder BeginStaticConstructor(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			InitBuilder();

			TextBuilder.Append("static ");

			if (method.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			SimpleName(method.ContainingType);
			TextBuilder.Append('(');
			TextBuilder.Append(')');

			BeginMethodBody(methodBody);

			return this;
		}

		/// <summary>
		/// Begins declaration of a struct.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to begin the declaration of.</param>
		public CodeBuilder BeginStruct(INamedTypeSymbol type)
		{
			Accessibility(type);

			if (type.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			if (type.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			if (type.IsRefLikeType)
			{
				TextBuilder.Append("ref ");
			}

			if (type.IsPartial())
			{
				TextBuilder.Append("partial ");
			}

			TextBuilder.Append("struct ");
			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters);
				BaseTypeList(type);
				ConstraintList(type.TypeParameters);
			}
			else
			{
				BaseTypeList(type);
			}

			NewLine();
			return BeginBlock();
		}

		/// <summary>
		/// Writes constraint of the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write the constraint of.</param>
		public CodeBuilder Constraint(ITypeParameterSymbol typeParameter)
		{
			InitBuilder();

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
					CommaSpace();
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
					CommaSpace();
					hasConstraint = true;
				}
			}
		}

		/// <summary>
		/// Writes a list of constraints of the specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write the constraints of.</param>
		public CodeBuilder ConstraintList(IEnumerable<ITypeParameterSymbol> typeParameters)
		{
			InitBuilder();

			foreach (ITypeParameterSymbol typeParameter in typeParameters)
			{
				if (typeParameter.HasConstraint())
				{
					TextBuilder.Append(" where ");
					SimpleName(typeParameter);
					TextBuilder.Append(':');
					Space();
					Constraint(typeParameter);
				}
			}

			return this;
		}

		/// <summary>
		/// Writes default value of the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to write the default value of.</param>
		public CodeBuilder DefaultValue(IParameterSymbol parameter)
		{
			InitBuilder();

			if (parameter.HasExplicitDefaultValue)
			{
				Space();
				TextBuilder.Append('=');
				Space();

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
		/// Writes a delegate declaration.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to write the declaration of.</param>
		/// <exception cref="ArgumentException"><see cref="INamedTypeSymbol.DelegateInvokeMethod"/> cannot be <see langword="null"/>.</exception>
		public CodeBuilder Delegate(INamedTypeSymbol type)
		{
			if(type.DelegateInvokeMethod is null)
			{
				throw new ArgumentException("DelegateInvokeMethod cannot be null", nameof(type));
			}

			Accessibility(type);

			if(type.IsUnsafe())
			{
				TextBuilder.Append("unsafe ");
			}

			TextBuilder.Append("delegate ");

			ReturnType(type.DelegateInvokeMethod);

			SimpleName(type);

			if (type.IsGenericType)
			{
				TypeParameterList(type.TypeParameters, true);
				ParameterList(type.DelegateInvokeMethod);
				ConstraintList(type.TypeParameters);
			}
			else
			{
				ParameterList(type.DelegateInvokeMethod);
			}

			TextBuilder.Append(';');
			NewLine();

			return this;
		}

		/// <summary>
		/// Writes the <see langword="dynamic"/> type.
		/// </summary>
		/// <param name="type"><see cref="IDynamicTypeSymbol"/> to write.</param>
		public CodeBuilder Dynamic(IDynamicTypeSymbol type)
		{
			return Dynamic(type.NullableAnnotation);
		}

		/// <summary>
		/// Writes the <see langword="dynamic"/> type.
		/// </summary>
		/// <param name="annotation"><see cref="NullableAnnotation"/> that should be used.</param>>
		public CodeBuilder Dynamic(NullableAnnotation annotation = default)
		{
			InitBuilder();

			TextBuilder.Append("dynamic");
			Nullability(annotation);
			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IFunctionPointerTypeSymbol"/> to write.</param>
		public CodeBuilder FunctionPointer(IFunctionPointerTypeSymbol pointer)
		{
			InitBuilder();

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
		/// Writes name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to write the name of.</param>
		/// <param name="format">Format of the name.</param>
		public CodeBuilder Name(ISymbol symbol, SymbolName format = default)
		{
			InitBuilder();

			if (symbol is INamedTypeSymbol type && KeywordType(type, format == SymbolName.SystemName))
			{
				return this;
			}

			switch (format)
			{
				case SymbolName.Default:
					SimpleName(symbol);
					break;

				case SymbolName.Generic:
					GenericName(symbol, false, false);
					break;

				case SymbolName.VarianceGeneric:
					GenericName(symbol, false, true);
					break;

				case SymbolName.Substituted:
					GenericName(symbol, true, false);
					break;

				default:
					goto case SymbolName.Default;
			}

			return this;
		}

		/// <summary>
		/// Writes nullability marker '?' if the specified <paramref name="type"/> is nullable.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to write the nullability of.</param>
		public CodeBuilder Nullability(ITypeSymbol type)
		{
			return Nullability(type.NullableAnnotation);
		}

		/// <summary>
		/// Writes nullability marker if the <paramref name="annotation"/> is equal to <see cref="NullableAnnotation.Annotated"/>.
		/// </summary>
		/// <param name="annotation"><see cref="NullableAnnotation"/> to write.</param>
		public CodeBuilder Nullability(NullableAnnotation annotation)
		{
			InitBuilder();

			if (annotation == NullableAnnotation.Annotated)
			{
				TextBuilder.Append('?');
			}

			return this;
		}

		/// <summary>
		/// Writes the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to write.</param>
		public CodeBuilder Parameter(IParameterSymbol parameter)
		{
			InitBuilder();

			if (parameter.IsThis)
			{
				TextBuilder.Append("this ");
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

			if (parameter.IsDiscard)
			{
				TextBuilder.Append('_');
				return this;
			}

			if (parameter.IsParams)
			{
				TextBuilder.Append("params ");
			}

			Type(parameter.Type);
			SimpleName(parameter);
			DefaultValue(parameter);

			return this;
		}

		/// <summary>
		/// Writes parameter list of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to write the parameter list of.</param>
		public CodeBuilder ParameterList(IMethodSymbol method)
		{
			return ParameterList(method.Parameters, method.IsVararg);
		}

		/// <summary>
		/// Writes a list of specified <paramref name="parameters"/>.
		/// </summary>
		/// <param name="parameters">Collection of <see cref="IParameterSymbol"/> to write.</param>
		/// <param name="isArgList">Determines whether the <see langword="__arglist"/> should be written in the parameter list.</param>
		public CodeBuilder ParameterList(ImmutableArray<IParameterSymbol> parameters, bool isArgList = false)
		{
			InitBuilder();

			TextBuilder.Append('(');

			if (parameters.Length > 0)
			{
				Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					CommaSpace();
					Parameter(parameters[i]);
				}

				if (isArgList)
				{
					CommaSpace();
					TextBuilder.Append("__arglist");
				}
			}
			else if (isArgList)
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
		public CodeBuilder ParameterList(IEnumerable<IParameterSymbol> parameters, bool isArgList = false)
		{
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
		/// Writes the return type of the specified <paramref name="method"/> (including the <see langword="ref"/> and <see langword="ref"/> <see langword="readonly"/> modifiers).
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to write the return type of.</param>
		public CodeBuilder ReturnType(IMethodSymbol method)
		{
			InitBuilder();

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
				Space();
			}

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
			InitBuilder();

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
					SimpleName(type);
					Nullability(annotation);
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
				Name(type.TypeArguments[0], SymbolName.Substituted);
				TextBuilder.Append('?');
				return this;
			}

			Name(type, SymbolName.Substituted);
			Nullability(annotation);

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Collection of <see cref="ITypeSymbol"/> to write.</param>
		public CodeBuilder TypeArgumentList(ImmutableArray<ITypeSymbol> typeArguments)
		{
			InitBuilder();

			if (typeArguments.Length == 0)
			{
				return this;
			}

			TextBuilder.Append('<');
			Type(typeArguments[0]);

			for (int i = 1; i < typeArguments.Length; i++)
			{
				CommaSpace();
				Type(typeArguments[0]);
			}

			TextBuilder.Append('>');

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Collection of <see cref="ITypeSymbol"/> to write.</param>
		public CodeBuilder TypeArgumentList(IEnumerable<ITypeSymbol> typeArguments)
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
			InitBuilder();

			SimpleName(typeParameter);
			Nullability(annotation);

			return this;
		}

		/// <summary>
		/// Writes a list of specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		public CodeBuilder TypeParameterList(ImmutableArray<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
		{
			InitBuilder();

			if (typeParameters.Length == 0)
			{
				return this;
			}

			TextBuilder.Append('<');

			if (includeVariance)
			{
				Variance(typeParameters[0]);
				SimpleName(typeParameters[0]);

				for (int i = 1; i < typeParameters.Length; i++)
				{
					CommaSpace();
					Variance(typeParameters[i]);
					SimpleName(typeParameters[i]);
				}
			}
			else
			{
				SimpleName(typeParameters[0]);

				for (int i = 1; i < typeParameters.Length; i++)
				{
					CommaSpace();
					SimpleName(typeParameters[i]);
				}
			}

			TextBuilder.Append('>');

			return this;
		}

		/// <summary>
		/// Writes all specified <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Collection of <see cref="ITypeParameterSymbol"/> to write.</param>
		///  <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		public CodeBuilder TypeParameterList(IEnumerable<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
		{
			return TypeParameterList(typeParameters.ToImmutableArray(), includeVariance);
		}

		/// <summary>
		/// Writes variance of the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to write the variance of.</param>
		public CodeBuilder Variance(ITypeParameterSymbol typeParameter)
		{
			return Variance(typeParameter.Variance);
		}

		/// <summary>
		/// Writes the specified <paramref name="variance"/>.
		/// </summary>
		/// <param name="variance">Kind of variance to write.</param>
		public CodeBuilder Variance(VarianceKind variance)
		{
			InitBuilder();

			if (variance.GetText() is string value)
			{
				TextBuilder.Append(value);
			}

			return this;
		}

		private void GenericName(ISymbol symbol, bool parametersOrArguments, bool includeVariance)
		{
			if (symbol is INamedTypeSymbol t)
			{
				GenericName(t, parametersOrArguments, includeVariance);
			}
			else if (symbol is IMethodSymbol m)
			{
				GenericName(m, includeVariance, includeVariance);
			}
			else
			{
				TextBuilder.Append(symbol.GetVerbatimName());
			}
		}

		private void GenericName(INamedTypeSymbol type, bool parametersOrArguments, bool includeVariance)
		{
			SimpleName(type);

			if (parametersOrArguments)
			{
				TypeParameterList(type.TypeParameters, includeVariance);
			}
			else
			{
				TypeArgumentList(type.TypeArguments);
			}
		}

		private void GenericName(IMethodSymbol method, bool parametersOrArguments, bool includeVariance)
		{
			SimpleName(method);

			if (parametersOrArguments)
			{
				TypeParameterList(method.TypeParameters, includeVariance);
			}
			else
			{
				TypeArgumentList(method.TypeArguments);
			}
		}

		private bool KeywordType(INamedTypeSymbol type, bool useSystemName)
		{
			if (useSystemName)
			{
				if (type is IDynamicTypeSymbol)
				{
					TextBuilder.Append("Object");
					return true;
				}
				else if (type.SpecialType.IsKeyword())
				{
					TextBuilder.Append(type.Name);
					return true;
				}

				return false;
			}

			if (type.GetTypeKeyword() is string keyword)
			{
				TextBuilder.Append(keyword);
				return true;
			}

			return false;
		}

		private void SimpleName(ISymbol symbol)
		{
			TextBuilder.Append(symbol.GetVerbatimName());
		}

		private void WriteMethodHead(IMethodSymbol method, MethodBody methodBody)
		{
			ReturnType(method);
			SimpleName(method);

			if (method.IsGenericMethod)
			{
				TypeParameterList(method.TypeParameters);
				ParameterList(method);
				ConstraintList(method.TypeParameters);
			}
			else
			{
				ParameterList(method);
			}

			BeginMethodBody(methodBody);
		}
	}
}