using System;

namespace Legs;

public class DoubleJump : ILegAbility {
    private bool jump_used = false;

    public void Apply(Player player, double delta, ref Vector3 velocity) {
        var input_jmp = Input.IsActionJustPressed("move_jump");
        Vector3 direction = Vector3.Zero;
        Basis cameraBasis = player.camera_yaw.GlobalTransform.Basis;
        bool input_foward = Input.IsActionPressed("move_forward");
        bool input_back = Input.IsActionPressed("move_back");
        bool input_left = Input.IsActionPressed("move_left");
        bool input_right = Input.IsActionPressed("move_right");
        var any_input = input_foward || input_back || input_left || input_right;

        if (input_foward)
            direction -= cameraBasis.Z;
        if (input_back)
            direction += cameraBasis.Z;
        if (input_left)
            direction -= cameraBasis.X;
        if (input_right)
            direction += cameraBasis.X;
        direction = Player.HorNormalHelper(direction);

        if (player.IsOnFloor() && input_jmp) {
            jump_used = false;
        } else {
            if (!jump_used && input_jmp) {
                velocity.Y = player.jump_vert_vel;
                jump_used = true;
                if (any_input) {
                    velocity.X = direction.X * player.move_speed;
                    velocity.Z = direction.Z * player.move_speed;
                }
            }
        }
    }
}
