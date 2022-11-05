// See https://aka.ms/new-console-template for more information


using ServiceCollectionGenerator.Attributes;
//using ServiceCollectionGenerator.Attributes;

namespace Benchmark;

[ServiceCollectionContext]
[SerivceCollection(typeof(Test))]
[SerivceCollection(typeof(Test2))]
public static partial class TestContext
{

}