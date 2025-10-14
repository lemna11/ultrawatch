public partial class Pellet : Decal {
	public override void _Ready() {
		var timer = GetTree().CreateTimer(5.0f);
		timer.Timeout += QueueFree;
	}
}
