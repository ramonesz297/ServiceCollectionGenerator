using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace ServiceCollectionGenerator
{
    public partial class Generator
    {
        internal const string _contextAttributeName = "ServiceCollectionGenerator.Attributes.ServiceCollectionContextAttribute";
        private const string _serviceAttributeName = "ServiceCollectionGenerator.Attributes.SerivceCollectionAttribute";

        internal sealed class Parser
        {
            private readonly CancellationToken _cancellationToken;
            private readonly Compilation _compilation;
            private readonly Action<Diagnostic> _reportDiagnostic;
            public Parser(Compilation compilation, Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
            {
                _compilation = compilation;
                _cancellationToken = cancellationToken;
                _reportDiagnostic = reportDiagnostic;
            }

            internal static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
                node is ClassDeclarationSyntax _n and { AttributeLists.Count: > 0, Arity: 0 };

            internal void Diag(DiagnosticDescriptor desc, Location? location, params object?[]? messageArgs)
            {
                _reportDiagnostic(Diagnostic.Create(desc, location, messageArgs));
            }

            internal static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
            {
                var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

                foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
                {
                    foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                    {
                        if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                        {
                            continue;
                        }
                        

                        INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

                        string fullName = attributeContainingTypeSymbol.ToDisplayString();

                        if (fullName == _contextAttributeName)
                        {
                            return classDeclarationSyntax;
                        }
                    }
                }

                return null;
            }

            internal List<ServiceContextToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> services, CancellationToken ct)
            {
                var servicesToGenerate = new List<ServiceContextToGenerate>();

                INamedTypeSymbol? contextAttribute = compilation.GetTypeByMetadataName(_contextAttributeName);

                if (contextAttribute is null)
                {
                    return servicesToGenerate;
                }

                foreach (var classDeclarationSyntax in services)
                {
                    ct.ThrowIfCancellationRequested();

                    bool isStatic = false;
                    bool isPartial = false;
                    foreach (SyntaxToken mod in classDeclarationSyntax.Modifiers)
                    {
                        if (mod.IsKind(SyntaxKind.PartialKeyword))
                        {
                            isPartial = true;
                        }
                        else if (mod.IsKind(SyntaxKind.StaticKeyword))
                        {
                            isStatic = true;
                        }
                    }


                    if (!isPartial || !isStatic)
                    {
                        Diag(DiagnosticDescriptors.ContextClassMustBeStaticPartial, classDeclarationSyntax.GetLocation());
                        continue;
                    }

                    if (classDeclarationSyntax.Arity > 0)
                    {
                        Diag(DiagnosticDescriptors.ContextClassMustNotBeGeneric, classDeclarationSyntax.GetLocation());
                        continue;
                    }

                    SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

                    if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol contextSymbol)
                    {
                        // something went wrong, bail out
                        continue;
                    }

                    //classDeclarationSyntax.
                    var servicesDeclarations = new List<ServiceDeclarationToGenerate>();

                    string serviceContextName = contextSymbol.ToString();

                    var attrs = contextSymbol.GetAttributes();

                    foreach (var attribute in attrs)
                    {
                        if (attribute.AttributeClass?.ToDisplayString() != _serviceAttributeName)
                        {
                            continue;
                        }

                        //todo args validation

                        servicesDeclarations.Add(new(attribute.ConstructorArguments));
                    }

                    servicesToGenerate.Add(new ServiceContextToGenerate(contextSymbol, servicesDeclarations));

                }
                return servicesToGenerate;
            }


        }

        internal readonly struct ServiceContextToGenerate
        {
            public readonly string Name;
            public readonly string Namespace;
            public readonly List<ServiceDeclarationToGenerate> Declarations;
            public ServiceContextToGenerate(INamedTypeSymbol symbol, List<ServiceDeclarationToGenerate> declarations)
            {
                Name = symbol.Name;
                Namespace = symbol.ContainingNamespace.ToDisplayString();
                Declarations = declarations;
            }
        }

        internal readonly struct ServiceDeclarationToGenerate
        {
            private readonly int? _lifetime = null;
            private readonly INamedTypeSymbol? _declarationSymbol = null;
            private readonly INamedTypeSymbol? _implemenationSymbol = null;
            public readonly ImmutableArray<IParameterSymbol> Parameters;

            public ServiceDeclarationToGenerate(ImmutableArray<TypedConstant> constructorArguments)
            {
                if (constructorArguments.Length == 3)
                {
                    _declarationSymbol = constructorArguments[0].Value as INamedTypeSymbol;
                    _implemenationSymbol = constructorArguments[1].Value as INamedTypeSymbol;
                    _lifetime = constructorArguments[2].Value switch
                    {
                        int i => i,
                        _ => null
                    };
                }
                else if (constructorArguments.Length == 2)
                {
                    _declarationSymbol = constructorArguments[0].Value as INamedTypeSymbol;
                    _implemenationSymbol = constructorArguments[0].Value as INamedTypeSymbol;
                    _lifetime = constructorArguments[1].Value switch
                    {
                        int i => i,
                        _ => null
                    };
                }
                else if (constructorArguments.Length == 1)
                {
                    _declarationSymbol = constructorArguments[0].Value as INamedTypeSymbol;
                    _implemenationSymbol = constructorArguments[0].Value as INamedTypeSymbol;
                    _lifetime = 2;
                }
                else
                {
                    throw new System.InvalidOperationException();
                }

                Parameters = GetImplemenationConstructorParameters();
            }


            private ImmutableArray<IParameterSymbol> GetImplemenationConstructorParameters()
            {
                var constructors = _implemenationSymbol?.InstanceConstructors;

                if (!constructors.HasValue)
                {
                    return ImmutableArray<IParameterSymbol>.Empty;
                }

                if (constructors.Value.IsEmpty)
                {
                    return ImmutableArray<IParameterSymbol>.Empty;
                }

                var constructor = constructors.Value.OrderByDescending(static x => x.Parameters.Length).FirstOrDefault();

                if (constructor is null)
                {
                    return ImmutableArray<IParameterSymbol>.Empty;
                }

                return constructor.Parameters;
            }

            public string GetLifetimeMethod()
            {
                return _lifetime switch
                {
                    0 => "AddSingleton",
                    1 => "AddScoped",
                    2 => "AddTransient",
                    _ => "AddTransient"
                };
            }


            public string? GetDeclarationName()
            {
                return this._declarationSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }

            public string? GetImplementationName()
            {
                return this._implemenationSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }

            public string? GetFactoryName()
            {
                return this._implemenationSymbol?.Name;
            }
        }
    }
}
