public partial class GrapplingHook : Node3D, IWeapon {
	[Export]
	public PackedScene hook_scene;

	public override void _Ready() {
		if (hook_scene is null) throw new NotSupportedException("Cant instantiate a grappling hook without a hook");
	}

	public override void _Process(double delta) {
	}

	public async void Shoot(WeaponResource weapon, Player player) {
		var hook_spawn_point = weapon.player_camera;
		var hook_instance = hook_scene.Instantiate<Hook>();
		hook_instance.Player = player;
		hook_spawn_point.AddChild(hook_instance);
		await ToSignal(GetTree(), "process_frame");
		hook_instance.Reparent(GetTree().CurrentScene);
	}

	public void Equip(WeaponResource weapon) {
	}

	public void Unequip(WeaponResource weapon) {
	}
}
