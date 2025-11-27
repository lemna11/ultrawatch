
namespace Legs;

public class StompAndJump : ILegAbility {
    private bool stomp_active = false;
    public float stomp_accel = 60.0f;
    private float stored_jump_vel = 0;
    private readonly float max_vel = 12.0f;
    private readonly float charge_gain = 4.0f;

    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity) {

        if (player.IsOnFloor()) {
            stomp_active = false;
            if (Input.IsActionPressed("move_jump")) {
                if (stored_jump_vel > max_vel) {
                    stored_jump_vel = max_vel;
                }
                velocity.Y = stored_jump_vel;
                stored_jump_vel = player.jump_vert_vel;
            } else if (Input.IsActionPressed("move_crouch")) {
                stored_jump_vel += charge_gain * (float)delta;
            }
        } else {
            if (Input.IsActionJustPressed("move_crouch")) {
                stomp_active = true;
            }

            if (stomp_active) {
                velocity.Y -= stomp_accel * (float)delta;
            }
        }
    }
}
