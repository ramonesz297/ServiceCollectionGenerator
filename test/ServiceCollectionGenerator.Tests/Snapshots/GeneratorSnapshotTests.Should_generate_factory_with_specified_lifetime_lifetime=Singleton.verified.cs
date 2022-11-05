﻿//HintName: ServiceCollectionExtensions.g.cs
// <auto-generated/>
using Microsoft.Extensions.DependencyInjection;
namespace Tests
{
	public static partial class ServicesContext
	{
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("ServiceCollectionGenerator",null)]
		private static global::Tests.ITest CreateTestImplementation(global::System.IServiceProvider provider)
		{
			return new global::Tests.TestImplementation(provider.GetRequiredService<global::Tests.ITest2>());
		}

		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("ServiceCollectionGenerator",null)]
		public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection AddServicesContext(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
		{
			services.AddSingleton(CreateTestImplementation);
			return services;
		}
	}
}

