using System.Numerics;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using Xunit;

namespace Tests;

public class ShootingFeature{
    [Fact]
    public void TorpedoCreate(){
        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<Hwdtech.ICommand>(
            "Scopes.Current.Set",
            IoC.Resolve<object>(
                "Scopes.New",
                IoC.Resolve<object>("Scopes.Root")
            )
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Object.Torpedo.Create", (object[] args) =>
        {
            var torpedo = new Mock<UObject>();
            torpedo.SetupGet((obj) => obj.properties).Returns(
                new Dictionary<string, object>(){
                    {"Instant velocity", args[0]}
                }
            );
            return torpedo.Object;
        }).Execute();

        var ship = new Mock<IMoveable>();
        ship.SetupGet(obj => obj.instant_velocity).Returns(new Vector2(1, 1));

        var object_pool = new Mock<ICollection<UObject>>();
        object_pool.Setup(pool => pool.Add(It.IsAny<UObject>())).Verifiable();


        new ShootCmd(ship.Object, object_pool.Object).Execute();


        object_pool.VerifyAll();
        Assert.Equal(
            IoC.Resolve<UObject>("Object.Torpedo.Create", ship.Object.instant_velocity).properties["Instant velocity"],
            ship.Object.instant_velocity
        );
    }
}