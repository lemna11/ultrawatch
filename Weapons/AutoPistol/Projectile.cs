namespace Weapons;

public partial class Projectile : RigidBody3D {
    [Export]
    private Timer _life_timer;

    [Export]
    private float _move_speed = 50.0f;

    public WeaponResource weapon;

    public override void _Ready() {
        LinearVelocity = -GlobalTransform.Basis.Z * _move_speed;
        BodyEntered += OnBodyEntered;
        _life_timer.Timeout += () => QueueFree();
        _life_timer.Start();
    }

    private void OnBodyEntered(Node body) {
        if (body is ITarget target) {
            target.TakeDamage(weapon);
        }
        QueueFree();
    }
}
