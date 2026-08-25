namespace Weapons;
public partial class PulseLaser : Node3D, IWeapon{
    
    [Export]
    int PulseLaserDmg = 80;
    [Export]
    float PulseLaserWindupLength = 2.5f;
    float CurrentWindupLength = 0.0f;
    //This is intended to behave similar to hanzos bow in overwatch but as a hitscan
    //key difference is if the player doesnt fully charge the shot, the shot doesnt happen at all 
    //the the current Windup length is reset
    [Export]
    int DmgFalloffStart = 30;
    [Export]
    float DmgFalloffPercentagePerInterval = 15;
    [Export]
    int DmgFalloffIntervalLength = 5;
    public void Equip(WeaponResource weapon) {
        
    }
    public void Unequip(WeaponResource weapon) {
        
    }
    public void Shoot(WeaponResource weapon, Player player) {
        //To implement: 
        //Logic to correctly handle the windup in here
        //get the Transform of the Player
        //shoot a ray with a infinite range at the current point of aim
        //make shooting animation happen the ray will determine the length
        //if the ray hits a player, the player takes dmg
    }
}