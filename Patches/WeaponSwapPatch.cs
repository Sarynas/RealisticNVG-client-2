using Aki.Reflection.Patching;
using BSG.CameraEffects;
using EFT;
using HarmonyLib;
using Comfort.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EFT.Player;
using LightStruct = GStruct155; //public static void Serialize(GInterface63 stream, ref GStruct155 tacticalComboStatus)
using EFT.InventoryLogic;
using static EFT.Interactive.Turnable;

namespace BorkelRNVG.Patches
{
    internal class WeaponSwapPatch : ModulePatch //supposed to turn off devices when swapping/holstering weapons
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2195), "InitHandsContainer");
        }

        [PatchPrefix]
        private static void PatchPrefix(NightVision __instance, ref Shader ___Shader)
        {
            Logger.LogMessage("SWAPPING");
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
            FirearmController fc = player.HandsController as FirearmController;
            foreach (Mod mod in fc.Item.Mods)
            {
                LightComponent light;
                if (mod.TryGetItemComponent<LightComponent>(out light))
                {
                    bool isOn = light.IsActive;
                    bool state = false;
                    if (isOn)
                        state = false;
                    fc.SetLightsState(new LightStruct[]
                    {
                    new LightStruct
                    {
                        Id = light.Item.Id,
                        IsActive = state,
                        LightMode = light.SelectedMode
                    }
                        }, false);
                }
            }
        }
    }
}
