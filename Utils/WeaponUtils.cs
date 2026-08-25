namespace WeaponUtils;

using System.Threading.Tasks;

using Weapons;

public static class WeaponUtils {

    //This function returns either the object that was hit or the max length
    private static async Task DrawTracerHelper(
        Node3D ParentNode,
        Vector3 Origin,
        Vector3 Destination,
        float Lifetime,
        float StartRadius,
        float EndRadius,
        Color TracerColor,
        float TracerAlpha,
        float TracerGlow){
        Vector3 DirectionVector = Destination - Origin;
        float Length = DirectionVector.Length();

        if(Length <= 0) {
            return;
        }
        Vector3 Direction = DirectionVector.Normalized();
        Vector3 MidPoint = (Origin + Destination) / 2.0f;
        var CoreCylinder = new CylinderMesh{
            Height = Length,

            // Width here means full beam diameter.
            TopRadius = EndRadius,
            BottomRadius = StartRadius,

            // You really don't need 64 sides for a tiny tracer.
            RadialSegments = 8
        };
        //For the rest of the function this is AI code, due to this highkey flying above my head

        var Material = new StandardMaterial3D
        {
            AlbedoColor = new Color(
                TracerColor.R,
                TracerColor.G,
                TracerColor.B,
                TracerAlpha
            ),
            ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            EmissionEnabled = true,
            Emission = TracerColor,
            EmissionEnergyMultiplier = TracerGlow
        };

        CoreCylinder.Material = Material;

        var CoreTracer = new MeshInstance3D
        {
            Mesh = CoreCylinder,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            TopLevel = true
        };

        ParentNode.AddChild(CoreTracer);

        Quaternion Rotation = new Quaternion(
            Vector3.Up,
            Direction
        );

        CoreTracer.GlobalTransform = new Transform3D(
            new Basis(Rotation),
            MidPoint
        );

        await ParentNode.ToSignal(
            ParentNode.GetTree().CreateTimer(Lifetime),
            SceneTreeTimer.SignalName.Timeout
        );

        if (GodotObject.IsInstanceValid(CoreTracer)){
            CoreTracer.QueueFree();
        }

    }
    public static void DrawTracer(
        Node3D ParentNode,
        Vector3 Origin,
        Vector3 Destination,
        float Lifetime,
        float CoreStartRadius,
        float CoreEndRadius,
        Color CoreTracerColor,
        float CoreTracerAlpha,
        float CoreTracerGlow,
        float HaloStartRadius,
        float HaloEndRadius,
        Color HaloTracerColor,
        float HaloTracerAlpha,
        float HaloTracerGlow
    ){
       _= DrawTracerHelper(ParentNode,
       Origin,
       Destination,
       Lifetime,
       CoreStartRadius,
       CoreEndRadius,
       CoreTracerColor,
       CoreTracerAlpha,
       CoreTracerGlow);

       _= DrawTracerHelper(ParentNode,
       Origin,
       Destination,
       Lifetime,
       HaloStartRadius,
       HaloEndRadius,
       HaloTracerColor,
       HaloTracerAlpha,
       HaloTracerGlow);
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
        Camera3D Cam = player.GetNode<Camera3D>("CameraYaw/CameraPitch/Camera3D");
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