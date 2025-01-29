using BorkelRNVG.Helpers;
using BorkelRNVG.Helpers.Configuration;
using BorkelRNVG.Helpers.Enum;
using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System.Collections;
using System.Reflection;
using UnityEngine;
using static ApplicationConfigClass;

namespace BorkelRNVG.Patches
{
    public class InitiateShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod(nameof(Player.FirearmController.InitiateShot));
        }

        [PatchPostfix]
        private static void PatchPostfix(Player.FirearmController __instance)
        {
            AutoGatingController gatingInst = AutoGatingController.Instance;
            if (gatingInst == null) return;

            string nvgId = Util.GetCurrentNvgItemId();
            if (nvgId == null) return;

            NightVisionConfig nvgConfig = NightVisionItemConfig.Get(nvgId).NightVisionConfig;
            if (nvgConfig == null) return;

            EMuzzleDeviceType deviceType = Util.GetSuppressedOrFlashHidden(__instance);

            float gatingMult;
            switch (deviceType)
            {
                case EMuzzleDeviceType.None:
                    gatingMult = 0.15f; 
                    break;
                case EMuzzleDeviceType.Suppressor:
                    gatingMult = 1.0f;
                    break;
                case EMuzzleDeviceType.FlashHider:
                    gatingMult = 0.3f;
                    break;
                default:
                    gatingMult = 1.0f;
                    break;
            }

            AutoGatingController.Instance?.StartCoroutine(AdjustAutoGating(0.05f, gatingMult, gatingInst, nvgConfig));
        }

        private static IEnumerator AdjustAutoGating(float delay, float multiplier, AutoGatingController gatingController, NightVisionConfig nvgConfig)
        {
            yield return new WaitForSeconds(delay);

            float newBrightness = Mathf.Clamp(gatingController.GatingMultiplier * multiplier, nvgConfig.MinBrightness.Value, nvgConfig.MaxBrightness.Value);
            gatingController.GatingMultiplier = newBrightness;
        }
    }
}
