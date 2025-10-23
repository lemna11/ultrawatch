public partial class Rocket : RigidBody3D {
	[Export]
	private Timer _life_timer;

	[Export]
	private float _move_speed = 10.0f;

	[Export]
	private Area3D _explosion_area;

	public WeaponResource weapon;

	public override void _Ready() {
		LinearVelocity = -GlobalTransform.Basis.Y * _move_speed;
		BodyEntered += OnBodyEntered;
		_life_timer.Timeout += () => QueueFree();
		_life_timer.Start();
	}

	private void OnBodyEntered(Node body) {
		var explosion = new SphereMesh {
			Radius = 0.5f
		};
		var explosion_instance = new MeshInstance3D {
			Mesh = explosion,
			MaterialOverride = new StandardMaterial3D() {
				AlbedoColor = Colors.Orange
			},
			Position = GlobalPosition
		};
		foreach (var item in _explosion_area.GetOverlappingBodies()) {
			HandleExplosion(item);
		}
		var world = GetTree().CurrentScene;
		world.AddChild(explosion_instance);
		explosion_instance.GetTree().CreateTimer(0.5f).Timeout += () => explosion_instance.QueueFree();
		QueueFree();
	}

	private void HandleExplosion(Node3D body) {
		if (body is null) return;
		Vector3 dir = body.GlobalPosition - GlobalPosition;
		float dist = (float)dir.Length();
		const float impulseMultiplier = 2f;
		float effectiveDist = Math.Max(0.5f, dist);
		float impulse = weapon.damage * impulseMultiplier / effectiveDist;

		if (body is ITarget target) {
			target.TakeDamage(weapon);
		}

		if (body is RigidBody3D rb) {
			rb.ApplyCentralImpulse(dir * impulse);
		} else if (body is CharacterBody3D cb) {
			cb.Velocity += dir * impulse;
		}
	}
}
