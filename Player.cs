public partial class Player : CharacterBody3D {
    [Export]
    public Node3D camera_yaw;

    [Export]
    public Node3D camera_pitch;

    [Export]
    public float move_speed = 6.0f;

    [Export]
    public float jump_velocity = 4.5f;

    [Export]    
     public float jump_vert_vel = 4.5f;

    [Export]
    public float jump_hor_accel = 12.0f;

     [Export]
    public float jump_max_velocity_deccel = 3.0f;

    [Export]
    public float mouse_sensitivity = 3.0f;

    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;
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


        direction = direction.Normalized();
        //apply gravity
        if (!IsOnFloor()) {
            Vector3 gravity = GetGravity();
            velocity += (gravity * (float)delta);
        }
        //jump case
        if (Input.IsActionPressed("move_jump") && IsOnFloor()) {
            velocity.Y = jump_vert_vel;
        }
        if ((direction.X != 0 || direction.Z != 0) && IsOnFloor()) {//input on ground
            velocity.X = direction.X * move_speed;
            velocity.Z = direction.Z * move_speed;
        } else if ((direction.X == 0 && direction.Z == 0) && IsOnFloor()) {//no input on ground
            velocity.X = Mathf.MoveToward(velocity.X, 0, move_speed);
            velocity.Z = Mathf.MoveToward(velocity.X, 0, move_speed);

            

        } else if ((direction.X != 0  || direction.Z != 0) && !IsOnFloor()) {//input when airborne
            var attempted_x_vel = velocity.X + direction.X * jump_hor_accel * delta;
            if (velocity.X <= move_speed) {
                if (Mathf.Abs(attempted_x_vel) <= move_speed) {
                    velocity.X = (float)attempted_x_vel;
                } else {
                    velocity.X = move_speed * direction.X;
                }
            } else {
                if (Mathf.Abs(attempted_x_vel) < velocity.X) {
                    velocity.X = (float)attempted_x_vel;
                } else {
                    velocity.X = (float) Mathf.MoveToward(velocity.X, 0, jump_max_velocity_deccel * delta);
                }
            }
            var attempted_z_vel = velocity.Z + direction.Z * jump_hor_accel * delta;
            if (velocity.Z <= move_speed) {
                if (Mathf.Abs(attempted_z_vel) <= move_speed) {
                    velocity.Z = (float)attempted_z_vel;
                } else {
                    velocity.Z = move_speed * direction.Z;
                }
            } else {
                if (Mathf.Abs(attempted_z_vel) < velocity.Z) {
                    velocity.Z = (float)attempted_z_vel;
                } else {
                    velocity.Z = (float) Mathf.MoveToward(velocity.Z, 0, jump_max_velocity_deccel * delta);
                }
            }
        } else if ((direction.X == 0 && direction.Z == 0) && !IsOnFloor()) {//no input when airborne
            if (Mathf.Abs(velocity.X) <= move_speed) {
                velocity.X = (float) Mathf.MoveToward(velocity.X, 0, jump_hor_accel * delta);
            } else {
                velocity.X = (float) Mathf.MoveToward(velocity.X, 0, jump_max_velocity_deccel * delta);
            }
            if (Mathf.Abs(velocity.Z) <= move_speed) {
                velocity.Z = (float) Mathf.MoveToward(velocity.Z, 0, jump_hor_accel * delta);
            } else {
                velocity.Z = (float) Mathf.MoveToward(velocity.Z, 0, jump_max_velocity_deccel * delta);
            }
        }

            Velocity = velocity;
        MoveAndSlide();
    }

}
