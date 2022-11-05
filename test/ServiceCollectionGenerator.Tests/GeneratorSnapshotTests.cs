namespace ServiceCollectionGenerator.Tests
{
    [UsesVerify]
    public class GeneratorSnapshotTests
    {


        [Fact]
        public Task Should_show_warning_when_context_is_not_partial()
        {
            var source = $@"
using ServiceCollectionGenerator.Attributes;
namespace Tests
{{
    [ServiceCollectionContext]
    public static class ServicesContext
    {{

    }}
}}
";
            return TestHelper.Verify(source);
        }

        [Fact]
        public Task Should_show_warning_when_context_is_not_static()
        {
            var source = $@"
using ServiceCollectionGenerator.Attributes;
namespace Tests
{{
    [ServiceCollectionContext]
    public partial class ServicesContext
    {{

    }}
}}
";
            return TestHelper.Verify(source);
        }

        [Fact]
        public Task Should_generate_factory_without_dependencies()
        {
            var source = $@"
using ServiceCollectionGenerator.Attributes;
namespace Tests
{{
    [ServiceCollectionContext]
    [SerivceCollection(typeof(ITest), typeof(TestImplementation))]
    public static partial class ServicesContext
    {{

    }}

    public interface ITest
    {{

    }}
 
    public class TestImplementation : ITest
    {{   
        public TestImplementation()
        {{
        }}
    }}
}}
";
            return TestHelper.Verify(source);
        }


        [Fact]
        public Task Should_generate_transient_factory_with_dependencies()
        {
            var source = @"
using ServiceCollectionGenerator.Attributes;
namespace Tests
{
    [ServiceCollectionContext]
    [SerivceCollection(typeof(ITest), typeof(TestImplementation))]
    public static partial class ServicesContext
    {

    }

    public interface ITest
    {

    }
    public interface ITest2 {}

    public class TestImplementation : ITest
    {   
        public TestImplementation(ITest2 test2)
        {
        }
    }
}
";
            return TestHelper.Verify(source);
        }

        [Theory]
        [InlineData("Transient")]
        [InlineData("Scoped")]
        [InlineData("Singleton")]
        public Task Should_generate_factory_with_specified_lifetime(string lifetime)
        {
            var source = $@"
using ServiceCollectionGenerator.Attributes;
namespace Tests
 {{
     [ServiceCollectionContext]
     [SerivceCollection(typeof(ITest), typeof(TestImplementation), Lifetime.{lifetime})]
     public static partial class ServicesContext
     {{
 
     }}
 
     public interface ITest
     {{
 
     }}
     public interface ITest2 {{}}
 
     public class TestImplementation : ITest
     {{   
         public TestImplementation(ITest2 test2)
         {{
         }}
     }}
 }}
 ";
            return TestHelper.Verify(source, lifetime);
        }

        [Fact]
        public Task Should_generate_transient_factory_by_implementation()
        {
            var source = $@"
using ServiceCollectionGenerator.Attributes;
namespace Tests
{{
    [ServiceCollectionContext]
    [SerivceCollection(typeof(TestImplementation))]
    public static partial class ServicesContext
    {{

    }}

    public interface ITest2 {{}}

    public class TestImplementation
    {{   
        public TestImplementation(ITest2 test2)
        {{
        }}
    }}
}}
";
            return TestHelper.Verify(source);
        }


    }
}