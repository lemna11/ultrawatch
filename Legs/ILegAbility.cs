namespace Legs;

public interface ILegAbility {
    // TODO: should probably pass direction as an arg but cannot be fucked
    public void Apply(Player player, double delta, ref Vector3 velocity);
}
