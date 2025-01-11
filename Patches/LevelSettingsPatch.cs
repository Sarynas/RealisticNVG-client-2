using SPT.Reflection.Patching;
using HarmonyLib;
using System.Reflection;

namespace BorkelRNVG.Patches
{
    internal class LevelSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LevelSettings), "method_0");
        }

        [PatchPrefix]
        private static void PatchPrefix(LevelSettings __instance)
        {
            Logger.LogMessage($"LevelSettings patch {__instance.AmbientType}");
            Logger.LogMessage($"nvcolor: {__instance.NightVisionSkyColor}");
            Logger.LogMessage($"scolor: {__instance.SkyColor}");
            __instance.NightVisionSkyColor = __instance.SkyColor;
        }
    }
}
