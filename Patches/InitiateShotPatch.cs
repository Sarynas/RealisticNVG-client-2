using BorkelRNVG.Helpers;
using BorkelRNVG.Helpers.Configuration;
using BorkelRNVG.Helpers.Enum;
using EFT;
using SPT.Reflection.Patching;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BorkelRNVG.Patches
{
    public class InitiateShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod(nameof(Player.FirearmController.InitiateShot));
        }

        private static float EaseOut(float val)
        {
            return 1 - Mathf.Pow(1 - val, 3);
        }

        private static float ClampDot90Deg(float dot)
        {
            return Mathf.Max(0, dot);
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance, AmmoItemClass ammo, Vector3 shotPosition, Vector3 shotDirection)
        {
            CameraClass cameraClass = Util.GetCameraClass();
            if (cameraClass == null || cameraClass.Camera == null) return;

            AutoGatingController gatingInst = AutoGatingController.Instance;
            if (gatingInst == null) return;

            string nvgId = Util.GetCurrentNvgItemId();
            if (nvgId == null) return;

            NightVisionConfig nvgConfig = NightVisionItemConfig.Get(nvgId).NightVisionConfig;
            if (nvgConfig == null) return;
            if (nvgConfig.AutoGatingEnabled.Value == false) return;

            EMuzzleDeviceType deviceType = Util.GetMuzzleDeviceType(__instance);

            Player mainPlayer = Util.GetPlayer();
            Player firearmOwner = __instance.GetComponentInParent<Player>();

            float gatingLerp;
            switch (deviceType)
            {
                case EMuzzleDeviceType.None:
                    gatingLerp = 0.15f; 
                    break;
                case EMuzzleDeviceType.Suppressor:
                    gatingLerp = 1.0f;
                    break;
                case EMuzzleDeviceType.FlashHider:
                    gatingLerp = 0.3f;
                    break;
                default:
                    gatingLerp = 1.0f;
                    break;
            }

            if (firearmOwner != mainPlayer)
            {
                Camera camera = cameraClass.Camera;

                Vector3 cameraPos = camera.transform.position;
                Vector3 dir = shotPosition - cameraPos;

                float maxShotDistance = 15f;
                float shotDistance = dir.magnitude;
                float shotDistanceMult = Mathf.Clamp01(1 - (shotDistance / maxShotDistance));
                bool isVisible = Util.VisibilityCheckBetweenPoints(cameraPos, shotPosition, LayerMaskClass.HighPolyWithTerrainMask);
                bool isOnScreen = Util.VisibilityCheckOnScreen(shotPosition);

                if (isVisible && isOnScreen)
                {
                    float finalGatingMult = Mathf.Lerp(0, 1 * shotDistanceMult, gatingLerp);
                    AutoGatingController.Instance?.StartCoroutine(AdjustAutoGating(0.05f, finalGatingMult, gatingInst, nvgConfig));
                }
            }
            else
            {
                AutoGatingController.Instance?.StartCoroutine(AdjustAutoGating(0.05f, gatingLerp, gatingInst, nvgConfig));
            }
        }

        private static IEnumerator AdjustAutoGating(float delay, float multiplier, AutoGatingController gatingController, NightVisionConfig nvgConfig)
        {
            yield return new WaitForSeconds(delay);

            float newBrightness = Mathf.Clamp(gatingController.GatingMultiplier * multiplier, nvgConfig.MinBrightness.Value, nvgConfig.MaxBrightness.Value);
            gatingController.GatingMultiplier = newBrightness;
        }
    }
}
