using SPT.Reflection.Patching;
using HarmonyLib;
using System.Reflection;

namespace BorkelRNVG.Patches
{
    internal class UltimateBloomPatch : ModulePatch //if Awake is prevented from running, the exaggerated bloom goes away
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(UltimateBloom), "Awake");
        }

        [PatchPrefix]
        private static void PatchPrefix(UltimateBloom __instance)
        {
            //Plugin.UltimateBloomInstance = __instance; //to disable it when NVG turns ON
            //__instance.gameObject.SetActive(false);
            //return true;
            //__instance.m_FlareMask = Plugin.maskFlare;
            //Plugin.UltimateBloomInstance.gameObject.SetActive(false);
        }
    }
}
