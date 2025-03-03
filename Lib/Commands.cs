using Hwdtech;

public interface ICommand{
    public void Execute();
}

public class ShootCmd : ICommand
{
    private readonly IMoveable shot_from;
    private readonly ICollection<UObject> objects;

    public ShootCmd(IMoveable shot_from, ICollection<UObject> objects)
    {
        this.shot_from = shot_from;
        this.objects = objects;
    }

    public void Execute()
    {
        objects.Add(
            IoC.Resolve<UObject>("Object.Torpedo.Create", shot_from.instant_velocity)
        );
    }
}
