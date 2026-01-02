namespace Weapons;

public interface IWeapon {
    void Shoot(WeaponResource weapon, Player player);

    bool ShootReleased(WeaponResource weapon, Player player) { return false; }

    void Equip(WeaponResource weapon);

    void Unequip(WeaponResource weapon);
}
