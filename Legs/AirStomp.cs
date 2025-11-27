
namespace Legs;

public class AirStomp : ILegAbility {
    private bool stomp_active = false;
    public float stomp_accel = 60.0f;

    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity) {
        if (player.IsOnFloor()) {
            stomp_active = false;
        }
        if (!player.IsOnFloor() && Input.IsActionJustPressed("move_crouch")) {
            stomp_active = true;
        }

        if (!player.IsOnFloor() && stomp_active) {
            velocity.Y -= stomp_accel * (float)delta;
        }
    }
}
