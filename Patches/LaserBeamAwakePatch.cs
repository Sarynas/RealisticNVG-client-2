using BorkelRNVG.Helpers;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace BorkelRNVG.Patches
{
    public class LaserBeamAwakePatch : ModulePatch
    {
        private static Material _beamMaterial;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LaserBeam), nameof(LaserBeam.Awake));
        }

        [PatchPostfix]
        private static void PatchPostfix(LaserBeam __instance)
        {
            //_beamMaterial = AssetHelper.LoadMaterial("assets/systems/effects/laserbeam/laserbeam_stencil.mat", $"{AssetHelper.assetsDirectory}\\Materials\\pein_mats");
            __instance.BeamMaterial = _beamMaterial;
        }
    }
}
