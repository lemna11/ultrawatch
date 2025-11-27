public partial class MapManager : Node3D {
    [Export]
    public PackedScene default_map;

    public Node3D current_map;

    public override void _Ready() {
        SetCurrentMap();
    }

    public void SetCurrentMap(PackedScene map = null) {
        map ??= default_map;

        current_map?.QueueFree();
        current_map = map.Instantiate<Node3D>();
        GetTree().CurrentScene.AddChild(current_map);
    }
}
