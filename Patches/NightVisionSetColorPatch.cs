using Aki.Reflection.Patching;
using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace BorkelRNVG.Patches
{
    public class NightVisionSetColorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NightVision), nameof(NightVision.StartSwitch));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref NightVision __instance)
        {
            __instance.Color.a = (float)254 / 255; //i think it does nothing
            __instance.MaskSize = 1; //does not affect the t-7 for some reason
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                return;
            }

            var player = gameWorld.MainPlayer;
            if (player == null)
            {
                return;
            }

            if (player.NightVisionObserver.Component == null
                || player.NightVisionObserver.Component.Item == null
                || player.NightVisionObserver.Component.Item.TemplateId == null)
            {
                return;
            }

            string nvgID = player.NightVisionObserver.Component.Item.TemplateId; //ID of the nvg
            //gpnvg18 id: 5c0558060db834001b735271
            if (nvgID == "5c0558060db834001b735271")
            {
                //vanilla intensity:2.27
                __instance.Intensity = Plugin.quadGain.Value * Plugin.globalGain.Value;
                //vanilla noiseintensity:0.02
                __instance.NoiseIntensity = Plugin.quadNoiseIntensity.Value;
                //vanilla noisescale:5 bigger number means smaller noise
                //__instance.NoiseScale = 0.95F; -> 0.05 in the bepinex menu, smaller number will mean smaller noise (easier for the user)
                __instance.NoiseScale = 1f - Plugin.quadNoiseSize.Value;
                __instance.MaskSize = Plugin.quadMaskSize.Value * Plugin.globalMaskSize.Value;
                __instance.Color.r = Plugin.quadR.Value / 255f;
                __instance.Color.g = Plugin.quadG.Value / 255f;
                __instance.Color.b = Plugin.quadB.Value / 255f;
            }
            //pvs14 id: 57235b6f24597759bf5a30f1
            if (nvgID == "57235b6f24597759bf5a30f1")
            {
                //vanilla intensity:2.27
                __instance.Intensity = Plugin.pvsGain.Value * Plugin.globalGain.Value;
                //vanilla noiseintensity:0.02
                __instance.NoiseIntensity = Plugin.pvsNoiseIntensity.Value;
                //vanilla noisescale:5
                __instance.NoiseScale = 1f - Plugin.pvsNoiseSize.Value;
                __instance.MaskSize = Plugin.pvsMaskSize.Value * Plugin.globalMaskSize.Value;
                __instance.Color.r = Plugin.pvsR.Value / 255;
                __instance.Color.g = Plugin.pvsG.Value / 255;
                __instance.Color.b = Plugin.pvsB.Value / 255;

            }
            //n15 id: 5c066e3a0db834001b7353f0
            if (nvgID == "5c066e3a0db834001b7353f0")
            {
                //vanilla intensity:1.8
                __instance.Intensity = Plugin.nGain.Value * Plugin.globalGain.Value;
                //vanilla noiseintensity:0.04
                __instance.NoiseIntensity = Plugin.nNoiseIntensity.Value;
                //vanilla noisescale:2
                __instance.NoiseScale = 1f - Plugin.nNoiseSize.Value;
                __instance.MaskSize = Plugin.nMaskSize.Value * Plugin.globalMaskSize.Value;
                __instance.Color.r = Plugin.nR.Value / 255;
                __instance.Color.g = Plugin.nG.Value / 255;
                __instance.Color.b = Plugin.nB.Value / 255;
            }
            //pnv10t id: 5c0696830db834001d23f5da
            if (nvgID == "5c0696830db834001d23f5da")
            {
                //vanilla intensity:2
                __instance.Intensity = Plugin.pnvGain.Value * Plugin.globalGain.Value;
                //vanilla noiseintensity:0.05
                __instance.NoiseIntensity = Plugin.pnvNoiseIntensity.Value;
                //vanilla noisescale:1
                __instance.NoiseScale = 1f - Plugin.pnvNoiseSize.Value;
                __instance.MaskSize = Plugin.pnvMaskSize.Value * Plugin.globalMaskSize.Value;
                __instance.Color.r = Plugin.pnvR.Value / 255;
                __instance.Color.g = Plugin.pnvG.Value / 255;
                __instance.Color.b = Plugin.pnvB.Value / 255;
            }

            __instance.ApplySettings();
        }
    }
}
