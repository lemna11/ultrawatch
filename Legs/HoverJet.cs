
namespace Legs;

public class HoverJet : ILegAbility {
    private readonly float move_speed = 12.0f;

    public void Apply(Player player, double delta, ref Vector3 velocity) {
        Vector3 direction = Vector3.Zero;
        Basis cameraBasis = player.camera_yaw.GlobalTransform.Basis;
        bool input_foward = Input.IsActionPressed("move_forward");
        bool input_back = Input.IsActionPressed("move_back");
        bool input_left = Input.IsActionPressed("move_left");
        bool input_right = Input.IsActionPressed("move_right");
        var any_input = input_foward || input_back || input_left || input_right;
        var no_input = !any_input;

        if (input_foward)
            direction -= cameraBasis.Z;
        if (input_back)
            direction += cameraBasis.Z;
        if (input_left)
            direction -= cameraBasis.X;
        if (input_right)
            direction += cameraBasis.X;
        direction = Player.HorNormalHelper(direction);

        // this shit took me a while to figure out
        if (player.IsOnFloor()) {
            velocity.X = player.Velocity.X;
            velocity.Z = player.Velocity.Z;
        }
        if (no_input && player.IsOnFloor()) {
            velocity = Player.KillMomentumProportionalHelper(velocity, player.universal_deccel * delta);
        } else if (any_input && player.IsOnFloor()) {
            velocity = Player.UniAccelDeccelHandler(velocity, direction, player.jump_hor_accel, player.universal_deccel, delta, move_speed);
        }
    }
}
