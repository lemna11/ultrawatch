public partial class RocketLauncher : Node3D, IWeapon {
    [Export]
    public Node3D rocket_spawn_point;

    [Export]
    public PackedScene rocket_scene;

    [Export]
    public float fire_rate = 1.0f;

    public void Shoot(WeaponResource weapon) {
        var rocket_instance = rocket_scene.Instantiate<Node>();
        rocket_spawn_point.AddChild(rocket_instance);
        rocket_instance.Reparent(GetTree().CurrentScene, true);
    }

    public void Equip(WeaponResource weapon) {
    }

    public void Unequip(WeaponResource weapon) {
    }
}
