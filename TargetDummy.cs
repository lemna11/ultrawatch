public partial class TargetDummy : RigidBody3D, ITarget {
    [Export]
    public Label3D label;

    [Export]
    public int health = 100;

    private int _max_health;

    public void TakeDamage(WeaponResource weapon) {
        health -= (int)weapon.damage;
        health = Mathf.Clamp(health, 0, _max_health);
        label.Text = $"{health}/{_max_health}";

        if (health <= 0) {
            // play death animation or effects here
            QueueFree();
        }
    }

    public override void _Ready() {
        _max_health = health;
        label.Text = $"{health}/{_max_health}";
    }
}
