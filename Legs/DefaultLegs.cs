
namespace Legs;

public class DefaultLegs : ILegAbility {
    public int MaxHealthModifier => 75;

    public void Apply(Player player, double delta, Vector3 direction, ref Vector3 velocity) {
    }

    public void Equip(Player player) {
        player.max_health += MaxHealthModifier;
        player.cur_health = player.max_health;
        player.update_hud?.Invoke();
    }

    public void Unequip(Player player) {
        player.max_health -= MaxHealthModifier;
        player.cur_health = player.max_health;
        player.update_hud?.Invoke();
    }
}
