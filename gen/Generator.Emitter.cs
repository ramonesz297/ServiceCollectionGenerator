using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServiceCollectionGenerator
{
    public partial class Generator
    {
        internal sealed class Emitter
        {
            private readonly StringBuilder _builder = new StringBuilder(1024);

            private const string _serviceParamCollection = "(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)";
            private const string _serviceMethdoSignature= "public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection Add";

            private const string _additionalAttr = "[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"ServiceCollectionGenerator\",null)]";
            private StringBuilder BeginClass()
            {
                return _builder.Append('\t');
            }

            private StringBuilder BeginMethod()
            {
                return _builder.Append('\t').Append('\t');
            }

            private StringBuilder BeginCode()
            {
                return _builder.Append('\t').Append('\t').Append('\t');
            }

            private StringBuilder WriteServiceInjectionExtension(in ServiceContextToGenerate serviceContext)
            {

                BeginMethod();
                _builder.AppendLine(_additionalAttr);
                BeginMethod();
                _builder.Append(_serviceMethdoSignature).Append(serviceContext.Name).AppendLine(_serviceParamCollection);
                BeginMethod().AppendLine("{");

                foreach (var service in serviceContext.Declarations)
                {
                    BeginCode().Append("services.").Append(service.GetLifetimeMethod()).Append('(').Append("Create").Append(service.GetFactoryName()).AppendLine(");");
                }

                BeginCode().AppendLine("return services;");
                BeginMethod().AppendLine("}");

                return _builder;
            }

            private StringBuilder WriteServiceFactory(in ServiceDeclarationToGenerate declarationToGenerate)
            {

                var declaration = declarationToGenerate.GetDeclarationName();
                var implementation = declarationToGenerate.GetImplementationName();
                var factoryName = declarationToGenerate.GetFactoryName();


                BeginMethod();
                _builder.AppendLine(_additionalAttr);
                BeginMethod();
                _builder.Append("private static ").Append(declaration).Append(" ").Append("Create").Append(factoryName).AppendLine("(global::System.IServiceProvider provider)");

                BeginMethod().AppendLine("{");

                if (declarationToGenerate.Parameters.IsEmpty)
                {
                    BeginCode().Append("return new ").Append(implementation).AppendLine("();");
                }
                else
                {
                    BeginCode().Append("return new ").Append(implementation).Append("(");
                    for (int i = 0; i < declarationToGenerate.Parameters.Length; i++)
                    {
                        var item = declarationToGenerate.Parameters[i];
                        var paramType = $"global::{item.Type.ToDisplayString()}";

                        _builder.Append("provider.GetRequiredService<").Append(paramType).Append(">()");
                        if (i < declarationToGenerate.Parameters.Length - 1)
                        {
                            _builder.Append(',');
                        }
                    }
                    _builder.AppendLine(");");

                }
                BeginMethod().AppendLine("}");
                return _builder;
            }

            internal string Emit(List<ServiceContextToGenerate> serviceContextToGenerates, CancellationToken cancellationToken)
            {
                _builder.Clear();
                _builder.AppendLine("// <auto-generated/>");
                _builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");

                foreach (var context in serviceContextToGenerates)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _builder.Append("namespace ").AppendLine(context.Namespace);
                    _builder.AppendLine("{");


                    BeginClass().Append("public static partial class ").AppendLine(context.Name);
                    BeginClass().AppendLine("{");

                    foreach (var item in context.Declarations)
                    {
                        WriteServiceFactory(item);
                    }

                    _builder.AppendLine();

                    WriteServiceInjectionExtension(context);

                    BeginClass().AppendLine("}");

                    _builder.AppendLine("}");

                    _builder.AppendLine();
                }

                return _builder.ToString();
            }
        }

    }
}
