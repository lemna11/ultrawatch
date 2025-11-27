namespace Utils;

public static class Util {
    public static float HorizontalLenght(Vector3 input) {
        return Mathf.Sqrt(input.X * input.X + input.Z * input.Z);
    }


    public static Vector3 KillMomentumProportionalHelper(Vector3 velocity_vector, double deccel) {
        var vect_len = HorizontalLenght(velocity_vector);
        float new_vect_len = (float)Mathf.MoveToward(vect_len, 0, deccel);
        var vect_normal = HorizontalNormal(velocity_vector);
        var new_velocity = vect_normal * new_vect_len;
        new_velocity.Y = velocity_vector.Y;
        return new_velocity;
    }

    public static Vector3 KillMomentumOnAxisHelper(Vector3 velocity_vector, Vector3 orientation, double deccel) {
        var vect_len = HorizontalLenght(velocity_vector);
        Vector3 new_vect = velocity_vector;
        Vector3 return_vect = Vector3.Zero;
        double deccel_vel_x = (1 - Mathf.Abs(orientation.X)) * deccel;
        double deccel_vel_z = (1 - Mathf.Abs(orientation.Z)) * deccel;
        new_vect.X = (float)Mathf.MoveToward(velocity_vector.X, 0, deccel_vel_x);
        new_vect.Z = (float)Mathf.MoveToward(velocity_vector.Z, 0, deccel_vel_z);
        Vector3 new_vect_normal = HorizontalNormal(new_vect);
        float return_vect_scalar = (float)Mathf.MoveToward(vect_len, 0, deccel);
        return_vect.X = new_vect_normal.X * return_vect_scalar;
        return_vect.Z = new_vect_normal.Z * return_vect_scalar;
        return_vect.Y = velocity_vector.Y;
        return return_vect;
    }

    public static Vector3 RescaleVector1ToVector2Helper(Vector3 vect1, Vector3 vect2) {
        Vector2 relevant_dimensions1 = Vector2.Zero;
        relevant_dimensions1.X = vect1.X;
        relevant_dimensions1.Y = vect1.Z;
        Vector2 relevant_dimensions2 = Vector2.Zero;
        relevant_dimensions2.X = vect2.X;
        relevant_dimensions2.Y = vect2.Z;
        Vector2 scaled_relevant_dimensions = relevant_dimensions1.Normalized() * relevant_dimensions2.Length();
        Vector3 out_vect = Vector3.Zero;
        out_vect.X = scaled_relevant_dimensions.X;
        out_vect.Y = vect1.Y;
        out_vect.Z = scaled_relevant_dimensions.Y;
        return out_vect;
    }

    public static Vector3 HorizontalNormal(Vector3 input) {
        Vector2 hor_vect = Vector2.Zero;
        hor_vect.X = input.X;
        hor_vect.Y = input.Z;
        hor_vect = hor_vect.Normalized();
        Vector3 output = Vector3.Zero;
        output.X = hor_vect.X;
        output.Z = hor_vect.Y;
        return output;
    }

    private static bool IsCounterStrafingHelper(Vector3 orientation, Vector3 velocity) {
        if (velocity == Vector3.Zero) {
            return false;
        }
        Vector3 diff_vect = orientation - HorizontalNormal(velocity);
        double angle = Mathf.Asin(HorizontalLenght(diff_vect) / 2) * 2;
        if (Mathf.Abs(angle) <= Mathf.Pi / 2) {
            return false;
        }
        return true;
    }

    public static Vector3 UniAccelDeccelHandler(Vector3 velocity, Vector3 direction, float accel, float deccel, double delta, float max_spd) {
        if (direction == Vector3.Zero)
            velocity = KillMomentumProportionalHelper(velocity, deccel * delta);
        else {
            velocity += direction * accel * (float)delta;
            if (HorizontalLenght(velocity) > max_spd) {
                velocity = KillMomentumProportionalHelper(velocity, deccel * delta);
            }
        }
        return velocity;
    }

}
