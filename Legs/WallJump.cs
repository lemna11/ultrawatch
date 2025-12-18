namespace Legs;

public sealed class WallJump : ILegAbility {
    private readonly float normal_vel = 3.0f;
    private readonly float spd_bump = 4.0f;
    private readonly float stick_grvty = 24.0f;
    public int MaxHealthModifier => 0;

    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity) {
        if (!player.IsOnFloor() && player.IsOnWall()) {
            velocity.Y = player.Velocity.Y;
            Vector3 wall_normal = player.GetWallNormal();
            velocity -= wall_normal * stick_grvty * (float)delta;
            velocity.Y = Mathf.MoveToward(velocity.Y, 0, player.universal_deccel * (float)delta);
            if (Input.IsActionJustPressed("move_jump")) {
                velocity += wall_normal * normal_vel;
                velocity += direction * spd_bump;
                velocity.Y = player.jump_vert_vel;
            } else {
                velocity.X -= wall_normal.X * stick_grvty * (float)delta;
                velocity.Z -= wall_normal.Z * stick_grvty * (float)delta;
            }
        }
    }

    public void Equip(Player player) {
        player.max_health += MaxHealthModifier;
        player.cur_health = player.max_health;
        player.update_hud?.Invoke();
    }

    public void Unequip(Player player) {
        player.max_health -= MaxHealthModifier;
        player.cur_health = player.max_health;
        player.update_hud?.Invoke();
    }
}
