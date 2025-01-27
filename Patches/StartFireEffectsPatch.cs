using BorkelRNVG.Helpers;
using BorkelRNVG.Helpers.Configuration;
using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BorkelRNVG.Patches
{
    public class StartFireEffectsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsEffects).GetMethod(nameof(FirearmsEffects.StartFireEffects));
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsEffects __instance)
        {
            AutoGatingController inst = AutoGatingController.Instance;
            if (inst == null) return;

            string nvgId = Util.GetCurrentNvgItemId();
            if (nvgId == null) return;

            NightVisionConfig nvgConfig = NightVisionItemConfig.Get(nvgId).NightVisionConfig;
            if (nvgConfig == null) return;

            Player player = Singleton<GameWorld>.Instance.MainPlayer;

            float newBrightness = Mathf.Clamp(inst.GatingMultiplier * 0.3f, nvgConfig.MinBrightness.Value, nvgConfig.MaxBrightness.Value);
            inst.GatingMultiplier = newBrightness;
        }
    }
}
