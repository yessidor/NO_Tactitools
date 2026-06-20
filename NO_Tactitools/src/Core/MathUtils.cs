using UnityEngine;

namespace NO_Tactitools.Core;

public class MathUtils {
    public static int Sign(float f) {
        return f < 0 ? -1 : f > 0 ? 1 : 0;
    }

    public static float ClampAngle(float angle) {
        if (Mathf.Abs(angle) > 180f)
            angle = angle - Sign(angle) * 360f;
        return angle;
    }

    public static Vector3 GetRectLineIntersection(Rect rect, Vector3 inside, Vector3 outside) {
        Vector3 direction = outside - inside;
        var intersection = GetRectRayIntersection(rect, inside, direction, out float t);
        return t > 1.0f ? outside : inside + t * direction;
    }

    public static Vector3 GetRectRayIntersection(Rect rect, Vector3 inside, Vector3 direction, out float u) {
        float tClosest = float.MaxValue;
        float xMin = rect.x, xMax = rect.x + rect.width, yMin = rect.y, yMax = rect.y + rect.height;

        void check_x(float t) {
            if (t > 0 && t < tClosest) {
                float y = inside.y + t * direction.y;
                if (y >= yMin && y <= yMax)
                    tClosest = t;
            }
        }

        void check_y(float t) {
            if (t > 0 && t < tClosest) {
                float x = inside.x + t * direction.x;
                if (x >= xMin && x <= xMax)
                    tClosest = t;
            }
        }

        if (Mathf.Abs(direction.x) > Mathf.Epsilon) {
            // Check intersection with left edge (x = xMin)
            float t = (xMin - inside.x) / direction.x;
            check_x(t);

            // Check intersection with right edge (x = xMax)
            t = (xMax - inside.x) / direction.x;
            check_x(t);
        }

        if (Mathf.Abs(direction.y) > Mathf.Epsilon) {
            // Check intersection with bottom edge (y = yMin)
            float t = (yMin - inside.y) / direction.y;
            check_y(t);

            // Check intersection with top edge (y = yMax)
            t = (yMax - inside.y) / direction.y;
            check_y(t);
        }

        var result = inside + tClosest * direction;
        u = tClosest;
        return result;
    }
}
