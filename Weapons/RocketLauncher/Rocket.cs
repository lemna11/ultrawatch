public partial class Rocket : Area3D
{
    [Export]
    private Timer life_timer;

    [Export]
    private float move_speed = 10.0f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        life_timer.Timeout += () => QueueFree();
        life_timer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += -GlobalTransform.Basis.Y * (float)(move_speed * delta);
    }

    private void OnBodyEntered(Node body)
    {
        var explosion = new SphereMesh
        {
            Radius = 0.5f
        };
        var explosion_instance = new MeshInstance3D
        {
            Mesh = explosion,
            MaterialOverride = new StandardMaterial3D()
            {
                AlbedoColor = Colors.Orange
            },
            Position = GlobalPosition
        };
        var world = GetTree().CurrentScene;
        world.AddChild(explosion_instance);
        explosion_instance.GetTree().CreateTimer(0.5f).Timeout += () => explosion_instance.QueueFree();
        QueueFree();
    }
}
