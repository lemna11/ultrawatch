public partial class WeaponResource : Resource {
    [Export]
    public float damage = 10.0f;

    [Export]
    public float fire_rate = 1.0f;

    [Export]
    public Vector3 weapon_offset = new(0, 0, 0);

    [Export]
    public PackedScene weapon_model;

    [Export]
    public string equip_animation = "Equip";

    [Export]
    public string shoot_animation = "Shoot";

    [Export]
    public AudioStream shoot_sound;

    public bool can_fire = true;

    public Node3D player_camera;
}
