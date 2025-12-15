public partial class AutoPistol : Node3D, IWeapon {
    [Export]
    public PackedScene projectile_scene;

    [Export]
    public float spread_angle = 5.0f;

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready() {
        _rng.Randomize();
    }

    public void Shoot(WeaponResource weapon, Player player) {
        for (int i = 0; i < 3; i++) {
            var projectile_spawn_point = weapon.player_camera;
            var projectile_instance = projectile_scene.Instantiate<Projectile>();
            projectile_instance.weapon = weapon;

            PhysicsServer3D.BodyAddCollisionException(projectile_instance.GetRid(), player.GetRid());

            Transform3D spawn_transform = projectile_spawn_point.GlobalTransform;
            Basis spawn_basis = spawn_transform.Basis;

            float yaw_spread = Mathf.DegToRad(_rng.RandfRange(-spread_angle, spread_angle));
            float pitch_spread = Mathf.DegToRad(_rng.RandfRange(-spread_angle, spread_angle));

            Basis new_basis = spawn_basis.Rotated(spawn_basis.Y, yaw_spread).Rotated(spawn_basis.X, pitch_spread);

            projectile_instance.GlobalTransform = new Transform3D(new_basis, spawn_transform.Origin);

            GetTree().CurrentScene.AddChild(projectile_instance);
        }
    }

    public void Equip(WeaponResource weapon) {
    }

    public void Unequip(WeaponResource weapon) {
    }
}
