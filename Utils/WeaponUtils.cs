namespace WeaponUtils;

using System.Threading.Tasks;

using Weapons;

public static class WeaponUtils {

    //This function returns either the object that was hit or the max length
    public static async Task DrawTracer(
        Node3D ParentNode,
        Vector3 Origin,
        Vector3 Destination,
        float Radius,
        float Lifetime,
        Color TracerColor
    ){
        Vector3 DirectionVector = Destination - Origin;
        float Length = DirectionVector.Length();

        if(Length <= 0) {
            return;
        }
        Vector3 Direction = DirectionVector.Normalized();
        Vector3 MidPoint = (Origin + Destination) / 2.0f;
        var Cylinder = new CylinderMesh{
            Height = Length,

            // Width here means full beam diameter.
            TopRadius = Radius,
            BottomRadius = Radius,

            // You really don't need 64 sides for a tiny tracer.
            RadialSegments = 8
        };
        //For the rest of the function this is AI code, due to this highkey flying above my head

        var Material = new StandardMaterial3D
        {
            AlbedoColor = TracerColor,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };

        Cylinder.Material = Material;

        var Tracer = new MeshInstance3D
        {
            Mesh = Cylinder,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };

        ParentNode.AddChild(Tracer);

        Quaternion Rotation = new Quaternion(
            Vector3.Up,
            Direction
        );

        Tracer.GlobalTransform = new Transform3D(
            new Basis(Rotation),
            MidPoint
        );

        await ParentNode.ToSignal(
            ParentNode.GetTree().CreateTimer(Lifetime),
            SceneTreeTimer.SignalName.Timeout
        );

        if (GodotObject.IsInstanceValid(Tracer)){
            Tracer.QueueFree();
        }
    }
    
    public struct HitscanResult
    {
        public bool Hit;
        public Vector3 Position;
        public GodotObject Collider;
        public float Distance;
    }
    public static float CalculateDamageFalloff(
        float BaseDamage,
        float HitDistance,
        float FirstInterval,
        float IntervalLength,
        int MaxInterval,
        float FalloffMultiplier){
        float DistanceIntoFalloff = HitDistance - FirstInterval;

        if (DistanceIntoFalloff < 0)
            return BaseDamage;

        int IntervalAmount = Math.Min(
            (int)(DistanceIntoFalloff / IntervalLength) + 1,
            MaxInterval
        );

        return Mathf.Max(
            BaseDamage * (1.0f - FalloffMultiplier * IntervalAmount),
            0.0f
        );
    }


    public static HitscanResult Hitscan(Player player, World3D WorldNode, WeaponResource Weapon, float WeaponMaxRange) {

        var SpaceState = WorldNode.DirectSpaceState;
        Camera3D Cam = player.GetNode<Camera3D>("Camera3D");
        Vector2 MousePosition = player.GetViewport().GetMousePosition();
        Vector3 Origin = Cam.ProjectRayOrigin(MousePosition);
        Vector3 End = Origin + Cam.ProjectRayNormal(MousePosition) * WeaponMaxRange;
        var Query = PhysicsRayQueryParameters3D.Create(Origin, End, player.CollisionMask);
        Query.Exclude = [player.GetRid()];
        var Result = SpaceState.IntersectRay(Query);

        if (Result.Count > 0){
            Vector3 CollisionVector = Result["position"].AsVector3();
            GodotObject Collider = Result["collider"].AsGodotObject();
            float HitDist = Origin.DistanceTo(CollisionVector);

            return new HitscanResult
            {
                Hit = true,
                Position = CollisionVector,
                Collider = Collider,
                Distance = HitDist
            };
        }
        return new HitscanResult
            {
                Hit = false,
                Position = End,
                Collider = null,
                Distance = WeaponMaxRange
            };
    }
}