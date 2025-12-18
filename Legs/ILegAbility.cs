namespace Legs;

public interface ILegAbility {
    public int MaxHealthModifier { get; }

    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity);

    public void Equip(Player player);

    public void Unequip(Player player);
}
