public interface IWeapon {
    void Shoot(WeaponResource weapon);

    void Equip(WeaponResource weapon);

    void Unequip(WeaponResource weapon);
}
