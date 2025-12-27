using System.Collections.Generic;

public partial class Sword : Node3D, IWeapon {
    [Export]
    public float YankStrength = 20.0f;

    [Export]
    public Area3D Hitbox;

    private bool _usedMidair = false;
    private WeaponResource _currentWeapon;
    private Player _ownerPlayer;
    private readonly HashSet<ITarget> _hitTargets = [];

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
        _currentWeapon = weapon;
        _ownerPlayer = player;

        var camera = weapon.player_camera;
        if (camera != null) {
            Vector3 forward = -camera.GlobalTransform.Basis.Z;
            player.Velocity = forward * YankStrength;
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
