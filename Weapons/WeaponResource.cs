namespace Weapons;

public enum FireMode {
    Instant,
    Charge
}

public partial class WeaponResource : Resource {
    [Export]
    public float damage = 10.0f;

    [Export]
    public float fire_rate = 1.0f;

    [Export]
    public float reload_time = 2.0f;

    [Export]
    public Vector3 right_arm_weapon_offset = new(0, 0, 0);

    [Export]
    public Vector3 left_arm_weapon_offset = new(0, 0, 0);

    [Export]
    public PackedScene weapon_model;

    [Export]
    public string equip_animation = "Equip";

    [Export]
    public string shoot_animation = "Shoot";

    [Export]
    public AudioStream shoot_sound;

    [Export]
    public FireMode fireMode = FireMode.Instant;

    [Export]
    public int magazine_size = 1;

    private int _current_ammo = -1;
    public int current_ammo {
        get {
            if (_current_ammo == -1)
                _current_ammo = magazine_size;
            return _current_ammo;
        }
        set => _current_ammo = value;
    }

    public Timer timer;

    public bool can_fire = true;

    public Node3D player_camera;
}
