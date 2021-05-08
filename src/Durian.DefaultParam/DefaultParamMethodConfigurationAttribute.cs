﻿using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Static class that contains all the metadata of the actual <c>DefaultParamMethodConfigurationAttribute</c>.
	/// </summary>
	public static class DefaultParamMethodConfigurationAttribute
	{
		/// <summary>
		/// Name of the <c>DefaultParamMethodConfigurationAttribute</c> without the 'Attribute' suffix, i.e. 'DefaultParamConfiguration'.
		/// </summary>
		public static string AttributeName => "DefaultParamMethodConfiguration";

		/// <summary>
		/// Full name of the <c>DefaultParamMethodConfigurationAttribute</c>, i.e. 'DefaultParamConfigurationAttribute'.
		/// </summary>
		public static string FullTypeName => DurianStrings.GetFullAttributeType(AttributeName);

		/// <summary>
		/// Fully qualified name of the <c>DefaultParamMethodConfigurationAttribute</c>, i.e. 'Durian.Configuration.DefaultParamConfigurationAttribute'.
		/// </summary>
		public static string FullyQualifiedName => DurianStrings.GetFullyQualifiedConfigurationAttribute(AttributeName);

		/// <summary>
		/// Name of the <c>DefaultParamMethodConfigurationAttribute.CallInsteadOfCopying</c> property, i.e. 'CallInsteadOfCopying'.
		/// </summary>
		public static string CallInsteadOfCopyingProperty => "CallInsteadOfCopying";

		/// <summary>
		/// Returns full source code of the <c>DefaultParamMethodConfigurationAttribute</c>.
		/// </summary>
		public static string GetText()
		{
			return
$@"{AutoGenerated.GetHeader(DefaultParamGenerator.GeneratorName, DefaultParamGenerator.Version)}
using System;
using System.CodeDom.Compiler;
using {DurianStrings.GeneratorAttributesNamespace};

namespace {DurianStrings.ConfigurationNamespace}
{{
	/// <summary>
	/// Configures how the <see cref=""{DefaultParamAttribute.FullTypeName}""/> behaves when applied to this method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	[DurianGenerated]
	public sealed class {FullTypeName} : Attribute
	{{
		/// <summary>
		/// Determines whether the generated method should call this method instead of copying its contents.
		/// </summary>
		public bool {CallInsteadOfCopyingProperty} {{ get; set; }}

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

		/// <summary>
		/// Returns full source code of the <c>DefaultParamMethodConfigurationAttribute</c> as an instance of <see cref="SourceText"/> with the <see cref="Encoding.UTF8"/> applied.
		/// </summary>
		public static SourceText CreateSourceText()
		{
			return SourceText.From(GetText(), Encoding.UTF8);
		}

		/// <summary>
		/// Returns full <see cref="CSharpSyntaxTree"/> of the <c>DefaultParamAttribute</c>.
		/// </summary>
		/// <param name="parseOptions"><see cref="CSharpParseOptions"/> that should be used when parsing the source code returned by the <see cref="GetText"/> method.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static CSharpSyntaxTree CreateSyntaxTree(CSharpParseOptions? parseOptions = null, CancellationToken cancellationToken = default)
		{
			return (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(GetText(), encoding: Encoding.UTF8, options: parseOptions, cancellationToken: cancellationToken);
		}
	}
}
