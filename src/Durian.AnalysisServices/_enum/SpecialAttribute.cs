// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Durian.Analysis
{
	/// <summary>
	/// Defines all attribute types that are considered special in some way.
	/// </summary>
	public enum SpecialAttribute
	{
		/// <summary>
		/// The attribute is not special.
		/// </summary>
		None = 0,

		/// <summary>
		/// The attribute is <see cref="ObsoleteAttribute"/>.
		/// </summary>
		Obsolete = 1,

		/// <summary>
		/// The attribute is <see cref="ConditionalAttribute"/>
		/// </summary>
		Conditional = 2,

		/// <summary>
		/// The attribute is <see cref="AttributeUsageAttribute"/>.
		/// </summary>
		AttributeUsage = 3,

		/// <summary>
		/// The attribute is <see cref="CLSCompliantAttribute"/>.
		/// </summary>
		CLSCompliant = 4,

		/// <summary>
		/// The attribute is <see cref="DllImportAttribute"/>
		/// </summary>
		DllImport = 5,

		/// <summary>
		/// The attribute is <see cref="FlagsAttribute"/>.
		/// </summary>
		Flags = 6,

		/// <summary>
		/// The attribute is <see cref="StructLayoutAttribute"/>.
		/// </summary>
		StructLayout = 7,

		/// <summary>
		/// The attribute is <see cref="MarshalAsAttribute"/>
		/// </summary>
		MarshalAs = 8,

		/// <summary>
		/// The attribute is <see cref="FieldOffsetAttribute"/>.
		/// </summary>
		FieldOffset = 9,

		/// <summary>
		/// The attribute is <see cref="MethodImplAttribute"/>.
		/// </summary>
		MethodImpl = 10,

		/// <summary>
		/// The attribute is <see cref="ThreadStaticAttribute"/>.
		/// </summary>
		ThreadStatic = 11,

		/// <summary>
		/// The attribute is <see cref="CallerFilePathAttribute"/>
		/// </summary>
		CallerFilePath = 12,

		/// <summary>
		/// The attribute is <see cref="CallerLineNumberAttribute"/>
		/// </summary>
		CallerLineNumber = 13,

		/// <summary>
		/// The attribute is <see cref="CallerMemberNameAttribute"/>
		/// </summary>
		CallerMemberName = 14,

		/// <summary>
		/// The attribute is <c>System.Runtime.CompilerServices.CallerArgumentExpression</c>.
		/// </summary>
		CallerArgumentExpression = 15,

		/// <summary>
		/// The attribute is <c>System.Runtime.CompilerServices.SkipLocalsInitAttribute</c>.
		/// </summary>
		SkipLocalsInit = 16,

		/// <summary>
		/// The attribute is <c>System.Runtime.CompilerServices.ModuleInitializerAttribute</c>.
		/// </summary>
		ModuleInitializer = 17,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute</c>.
		/// </summary>
		DoesNotReturn = 18,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute</c>.
		/// </summary>
		DoesNotReturnIf = 19,

		/// <summary>
		/// The attribute is <see cref="GeneratedCodeAttribute"/>
		/// </summary>
		GeneratedCode = 20,
	}
}
