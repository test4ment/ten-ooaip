using System.Numerics;

public interface UObject
{
    public IDictionary<string, object> properties {get;}
}

public interface IMoveable
{
    public Vector2 position {get; set;}
    public Vector2 instant_velocity {get;}
}
