public partial class WeaponResource : Resource
{
    [Export]
    public float damage = 10.0f;

    [Export]
    public float fire_rate = 1.0f;

    [Export]
    public Vector3 weapon_offset = new(0, 0, 0);

    [Export]
    public PackedScene weapon_model;
}
