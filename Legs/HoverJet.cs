namespace Legs;

using static Utils.Util;

public class HoverJet : ILegAbility {
    private readonly float move_speed = 12.0f;

    public int MaxHealthModifier => 25;

    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity) {
        if (player.IsOnFloor()) {
            velocity.X = player.Velocity.X;
            velocity.Z = player.Velocity.Z;
        }
        velocity = UniAccelDeccelHandler(velocity, direction, player.universal_accel, player.universal_deccel, delta, move_speed);
    }
}
