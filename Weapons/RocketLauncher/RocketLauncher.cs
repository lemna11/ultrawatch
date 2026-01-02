using Weapons;

namespace Weapons.RocketLauncher;

public partial class RocketLauncher : Node3D, IWeapon {
    [Export]
    public PackedScene rocket_scene;

    public async void Shoot(WeaponResource weapon, Player _) {
        weapon.current_ammo -= 1;
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
