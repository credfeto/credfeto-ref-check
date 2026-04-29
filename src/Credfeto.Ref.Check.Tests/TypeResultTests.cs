using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Ref.Check.Tests;

public sealed class TypeResultTests : TestBase
{
    [Fact]
    public void TypeResultHasTypeName()
    {
        TypeResult result = new()
        {
            TypeName = "MyClass",
            Namespace = "My.Namespace",
            FullyQualifiedName = "My.Namespace.MyClass",
            Kind = TypeKind.Class,
            RegistrationOnly = false,
        };

        Assert.Equal(expected: "MyClass", actual: result.TypeName);
    }

    [Fact]
    public void TypeResultHasNamespace()
    {
        TypeResult result = new()
        {
            TypeName = "MyClass",
            Namespace = "My.Namespace",
            FullyQualifiedName = "My.Namespace.MyClass",
            Kind = TypeKind.Class,
            RegistrationOnly = false,
        };

        Assert.Equal(expected: "My.Namespace", actual: result.Namespace);
    }

    [Fact]
    public void TypeResultHasFullyQualifiedName()
    {
        TypeResult result = new()
        {
            TypeName = "MyClass",
            Namespace = "My.Namespace",
            FullyQualifiedName = "My.Namespace.MyClass",
            Kind = TypeKind.Class,
            RegistrationOnly = false,
        };

        Assert.Equal(expected: "My.Namespace.MyClass", actual: result.FullyQualifiedName);
    }

    [Fact]
    public void TypeResultHasKind()
    {
        TypeResult result = new()
        {
            TypeName = "MyInterface",
            Namespace = "My.Namespace",
            FullyQualifiedName = "My.Namespace.MyInterface",
            Kind = TypeKind.Interface,
            RegistrationOnly = false,
        };

        Assert.Equal(expected: TypeKind.Interface, actual: result.Kind);
    }

    [Fact]
    public void TypeResultHasRegistrationOnlyFlag()
    {
        TypeResult result = new()
        {
            TypeName = "MyClass",
            Namespace = "My.Namespace",
            FullyQualifiedName = "My.Namespace.MyClass",
            Kind = TypeKind.Class,
            RegistrationOnly = true,
        };

        Assert.True(result.RegistrationOnly, "Expected RegistrationOnly to be true");
    }

    [Fact]
    public void TypeResultRegistrationOnlyIsFalseByDefault()
    {
        TypeResult result = new()
        {
            TypeName = "MyClass",
            Namespace = "My.Namespace",
            FullyQualifiedName = "My.Namespace.MyClass",
            Kind = TypeKind.Class,
            RegistrationOnly = false,
        };

        Assert.False(result.RegistrationOnly, "Expected RegistrationOnly to be false");
    }
}
