using Aki.Reflection.Patching;
using BSG.CameraEffects;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BorkelRNVG.Patches
{
    internal class NightVisionMethod_1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NightVision), "method_1");
        }

        [PatchPrefix]
        private static void PatchPrefix(NightVision __instance, bool __0) //if i use the name of the parameter it doesn't work, __0 works correctly
        {
            Logger.LogMessage($"NVG ON? {__0}");
            if (__0)//if on = true
                Plugin.UltimateBloomInstance.enabled = false;
                //Plugin.UltimateBloomInstance.m_BloomIntensity = 0f;
            else
                Plugin.UltimateBloomInstance.enabled = true;
            //Plugin.UltimateBloomInstance.m_BloomIntensity = Plugin.UltimateBloomIntensity;
        }
    }
}
