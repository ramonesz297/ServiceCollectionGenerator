using System;

namespace ServiceCollectionGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SerivceCollectionAttribute : Attribute
    {
        public SerivceCollectionAttribute(Type declaration, Type implementation, Lifetime lifetime = Lifetime.Transient)
        {
            Declaration = declaration;
            Implementation = implementation;
            Lifetime = lifetime;
        }

        public SerivceCollectionAttribute(Type declaration, Lifetime lifetime = Lifetime.Transient)
        {
            Declaration = declaration;
            Implementation = null;
        }

        public Type Declaration { get; }
        public Type Implementation { get; }
        public Lifetime Lifetime { get; }
    }
}
