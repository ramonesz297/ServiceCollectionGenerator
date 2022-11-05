using System.Runtime.CompilerServices;

namespace ServiceCollectionGenerator.Tests
{
    public static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Init()
        {
            VerifySourceGenerators.Enable();
        }
    }
}