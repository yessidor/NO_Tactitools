using UnityEngine;
using NO_Tactitools.Core;

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
        Vector3 intersection = MathUtils.GetRectLineIntersection(rect, inside, screenCoords);
        if (screenCoords != intersection || !zPositive) {
            Vector3 direction = (intersection - inside).normalized;
            var directionUnmod = direction;
            if (!zPositive) {
                direction.x *= -1;
                direction.y *= -1;
            }
            rayToScreen = MathUtils.GetRectRayIntersection(rect, inside, direction, out float _);
            arrowAngle = -Mathf.Atan2(direction.x, direction.y);
            return true;
        }
        else {
            rayToScreen = screenCoords;
            arrowAngle = 0f;
            return false;
        }
    }
}
