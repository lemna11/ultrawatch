using System.Runtime.CompilerServices;

using Godot.NativeInterop;

public partial class Player : CharacterBody3D {
    [Export]
    public Node3D camera_yaw;

    [Export]
    public Node3D camera_pitch;

    [Export]
    public float move_speed = 6.0f;

    [Export]
    public float crouch_slide_velocity = 12.0f;

    [Export]
    public float crouch_slide_accel = 18.0f;

    [Export]
    public float universal_deccel = 12.0f;

    [Export]
    public float jump_velocity = 4.5f;

    [Export]
    public float jump_vert_vel = 4.5f;

    [Export]
    public float jump_hor_accel = 12.0f;

    [Export]
    public float stomp_accel = 60.0f;

    [Export]
    public float mouse_sensitivity = 3.0f;

    [Export]
    public bool double_jmp_enabled = true;

    [Export]
    public bool stomp_enabled = true;
    private bool double_jmp_active = false;

    private bool initial_jmp_press = false;
    private bool initial_jmp_release = false;
    private bool crouch_locked = false;

    private bool stomp_active = false;

    public float HorLenHelper(Vector3 input) {
        return Mathf.Sqrt(input.X * input.X + input.Z * input.Z);
    }
    public Vector3 KillMomentumProportionalHelper(Vector3 velocity_vector, double deccel) {
        var vect_len = HorLenHelper(velocity_vector);
        float new_vect_len = (float)Mathf.MoveToward(vect_len, 0, deccel);
        var vect_normal = HorNormalHelper(velocity_vector);
        var new_velocity = vect_normal * new_vect_len;
        new_velocity.Y = velocity_vector.Y;
        return new_velocity;
    }
    public Vector3 KillMomentumOnAxisHelper(Vector3 velocity_vector, Vector3 orientation, double deccel) {
        var vect_len = HorLenHelper(velocity_vector);
        Vector3 new_vect = velocity_vector;
        Vector3 return_vect = Vector3.Zero;
        double deccel_vel_x = (1 - Mathf.Abs(orientation.X)) * deccel;
        double deccel_vel_z = (1 - Mathf.Abs(orientation.Z)) * deccel;
        new_vect.X = (float)Mathf.MoveToward(velocity_vector.X, 0, deccel_vel_x);
        new_vect.Z = (float)Mathf.MoveToward(velocity_vector.Z, 0, deccel_vel_z);
        Vector3 new_vect_normal = HorNormalHelper(new_vect);
        float return_vect_scalar = (float)Mathf.MoveToward(vect_len, 0, deccel);
        return_vect.X = new_vect_normal.X * return_vect_scalar; // I have no fucking idea why I have to put the *0.5 there. If I dont put it there the decceleration is twice the deccel intended when counterstrafing
        return_vect.Z = new_vect_normal.Z * return_vect_scalar;
        return_vect.Y = velocity_vector.Y;
        return return_vect;

    }

    public Vector3 RescaleVector1ToVector2Helper(Vector3 vect1, Vector3 vect2) {
        Vector2 relevant_dimensions1 = Vector2.Zero;
        relevant_dimensions1.X = vect1.X;
        relevant_dimensions1.Y = vect1.Z;
        Vector2 relevant_dimensions2 = Vector2.Zero;
        relevant_dimensions2.X = vect2.X;
        relevant_dimensions2.Y = vect2.Z;
        Vector2 scaled_relevant_dimensions = relevant_dimensions1.Normalized() * relevant_dimensions2.Length();
        Vector3 out_vect = Vector3.Zero;
        out_vect.X = scaled_relevant_dimensions.X;
        out_vect.Y = vect1.Y;
        out_vect.Z = scaled_relevant_dimensions.Y;
        return out_vect;
    }

    public Vector3 HorNormalHelper(Vector3 input) {
        Vector2 hor_vect = Vector2.Zero;
        hor_vect.X = input.X;
        hor_vect.Y = input.Z;
        hor_vect = hor_vect.Normalized();
        Vector3 output = Vector3.Zero;
        output.X = hor_vect.X;
        output.Z = hor_vect.Y;
        return output;
    }

    private bool IsCounterStrafingHelper(Vector3 orientation, Vector3 velocity) {
        if (velocity == Vector3.Zero) {
            return false;
        }
        Vector3 diff_vect = orientation - HorNormalHelper(velocity);
        double angle = Mathf.Asin((HorLenHelper(diff_vect) / 2)) * 2;
        if (Mathf.Abs(angle) <= Mathf.Pi / 2) {
            return false;
        }
        return true;
    }

    private Vector3 UniAccelDeccelHandler(Vector3 velocity, bool[] inputs, Vector3 direction, Vector3 foward_orientaion, Vector3 right_orientation, float accel, float deccel, double delta, float max_spd) {
        bool input_foward = inputs[0];
        bool input_back = inputs[1];
        bool input_left = inputs[2];
        bool input_right = inputs[3];
        int foward_axis = 0;
        int side_axis = 0;
        bool foward_accel = false;
        bool side_accel = false;
        if (input_foward) {
            foward_axis++;
        }
        if (input_back) {
            foward_axis--;
        }
        if (input_left) {
            side_axis--;
        }
        if (input_right) {
            side_axis++;
        }
        if (foward_axis != 0) {
            if (!IsCounterStrafingHelper(foward_orientaion * foward_axis, velocity)) {
                foward_accel = true;
            } else {
                foward_axis = -foward_axis;
            }
        }
        if (side_axis != 0) {
            if (!IsCounterStrafingHelper(right_orientation * side_axis, velocity)) {
                side_accel = true;
            } else {
                side_axis = -side_axis;
            }
        }
        if (foward_accel && side_accel) {
            velocity += direction * accel * (float)delta;  
        } else if (!foward_accel && !side_accel) {
            velocity = KillMomentumProportionalHelper(velocity, deccel * delta);
        } else {
            if (foward_accel) {
                velocity += foward_orientaion * (float)(foward_axis * delta * accel);
            } else {
                velocity.X -= (foward_orientaion.X * ((float)(foward_axis * delta * deccel)));
                velocity.Z -= (foward_orientaion.Z * ((float)(foward_axis * delta * deccel)));
            }
            if (side_accel) {
                velocity += right_orientation * (float)(side_axis * delta * accel);
            } else {
                velocity.X -= (right_orientation.X * ((float)(side_axis * delta * deccel)));
                velocity.Z -= (right_orientation.Z * ((float)(side_axis * delta * deccel)));
            }
        }
        if (HorLenHelper(velocity) > max_spd) {
            GD.Print("max speed triggered");
            velocity = KillMomentumProportionalHelper(velocity, deccel * delta);
        }
        return velocity;
    }

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
        bool input_foward = Input.IsActionPressed("move_forward");
        bool input_back = Input.IsActionPressed("move_back");
        bool input_left = Input.IsActionPressed("move_left");
        bool input_right = Input.IsActionPressed("move_right");
        bool input_crouch = Input.IsActionPressed("move_crouch");
        bool input_jmp = Input.IsActionPressed("move_jump");
        bool[] inputs = { input_foward, input_back, input_left, input_right };
        var any_input = input_foward || input_back || input_left || input_right;
        var no_input = !any_input;
        var velocity = Velocity;
        if (input_foward)
            direction -= cameraBasis.Z;
        if (input_back)
            direction += cameraBasis.Z;
        if (input_left)
            direction -= cameraBasis.X;
        if (input_right)
            direction += cameraBasis.X;
        direction = HorNormalHelper(direction);
        Vector3 orientation = Vector3.Zero;
        orientation -= cameraBasis.Z;
        Vector3 quarter_cirle_right_rotated_orientation = cameraBasis.X;
        //apply gravity
        if (!IsOnFloor()) {
            Vector3 gravity = GetGravity();
            velocity += (gravity * (float)delta);
        }
        //jump case
        if (Input.IsActionPressed("move_jump") && IsOnFloor()) {
            velocity.Y = jump_vert_vel;
        }

        if (no_input && IsOnFloor()) {//no input on floor
            stomp_active = false;
            double_jmp_active = false;
            initial_jmp_press = false;
            initial_jmp_release = false;
            if (HorLenHelper(velocity) > move_speed) {
                velocity = UniAccelDeccelHandler(velocity, inputs, direction, orientation, quarter_cirle_right_rotated_orientation, 0, universal_deccel, delta, move_speed);
            } else {
                velocity.X = 0;
                velocity.Z = 0;
            }
        } else if (any_input && IsOnFloor()) {//no input on floor
            stomp_active = false;
            double_jmp_active = false;
            initial_jmp_press = false;
            initial_jmp_release = false;
            if (HorLenHelper(velocity) > move_speed) {
                velocity = UniAccelDeccelHandler(velocity, inputs, direction, orientation, quarter_cirle_right_rotated_orientation, 0, universal_deccel, delta, move_speed);
            } else {
                velocity.X = direction.X * move_speed;
                velocity.Z = direction.Z * move_speed;
            }
        } else if (no_input && !IsOnFloor()) {//no input when airborne		
            if (input_crouch && stomp_enabled) {
                velocity.Y -= stomp_accel * (float)delta;
                stomp_active = true;
            }
            if (input_jmp && !initial_jmp_press) {
                initial_jmp_press = true;
            }
            if(!input_jmp && initial_jmp_press) {
                initial_jmp_release = true;
            }
            if(!double_jmp_active && input_jmp && initial_jmp_release) {
                velocity.Y = jump_vert_vel;
                double_jmp_active = true;
            }
            velocity = KillMomentumProportionalHelper(velocity, universal_deccel * delta);
        } else if (any_input && !IsOnFloor()) {//input when airborne
            if (input_crouch && stomp_enabled) {
                velocity.Y -= stomp_accel * (float)delta;
                stomp_active = true;
            }
            if (input_crouch && stomp_enabled) {
                velocity.Y -= stomp_accel * (float)delta;
                stomp_active = true;
            }
            if (input_jmp && !initial_jmp_press) {
                initial_jmp_press = true;
            }
            if(!input_jmp && initial_jmp_press) {
                initial_jmp_release = true;
            }
            if (!double_jmp_active && input_jmp && initial_jmp_release) {
                velocity.Y = jump_vert_vel;
                double_jmp_active = true;
                velocity.X = direction.X * move_speed;
                velocity.Z = direction.Z * move_speed;
            } else {
                velocity = UniAccelDeccelHandler(velocity, inputs, direction, orientation, quarter_cirle_right_rotated_orientation, jump_hor_accel, universal_deccel, delta, move_speed);
            }
        }
        var total_vel = HorLenHelper(velocity);
        var delta_vel = Mathf.Abs(HorLenHelper(Velocity) - total_vel);
        var delta_x = Mathf.Abs(Velocity.X - velocity.X);
        var delta_z = Mathf.Abs(Velocity.Z - velocity.Z);
        GD.Print(delta_vel);
        Velocity = velocity;
        MoveAndSlide();
    }

}
