using System.Collections.Generic;

public partial class Sword : Node3D, IWeapon {
    [Export]
    public float MaxYankStrength = 40.0f;

    [Export]
    public float MaxChargeTime = 2.0f;

    [Export]
    public float ChargeMovementSpeedMultiplier = 0.3f;

    [Export]
    public Area3D Hitbox;

    private bool _usedMidair = false;
    private WeaponResource _currentWeapon;
    private Player _ownerPlayer;
    private readonly HashSet<ITarget> _hitTargets = [];
    private ulong _start_charging_time;
    private bool _isCharging = false;

    public override void _Ready() {
        if (Hitbox != null) {
            Hitbox.BodyEntered += OnBodyEntered;
            Hitbox.Monitoring = false;
        }
    }

    public override void _Process(double delta) {
        if (_ownerPlayer != null && _ownerPlayer.IsOnFloor()) {
            _usedMidair = false;
        }
    }

    public void Shoot(WeaponResource weapon, Player player) {
        if (_usedMidair && !player.IsOnFloor() && player == _ownerPlayer) return;

        _isCharging = true;
        _start_charging_time = Time.GetTicksMsec();
        _currentWeapon = weapon;
        _ownerPlayer = player;
        player.movementSpeedModifier = ChargeMovementSpeedMultiplier;
    }

    public bool ShootReleased(WeaponResource weapon, Player player) {
        if (!_isCharging) return false;

        if (_usedMidair && !player.IsOnFloor() && player == _ownerPlayer) {
            _isCharging = false;
            return false;
        }

        player.ResetModifiers();

        float chargeTimeInSeconds = (Time.GetTicksMsec() - _start_charging_time) / 1000.0f;
        float chargePercentage = Mathf.Min(chargeTimeInSeconds / MaxChargeTime, 1.0f);
        var camera = weapon.player_camera;
        if (camera != null) {
            Vector3 forward = -camera.GlobalTransform.Basis.Z;
            player.Velocity = forward * MaxYankStrength * chargePercentage;
        }

        if (!player.IsOnFloor()) {
            _usedMidair = true;
        }

        if (Hitbox != null) {
            Hitbox.Monitoring = true;
            GetTree().CreateTimer(weapon.fire_rate).Timeout += () => {
                if (IsInstanceValid(Hitbox)) {
                    Hitbox.Monitoring = false;
                }
                _hitTargets.Clear();
            };
        }

        _isCharging = false;
        return true;
    }

    private void OnBodyEntered(Node body) {
        if (body == _ownerPlayer) return;

        if (body is ITarget target && _hitTargets.Add(target) && _currentWeapon != null) {
            target.TakeDamage(_currentWeapon);
        }
    }

    public void Equip(WeaponResource weapon) {
    }

    public void Unequip(WeaponResource weapon) {
    }
}
