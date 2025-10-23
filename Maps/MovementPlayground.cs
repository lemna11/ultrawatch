using System.Threading.Tasks;

public partial class MovementPlayground : Node3D, IMap {
    [Export]
    public Node3D spawn_point;

    public Task Spawn(PackedScene scene) {
        var node = scene.Instantiate<Node3D>();
        GetTree().CurrentScene.AddChild(node);
        node.GlobalTransform = spawn_point.GlobalTransform;
        return Task.CompletedTask;
    }
}
