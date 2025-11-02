public partial class WeaponManager : Node3D {
    private enum EquipSide {
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

    private Node3D _current_weapon_instance;

    private Timer _timer;

    private async void UpdateWeaponModel(string weaponPath = null, EquipSide side = EquipSide.Right) {
        if (weaponPath != null) {
            switch (side) {
                case EquipSide.Right:
                    right_current_weapon = ResourceLoader.Load<WeaponResource>(weaponPath);
                    break;
                case EquipSide.Left:
                    left_current_weapon = ResourceLoader.Load<WeaponResource>(weaponPath);
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
                EquipSide.Left => right_current_weapon.left_arm_weapon_offset,
                _ => throw new NotSupportedException()
            };
            if (_current_weapon_instance is not null) {
                (_current_weapon_instance as IWeapon).Unequip(current_weapon);
                _current_weapon_instance.QueueFree();
                _timer?.Stop();
                _timer?.QueueFree();
            }
            if (weapon_holder is not null && current_weapon.weapon_model is not null) {
                current_weapon.player_camera = player.GetNode<Node3D>("CameraYaw/CameraPitch/Camera3D");
                _current_weapon_instance = current_weapon.weapon_model.Instantiate<Node3D>();
                if (_current_weapon_instance as IWeapon is null) {
                    GD.PrintErr("The weapon model does not implement IWeapon interface.");
                    throw new InvalidOperationException("The weapon model does not implement IWeapon interface.");
                }
                _current_weapon_instance.Position = weapon_position;
                weapon_holder.CallDeferred(Node.MethodName.AddChild, _current_weapon_instance);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                current_weapon.can_fire = false;
                (_current_weapon_instance as IWeapon).Equip(current_weapon);
                if (_current_weapon_instance.HasNode("AnimationPlayer")
                    && current_weapon.equip_animation is not null or ""
                    && _current_weapon_instance.GetNode<Node>("AnimationPlayer") is AnimationPlayer animationPlayer
                    && animationPlayer.HasAnimation(current_weapon.equip_animation)) {
                    var waitTime = animationPlayer.GetAnimation(current_weapon.equip_animation).Length;
                    ShootAgainIn(waitTime);
                    PlayAnimation(current_weapon.equip_animation);
                } else {
                    current_weapon.can_fire = true;
                }
            }
        }
    }

    public override void _Process(double delta) {
        if (Input.IsActionPressed("fire") && right_current_weapon.can_fire && _current_weapon_instance is not null) {
            (_current_weapon_instance as IWeapon).Shoot(right_current_weapon, player as Player);
            ShootAgainIn(1.0f / right_current_weapon.fire_rate);
            PlayAnimation(right_current_weapon.shoot_animation);
            PlaySound(right_current_weapon.shoot_sound);
        }

        if (Input.IsKeyPressed(Key.Key1) && right_current_weapon?.can_fire is true or null && _current_weapon_instance is not Shotgun) {
            UpdateWeaponModel("res://Weapons/Shotgun/Shotgun.tres");
        } else if (Input.IsKeyPressed(Key.Key2) && right_current_weapon?.can_fire is true or null && _current_weapon_instance is not RocketLauncher) {
            UpdateWeaponModel("res://Weapons/RocketLauncher/RocketLauncher.tres");
        } else if (Input.IsKeyPressed(Key.Key3) && right_current_weapon?.can_fire is true or null && _current_weapon_instance is not GrapplingHook) {
            UpdateWeaponModel("res://Weapons/GrapplingHook/GrapplingHook.tres", EquipSide.Left);
        }
    }

    public override void _Ready() {
        UpdateWeaponModel();
    }

    private void PlayAnimation(string animationName) {
        if (_current_weapon_instance is null || animationName is null or "" || !_current_weapon_instance.HasNode("AnimationPlayer")) return;
        var animationPlayer = _current_weapon_instance.GetNode<AnimationPlayer>("AnimationPlayer");
        if (!animationPlayer.HasAnimation(animationName)) return;
        animationPlayer.Queue(animationName);
    }

    private void PlaySound(AudioStream sound) {
        if (_current_weapon_instance is null || sound is null || !_current_weapon_instance.HasNode("AudioStreamPlayer3D")) return;
        var soundPlayer = _current_weapon_instance.GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
        soundPlayer.Stream = sound;
        soundPlayer.Play();
    }

    private void ShootAgainIn(float seconds) {
        if (right_current_weapon is null) return;
        right_current_weapon.can_fire = false;
        _timer = new Timer() {
            WaitTime = seconds,
            OneShot = true,
            Autostart = true
        };
        _timer.Timeout += () => {
            right_current_weapon.can_fire = true;
            _timer.QueueFree();
            _timer = null;
        };
        AddChild(_timer);
    }
}
