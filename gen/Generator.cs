using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceCollectionGenerator
{
    [Generator(LanguageNames.CSharp)]
    public partial class Generator : IIncrementalGenerator
    {

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<ClassDeclarationSyntax> declarations = context.SyntaxProvider.GetContextDeclarations();

            var combined = context.CompilationProvider.Combine(declarations.Collect());

            context.RegisterSourceOutput(combined,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static void Execute(Compilation compilation, IList<ClassDeclarationSyntax> services, SourceProductionContext context)
        {
            if (services is null or { Count: 0 })
            {
                return;
            }

            if (compilation.ReferencedAssemblyNames.All(x => !x.Name.Equals("Microsoft.Extensions.DependencyInjection", StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MicrosoftDINotReferenced, null));

                return;
            }

            var distinctServices = services.Distinct();

            var parser = new Parser(compilation, context.ReportDiagnostic, context.CancellationToken);

            List<ServiceContextToGenerate> contextsToGenerate = parser.GetTypesToGenerate(compilation, distinctServices, context.CancellationToken);

            if (contextsToGenerate.Count > 0)
            {
                var e = new Emitter();

                var result = e.Emit(contextsToGenerate, context.CancellationToken);
                context.AddSource("ServiceCollectionExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }
    }
}
