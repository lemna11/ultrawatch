public partial class RocketLauncher : Node3D, IWeapon {
    [Export]
    public PackedScene rocket_scene;

    [Export]
    public float fire_rate = 1.0f;

    public async void Shoot(WeaponResource weapon, Player _) {
        var rocket_spawn_point = weapon.player_camera;
        var rocket_instance = rocket_scene.Instantiate<Rocket>();
        rocket_instance.weapon = weapon;
        rocket_spawn_point.AddChild(rocket_instance);
        await ToSignal(GetTree(), "process_frame");
        rocket_instance.Reparent(GetTree().CurrentScene);
    }

    public void Equip(WeaponResource weapon) {
    }

    public void Unequip(WeaponResource weapon) {
    }
}
