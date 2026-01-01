
namespace Legs;

public class DefaultLegs : ILegAbility {
    public int MaxHealthModifier => 75;

    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity) {
    }
}
