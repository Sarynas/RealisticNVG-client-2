using BepInEx;
using BepInEx.Configuration;
using BorkelRNVG.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WindowsInput.Native;
using Comfort.Common;
using BepInEx.Logging;
using BorkelRNVG.Helpers.Configuration;
using BorkelRNVG.Helpers;

namespace BorkelRNVG
{
    [BepInPlugin("com.borkel.nvgmasks", "Borkel's Realistic NVGs", "1.6.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;

        //categories
        public static string miscCategory = "0. Miscellaneous";
        public static string gatingCategory = "1. Gating";
        public static string globalCategory = "2. Global";
        public static string gpnvgCategory = "3. GPNVG-18";
        public static string pvsCategory = "4. PVS-14";
        public static string nCategory = "5. N-15";
        public static string pnvCategory = "6. PNV-10T";
        public static string t7Category = "7. T-7";

        // global
        public static ConfigEntry<float> globalMaskSize;
        public static ConfigEntry<float> globalGain;

        // T-7 specific
        public static ConfigEntry<bool> t7Pixelation;
        public static ConfigEntry<bool> t7HzLock;

        //sprint patch stuff
        public static ConfigEntry<bool> enableSprintPatch;
        public static bool isSprinting = false;
        public static bool wasSprinting = false;
        public static Dictionary<string, bool> LightDictionary = new Dictionary<string, bool>();

        //UltimateBloom stuff
        //public static BloomAndFlares BloomAndFlaresInstance;
        //public static UltimateBloom UltimateBloomInstance;

        //Reshade stuff
        public static VirtualKeyCode nvgKey = VirtualKeyCode.NUMPAD0;
        public static ConfigEntry<bool> enableReshade;
        public static ConfigEntry<bool> disableReshadeInMenus;
        //public static bool disabledInMenu = false;

        // Gating
        public static ConfigEntry<KeyCode> gatingInc;
        public static ConfigEntry<KeyCode> gatingDec;
        public static ConfigEntry<int> gatingLevel;
        public static ConfigEntry<bool> enableAutoGating;

        public static bool nvgOn = false;

        private void Awake()
        {
            // BepInEx F12 menu
            Log = Logger;

            // Miscellaneous
            enableSprintPatch = Config.Bind(miscCategory, "Sprint toggles tactical devices. DO NOT USE WITH FIKA.", false, "Sprinting will toggle tactical devices until you stop sprinting, this mitigates the IR lights being visible outside of the NVGs. I recommend enabling this feature.");
            enableReshade = Config.Bind(miscCategory, "Enable ReShade input simulation", false, "Will enable the input simulation to enable the ReShade, will use numpad keys. GPNVG-18 -> numpad 9. PVS-14 -> numpad 8. N-15 -> numpad 7. PNV-10T -> numpad 6. Off -> numpad 5. Only enable if you've installed the ReShade.");
            disableReshadeInMenus = Config.Bind(miscCategory, "Disable ReShade when in menus", true, "Is a bit wonky in the hideout, but works well in-raid.");
            
            // Gating
            gatingInc = Config.Bind(gatingCategory, "1. Manual gating increase", KeyCode.None, "Increases the gain by 1 step. There's 5 levels (-2...2), default level is the third level (0).");
            gatingDec = Config.Bind(gatingCategory, "2. Manual gating decrease", KeyCode.None, "Decreases the gain by 1 step. There's 5 levels (-2...2), default level is the third level (0).");
            gatingLevel = Config.Bind(gatingCategory, "3. Gating level", 0, "Will reset when the game opens. You are supposed to use the gating increase/decrease keys to change the gating level, but you are free to change it manually if you want to make sure you are at a specific gating level.");
            enableAutoGating = Config.Bind(gatingCategory, "4. Enable Auto-Gating", false, "EXPERIMENTAL! WILL REDUCE FPS! Enables auto-gating (automatic brightness adjustment) for certain night vision devices. Auto-gating WILL NOT work without this enabled.");

            // Global multipliers
            globalMaskSize = Config.Bind(globalCategory, "1. Mask size multiplier", 1.07f, new ConfigDescription("Applies size multiplier to all masks", new AcceptableValueRange<float>(0f, 2f)));
            globalGain = Config.Bind(globalCategory, "2. Gain multiplier", 1f, new ConfigDescription("Applies gain multiplier to all NVGs", new AcceptableValueRange<float>(0f, 5f)));

            // other variables.. idk
            gatingLevel.Value = 0;

            // create nvg config classes
            Util.InitializeVars();
            AssetHelper.LoadTextures();
            AssetHelper.LoadShaders();
            AssetHelper.LoadAudioClips();
            NightVisionItemConfig.InitializeNVGs(Config);
            
            try
            {
                new NightVisionAwakePatch().Enable();
                new NightVisionApplySettingsPatch().Enable();
                new NightVisionSetMaskPatch().Enable();
                new ThermalVisionSetMaskPatch().Enable();
                new SprintPatch().Enable();
                new NightVisionMethod_1().Enable(); //reshade
                new MenuPatch().Enable(); //reshade
                new StartFireEffectsPatch().Enable();

                Logger.LogInfo("Patches enabled successfully!");
            }
            catch (Exception exception)
            {
                Logger.LogError(exception);
            }
            
            // umm......
            //new VignettePatch().Enable();
            //new EndOfRaid().Enable(); //reshade
            //new WeaponSwapPatch().Enable(); //not working
            //new UltimateBloomPatch().Enable(); //works if Awake is prevented from running
            //new LevelSettingsPatch().Enable();
        }

        void Update()
        {
            if(nvgOn)
            {
                if(Input.GetKeyDown(gatingInc.Value) && gatingLevel.Value < 2)
                {
                    gatingLevel.Value++;
                    Singleton<BetterAudio>.Instance.PlayAtPoint(new Vector3(0, 0, 0), AssetHelper.LoadedAudioClips["gatingKnob.wav"], 0, BetterAudio.AudioSourceGroupType.Nonspatial, 100, 1.0f, EOcclusionTest.None, null, false);
                }
                else if(Input.GetKeyUp(gatingDec.Value) && gatingLevel.Value > -2)
                {
                    gatingLevel.Value--;
                    Singleton<BetterAudio>.Instance.PlayAtPoint(new Vector3(0, 0, 0), AssetHelper.LoadedAudioClips["gatingKnob.wav"], 0, BetterAudio.AudioSourceGroupType.Nonspatial, 100, 1.0f, EOcclusionTest.None, null, false);
                }
            }
        }
    }
}
