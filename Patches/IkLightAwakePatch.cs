using EFT.Visual;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BorkelRNVG.Patches
{
    public class LightConfig
    {
        public Light light;
        public float intensity;
        public float range;
    }

    public class IkLightAwakePatch : ModulePatch
    {
        // IK light, {Light, Original Intensity}
        private static Dictionary<IkLight, LightConfig> _ikLights = new();

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(IkLight), nameof(IkLight.Awake));
        }
        
        public static void UpdateIntensityAll()
        {
            foreach (var kvp in _ikLights)
            {
                IkLight ikLight = kvp.Key;
                LightConfig light = kvp.Value;

                if (ikLight == null || light == null)
                {
                    _ikLights.Remove(ikLight);
                    continue;
                }

                UpdateIntensitySingle(ikLight);
            }
        }

        public static void UpdateIntensitySingle(IkLight ikLight)
        {
            bool found = _ikLights.TryGetValue(ikLight, out LightConfig lightConfig);
            FieldInfo intensityField = AccessTools.Field(typeof(IkLight), "float_0");

            if (!found)
            {
                Light spotLight = ikLight.Light;
                float origIntensity = (float)intensityField.GetValue(ikLight);

                _ikLights.Add(ikLight, new LightConfig()
                {
                    light = spotLight,
                    intensity = origIntensity,
                    range = spotLight.range
                });

                intensityField.SetValue(ikLight, origIntensity * Plugin.irFlashlightBrightnessMult.Value);
                spotLight.range *= Plugin.irFlashlightRangeMult.Value;
            }
            else
            {
                Light spotLight = lightConfig.light;
                float origIntensity = lightConfig.intensity;
                float origRange = lightConfig.range;

                intensityField.SetValue(ikLight, origIntensity * Plugin.irFlashlightBrightnessMult.Value);
                spotLight.range = origRange * Plugin.irFlashlightRangeMult.Value;
            }   
        }

        [PatchPostfix]
        private static void PatchPostfix(IkLight __instance)
        {
            UpdateIntensitySingle(__instance);
        }
    }
}
