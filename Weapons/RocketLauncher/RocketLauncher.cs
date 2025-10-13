public partial class RocketLauncher : Node3D, IWeapon
{
	[Export]
	public Node3D rocket_spawn_point;

	[Export]
	public PackedScene rocket_scene;

	[Export]
	public float fire_rate = 1.0f;

	public void Shoot()
	{
		var rocket_instance = rocket_scene.Instantiate<Node>();
		rocket_spawn_point.AddChild(rocket_instance);
		rocket_instance.Reparent(GetTree().CurrentScene, true);
		GetNode<AnimationPlayer>("AnimationPlayer").Queue("Shoot");
	}
}
