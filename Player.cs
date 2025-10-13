public partial class Player : CharacterBody3D
{
    [Export]
    public Node3D camera_yaw;

    [Export]
    public Node3D camera_pitch;

    [Export]
    public float move_speed = 5.0f;

    [Export]
    public float jump_velocity = 4.5f;

    [Export]
    public float mouse_sensitivity = 3.0f;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement(delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
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
        else if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
                Input.MouseMode = Input.MouseModeEnum.Visible;
            else
                Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else
            base._Input(@event);
    }

    private void HandleMovement(double delta)
    {
        Vector3 direction = Vector3.Zero;
        Basis cameraBasis = camera_yaw.GlobalTransform.Basis;

        if (Input.IsActionPressed("move_forward"))
            direction -= cameraBasis.Z;
        if (Input.IsActionPressed("move_back"))
            direction += cameraBasis.Z;
        if (Input.IsActionPressed("move_left"))
            direction -= cameraBasis.X;
        if (Input.IsActionPressed("move_right"))
            direction += cameraBasis.X;

        direction = direction.Normalized();
        Velocity = direction * move_speed;

        if (Input.IsActionPressed("move_jump") && IsOnFloor())
            Velocity += Vector3.Up * jump_velocity;

        MoveAndSlide();
    }

}
