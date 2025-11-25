namespace Legs;

public class ChargeJump : ILegAbility {
    private float stored_jump_vel = 0;
    private readonly float max_vel = 12.0f;
    private readonly float charge_gain = 4.0f;

    public void Apply(Player player, double delta, ref Vector3 velocity) {
        var no_input = !(Input.IsActionPressed("move_forward")
                        || Input.IsActionPressed("move_back")
                        || Input.IsActionPressed("move_left")
                        || Input.IsActionPressed("move_right"));

        if (stored_jump_vel < player.jump_vert_vel) {
            stored_jump_vel = player.jump_vert_vel;
        }

        if (Input.IsActionPressed("move_jump") && player.IsOnFloor()) {
            if (stored_jump_vel > max_vel) {
                stored_jump_vel = max_vel;
            }
            velocity.Y = stored_jump_vel;
            stored_jump_vel = player.jump_vert_vel;
        } else if (no_input && player.IsOnFloor() && Input.IsActionPressed("move_crouch")) {
            stored_jump_vel += charge_gain * (float)delta;
        }
    }
}
