using Hwdtech;
using Hwdtech.Ioc;
using Moq;

namespace Tests;

public class DependencyInjectionFeature{
    [Fact]
    public void DependencyInjectionTest(){
        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<Hwdtech.ICommand>(
            "Scopes.Current.Set",
            IoC.Resolve<object>(
                "Scopes.New",
                IoC.Resolve<object>("Scopes.Root")
            )
        ).Execute();

        var moveable_dep = new Mock<IMoveable>().Object;
        var cmd_dep = new Mock<ICommand>().Object;
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", $"Dependency.{typeof(IMoveable)}", (object[] args) => {return moveable_dep;}).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", $"Dependency.{typeof(ICommand)}", (object[] args) => {return cmd_dep;}).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", $"Dependency.{typeof(int)}", (object[] args) => {return (object)42;}).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", $"DependencyInjection.GetInstance", (object[] args) => {
                Type type = (Type)args[0];
                return DependencyInjection.GetInstance(type);
            }).Execute();

        var inst = IoC.Resolve<SomeClass>("DependencyInjection.GetInstance", typeof(SomeClass));

        Assert.NotNull(inst);
        Assert.True(ReferenceEquals(inst.moveable_dep, moveable_dep));
        Assert.True(ReferenceEquals(inst.command_dep, cmd_dep));
        Assert.Equal(42, inst.answer);
    }
}

internal class SomeClass{
    public readonly IMoveable moveable_dep;
    public readonly ICommand command_dep;
    public readonly int answer;

    public SomeClass(IMoveable moveable_dep, ICommand command_dep, int answer)
    {
        this.moveable_dep = moveable_dep;
        this.command_dep = command_dep;
        this.answer = answer;
    }
}
