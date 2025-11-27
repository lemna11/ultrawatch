using System.ComponentModel;

using Legs;

using static Utils.Util;

public partial class Player : CharacterBody3D, ITarget {
    public enum LegType {
        StompAndHighJump,
        DoubleJump,
        HoverJet,
        WallJump
    }

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
    public LegType leg_type;

    public int cur_health;

    public Action update_hud;

    private ILegAbility current_legs;

    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        cur_health = max_health;
        current_legs = leg_type switch {
            LegType.StompAndHighJump => new StompAndJump(),
            LegType.DoubleJump => new DoubleJump(),
            LegType.HoverJet => new HoverJet(),
            LegType.WallJump => new WallJump(),
            _ => throw new InvalidEnumArgumentException()
        };
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
        }
        // quick hack to toggle mouse capture
        else if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
                Input.MouseMode = Input.MouseModeEnum.Visible;
            else
                Input.MouseMode = Input.MouseModeEnum.Captured;
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

    public void TakeDamage(WeaponResource weapon) {
        cur_health = 0 > cur_health - weapon.damage ? 0 : (int)(cur_health - weapon.damage);
        update_hud();
    }
}
