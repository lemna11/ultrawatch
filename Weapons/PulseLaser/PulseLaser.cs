using System.Collections.Generic;
using System.ComponentModel;
using WeaponUtils;
namespace Weapons;
public partial class PulseLaser : Node3D, IWeapon{
	
	[Export]
	int PulseLaserDmg = 80;
	[Export]
	float PulseLaserWindupLength = 2.5f;
	float CurrentWindupLength = 0.0f;
	int MaxRange = 100;//This is the max range of the pulse laser
	//This is intended to behave similar to hanzos bow in overwatch but as a hitscan
	//key difference is if the player doesnt fully charge the shot, the shot doesnt happen at all 
	//the the current Windup length is reset
	[Export]
	float DmgFalloffMultiplier = 0.15f;
	[Export]
	float DmgFalloffIntervalLength = 5;
	[Export]
	int MaxInterval = 5;
	[Export]
	float FirstInterval = 10.0f;


	[Export]
	float PulseLaserCoreTracerStartRadius = 0.08f;
	[Export]
	float PulseLaserCoreTracerRadiusGain = 0.1f;
	[Export]
	float PulseLaserCoreTracerDuration = 0.3f;
	[Export]
	float PulseLaserHaloTracerDuration = 0.3f;
	[Export]
	Color PulseLaserCoreTracerColor;
	[Export]
	float PulseLaserCoreTracerAlpha = 0.8f;
	[Export]
	float PulseLaserCoreTracerGlow = 6.0f;

	
	[Export]
	float PulseLaserHaloTracerStartRadius = 0.7f;
	[Export]
	float PulseLaserHaloTracerRadiusGain= 0.1f;
	[Export]
	Color PulseLaserHaloTracerColor;
	[Export]
	float PulseLaserHaloTracerAlpha = 0.1f;

	[Export]
	float PulseLaserHaloTracerGlow = 24.0f;
	

	bool IsCharging = false;
	public void Equip(WeaponResource weapon) {
		
	}
	public void Unequip(WeaponResource weapon) {
		
	}

	public override void _Process(double delta)
	{
		if (IsCharging)
		{
			CurrentWindupLength += (float)delta;
		}
	}

	public void Shoot(WeaponResource weapon, Player player) {
		if (IsCharging){
			return;
		}
		IsCharging = true;
		CurrentWindupLength = 0.0f;
	}
	public bool ShootReleased(WeaponResource weapon, Player player) {
		if (!IsCharging)
		{
			return false;
		}
		IsCharging = false;
		weapon.current_ammo--;
		if (CurrentWindupLength < PulseLaserWindupLength)
		{
			CurrentWindupLength = 0.0f;
			return false;
		}
		WeaponUtils.WeaponUtils.HitscanResult Result = WeaponUtils.WeaponUtils.Hitscan(
			player,
			player.GetWorld3D(),
			weapon,
			MaxRange
		);
		float PulseLaserCoreTracerEndRadius = PulseLaserCoreTracerStartRadius + (PulseLaserCoreTracerRadiusGain * (Result.Distance / 10));
		float PulseLaserHaloTracerEndRadius = PulseLaserHaloTracerStartRadius + (PulseLaserHaloTracerRadiusGain * (Result.Distance / 10));
		Vector3 TracerOrigin = player.GlobalPosition;
		WeaponUtils.WeaponUtils.DrawTracer(player,
			TracerOrigin,
			Result.Position,
			PulseLaserCoreTracerDuration,
			PulseLaserCoreTracerStartRadius,
			PulseLaserCoreTracerEndRadius,
			PulseLaserCoreTracerColor,
			PulseLaserCoreTracerAlpha,
			PulseLaserCoreTracerGlow,
			PulseLaserHaloTracerDuration,
			PulseLaserHaloTracerStartRadius,
			PulseLaserHaloTracerEndRadius,
			PulseLaserHaloTracerColor,
			PulseLaserHaloTracerAlpha,
			PulseLaserHaloTracerGlow
			);

		if (Result.Hit) {
			if(Result.Collider is ITarget HitPlayer) {
				float Dmg = WeaponUtils.WeaponUtils.CalculateDamageFalloff(PulseLaserDmg,
					Result.Distance,
					FirstInterval,
					DmgFalloffIntervalLength,
					MaxInterval,
					DmgFalloffMultiplier);
					HitPlayer.TakeDamage(weapon, Dmg);
			}
			
		}

		CurrentWindupLength = 0.0f;

		 return true;
	}
}
