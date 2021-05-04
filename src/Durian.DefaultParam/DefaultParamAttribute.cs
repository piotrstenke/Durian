using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.DefaultParam
{
	public static class DefaultParamAttribute
	{
		public static string AttributeName => "DefaultParam";
		public static string FullTypeName => DurianStrings.GetFullAttributeType(AttributeName);
		public static string FullyQualifiedName => DurianStrings.GetFullyQualifiedAttribute(AttributeName);
		public static string TypeProperty => "Type";

		public static string GetText()
		{
			return
$@"{AutoGenerated.GetHeader(DefaultParamGenerator.GeneratorName, DefaultParamGenerator.Version)}
using System;
using System.CodeDom.Compiler;
using {DurianStrings.GeneratorAttributesNamespace};

namespace {DurianStrings.MainNamespace}
{{
	/// <summary>
	/// Applies a default type for the generic parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	[DurianGenerated]
	public sealed class {FullTypeName} : Attribute
	{{
		/// <summary>
		/// Type that is used as the default type for this generic parameter.
		/// </summary>
		public Type {TypeProperty} {{ get; }}

		/// <summary>
		/// Initializes a new instance of the <see cref=""{FullTypeName}""/> class.
		/// </summary>
		/// <param name=""type"">Type that is used as the default type for this generic parameter.</param>
		public {FullTypeName}(Type type)
		{{
			Type = type;
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
