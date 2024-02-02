using Aki.Reflection.Patching;
using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace BorkelRNVG.Patches
{
    internal class NightVisionSetMaskPatch : ModulePatch
    {
        // This will patch the instance of the NightVision class
        // Thanks Fontaine, Mirni, Cj, GrooveypenguinX, Choccster, kiobu-kouhai, GrakiaXYZ, kiki, Props (sorry if i forget someone)
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NightVision), nameof(NightVision.SetMask));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref NightVision __instance)
        {
            //replaces the masks in the class NightVision
            __instance.AnvisMaskTexture = Plugin.MaskAnvis;
            __instance.BinocularMaskTexture = Plugin.MaskBino;
            __instance.OldMonocularMaskTexture = Plugin.MaskMono;
            __instance.ThermalMaskTexture = Plugin.MaskMono;
            __instance.MaskSize = 1;
            Logger.LogMessage($"After Mask name: {__instance.Mask.name}");
            Logger.LogMessage($"After Anvismask name: {__instance.AnvisMaskTexture.name}");
            Logger.LogMessage($"After Binosmask name: {__instance.BinocularMaskTexture.name}");
            Logger.LogMessage($"After Monomask name: {__instance.OldMonocularMaskTexture.name}");
            //return false; //prevents original method from running, so we can fully override it //not needed

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

            string nvgID = player.NightVisionObserver.Component.Item.TemplateId;
            //gpnvg18 id: 5c0558060db834001b735271
            if (nvgID == "5c0558060db834001b735271")
            {
                //quadnod settings
                //vanilla intensity:2.27
                __instance.Intensity = 2.5F;
                //vanilla noiseintensity:0.02
                __instance.NoiseIntensity = 0.035F;
                //vanilla noisescale:5 smaller number means bigger noise
                __instance.NoiseScale = 0.95F;

                __instance.MaskSize = 0.96F;
                __instance.Color.r = (float)152 / 255;
                __instance.Color.g = (float)214 / 255;
                __instance.Color.b = (float)252 / 255;
                __instance.Color.a = (float)254 / 255;
            }
            //pvs14 id: 57235b6f24597759bf5a30f1
            if (nvgID == "57235b6f24597759bf5a30f1")
            {
                //vanilla intensity:2.27
                __instance.Intensity = 2.4F;
                //vanilla noiseintensity:0.02
                __instance.NoiseIntensity = 0.04F;
                //vanilla noisescale:5
                __instance.NoiseScale = 0.95F;

                __instance.MaskSize = 1F;
                __instance.Color.r = (float)95 / 255;
                __instance.Color.g = (float)210 / 255;
                __instance.Color.b = (float)255 / 255;
                __instance.Color.a = (float)254 / 255;

            }
            //n15 id: 5c066e3a0db834001b7353f0
            if (nvgID == "5c066e3a0db834001b7353f0")
            {
                //vanilla intensity:1.8
                __instance.Intensity = 2.1F;
                //vanilla noiseintensity:0.04
                __instance.NoiseIntensity = 0.05F;
                //vanilla noisescale:2
                __instance.NoiseScale = 0.85F;

                __instance.MaskSize = 1F;
                __instance.Color.r = (float)60 / 255;
                __instance.Color.g = (float)235 / 255;
                __instance.Color.b = (float)100 / 255;
                __instance.Color.a = (float)254 / 255;
                __instance.BinocularMaskTexture = Plugin.MaskBino;

            }
            //pnv10t id: 5c0696830db834001d23f5da
            if (nvgID == "5c0696830db834001d23f5da")
            {
                //vanilla intensity:2
                __instance.Intensity = 1.8F;
                //vanilla noiseintensity:0.05
                __instance.NoiseIntensity = 0.07F;
                //vanilla noisescale:1
                __instance.NoiseScale = 0.8F;

                __instance.MaskSize = 1F;
                __instance.Color.r = (float)60 / 255;
                __instance.Color.g = (float)210 / 255;
                __instance.Color.b = (float)60 / 255;
                __instance.Color.a = (float)254 / 255;
                __instance.BinocularMaskTexture = Plugin.MaskMono;

            }
        }
    }
}
