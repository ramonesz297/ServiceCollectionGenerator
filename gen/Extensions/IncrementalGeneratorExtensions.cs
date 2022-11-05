using System.Threading;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Xml.Linq;
using static ServiceCollectionGenerator.Generator;
using ServiceCollectionGenerator;

namespace Microsoft.CodeAnalysis
{
    internal static class SyntaxValueProviderExtensions
    {
#if ROSLYN4_4_OR_GREATER
        public static IncrementalValuesProvider<ClassDeclarationSyntax> GetContextDeclarations(this SyntaxValueProvider provider)
        {

            return provider.ForAttributeWithMetadataName(Generator._contextAttributeName,
                predicate: static (s, _) => Parser.IsSyntaxTargetForGeneration(s),
                transform: static (context, token) => context.TargetNode as ClassDeclarationSyntax).Where(static m => m is not null)!;
        }
#else

        public static IncrementalValuesProvider<ClassDeclarationSyntax> GetContextDeclarations(this SyntaxValueProvider provider)
        {
            return provider.CreateSyntaxProvider(
                predicate: static (s, _) => Parser.IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => Parser.GetSemanticTargetForGeneration(ctx)
            ).Where(static x => x is not null)!;
        }
#endif
    }

}