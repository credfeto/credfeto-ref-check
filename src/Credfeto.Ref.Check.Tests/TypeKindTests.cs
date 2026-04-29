using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Ref.Check.Tests;

public sealed class TypeKindTests : TestBase
{
    [Fact]
    public void ClassHasCorrectName()
    {
        Assert.Equal(expected: "Class", actual: TypeKind.Class.GetName());
    }

    [Fact]
    public void InterfaceHasCorrectName()
    {
        Assert.Equal(expected: "Interface", actual: TypeKind.Interface.GetName());
    }

    [Fact]
    public void StructHasCorrectName()
    {
        Assert.Equal(expected: "Struct", actual: TypeKind.Struct.GetName());
    }

    [Fact]
    public void EnumHasCorrectName()
    {
        Assert.Equal(expected: "Enum", actual: TypeKind.Enum.GetName());
    }

    [Fact]
    public void DelegateHasCorrectName()
    {
        Assert.Equal(expected: "Delegate", actual: TypeKind.Delegate.GetName());
    }

    [Fact]
    public void RecordHasCorrectName()
    {
        Assert.Equal(expected: "Record", actual: TypeKind.Record.GetName());
    }
}
