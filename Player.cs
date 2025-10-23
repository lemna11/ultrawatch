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
        Vector3 new_vect = velocity_vector;
        Vector3 return_vect = Vector3.Zero;
        new_vect.X = Mathf.MoveToward(velocity_vector.X, 0, (float)Mathf.Abs(orientation.X * deccel));
        new_vect.Z = Mathf.MoveToward(velocity_vector.Z, 0, (float)Mathf.Abs(orientation.Z * deccel));
        Vector3 new_vect_normal = HorNormalHelper(new_vect);
        return_vect.X = new_vect_normal.X * (float)(HorLenHelper(velocity_vector) - deccel); // I have no fucking idea why I have to put the *0.5 there. If I dont put it there the decceleration is twice the deccel intended when counterstrafing
        return_vect.Z = new_vect_normal.Z * (float)(HorLenHelper(velocity_vector) - deccel);
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
        var input_foward = Input.IsActionPressed("move_forward");
        var input_back = Input.IsActionPressed("move_back");
        var input_left = Input.IsActionPressed("move_left");
        var input_right = Input.IsActionPressed("move_right");
        var any_input = input_foward || input_back || input_left || input_right;
        var no_input = !any_input;
        var input_crouch = Input.IsActionPressed("move_crouch");
        var velocity = Velocity;
        if (input_foward)
            direction -= cameraBasis.Z;
        if (input_back)
            direction += cameraBasis.Z;
        if (input_left)
            direction -= cameraBasis.X;
        if (input_right)
            direction += cameraBasis.X;
        direction = direction.Normalized();
        Vector3 orientation = Vector3.Zero;
        orientation -= cameraBasis.Z;
        Vector3 quarter_cirle_right_rotated_orientation = cameraBasis.X;
        //apply gravity
        if (!IsOnFloor()) {
            Vector3 gravity = GetGravity();
            velocity += (gravity * (float)delta);
        }
        if (stomp_active && IsOnFloor()) {
            stomp_active = false;
            crouch_locked = true;
        }
        if (!input_crouch && crouch_locked) {
            crouch_locked = false;
        }
        //jump case
        if (Input.IsActionPressed("move_jump") && IsOnFloor()) {
            velocity.Y = jump_vert_vel;
        }
        if (any_input && IsOnFloor()) {//input on ground
            if (HorLenHelper(velocity) <= move_speed) {
                velocity.X = direction.X * move_speed;
                velocity.Z = direction.Z * move_speed;
            }


            if (input_crouch) {
                Vector3 candidate_vector = velocity;
                if (input_foward) {
                    candidate_vector += orientation * crouch_slide_accel * (float)delta;
                } else if (input_back) {
                    candidate_vector -= orientation * crouch_slide_accel * (float)delta;
                } else {
                    candidate_vector = KillMomentumOnAxisHelper(candidate_vector, orientation, universal_deccel * delta);
                }


                if (input_right) {
                    candidate_vector += quarter_cirle_right_rotated_orientation * crouch_slide_accel * (float)delta;
                } else if (input_left) {
                    candidate_vector -= quarter_cirle_right_rotated_orientation * crouch_slide_accel * (float)delta;
                } else {
                    candidate_vector = KillMomentumOnAxisHelper(candidate_vector, quarter_cirle_right_rotated_orientation, universal_deccel * delta);
                }

                if (HorLenHelper(candidate_vector) <= crouch_slide_velocity) {
                    velocity = candidate_vector;
                } else {
                    candidate_vector = RescaleVector1ToVector2Helper(candidate_vector, velocity);
                    var adj_factor = Mathf.MoveToward(HorLenHelper(candidate_vector), crouch_slide_velocity, universal_deccel * delta);
                    candidate_vector = HorNormalHelper(candidate_vector) * (float)adj_factor;
                    candidate_vector.Y = velocity.Y;
                    velocity = candidate_vector;
                }
            } else if (HorLenHelper(velocity) > move_speed) {
                Vector3 candidate_vector = velocity;
                if (input_foward) {
                    candidate_vector += orientation * jump_hor_accel * (float)delta;
                } else if (input_back) {
                    candidate_vector -= orientation * jump_hor_accel * (float)delta;
                } else {
                    candidate_vector = KillMomentumOnAxisHelper(candidate_vector, orientation, universal_deccel * delta);
                }


                if (input_right) {
                    candidate_vector += quarter_cirle_right_rotated_orientation * jump_hor_accel * (float)delta;
                } else if (input_left) {
                    candidate_vector -= quarter_cirle_right_rotated_orientation * jump_hor_accel * (float)delta;
                } else {
                    candidate_vector = KillMomentumOnAxisHelper(candidate_vector, quarter_cirle_right_rotated_orientation, universal_deccel * delta);
                }

                if (HorLenHelper(candidate_vector) <= move_speed) {
                    velocity = candidate_vector;
                } else {
                    candidate_vector = RescaleVector1ToVector2Helper(candidate_vector, velocity);
                    var adj_factor = Mathf.MoveToward(HorLenHelper(candidate_vector), move_speed, universal_deccel * delta);
                    candidate_vector = HorNormalHelper(candidate_vector) * (float)adj_factor;
                    candidate_vector.Y = velocity.Y;
                    velocity = candidate_vector;
                }
            }

        } else if (no_input && IsOnFloor()) {//no input on ground
            if (HorLenHelper(velocity) <= move_speed + 0.1f) {
                velocity.X = 0;
                velocity.Z = 0;
            } else {
                velocity = KillMomentumProportionalHelper(velocity, universal_deccel * delta);
            }
        } else if (any_input && !IsOnFloor()) {//input when airborne
            if (input_crouch || stomp_active) {
                velocity.Y -= stomp_accel * (float)delta;
                stomp_active = true;
            }
            Vector3 candidate_vector = velocity;
            int foward_input = 0;
            int side_input = 0;
            bool foward_accel = false;
            bool side_accel = false;
            if (input_foward) {
                foward_input++;
            }
            if (input_back) {
                foward_input--;
            }
            if (input_right) {
                side_input++;
            }
            if (input_left) {
                side_input--;
            }
            if (foward_input != 0) {
                if (!IsCounterStrafingHelper(orientation * foward_input, velocity)) {
                    foward_accel = true;
                }
            }
            if (side_input != 0) {
                if (!IsCounterStrafingHelper(quarter_cirle_right_rotated_orientation * side_input, velocity)) {
                    side_accel = true;
                }
            }
            //GD.Print(foward_accel);
            //GD.Print(side_accel);
            if (foward_accel && side_accel) {
                candidate_vector = candidate_vector + (orientation * (float)delta * foward_input * jump_hor_accel);
                candidate_vector = candidate_vector + (quarter_cirle_right_rotated_orientation * (float)delta * side_input * jump_hor_accel);
                var new_vel_normal = HorNormalHelper(candidate_vector);
                velocity = new_vel_normal * (float)(HorLenHelper(velocity) + (jump_hor_accel * delta));
                velocity.Y = candidate_vector.Y;
            } else {
                if (foward_accel) {
                    candidate_vector = candidate_vector + (orientation * ((float)delta * foward_input * jump_hor_accel));
                    var new_vel_normal = HorNormalHelper(candidate_vector);
                    candidate_vector = new_vel_normal * ((float)(HorLenHelper(velocity) + (jump_hor_accel * delta)));
                    candidate_vector.Y = velocity.Y;
                } else {
                    candidate_vector = KillMomentumOnAxisHelper(candidate_vector, orientation, universal_deccel * delta);
                }
                if (side_accel) {
                    candidate_vector = candidate_vector + (quarter_cirle_right_rotated_orientation * ((float)delta * side_input * jump_hor_accel));
                    var new_vel_normal = HorNormalHelper(candidate_vector);
                    candidate_vector = new_vel_normal * (float)(HorLenHelper(velocity) + (jump_hor_accel * delta));
                    candidate_vector.Y = velocity.Y;
                } else {
                    candidate_vector = KillMomentumOnAxisHelper(candidate_vector, quarter_cirle_right_rotated_orientation, universal_deccel * delta);
                }

                velocity = candidate_vector;
            }


        } else if (no_input && !IsOnFloor()) {//no input when airborne			
            velocity = KillMomentumProportionalHelper(velocity, universal_deccel * delta);



            if (input_crouch || stomp_active) {
                velocity.Y -= stomp_accel * (float)delta;
                stomp_active = true;
            }
        }
        var total_vel = HorLenHelper(velocity);
        var delta_vel = Mathf.Abs(HorLenHelper(Velocity) - total_vel);
        var delta_x = Mathf.Abs(Velocity.X - velocity.X);
        var delta_z = Mathf.Abs(Velocity.Z - velocity.Z);
        GD.Print(total_vel);
        GD.Print(velocity);
        GD.Print(delta_vel);
        GD.Print(delta_x);
        GD.Print(delta_z);

        Velocity = velocity;
        MoveAndSlide();
    }

}
