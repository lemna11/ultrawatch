public interface IWeapon {
    void Shoot(WeaponResource weapon, Player player);

    void Equip(WeaponResource weapon);

    void Unequip(WeaponResource weapon);
}
