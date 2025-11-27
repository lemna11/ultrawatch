namespace Legs;

public interface ILegAbility {
    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity);
}
