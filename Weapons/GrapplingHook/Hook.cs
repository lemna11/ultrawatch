public partial class Hook : RigidBody3D {
    [Export]
    private float _move_speed;

    [Export]
    private Timer _life_timer;

    [Export]
    private float _initial_pull_strength = 5f;

    [Export]
    private float _decay_rate = 30f;

    [Export]
    private float _upward_weight = 0.5f;

    [Export]
    private float _min_strength_threshold = 1f;

    public Player Player { get; set; }

    private Node3D _hookTarget;
    private bool _isHooked;
    private float _current_strength;

    public override void _Ready() {
        if (Player is null) throw new NotSupportedException("Cant have hook without player attached to it");
        LinearVelocity = -GlobalTransform.Basis.Z * _move_speed;
        BodyEntered += OnBodyEntered;
        _life_timer.Timeout += () => QueueFree();
        _life_timer.Start();
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (!_isHooked) return;
        if (Player == null || _hookTarget == null || !IsInstanceValid(_hookTarget)) {
            Detach();
            return;
        }

        var toTarget = _hookTarget.GlobalTransform.Origin - Player.GlobalTransform.Origin;
        if (toTarget.Length() < 0.01f) {
            Detach();
            return;
        }

        var dir = toTarget.Normalized();
        dir = (dir + Vector3.Up * _upward_weight).Normalized();

        var desired = dir * _current_strength;

        ApplyPullToPlayer(desired);

        _current_strength = MathF.Max(0f, _current_strength - _decay_rate * (float)delta);
        if (_current_strength <= _min_strength_threshold) Detach();
    }

    private void OnBodyEntered(Node node) {
        if (node is ITarget || node is Player) {
            if (node is Node3D nd) _hookTarget = nd;
            else _hookTarget = node.GetParent<Node3D>();

            if (_hookTarget == null) return;

            _isHooked = true;
            _current_strength = _initial_pull_strength;

            LinearVelocity = Vector3.Zero;
        }
    }

    private void ApplyPullToPlayer(Vector3 desiredVelocity) {
        Player.Velocity += desiredVelocity;
        Player.MoveAndSlide();
    }

    private void Detach() {
        _isHooked = false;
        _hookTarget = null;
        QueueFree();
    }
}
