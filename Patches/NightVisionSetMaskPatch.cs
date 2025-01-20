using SPT.Reflection.Patching;
using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using BorkelRNVG.Helpers.Configuration;


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
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            var player = gameWorld.MainPlayer;
            if (player == null) return;

            if (player.NightVisionObserver.Component == null
                || player.NightVisionObserver.Component.Item == null
                || player.NightVisionObserver.Component.Item.StringTemplateId == null)
                return;

            string nvgID = player.NightVisionObserver.Component.Item.StringTemplateId;
            Texture2D nvgMask = NightVisionItemConfig.Get(nvgID)?.BinocularMaskTexture;

            if (nvgMask == null) return;

            __instance.Mask = nvgMask;
        }
    }
}
