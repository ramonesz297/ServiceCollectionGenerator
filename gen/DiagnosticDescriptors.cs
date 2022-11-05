using Microsoft.CodeAnalysis;

namespace ServiceCollectionGenerator
{
    public static class DiagnosticDescriptors
    {
        public static DiagnosticDescriptor ContextClassMustBeStaticPartial { get; } = new DiagnosticDescriptor(
          id: "SRSCL1000",
          title: "Service collection context must be 'static partial' class",
          messageFormat: "Service collection context must be 'static partial' class",
          category: "ServiceCollectionSourceGen",
          DiagnosticSeverity.Error,
          isEnabledByDefault: true);

        public static DiagnosticDescriptor ContextClassMustNotBeGeneric { get; } = new DiagnosticDescriptor(
          id: "SRSCL1001",
          title: "Service collection context must not be 'generic' class",
          messageFormat: "Service collection context must not be 'generic' class",
          category: "ServiceCollectionSourceGen",
          DiagnosticSeverity.Error,
          isEnabledByDefault: true);

        public static DiagnosticDescriptor MicrosoftDINotReferenced { get; } = new DiagnosticDescriptor(
         id: "SRSCL1002",
         title: "Microsoft.Extensions.DependencyInjection must be referenced",
         messageFormat: "Microsoft.Extensions.DependencyInjection must be referenced",
         category: "ServiceCollectionSourceGen",
         DiagnosticSeverity.Error,
         isEnabledByDefault: true);
    }
}
