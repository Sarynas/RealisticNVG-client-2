using SPT.Reflection.Patching;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using BorkelRNVG.Helpers;
using BorkelRNVG.Helpers.Enum;

namespace BorkelRNVG.Patches
{
    internal class ThermalVisionSetMaskPatch : ModulePatch
    {
        // This will patch the instance of the ThermalVision class to edit the T-7

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ThermalVision), nameof(ThermalVision.SetMask));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref ThermalVision __instance)
        {
            if (__instance.IsPixelated)
            {
                return;
            }

            //this is all for the T7
            //__instance.TextureMask.Size = 1f;
            //__instance.ThermalVisionUtilities.MaskDescription.MaskSize = 1f; //for some reason changing mask size does not work

            MaskDescription maskDescription = __instance.ThermalVisionUtilities.MaskDescription;
            PixelationUtilities pixelationUtilities = __instance.PixelationUtilities;

            maskDescription.Mask = AssetHelper.MaskTextures[ENVGTexture.Thermal];
            maskDescription.Mask.wrapMode = TextureWrapMode.Clamp;
            maskDescription.OldMonocularMaskTexture = AssetHelper.MaskTextures[ENVGTexture.Monocular];
            maskDescription.OldMonocularMaskTexture.wrapMode = TextureWrapMode.Clamp;
            maskDescription.ThermalMaskTexture = AssetHelper.MaskTextures[ENVGTexture.Thermal];
            maskDescription.ThermalMaskTexture.wrapMode = TextureWrapMode.Clamp;

            __instance.IsPixelated = true;
            __instance.IsNoisy = false;
            __instance.IsMotionBlurred = true;
            __instance.PixelationUtilities = new PixelationUtilities();

            if(Plugin.t7Pixelation.Value)
            {
                //__instance.PixelationUtilities.Mode = GClass866.PixelationMode.CRT;
                pixelationUtilities.Mode = 0;
                pixelationUtilities.BlockCount = 320; //doesn't do anything really
                pixelationUtilities.PixelationMask = AssetHelper.MaskTextures[ENVGTexture.Pixel];
                pixelationUtilities.PixelationShader = AssetHelper.pixelationShader;
            }

            if(Plugin.t7HzLock.Value)
            {
                __instance.IsFpsStuck = true;
                __instance.StuckFpsUtilities = new StuckFPSUtilities()
                {
                    MinFramerate = 60,
                    MaxFramerate = 60
                };
            }
        }
    }
}
