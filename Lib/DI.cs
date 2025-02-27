using Hwdtech;

public static class DependencyInjection{
    public static object? GetInstance(Type type){
        var ctor = type.GetConstructors()[0];
        var parameters = ctor.GetParameters().Select(param => IoC.Resolve<object>($"Dependency.{param.ParameterType}"));
        return Activator.CreateInstance(type, parameters.ToArray());
    }
}
