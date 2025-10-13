public partial class WeaponManager : Node3D
{
    [Export]
    public CharacterBody3D player;

    [Export]
    public Node3D weapon_holder;

    [Export]
    public WeaponResource current_weapon;

    private Node3D current_weapon_instance;

    private bool can_fire = true;

    private void UpdateWeaponModel()
    {
        if (current_weapon is not null)
        {
            if (weapon_holder is not null && current_weapon.weapon_model is not null)
            {
                current_weapon_instance = current_weapon.weapon_model.Instantiate<Node3D>();
                if (current_weapon_instance as IWeapon is null)
                {
                    GD.PrintErr("The weapon model does not implement IWeapon interface.");
                    throw new InvalidOperationException("The weapon model does not implement IWeapon interface.");
                }
                current_weapon_instance.Position = current_weapon.weapon_offset;
                weapon_holder.CallDeferred(Node.MethodName.AddChild, current_weapon_instance);
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("fire") && can_fire && current_weapon_instance is not null)
        {
            (current_weapon_instance as IWeapon).Shoot();
            can_fire = false;
            ToSignal(GetTree().CreateTimer(1.0f / current_weapon.fire_rate), Timer.SignalName.Timeout).OnCompleted(() => can_fire = true);
        }

        if (Input.IsKeyPressed(Key.Key1))
        {
            current_weapon = ResourceLoader.Load<WeaponResource>("res://Weapons/Shotgun/Shotgun.tres");
            current_weapon_instance.QueueFree();
            UpdateWeaponModel();
        }
        else if (Input.IsKeyPressed(Key.Key2))
        {
            current_weapon = ResourceLoader.Load<WeaponResource>("res://Weapons/RocketLauncher/RocketLauncher.tres");
            current_weapon_instance.QueueFree();
            UpdateWeaponModel();
        }
    }

    public override void _Ready()
    {
        UpdateWeaponModel();
    }
}
