using Aki.Reflection.Patching;
using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Numerics;
using System.Reflection;
using UnityEngine;
using LightStruct = GStruct163; //public static void Serialize(GInterface63 stream, ref GStruct155 tacticalComboStatus)
using static EFT.Player;
using System.Collections;
using System.Threading.Tasks;

namespace BorkelRNVG.Patches
{
    internal class SprintPatch : ModulePatch
    {
        private static async Task ToggleLaserWithDelay(FirearmController fc, LightComponent light, bool newState, int delay)
        {
            await Task.Delay(delay);
            fc.SetLightsState(new LightStruct[]
            {
            new LightStruct
            {
                Id = light.Item.Id,
                IsActive = newState,
                LightMode = light.SelectedMode
            }
                }, false);
        }
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.LateUpdate));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player __instance)
        {
            if (!__instance.IsYourPlayer || __instance.CurrentManagedState==null ||
                __instance.CurrentManagedState.Name.ToString() == "Jump" || !Plugin.enableSprintPatch.Value || __instance.HandsController==null)
                return;
            Plugin.isSprinting = __instance.IsSprintEnabled;
            FirearmController fc = __instance.HandsController as FirearmController;
            if (fc == null)
                return;
            if (Plugin.isSprinting != Plugin.wasSprinting) //if the player goes from sprinting to not sprinting, or from not sprinting to sprinting
            {
                foreach(Mod mod in fc.Item.Mods)
                {
                    LightComponent light;
                    if (mod.TryGetItemComponent<LightComponent>(out light))
                    {
                        if (!Plugin.LightDictionary.ContainsKey(mod.Id))
                            Plugin.LightDictionary.Add(mod.Id, false);
                        bool isOn = light.IsActive;
                        bool state = false;
                        if (Plugin.isSprinting == false && !isOn && Plugin.LightDictionary[mod.Id])
                        {
                            state = true;
                            Plugin.LightDictionary[mod.Id] = false;
                            Task.Run(() => ToggleLaserWithDelay(fc, light, state, 300));
                            //delay of 300ms when turning on
                        }
                        else if(Plugin.isSprinting == true && isOn)
                        {
                            state = false;
                            Plugin.LightDictionary[mod.Id] = true;
                            Task.Run(() => ToggleLaserWithDelay(fc, light, state, 100));
                        }
                    }
                }
            }
            Plugin.wasSprinting = Plugin.isSprinting;
        }
    }
}
