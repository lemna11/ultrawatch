using System.Threading.Tasks;

namespace Weapons;

// TODO: refactor this class out of existence
// Player should own both weapons directly
// Weapons should do the input handling themselves
// Animations/Sounds handling should be exposed through signals on the weapon interface
// Ideally we also refactor WeaponResource out of existence too
// There is just too much variance between weapons to make a general manager class a good idea
public partial class WeaponManager : Node3D {
    public enum EquipSide {
        Right,
        Left
    }

    [Export]
    public CharacterBody3D player;

    [Export]
    public Node3D weapon_holder;

    [Export]
    public WeaponResource right_current_weapon;

    [Export]
    public WeaponResource left_current_weapon;

    public event Action update_hud;

    private Node3D _current_right_weapon_instance;
    private Node3D _current_left_weapon_instance;

    private async void UpdateWeaponModel(string weaponPath = null, EquipSide side = EquipSide.Right) {
        if (weaponPath != null) {
            switch (side) {
                case EquipSide.Right:
                    right_current_weapon = ResourceLoader.Load(weaponPath).Duplicate() as WeaponResource;
                    break;
                case EquipSide.Left:
                    left_current_weapon = ResourceLoader.Load(weaponPath).Duplicate() as WeaponResource;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        var current_weapon = side switch {
            EquipSide.Right => right_current_weapon,
            EquipSide.Left => left_current_weapon,
            _ => throw new NotSupportedException()
        };
        if (current_weapon is not null) {
            var weapon_position = side switch {
                EquipSide.Right => right_current_weapon.right_arm_weapon_offset,
                EquipSide.Left => left_current_weapon.left_arm_weapon_offset,
                _ => throw new NotSupportedException()
            };
            var current_weapon_instance = side switch {
                EquipSide.Right => _current_right_weapon_instance,
                EquipSide.Left => _current_left_weapon_instance,
                _ => throw new NotSupportedException()
            };
            if (current_weapon_instance is not null) {
                (current_weapon_instance as IWeapon).Unequip(current_weapon);
                current_weapon_instance.QueueFree();
            }
            if (weapon_holder is not null && current_weapon.weapon_model is not null) {
                current_weapon.player_camera = player.GetNode<Node3D>("CameraYaw/CameraPitch/Camera3D");
                current_weapon_instance = side switch {
                    EquipSide.Right =>
                        _current_right_weapon_instance = right_current_weapon.weapon_model.Instantiate<Node3D>(),
                    EquipSide.Left =>
                        _current_left_weapon_instance = left_current_weapon.weapon_model.Instantiate<Node3D>(),
                    _ => throw new NotSupportedException(),
                };

                if (current_weapon_instance as IWeapon is null) {
                    GD.PrintErr("The weapon model does not implement IWeapon interface.");
                    throw new InvalidOperationException("The weapon model does not implement IWeapon interface.");
                }
                current_weapon_instance.Position = weapon_position;
                weapon_holder.CallDeferred(Node.MethodName.AddChild, current_weapon_instance);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                current_weapon.can_fire = false;
                (current_weapon_instance as IWeapon).Equip(current_weapon);
                if (current_weapon_instance.HasNode("AnimationPlayer")
                    && current_weapon.equip_animation is not null or ""
                    && current_weapon_instance.GetNode<Node>("AnimationPlayer") is AnimationPlayer animationPlayer
                    && animationPlayer.HasAnimation(current_weapon.equip_animation)) {
                    var waitTime = animationPlayer.GetAnimation(current_weapon.equip_animation).Length;
                    ShootAgainIn(current_weapon, waitTime);
                    PlayAnimation(current_weapon_instance, current_weapon.equip_animation);
                } else {
                    current_weapon.can_fire = true;
                }
            }
        }
        if (update_hud is not null) {
            update_hud();
        }
    }

    public override void _UnhandledInput(InputEvent @event) {
        base._UnhandledInput(@event);

        bool updateHudNeeded = false;
        bool leftHasShot = false;
        bool rightHasShot = false;
        bool reloadAttempt = Input.IsActionJustPressed("reload");

        switch ((right_current_weapon?.fireMode, right_current_weapon?.can_fire, Input.IsActionPressed("fire"), Input.IsActionJustPressed("fire"), Input.IsActionJustReleased("fire"))) {
            case (FireMode.Instant, true, true, _, false):
                (_current_right_weapon_instance as IWeapon).Shoot(right_current_weapon, player as Player);
                rightHasShot = true;
                break;
            case (FireMode.Charge, true, _, true, false):
                (_current_right_weapon_instance as IWeapon).Shoot(right_current_weapon, player as Player);
                break;
            case (FireMode.Charge, true, _, false, true):
                if ((_current_right_weapon_instance as IWeapon).ShootReleased(right_current_weapon, player as Player)) {
                    rightHasShot = true;
                }
                break;
            default:
                break;
        }

        switch ((left_current_weapon?.fireMode, left_current_weapon?.can_fire, Input.IsActionPressed("fire_left"), Input.IsActionJustPressed("fire_left"), Input.IsActionJustReleased("fire_left"))) {
            case (FireMode.Instant, true, true, _, false):
                (_current_left_weapon_instance as IWeapon).Shoot(left_current_weapon, player as Player);
                leftHasShot = true;
                break;
            case (FireMode.Charge, true, _, true, false):
                (_current_left_weapon_instance as IWeapon).Shoot(left_current_weapon, player as Player);
                break;
            case (FireMode.Charge, true, _, false, true):
                if ((_current_left_weapon_instance as IWeapon).ShootReleased(left_current_weapon, player as Player)) {
                    leftHasShot = true;
                }
                break;
            default:
                break;
        }

        if (rightHasShot && right_current_weapon is not null) {
            ShootAgainIn(right_current_weapon, 1.0f / right_current_weapon.fire_rate);
            PlayAnimation(_current_right_weapon_instance, right_current_weapon.shoot_animation);
            PlaySound(_current_right_weapon_instance, right_current_weapon.shoot_sound);
            updateHudNeeded = true;
        }
        if (leftHasShot && left_current_weapon is not null) {
            ShootAgainIn(left_current_weapon, 1.0f / left_current_weapon.fire_rate);
            PlayAnimation(_current_left_weapon_instance, left_current_weapon.shoot_animation);
            PlaySound(_current_left_weapon_instance, left_current_weapon.shoot_sound);
            updateHudNeeded = true;
        }
        if (reloadAttempt
                && (right_current_weapon is null || right_current_weapon.can_fire)
                && (left_current_weapon is null || left_current_weapon.can_fire)) {
            if (right_current_weapon is not null) {
                Reload(right_current_weapon);
            }
            if (left_current_weapon is not null) {
                Reload(left_current_weapon);
            }
        }
        if (updateHudNeeded) {
            update_hud?.Invoke();
        }
    }

    public void ChangeWeapon(string weaponPath, EquipSide side) {
        if (weaponPath is null or "") return;
        if (side == EquipSide.Right && right_current_weapon is not null && right_current_weapon.timer is not null) {
            right_current_weapon.timer.QueueFree();
            right_current_weapon.timer = null;
            return;
        }
        if (side == EquipSide.Left && left_current_weapon is not null && left_current_weapon.timer is not null) {
            left_current_weapon.timer.QueueFree();
            left_current_weapon.timer = null;
            return;
        }
        UpdateWeaponModel(weaponPath, side);
    }

    public override void _Ready() {
        UpdateWeaponModel();
    }

    private static void PlayAnimation(Node3D _current_weapon_instance, string animationName) {
        if (_current_weapon_instance is null || animationName is null or "" || !_current_weapon_instance.HasNode("AnimationPlayer")) return;
        var animationPlayer = _current_weapon_instance.GetNode<AnimationPlayer>("AnimationPlayer");
        if (!animationPlayer.HasAnimation(animationName)) return;
        animationPlayer.Queue(animationName);
    }

    private static void PlaySound(Node3D _current_weapon_instance, AudioStream sound) {
        if (_current_weapon_instance is null || sound is null || !_current_weapon_instance.HasNode("AudioStreamPlayer3D")) return;
        var soundPlayer = _current_weapon_instance.GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
        soundPlayer.Stream = sound;
        soundPlayer.Play();
    }

    private void ShootAgainIn(WeaponResource current_weapon, float seconds) {
        if (current_weapon is null) return;
        current_weapon.can_fire = false;
        current_weapon.timer = new Timer() {
            WaitTime = seconds,
            OneShot = true,
            Autostart = true
        };
        current_weapon.timer.Timeout += () => {
            current_weapon.can_fire = true;
            current_weapon.timer?.QueueFree();
            current_weapon.timer = null;
            if (current_weapon.current_ammo <= 0) {
                Reload(current_weapon);
            }
        };
        AddChild(current_weapon.timer);
    }

    private async void Reload(WeaponResource weapon) {
        if (weapon is null) return;
        weapon.can_fire = false;
        weapon.timer = new Timer() {
            WaitTime = weapon.reload_time,
            OneShot = true,
            Autostart = true
        };
        weapon.timer.Timeout += () => {
            weapon.current_ammo = weapon.magazine_size;
            weapon.can_fire = true;
            weapon.timer.QueueFree();
            weapon.timer = null;
            if (update_hud is not null) {
                update_hud();
            }
        };
        AddChild(weapon.timer);
    }
}
