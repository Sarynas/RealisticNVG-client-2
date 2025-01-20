using SPT.Reflection.Patching;
using BSG.CameraEffects;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using BorkelRNVG.Helpers;
using BorkelRNVG.Helpers.Enum;

namespace BorkelRNVG.Patches
{
    internal class NightVisionAwakePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NightVision), "Awake");
        }

        [PatchPrefix]
        private static void PatchPrefix(NightVision __instance, ref Shader ___Shader) //___Shader is the same as __instance.Shader
        {
            //replaces the masks in the class NightVision and applies visual changes
            //Plugin.UltimateBloomInstance = __instance.GetComponent<UltimateBloom>(); //to disable it when NVG turns ON
            //Plugin.BloomAndFlaresInstance = __instance.GetComponent<BloomAndFlares>(); //to disable it when NVG turns ON

            __instance.AnvisMaskTexture = AssetHelper.MaskTextures[ENVGTexture.Anvis];
            __instance.BinocularMaskTexture = AssetHelper.MaskTextures[ENVGTexture.Binocular];
            __instance.OldMonocularMaskTexture = AssetHelper.MaskTextures[ENVGTexture.Monocular];
            __instance.ThermalMaskTexture = AssetHelper.MaskTextures[ENVGTexture.Thermal];
            __instance.Noise = AssetHelper.MaskTextures[ENVGTexture.Noise];

            // :^)
            if (__instance.Color.g > 0.9f)
            {
                ___Shader = AssetHelper.nightVisionShader;
            }
        }
    }
}
