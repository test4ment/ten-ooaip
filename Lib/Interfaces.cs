using System.Numerics;

public abstract class UObject
{
    public required IDictionary<string, object> properties;
}

public interface IMoveable
{
    public Vector2 position {get; set;}
    public Vector2 instant_velocity {get;}
}
