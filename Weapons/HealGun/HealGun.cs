public partial class HealGun : Node3D, IWeapon {
    [Export] public PackedScene projectileScene;

    public async void Shoot(WeaponResource weapon, Player player) {
        var projectileSpawnPoint = weapon.player_camera;
        var projectileInstance = projectileScene.Instantiate<HealProjectile>();

        projectileInstance.weapon = weapon;
        projectileSpawnPoint.AddChild(projectileInstance);
        await ToSignal(GetTree(), "process_frame");
        projectileInstance.Reparent(GetTree().CurrentScene);
    }

    public void Equip(WeaponResource weapon) {
    }

    public void Unequip(WeaponResource weapon) {
    }
}
