namespace Legs;

public class DoubleJump : ILegAbility {
    private bool jump_used = false;

    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity) {
        var input_jmp = Input.IsActionJustPressed("move_jump");

        if (player.IsOnFloor() && input_jmp) {
            jump_used = false;
        } else {
            if (!jump_used && input_jmp) {
                velocity.Y = player.jump_vert_vel;
                jump_used = true;
                if (direction != Vector3.Zero) {
                    velocity.X = direction.X * player.move_speed;
                    velocity.Z = direction.Z * player.move_speed;
                }
            }
        }
    }
}
