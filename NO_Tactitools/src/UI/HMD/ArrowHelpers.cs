using UnityEngine;

namespace NO_Tactitools.UI.HMD;

class ArrowHelpers {
    public static bool PinToScreenEdge(Vector3 coords, out Vector3 rayToScreen, out float arrowAngle) {
        //rayToScreen will jump when screenCoords.z passes through 0f
        Vector3 screenCoords = SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(coords);
        var screenCoordsWTS = screenCoords;
        bool zPositive = screenCoords.z >= 0f;
        screenCoords.z = 0f;
        Rect rect = new Rect (0f, 0f, (float)Screen.width, (float)Screen.height);
        Vector3 inside = new Vector3 (0.5f * Screen.width, 0.5f * Screen.height, 0f);
        Vector3 intersection = MathHelpers.GetRectLineIntersection(rect, inside, screenCoords);
        if (screenCoords != intersection || !zPositive) {
            Vector3 direction = (intersection - inside).normalized;
            var directionUnmod = direction;
            if (!zPositive) {
                direction.x *= -1;
                direction.y *= -1;
            }
            rayToScreen = MathHelpers.GetRectRayIntersection(rect, inside, direction, out float _);
            arrowAngle = -Mathf.Atan2(direction.x, direction.y);
            //Plugin.Log($"PinToScreenEdge(): coords:{coords}; screenCoordsWTS:{screenCoordsWTS}; screenCoords:{screenCoords}; intersection: {intersection}; directionUnmod:{directionUnmod}; direction: {direction}; rayToScreen:{rayToScreen}; arrowAngle:{arrowAngle}");
            return true;
        }
        else {
            rayToScreen = screenCoords;
            arrowAngle = 0f;
            return false;
        }
    }
}


class MathHelpers {
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
