using SPT.Reflection.Patching;
using EFT.UI;
using EFT.UI.Screens;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WindowsInput.Native;
using Comfort.Common;
using WindowsInput;
using EFT;
using UnityEngine.UIElements;

namespace BorkelRNVG.Patches
{
    internal class MenuPatch : ModulePatch //if Awake is prevented from running, the exaggerated bloom goes away
    {
        private static async Task ToggleReshadeAsync(InputSimulator inputSimulator, VirtualKeyCode key)
        {
            inputSimulator.Keyboard.KeyDown(key);
            await Task.Delay(200);
            inputSimulator.Keyboard.KeyUp(key);
        }
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MenuTaskBar), "OnScreenChanged");
        }

        [PatchPrefix]
        private static void PatchPrefix(EEftScreenType eftScreenType)
        {
            if (!Plugin.enableReshade.Value || !Plugin.disableReshadeInMenus.Value)
                return;
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

            if (player.NightVisionObserver.Component == null
                || player.NightVisionObserver.Component.Item == null
                || player.NightVisionObserver.Component.Item.TemplateId == null)
            {
                return;
            }
            InputSimulator poop = new InputSimulator();
            switch (eftScreenType)
            {
                case EEftScreenType.None:
                case EEftScreenType.BattleUI:
                    if(Plugin.nvgOn)
                            Task.Run(() => ToggleReshadeAsync(poop, Plugin.nvgKey));
                    break;
                default:
                    if(Plugin.nvgOn)
                        Task.Run(() => ToggleReshadeAsync(poop, VirtualKeyCode.NUMPAD5));
                    break;
            }
        }
    }
}
