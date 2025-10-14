public partial class Shotgun : Node3D, IWeapon {
    [Export]
    public Node3D pellet_spawn_point;

    [Export]
    public int pellet_count = 6;

    [Export]
    public float spread_angle = 10.0f;

    [Export]
    public float fire_rate = 1.0f;

    [Export]
    public PackedScene decal_scene;

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready() {
        _rng.Randomize();
    }

    private void FirePellet() {
        Vector3 direction = -pellet_spawn_point.GlobalTransform.Basis.Z;

        float yaw = Mathf.DegToRad(_rng.RandfRange(-spread_angle, spread_angle));
        float pitch = Mathf.DegToRad(_rng.RandfRange(-spread_angle, spread_angle));
        Basis spreadBasis = new Basis(Vector3.Up, yaw) * new Basis(Vector3.Right, pitch);
        direction = (spreadBasis * direction).Normalized();

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(pellet_spawn_point.GlobalPosition, pellet_spawn_point.GlobalPosition + direction * 100.0f);
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0) {
            var hitPosition = (Vector3)result["position"];

            if (decal_scene != null) {
                var effect = decal_scene.Instantiate<Node3D>();
                GetTree().CurrentScene.AddChild(effect);
                effect.GlobalPosition = hitPosition;
            }
        }
    }

    public void Shoot(WeaponResource weapon) {
        for (int i = 0; i < pellet_count; i++) {
            FirePellet();
        }
    }

    public void Equip(WeaponResource weapon) {
    }

    public void Unequip(WeaponResource weapon) {
    }
}
