namespace Maps;

public partial class JumpPad : Area3D
{
    [Export]
    public Vector3 launch_velocity = new Vector3(0, 10, 0);

    public override void _Ready() {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body) {
        if (body is CharacterBody3D characterBody) {
            characterBody.Velocity = launch_velocity;
        }
    }
}
