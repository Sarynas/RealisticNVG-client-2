using Aki.Reflection.Patching;
using BSG.CameraEffects;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BorkelRNVG.Patches
{
    internal class NightVisionApplySettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NightVision), nameof(NightVision.ApplySettings));
        }

        [PatchPrefix]
        private static void PatchPrefix(NightVision __instance, ref TextureMask ___TextureMask, ref Texture ___Mask)
        {
            if (___TextureMask == null)
            {
                return;
            }

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
    }
}
