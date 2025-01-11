using SPT.Reflection.Patching;
using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using WindowsInput.Native;
using System.Collections.Generic;

namespace BorkelRNVG.Patches
{
    internal class NightVisionApplySettingsPatch : ModulePatch
    {
        public static List<NightVisionItemConfig> nightVisionConfigs = new List<NightVisionItemConfig>();

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NightVision), nameof(NightVision.ApplySettings));
        }

        [PatchPrefix]
        private static void PatchPrefix(ref NightVision __instance, ref TextureMask ___TextureMask, ref Texture ___Mask)
        {
            ApplyModSettings(ref __instance);

            if (___TextureMask == null) return;

            int maskId = Shader.PropertyToID("_Mask");
            int invMaskSizeId = Shader.PropertyToID("_InvMaskSize");
            int invAspectId = Shader.PropertyToID("_InvAspect");
            int cameraAspectId = Shader.PropertyToID("_CameraAspect");

            var material = (Material)AccessTools.Property(__instance.GetType(), "Material_0").GetValue(__instance);

            var lensMask = Plugin.GetMatchingLensMask(___Mask);
            if (lensMask != null)
            {
                material.SetTexture(maskId, lensMask);
            }

            material.SetFloat(invMaskSizeId, 1f / __instance.MaskSize);

            float invAspectValue = ___Mask != null
                ? ___Mask.height / (float)___Mask.width
                : 1f;
            material.SetFloat(invAspectId, invAspectValue);

            var textureMaskCamera = (Camera)AccessTools.Field(___TextureMask.GetType(), "camera_0").GetValue(___TextureMask);
            float cameraAspectValue = textureMaskCamera != null
                ? textureMaskCamera.aspect
                : Screen.width / (float)Screen.height;
            material.SetFloat(cameraAspectId, cameraAspectValue);
        }

        private static void ApplyModSettings(ref NightVision nightVision)
        {
            nightVision.Color.a = 1;
            nightVision.MaskSize = 1; // does not affect the t-7 for some reason

            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            var player = gameWorld.MainPlayer;
            if (player == null) return;

            if (player.NightVisionObserver.Component == null
                || player.NightVisionObserver.Component.Item == null
                || player.NightVisionObserver.Component.Item.StringTemplateId == null)
                return;

            string nvgID = player.NightVisionObserver.Component.Item.StringTemplateId; // ID of the NVG
            NightVisionItemConfig nvgConfig = NightVisionItemConfig.Get(nvgID);

            if (nvgConfig != null)
            {
                nvgConfig.Update();

                // grab the values from the (now updated) night vision item config
                nightVision.Intensity = nvgConfig.Intensity;
                nightVision.NoiseIntensity = nvgConfig.NoiseIntensity;
                nightVision.NoiseScale = nvgConfig.NoiseScale;
                nightVision.MaskSize = nvgConfig.MaskSize;
                nightVision.Color.r = nvgConfig.R;
                nightVision.Color.g = nvgConfig.G;
                nightVision.Color.b = nvgConfig.B;
                Plugin.nvgKey = nvgConfig.Key;
            }
        }
    }
}