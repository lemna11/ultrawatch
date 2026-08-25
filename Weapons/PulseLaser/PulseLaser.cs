using System.Collections.Generic;
using System.ComponentModel;

namespace Weapons;
public partial class PulseLaser : Node3D, IWeapon{
    
    [Export]
    int PulseLaserDmg = 80;
    [Export]
    float PulseLaserWindupLength = 2.5f;
    float CurrentWindupLength = 0.0f;
    int RayLength = 100;//This is the max range of the pulse laser
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

        //THIS IS UNTESTED CODE! The code is taken from this: https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html
        //To my knowledge now I need to iterate over the results and find the collision entry
        //with the lowest euclidean distance from the player
        //And I also need to exclude the players own hitbox
        //since the docs said that I need to explicitly remove that

        //Next up is: iterate over results. and if the closest object is a hostile player do the dmg
        //also draw a colored line between the player and the point of aim (shot animation stuff)

        var SpaceState = GetWorld3D().DirectSpaceState;
        Camera3D Cam = player.GetNode<Camera3D>("Camera3D");
        Vector2 MousePosition = player.GetViewport().GetMousePosition();
        Vector3 Origin = Cam.ProjectRayOrigin(MousePosition);
        Vector3 End = Origin + Cam.ProjectRayNormal(MousePosition) * RayLength;
        var Query = PhysicsRayQueryParameters3D.Create(Origin, End, player.CollisionMask);
        var Result = SpaceState.IntersectRay(Query);
        float LowestEuclideanDist = RayLength;
        int ColliderIDOfThatObject = 0;
        //Iterate over all results by key in here (I fucking suck at c# so I have no idea how.)
        foreach(var ResultEntry in Result) {//the key with this should be the collision
        //ID and the value should be the vector of the collision object.


            Vector3 CollisionVector = ResultEntry.Key.As<Vector3>();
            int ColliderID = ResultEntry.Value.As<int>();
            //in all honesty I have no idea if this is correct. I need to do more reseach.

            float EuclideanDist = (CollisionVector - player.Position).Length();
            
            bool IsValidCollision = false;
            //ADD CODEBLOCK TO CHECK IF THE COLLISION ID BELONGS TO THE PLAYER
            //OR ANY PLAYER SUBOBJECT OF THAT PLAYER


            if(EuclideanDist < LowestEuclideanDist && IsValidCollision) {//THIS WILL NOT WORK!
            //I need to add some sort of exclusion mechanic collisions with IDs belonging
            //to the origin player
                LowestEuclideanDist = EuclideanDist;
                ColliderIDOfThatObject = ColliderID;
            }

        }
        
        //Add logic that checks if the collision ID belongs to an enemy player
        //and if yes, do damage to that player
        //at the end draw a BLUE line between the points.


    }
}