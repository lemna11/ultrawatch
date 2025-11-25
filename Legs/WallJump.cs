
namespace Legs;

public sealed class WallJump : ILegAbility {
    private bool jmped = false;
    private readonly float normal_vel = 3.0f;
    private readonly float spd_bump = 4.0f;
    private readonly float stick_grvty = 24.0f;

    public void Apply(Player player, double delta, ref Vector3 velocity) {
        Vector3 direction = Vector3.Zero;
        Basis cameraBasis = player.camera_yaw.GlobalTransform.Basis;
        if (Input.IsActionPressed("move_forward"))
            direction -= cameraBasis.Z;
        if (Input.IsActionPressed("move_back"))
            direction += cameraBasis.Z;
        if (Input.IsActionPressed("move_left"))
            direction -= cameraBasis.X;
        if (Input.IsActionPressed("move_right"))
            direction += cameraBasis.X;

        if (!player.IsOnFloor() && player.IsOnWall() && !jmped) {
            velocity.Y = player.Velocity.Y;
            Vector3 wall_normal = player.GetWallNormal();
            velocity -= wall_normal * stick_grvty * (float)delta;
            velocity.Y = Mathf.MoveToward(velocity.Y, 0, player.universal_deccel * (float)delta);
            if (Input.IsActionJustPressed("move_jump") && !jmped) {
                jmped = true;
                velocity += wall_normal * normal_vel;
                velocity += direction * spd_bump;
                velocity.Y = player.jump_vert_vel;
            } else {
                velocity.X -= wall_normal.X * stick_grvty * (float)delta;
                velocity.Z -= wall_normal.Z * stick_grvty * (float)delta;
            }
        } else if (player.IsOnFloor()) {
            jmped = false;
        }
    }
}
