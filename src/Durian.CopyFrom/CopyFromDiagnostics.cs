// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.CopyFrom
{
    /// <summary>
    /// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by the <see cref="CopyFromGenerator"/> or one of the analyzers.
    /// </summary>
    public static class CopyFromDiagnostics
    {
        /// <summary>
        /// Provides a diagnostic message indicating that a containing type of a member marked with the <c>Durian.CopyFromTypeAttribute</c> or <c>Durian.CopyFromMethodAttribute</c> must be <see langword="partial"/>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0201_ContainingTypeMustBePartial = new(
            id: "DUR0201",
            title: "Containing type of a member with the CopyFromTypeAttribute or CopyFromMethodAttribute must be partial",
            messageFormat: "'{0}': Containing type of a member with the CopyFromTypeAttribute or CopyFromMethodAttribute must be partial",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0201.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a member marked with the <c>Durian.CopyFromTypeAttribute</c> or <c>Durian.CopyFromMethodAttribute</c> must be <see langword="partial"/>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0202_MemberMustBePartial = new(
            id: "DUR0202",
            title: "Member marked with the CopyFromTypeAttribute or CopyFromMethodAttribute must be partial",
            messageFormat: "'{0}': Member marked with the CopyFromTypeAttribute or CopyFromMethodAttribute must be partial",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0202.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the value of <c>Durian.CopyFromTypeAttribute.Source</c> or <c>Durian.CopyFromMethodAttribute.Source</c> property cannot be resolved.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0203_MemberCannotBeResolved = new(
            id: "DUR0203",
            title: "Target member cannot be resolved",
            messageFormat: "'{0}': Member '{1}' cannot be resolved",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0203.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a member resolved from the value of <c>Durian.CopyFromTypeAttribute.Source</c> or <c>Durian.CopyFromMethodAttribute.Source</c> property is not compatible with the current member kind.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0204_WrongTargetMemberKind = new(
            id: "DUR0204",
            title: "Target member is not compatible",
            messageFormat: "'{0}': Member '{1}' is not compatible with the current member",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0204.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the target implementation is not accessible.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0205_ImplementationNotAccessible = new(
            id: "DUR0205",
            title: "Implementation of the target member is not accessible",
            messageFormat: "'{0}': Implementation of member '{1}' is not accessible",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0205.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that two <c>Durian.CopyFromTypeAttribute</c>s are equivalent.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0206_EquivalentTarget = new(
            id: "DUR0206",
            title: "Equivalent CopyFromTypeAttribute already specified",
            messageFormat: "'{0}': Equivalent CopyFromTypeAttribute already specified",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0206.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a member cannot specify itself or its parent as an argument for either the <c>Durian.CopyFromTypeAttribute</c> or <c>Durian.CopyFromMethodAttribute</c>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0207_MemberCannotCopyFromItselfOrItsParent = new(
            id: "DUR0207",
            title: "Member cannot copy from itself, parent, child or outer type",
            messageFormat: "'{0}': Member cannot copy from itself or its parent, child or outer type",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0207.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a two or more members were resolved using the specified name.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0208_MemberConflict = new(
            id: "DUR0208",
            title: "Two or more members were resolved",
            messageFormat: "'{0}': Two or more members with name '{1}' were resolved",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0208.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that only methods with implementation are valid targets of the <c>Durian.CopyFromMethodAttribute</c>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0209_CannotCopyFromMethodWithoutImplementation = new(
            id: "DUR0209",
            title: "Cannot copy from a method without implementation",
            messageFormat: "'{0}': Cannot copy from a method without implementation",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0209.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a <c>Durian.CopyFromMethodAttribute</c> was specified on an invalid method kind.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0210_InvalidMethodKind = new(
            id: "DUR0210",
            title: "CopyFromMethodAttribute is not valid on this kind of method",
            messageFormat: "'{0}': CopyFromMethodAttribute is not valid on this kind of method",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0210.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a method marked with the <c>Durian.CopyFromMethodAttribute</c> already has a declaration.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0211_MethodAlreadyHasImplementation = new(
            id: "DUR0211",
            title: "Method marked with the CopyFromMethodAttribute already has a declaration",
            messageFormat: "'{0}': Method marked with the CopyFromMethodAttribute already has a declaration",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0211.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a target member does not have a return type.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0212_TargetDoesNotHaveReturnType = new(
            id: "DUR0212",
            title: "Target member does not have a return type",
            messageFormat: "'{0}': Target member '{1}' does not have a return type",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0212.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a target member cannot have a return type.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0213_TargetCannotHaveReturnType = new(
            id: "DUR0213",
            title: "Target member cannot have a return type",
            messageFormat: "'{0}': Target member cannot have a return type",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0213.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a specified <c>Durian.PatternAttribute</c> has invalid values.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0214_InvalidPatternAttributeSpecified = new(
            id: "DUR0214",
            title: "Invalid PatternAttribute specified",
            messageFormat: "'{0}': Invalid PatternAttribute specified",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0214.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a <c>Durian.PatternAttribute</c> is redundant.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0215_RedundantPatternAttribute = new(
            id: "DUR0215",
            title: "PatternAttribute is redundant",
            messageFormat: "'{0}': PatternAttribute is redundant",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0215.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that two <c>Durian.PatternAttribute</c>s are equivalent.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0216_EquivalentPatternAttribute = new(
            id: "DUR0216",
            title: "PatternAttribute with equivalent pattern already specified",
            messageFormat: "'{0}': PatternAttribute with equivalent pattern already specified",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0216.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a type parameter is not valid for the target type.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0217_TypeParameterIsNotValid = new(
            id: "DUR0217",
            title: "Type is not a valid type argument",
            messageFormat: "'{0}': Type '{1}' is not a valid type argument",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0217.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a partial part name is unknown.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0218_UnknownPartialPartName = new(
            id: "DUR0218",
            title: "Unknown partial part",
            messageFormat: "'{0}': Unknown partial part '{1}' of type '{2}'",
            category: "Durian.CopyFrom",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0218.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Documentation directory of the <c>DefaultParam</c> module.
        /// </summary>
        public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/CopyFrom";
    }
}