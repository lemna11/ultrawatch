using Legs;

using UI;

using static Utils.Util;

public partial class Player : CharacterBody3D, ITarget {
    [Export]
    public Node3D camera_yaw;

    [Export]
    public Node3D camera_pitch;

    [Export]
    public float move_speed = 6.0f;

    [Export]
    public float crouch_speed = 3.0f;

    [Export]
    public float universal_deccel = 16.0f;

    [Export]
    public float universal_accel = 12.0f;

    [Export]
    public float jump_vert_vel = 4.5f;

    [Export]
    public float mouse_sensitivity = 3.0f;

    [Export]
    public int max_health = 100;

    [Export]
    public Control weapon_selection_menu;
    private LoadoutSelectionMenu _weaponSelectionMenuInstance;

    public int cur_health;

    public Action update_hud;

    private ILegAbility current_legs;

    private string current_body = string.Empty;

    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        if (weapon_selection_menu as LoadoutSelectionMenu is null) throw new ArgumentNullException("weapon_selection_menu must be of type LoadoutSelectionMenu");

        ChangeLegAbility(new DefaultLegs());
        update_hud();

        var weaponManager = GetNode<WeaponManager>("WeaponManager");
        _weaponSelectionMenuInstance = weapon_selection_menu as LoadoutSelectionMenu;
        _weaponSelectionMenuInstance.OnWeaponSelected += weaponManager.ChangeWeapon;
        _weaponSelectionMenuInstance.OnLegSelected += OnLegSelected;
        _weaponSelectionMenuInstance.OnBodySelected += OnBodySelected;
    }

    public override void _ExitTree() {
        if (_weaponSelectionMenuInstance is not null) {
            _weaponSelectionMenuInstance.OnWeaponSelected -= GetNode<WeaponManager>("WeaponManager").ChangeWeapon;
            _weaponSelectionMenuInstance.OnLegSelected -= OnLegSelected;
            _weaponSelectionMenuInstance.OnBodySelected += OnBodySelected;
        }
    }

    public void ChangeLegAbility(ILegAbility newLegs) {
        if (newLegs == null) return;
        current_legs?.Unequip(this);
        current_legs = newLegs;
        current_legs.Equip(this);
    }

    private void OnLegSelected(string legClassName) {
        ILegAbility newLegs = legClassName switch {
            "DefaultLegs" => new DefaultLegs(),
            "StompAndJump" => new StompAndJump(),
            "DoubleJump" => new DoubleJump(),
            "HoverJet" => new HoverJet(),
            "WallJump" => new WallJump(),
            _ => throw new ArgumentException("Invalid leg class name")
        };

        ChangeLegAbility(newLegs);
    }

    // all of these values will end up in resource files to be configurable via
    // the godot editor in the future
    private void OnBodySelected(string bodyName) {
        switch (current_body) {
            case "Light":
                max_health -= 25;
                break;
            case "Medium":
                max_health -= 75;
                break;
            case "Heavy":
                max_health -= 150;
                break;
            default: break;
        }

        current_body = bodyName;

        switch (bodyName) {
            case "Light":
                move_speed = 9.0f;
                crouch_speed = 4.5f;
                max_health += 25;
                break;
            case "Medium":
                move_speed = 6.0f;
                crouch_speed = 3.0f;
                max_health += 75;
                break;
            case "Heavy":
                move_speed = 4.5f;
                crouch_speed = 2.25f;
                max_health += 150;
                break;
            default: break;
        }

        cur_health = max_health;
        update_hud();
    }

    public override void _Process(double delta) {
        base._Process(delta);
    }

    public override void _PhysicsProcess(double delta) {
        HandleMovement(delta);
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured) {
            // default godot sens is absurdly high, so we scale it down
            camera_yaw.RotateY(-mouseMotion.Relative.X * mouse_sensitivity * 0.00166666667f);
            camera_pitch.RotateX(-mouseMotion.Relative.Y * mouse_sensitivity * 0.00166666667f);

            camera_pitch.RotationDegrees = new Vector3(
                Mathf.Clamp(camera_pitch.RotationDegrees.X, -90, 90),
                camera_pitch.RotationDegrees.Y,
                camera_pitch.RotationDegrees.Z
            );
        } else if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
            weapon_selection_menu?.Call("ToggleMenu");
        } else
            base._Input(@event);
    }

    private void HandleMovement(double delta) {
        Vector3 direction = Vector3.Zero;
        Basis cameraBasis = camera_yaw.GlobalTransform.Basis;
        var velocity = Velocity;
        if (Input.IsActionPressed("move_forward"))
            direction -= cameraBasis.Z;
        if (Input.IsActionPressed("move_back"))
            direction += cameraBasis.Z;
        if (Input.IsActionPressed("move_left"))
            direction -= cameraBasis.X;
        if (Input.IsActionPressed("move_right"))
            direction += cameraBasis.X;
        direction = HorizontalNormal(direction);
        var speed = Input.IsActionPressed("move_crouch") ? crouch_speed : move_speed;

        //apply gravity
        if (!IsOnFloor()) {
            Vector3 gravity = GetGravity();
            velocity += gravity * (float)delta;
        }
        //jump case
        if (Input.IsActionPressed("move_jump") && IsOnFloor()) {
            velocity.Y = jump_vert_vel;
        }

        if (IsOnFloor() && HorizontalLenght(velocity) <= move_speed) {//input on floor
            velocity.X = direction.X * speed;
            velocity.Z = direction.Z * speed;
        } else {
            velocity = UniAccelDeccelHandler(velocity, direction, universal_accel, universal_deccel, delta, move_speed);
        }

        current_legs.Apply(this, delta, direction, ref velocity);
        Velocity = velocity;
        MoveAndSlide();
    }

    public void TakeDamage(WeaponResource weapon, float damageAfterModifiers = 0) {
        var damage = damageAfterModifiers != 0 ? damageAfterModifiers : weapon.damage;
        cur_health = 0 > cur_health - damage ? 0 : (int)(cur_health - damage);
        update_hud();
    }
}
