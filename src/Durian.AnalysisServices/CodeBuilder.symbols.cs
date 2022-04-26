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
	public partial class CodeBuilder2
	{
		/// <summary>
		/// Writes declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to write the declaration for.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public void BeginMethodDeclaration(IMethodSymbol method, MethodBody methodBody = MethodBody.Block)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			TextBuilder.Append(AnalysisUtilities.GetAccessiblityText(method.DeclaredAccessibility));
			TextBuilder.Append(' ');

			if (method.IsStatic)
			{
				TextBuilder.Append("static ");
			}

			if (method.IsReadOnly)
			{
				TextBuilder.Append("readonly ");
			}

			if (method.IsAbstract)
			{
				TextBuilder.Append("abstract ");
			}
			else if (method.IsSealed)
			{
				TextBuilder.Append("sealed ");
			}

			if (method.IsOverride)
			{
				TextBuilder.Append("override ");
			}
			else if (method.IsVirtual)
			{
				TextBuilder.Append("virtual ");
			}

			if (method.IsExtern)
			{
				TextBuilder.Append("extern");
			}

			if (method.IsAsync)
			{
				TextBuilder.Append("async ");
			}

			if (method.IsPartial())
			{
				TextBuilder.Append("partial ");
			}

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

				TextBuilder.Append(method.ReturnType.GetGenericName(GenericSubstitution.TypeArguments));
				TextBuilder.Append(' ');
			}

			TextBuilder.Append(method.Name);

			if (method.IsGenericMethod)
			{
				WriteTypeParameters(method.TypeParameters);
			}

			WriteParameters(method.Parameters);


		}

		public void WriteParameters(ImmutableArray<IParameterSymbol> parameters)
		{
			TextBuilder.Append('(');

			if (parameters.Length > 0)
			{
				WriteParameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					TextBuilder.Append(',');
					TextBuilder.Append(' ');
					WriteParameter(parameters[i]);
				}
			}

			TextBuilder.Append(')');
		}

		public void WriteParameters(IParameterSymbol[] parameters)
		{
			if (parameters is null)
			{
				throw new ArgumentNullException(nameof(parameters));
			}

			WriteParameters(parameters.ToImmutableArray());
		}

		public void WriteParameter(IParameterSymbol parameter)
		{
			if (parameter is null)
			{
				throw new ArgumentNullException(nameof(parameter));
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

			WriteTypeName(parameter.Type);
		}

		public void WriteTypeParameters(ImmutableArray<ITypeParameterSymbol> typeParameters)
		{
			if (typeParameters.Length == 0)
			{
				return;
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
		}

		public void WriteTypeParameters(ITypeParameterSymbol[] typeParameters)
		{
			if (typeParameters is null)
			{
				throw new ArgumentNullException(nameof(typeParameters));
			}

			if (typeParameters.Length == 0)
			{
				return;
			}

			WriteTypeParameters(typeParameters.ToImmutableArray());
		}

		public void WriteArray(IArrayTypeSymbol array)
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

				WriteTypeName(element);
				WriteArrayBrackets(array);

				while (childArrays.Count > 0)
				{
					elementArray = childArrays.Dequeue();
					CheckNullable(elementArray);
					WriteArrayBrackets(elementArray);
				}
			}
			else
			{
				WriteTypeName(element);
				WriteArrayBrackets(array);
			}

			CheckNullable(array);

			void CheckNullable(IArrayTypeSymbol a)
			{
				if (a.NullableAnnotation == NullableAnnotation.Annotated)
				{
					TextBuilder.Append('?');
				}
			}

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

		public void WriteFunctionPointer(IFunctionPointerTypeSymbol pointer)
		{
			if (pointer is null)
			{
				throw new ArgumentNullException(nameof(pointer));
			}

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
				WriteParameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					TextBuilder.Append(", ");
					WriteParameter(parameters[i]);
				}
			}

			WriteTypeName(signature.ReturnType);

			TextBuilder.Append('>');
		}

		public void WritePointer(IPointerTypeSymbol pointer)
		{
			if (pointer is null)
			{
				throw new ArgumentNullException(nameof(pointer));
			}

			WriteTypeName(pointer.PointedAtType);
			TextBuilder.Append('*');
		}

		public void WriteTypeName(ITypeSymbol type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			switch (type)
			{
				case INamedTypeSymbol named:
					WriteTypeName(named);
					return;

				case IArrayTypeSymbol array:
					WriteArray(array);
					return;

				case IDynamicTypeSymbol:
					TextBuilder.Append("dynamic");
					break;

				case IPointerTypeSymbol pointer:
					WritePointer(pointer);
					return;

				case IFunctionPointerTypeSymbol functionPointer:
					WriteFunctionPointer(functionPointer);
					return;

				default:
					TextBuilder.Append(type.Name);
					break;
			}

			if (type.NullableAnnotation == NullableAnnotation.Annotated)
			{
				TextBuilder.Append('?');
			}
		}

		public void WriteTypeName(INamedTypeSymbol type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (type.IsValueType && type.ConstructedFrom is not null && type.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T && type.TypeArguments.Length > 0)
			{
				string name = type.TypeArguments[0].GetGenericName(GenericSubstitution.TypeArguments);
				TextBuilder.Append(AnalysisUtilities.TypeToKeyword(name));
				TextBuilder.Append('?');
			}
			else
			{
				string name = type.TypeArguments.Length > 0 ? type.GetGenericName(GenericSubstitution.TypeArguments) : type.Name;
				TextBuilder.Append(AnalysisUtilities.TypeToKeyword(name));

				if (type.NullableAnnotation == NullableAnnotation.Annotated)
				{
					TextBuilder.Append('?');
				}
			}
		}
	}
}