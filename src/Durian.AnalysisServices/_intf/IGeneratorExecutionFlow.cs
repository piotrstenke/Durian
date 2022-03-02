// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
    internal interface IGeneratorExecutionFlow
    {
        void AfterExecution(in GeneratorExecutionContext context);

        void AfterExecutionOfGroup(IFilterGroup filterGroup, in GeneratorExecutionContext context);

        void BeforeExecution(in GeneratorExecutionContext context);

        void BeforeExecutionOfGroup(IFilterGroup filterGroup, in GeneratorExecutionContext context);

        void BeforeFiltrationOfGroup(IFilterGroup filterGroup, in GeneratorExecutionContext context);

        void BeforeGeneratedSymbolFiltration(IFilterGroup filterGroup, in GeneratorExecutionContext context);

        bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context);

        void IterateThroughFilter(ISyntaxFilter filter, in GeneratorExecutionContext context);

        bool ValidateCompilation(CSharpCompilation compilation, in GeneratorExecutionContext context);

        bool ValidateSyntaxReceiver(IDurianSyntaxReceiver syntaxReceiver);
    }
}