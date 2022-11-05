// See https://aka.ms/new-console-template for more information


using Microsoft.Extensions.DependencyInjection;
//using ServiceCollectionGenerator.Attributes;

namespace Benchmark;
internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        IServiceCollection s = new ServiceCollection();
        s.AddTestContext();

        var sp = s.BuildServiceProvider();

        var service = sp.GetRequiredService<Test>();

        Console.WriteLine("Service is - {0}", service.ToString());

    }
}
