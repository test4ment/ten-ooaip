using System.Numerics;
using System.Reflection;
using Hwdtech;
using Hwdtech.Ioc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Moq;
using Xunit;

namespace Tests;

public class CodeGenFeature
{
    [Fact]
    public void AdapterCodeGen()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<Hwdtech.ICommand>(
            "Scopes.Current.Set",
            IoC.Resolve<object>(
                "Scopes.New",
                IoC.Resolve<object>("Scopes.Root")
            )
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Adapters.Code.Generate", (object[] args) =>
        {
            return AdapterGenerator<IMoveable>.GenerateCodeString((Type)args[0]);
        }).Execute();

        var props = new Mock<IDictionary<string, object>>();
        props.SetupGet(cls => cls["Position"]).Returns(Vector2.Zero).Verifiable();
        props.SetupSet((dict) => dict["Position"] = Vector2.One).Verifiable();
        props.SetupGet(cls => cls["Instant velocity"]).Returns(Vector2.Zero).Verifiable();

        var move_uobject = new Mock<UObject>();
        move_uobject.SetupGet(cls => cls.properties).Returns(
            props.Object
        );

        var resulting_code = IoC.Resolve<string>("Adapters.Code.Generate", typeof(UObject), typeof(IMoveable));

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(resulting_code);
        MetadataReference[] references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(Assembly.Load("System").Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Numerics").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Numerics.Vectors").Location),
            MetadataReference.CreateFromFile(typeof(UObject).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IMoveable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Vector2).Assembly.Location),
        };
        CSharpCompilation compilation = CSharpCompilation.Create(
                Path.GetRandomFileName(),
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        ms.Seek(0, SeekOrigin.Begin);
        Assembly assembly = Assembly.Load(ms.ToArray());

        Type? type = assembly.GetType("IMoveableAdapter") ?? throw new Exception();
        var obj = (IMoveable)(Activator.CreateInstance(type, move_uobject.Object) ?? throw new Exception());

        obj.position += Vector2.One;
        var vel = obj.instant_velocity;
        props.VerifyAll();
    }
}
