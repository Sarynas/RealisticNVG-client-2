using EFT;
using SPT.Reflection.Patching;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BorkelRNVG.Patches
{
    public class LightAndSoundShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(FirearmsEffects).GetMethod(nameof(FirearmsEffects.StartFireEffects));
        }

        [PatchPostfix]
        private static void PatchPostfix(FirearmsEffects __instance)
        {
            AutoGatingController inst = AutoGatingController.Instance;
            inst?.StartCoroutine(ForceCompute(inst));
        }

        private static IEnumerator ForceCompute(AutoGatingController __instance)
        {
            yield return new WaitForSeconds(0.05f);

            __instance.StartCoroutine(__instance.ComputeBrightness());
        }
    }
}
