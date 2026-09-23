using HarmonyLib;
using UnityEngine;
using NO_Tactitools.Core;

namespace NO_Tactitools.UI.HMD;

public class HMDCamComponent {
    public static Vector2 Position { set { field = value; UpdatePanelPlacement(); } get; } = Vector2.zero;
    public static Vector2 Size { set { field = value; UpdatePanelPlacement(); } get; } = Vector2.one;
    public static float Transparency {
        set {
            field = value;
            color.a = value;
            if (panel != null) {
                var videoImage = panel.GetComponent<UnityEngine.UI.RawImage>();
                if (videoImage != null)
                    videoImage.color = color;
            }
        }
        get;
    } = 1.0f;
    public static bool SuppressMFDCam = false;

    private static TraverseCache<TargetCam, Camera> camCache = new ("cam");
    private static TraverseCache<TacScreen, GameObject> targetCamDisplayCache = new ("targetCamDisplay");
    private static TraverseCache<TacScreen, GameObject> landingCamDisplayCache = new ("landingCamDisplay");

    private static GameObject panel = null;
    private static Color color = Color.white;

	private static Texture GetTargetCamTexture() {
		var targetCam = UIBindings.Game.GetTargetCamComponent();
		if (targetCam == null) {
            Plugin.Log("[HMDCAM] GetTargetCamTexture(): targetCam is null, cannot get camera texture");
			return null;
        }

		Texture texture = null;

        /* Texture is retrieved just from TargetCam.cam, not from TargetCam.targetScreenRenderer.
           This fixes HMD Cam displaying tac screen texture with F-117A mod installed
           (https://github.com/blacknight2u/nuclear-option-f117). */
        var cam = camCache.GetValue(targetCam);
        if (cam == null)
            Plugin.Log("[HMDCAM] GetTargetCamTexture(): cannot get camera from target camera component");
        else
            texture = cam.targetTexture;

        Plugin.Log($"[HMDCAM] GetTargetCamTexture(): {(texture == null ? "cannot get" : "got")} camera texture from camera");

		return texture;
	}

    private static void CreatePanel() {
        Plugin.Log("[HMDCAM] creating panel");

        var combatHUDTransform = UIBindings.Game.GetCombatHUDTransform();
        if (combatHUDTransform == null) {
            Plugin.Log("[HMDCAM] CreatePanel(): cannot get combat HUD transform");
            return;
        }

        var videoTexture = GetTargetCamTexture();
        if (videoTexture == null) {
            Plugin.Log("[HMDCAM] CreatePanel(): cannot get camera texture");
            return;
        }

        if (panel != null) {
            Plugin.Log("[HMDCAM] CreatePanel(): destroying existing panel");
            DestroyPanel();
        }

        panel = new GameObject("HMDCam");
        var panelTransform = panel.AddComponent<RectTransform>();
        panelTransform.SetParent(combatHUDTransform, false);
        panelTransform.localScale = Vector3.one;
        panelTransform.pivot = Vector2.zero;
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.zero;

        var videoImage = panel.AddComponent<UnityEngine.UI.RawImage>();
        videoImage.texture = videoTexture;
        videoImage.color = color;
        videoImage.raycastTarget = false;

        /* Disabling panel after its creation. Otherwise HMD Cam displays stale feed after jumping to AI aircraft using
           WingCommand mod (https://github.com/GrabowMar/NuclearOption-WingCommand). */
        panel.SetActive(false);

        UpdatePanelPlacement();

        Plugin.Log("[HMDCAM] created panel");
    }

    private static void DestroyPanel() {
        Plugin.Log("[HMDCAM] destroying panel");

        if (panel != null) {
            UnityEngine.Object.Destroy(panel);
            panel = null;
        }
    }

    private static void UpdatePanelPlacement() {
        if (panel == null) {
            Plugin.Log("[HMDCAM] UpdatePanelPlacement(): panel is null");
            return;
        }

        var panelTransform = panel.GetComponent<RectTransform>();
        if (panelTransform == null) {
            Plugin.Log("[HMDCAM] UpdatePanelPlacement(): panel transform is null");
            return;
        }

        panelTransform.anchoredPosition = Position;
        panelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Size.x);
        panelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Size.y);
    }

    private static void OnCamToggleCallback(TargetCam.OnCamToggle oct) {
        if (panel != null) {
            panel.SetActive(oct.enabled);
            Plugin.Log($"[HMDCAM] OnCamToggleCallback(): panel {(oct.enabled ? "enabled" : "disabled")}; cam mode: {oct.camMode}");
        }
        else
            Plugin.Log("[HMDCAM] OnCamToggleCallback(): panel is null");

        if (SuppressMFDCam && oct.enabled) {
            var tacScreen = UIBindings.Game.GetTacScreenComponent();
            if (tacScreen != null) {
                GameObject display = null;
                var camMode = oct.camMode;
                if (camMode == TargetCam.CamMode.targetForward || camMode == TargetCam.CamMode.targetRear) {
                    display = targetCamDisplayCache.GetValue(tacScreen);
                }
                else if (camMode == TargetCam.CamMode.landingMode) {
                    display = landingCamDisplayCache.GetValue(tacScreen);
                }
                if (display != null) {
                    Plugin.Log($"[HMDCAM] OnCamToggleCallback(): suppressing {(camMode != TargetCam.CamMode.landingMode ? "target" : "landing")} cam display");
                    display.SetActive(false);
                }
            }
        }
    }

    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class OnMainMenuStart {
        private static bool initialized = false;
        static void Postfix() {
            if (!initialized) {

                Plugin.Log($"[HMDCAM] Initializing HMD Cam component.");

                Plugin.harmony.PatchAll(typeof(OnTargetCamInitialize));
                Plugin.harmony.PatchAll(typeof(OnTargetCamOnDestroy));

                var bindings = new BindingHelper.Binding[] {
                    new (typeof(HMDCamComponent), "Position", Plugin.HMDCam.Position),
                    new (typeof(HMDCamComponent), "Size", Plugin.HMDCam.Size),
                    new (typeof(HMDCamComponent), "Transparency", Plugin.HMDCam.Transparency),
                    new (typeof(HMDCamComponent), "SuppressMFDCam", Plugin.HMDCam.SuppressMFDCam),
                };
                BindingHelper.ApplyBindings(bindings);

                Plugin.Log($"[HMDCAM] Initialized HMD Cam component.");

                initialized = true;
            }
        }
    }

    /* This patch needs to be executed before TargetCam.Initialize() patch in DynamicLandingCamComponent.
       If KeepOnAfterTouchDown in DynamicLandingCamComponent is true, DynamicLandingCamComponent calls TargetCam.SetLandingCam(),
       which in turn invokes callbacks registered in onCamToggle. HMDCamComponent registers such callback to suppress MFD cam.
       So HMDCamComponent.OnTargetCamInitialize.Postfix() needs to execute before DynamicLandingCamComponent.OnTargetCamInitialize.Postfix(). */
    [HarmonyBefore(["yessidor.no_tactitools_plus.dynamic_landing_cam"])]
    [HarmonyPatch(typeof(TargetCam), "Initialize")]
    public class OnTargetCamInitialize {
        public static void Postfix(TargetCam __instance, UnitPart ___attachedPart) {
            var aircraft = ___attachedPart.parentUnit as Aircraft;
            if (aircraft != null && aircraft.Identity.HasAuthority) {
                CreatePanel();
                __instance.onCamToggle += OnCamToggleCallback;
            }
        }
    }

    [HarmonyBefore(["yessidor.no_tactitools_plus.dynamic_landing_cam"])]
    [HarmonyPatch(typeof(TargetCam), "OnDestroy")]
    public class OnTargetCamOnDestroy {
        public static void Prefix(TargetCam __instance, UnitPart ___attachedPart) {
            var aircraft = ___attachedPart.parentUnit as Aircraft;
            if (aircraft != null && aircraft.Identity.HasAuthority) {
                __instance.onCamToggle -= OnCamToggleCallback;
                DestroyPanel();
            }
        }
    }
}
