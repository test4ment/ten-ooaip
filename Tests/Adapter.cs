using System.Numerics;
using Hwdtech;
using Hwdtech.Ioc;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Tests;

public class CodeGenFeature
{
    [Fact]
    public void GivenIoC()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<Hwdtech.ICommand>(
            "Scopes.Current.Set",
            IoC.Resolve<object>(
                "Scopes.New",
                IoC.Resolve<object>("Scopes.Root")
            )
        ).Execute();

        var adapters_code_tree = new Dictionary<object, object>();
        var objects_lexemes = new Dictionary<Type, Dictionary<string, string>>(){
            {
                typeof(UObject), new Dictionary<string, string>(){
                    {"get_method", ".properties.Get"},
                    {"set_method", ".properties.Set"},
                }
            }
        };

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Adapters.Source.Lexemes", (object[] args) =>
        {
            return objects_lexemes[(Type)args[0]];
        }).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Adapters.Code.Generate", (object[] args) =>
        {
            return AdapterGenerator<IMoveable>.GenerateCodeString((Type)args[0]);
        }).Execute();


        var resulting_code = IoC.Resolve<string>("Adapters.Code.Generate", typeof(UObject), typeof(IMoveable));


        var awaited_code = """
            public class IMoveableAdapter : IMoveable
            {
                private readonly UObject _obj;
                public IMoveableAdapter(UObject obj)
                {
                    _obj = obj;
                }
                public System.Numerics.Vector2 position {
                    get => (System.Numerics.Vector2)_obj.properties.Get("Position");
                    set => _obj.properties.Set("Position", value);
                }
                public System.Numerics.Vector2 instant_velocity {
                    get => (System.Numerics.Vector2)_obj.properties.Get("Instant velocity");                       
                }
            }
            """;

        var awaited_syntaxtree = CSharpSyntaxTree.ParseText(awaited_code);

        Assert.True(awaited_syntaxtree.IsEquivalentTo(
                CSharpSyntaxTree.ParseText(resulting_code)
            )
        );
    }
}

internal abstract class UObject
{
    public required IDictionary<string, object> properties;
}

internal interface IMoveable
{
    public Vector2 position {get; set;}
    public Vector2 instant_velocity {get;}
}
