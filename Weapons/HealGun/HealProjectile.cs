using Weapons;

namespace Weapons.HealGun;

public partial class HealProjectile : RigidBody3D {
    [Export]
    private Timer _lifeTimer;

    [Export]
    private float _moveSpeed = 2.0f;

    [Export]
    private float _baseHealAmount = 10.0f;

    [Export]
    private float _healRampUpPerSecond = 5.0f;

    private DateTime _startTime;

    public WeaponResource weapon;

    public override void _Ready() {
        _startTime = DateTime.Now;
        LinearVelocity = -GlobalTransform.Basis.Z * _moveSpeed;
        BodyEntered += OnBodyEntered;
        _lifeTimer.Timeout += () => QueueFree();
        _lifeTimer.Start();
    }

    private void OnBodyEntered(Node body) {
        double flightTime = (DateTime.Now - _startTime).TotalSeconds;
        float finalHealAmount = _baseHealAmount + (float)(flightTime * _healRampUpPerSecond);

        if (body is ITarget target) {
            target.TakeDamage(weapon, finalHealAmount);
        }

        QueueFree();
    }
}
