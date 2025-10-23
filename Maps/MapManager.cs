public partial class MapManager : Node3D {
    [Export]
    public PackedScene default_map;

    public PackedScene current_map;

    public override void _Ready() {
        SetCurrentMap();
    }

    public void SetCurrentMap(PackedScene map = null) {
        map ??= default_map;

        GetTree().CurrentScene.AddChild(map.Instantiate());
    }
}
