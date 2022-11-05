using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using ServiceCollectionGenerator;
using ServiceCollectionGenerator.Attributes;

namespace ServiceCollectionGenerator.Tests
{
    public static class TestHelper
    {
        public static Task Verify(string source, params object[] parameters)
        {
            // Parse the provided string into a C# syntax tree
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            // Create references for assemblies we require
            // We could add multiple references if required

            var references = AppDomain.CurrentDomain.GetAssemblies()
               .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
               .Select(_ => MetadataReference.CreateFromFile(_.Location))
               .Concat(new[]
               {
                    MetadataReference.CreateFromFile(typeof(ServiceCollection).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Generator).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(SerivceCollectionAttribute).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ServiceLifetime).Assembly.Location),
               });

            // Create a Roslyn compilation for the syntax tree.
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: new[] { syntaxTree },
                references: references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            // Create an instance of our EnumGenerator incremental source generator
            var generator = new Generator();

            // The GeneratorDriver is used to run our generator against a compilation
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the source generator!
            driver = driver.RunGenerators(compilation);

            var task = Verifier.Verify(driver);

            if (parameters is { Length: > 0 })
            {
                task = task.UseParameters(parameters);
            }

            return task.UseDirectory("Snapshots");
        }
    }
}