﻿using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.DefaultParam
{
	public static class DefaultParamConfigurationAttribute
	{
		public static string AttributeName => "DefaultParamConfiguration";
		public static string FullTypeName => DurianStrings.GetFullAttributeType(AttributeName);
		public static string FullyQualifiedName => DurianStrings.GetFullyQualifiedConfigurationAttribute(AttributeName);
		public static string AllowOverridingOfDefaultParamValuesProperty => "AllowOverridingOfDefaultParamValues";
		public static string AllowAddingDefaultParamToNewParametersProperty => "AllowAddingDefaultParamToNewParameters";
		public static string ApplyNewToGeneratedMembersWithEquivalentSignatureProperty => "ApplyNewToGeneratedMembersWithEquivalentSignature";
		public static string CallInsteadOfCopyingProperty => "CallInsteadOfCopying";

		public static string GetText()
		{
			return
$@"{AutoGenerated.GetHeader(DefaultParamGenerator.GeneratorName, DefaultParamGenerator.Version)}
using System;
using System.CodeDom.Compiler;

namespace {DurianStrings.ConfigurationNamespace}
{{
	/// <summary>
	/// Configures how the <see cref=""{DefaultParamAttribute.FullTypeName}""/> behaves in the current assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class {FullTypeName} : Attribute
	{{
		/// <summary>
		/// Determines whether the generated method should call this method instead of copying its contents.
		/// </summary>
		public bool {CallInsteadOfCopyingProperty} {{ get; set; }}

		/// <summary>
		/// Determines whether to allow to override values of <see cref=""{DefaultParamAttribute.FullTypeName}""/>s for virtual methods. Defaults to <see langword=""false""/>.
		/// </summary>
		/// <example>
		/// The following sample...
		/// <code>
		/// partial class Test
		/// {{
		///		public virtual void Method&lt;[{DefaultParamAttribute.AttributeName}(typeof(string)]T&gt;(T value)
		///		{{
		///		}}
		/// }}
		/// 
		/// partial class Derived : Test
		/// {{
		///		public override void Method&lt;[{DefaultParamAttribute.AttributeName}(typeof(int)]T&gt;(T value)
		///		{{
		///		}}
		/// }}
		/// </code>
		/// ...will produce the following code:
		/// <code>
		/// partial class Test
		/// {{
		///		public virtual void Method(string value)
		///		{{
		///		}}
		/// }}
		/// 
		/// partial class Derived : Test
		/// {{
		///		public override void Method(string value)
		///		{{
		///		}}
		///		
		///		public void Method(int value)
		///		{{
		///		}}
		/// }}
		/// </code>
		/// </example>
		public bool {AllowOverridingOfDefaultParamValuesProperty} {{ get; set; }}

		/// <summary>
		/// Determines whether to allow adding new <see cref=""{DefaultParamAttribute.FullTypeName}""/>s to type parameters that don't have the <see cref=""{DefaultParamAttribute.FullTypeName}""/> defined on the base method. Defaults to <see langword=""false""/>.
		/// </summary>
		/// <example>
		/// The following sample...
		/// <code>
		/// partial class Test
		/// {{
		///		public virtual void Method&lt;T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U&gt;(T value, U obj)
		///		{{
		///		}}
		///	}}
		///
		///	partial class Derived : Test
		///	{{
		///		public override void Method&lt;[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U&gt;(T value, U obj)
		///		{{
		///		}}
		///	}}
		/// </code>
		/// ...will produce the following code:
		/// <code>
		/// partial class Test
		///	{{
		///		public virtual void Method&lt;T&gt;(T value, string obj)
		///		{{
		///		}}
		///	}}
		///
		///	partial class Derived
		///	{{
		///		public override void Method&lt;T&gt;(T value, string obj)
		///		{{
		///		}}
		///
		///		public void Method(int value, string obj)
		///		{{
		///		}}
		///	}}
		/// </code>
		/// </example>
		public bool {AllowAddingDefaultParamToNewParametersProperty} {{ get; set; }}

		/// <summary>
		/// Determines whether to apply the 'new' modifier to the generated member when possible instead of reporting an error. Defaults to <see langword=""false""/>.
		/// </summary>
		/// <example>
		/// The following sample...
		/// <code>
		/// partial class Test
		/// {{
		///		public virtual void Method&lt;[{DefaultParamAttribute.AttributeName}(typeof(string)]T&gt;(T value)
		///		{{
		///		}}
		///		
		///		public void Method(int value)
		///		{{
		///		}}
		/// }}
		/// 
		/// partial class Derived : Test
		/// {{
		///		public override void Method&lt;[{DefaultParamAttribute.AttributeName}(typeof(int)]T&gt;(T value)
		///		{{
		///		}}
		/// }}
		/// </code>
		/// ...will produce the following code:
		/// <code>
		/// partial class Test
		/// {{
		///		public virtual void Method(string value)
		///		{{
		///		}}
		/// }}
		/// 
		/// partial class Derived : Test
		/// {{
		///		public override void Method(string value)
		///		{{
		///		}}
		///		
		///		public new void Method(int value)
		///		{{
		///		}}
		/// }}
		/// </code>
		/// </example>
		public bool {ApplyNewToGeneratedMembersWithEquivalentSignatureProperty} {{ get; set; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{FullTypeName}""/> class.
		/// </summary>
		public {FullTypeName}()
		{{
		}}
	}}
}}
";
		}

		public static SourceText CreateSourceText()
		{
			return SourceText.From(GetText(), Encoding.UTF8);
		}

		public static CSharpSyntaxTree CreateSyntaxTree()
		{
			return (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(GetText(), encoding: Encoding.UTF8);
		}
	}
}
