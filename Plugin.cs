using EFT;
using Aki.Reflection.Patching;
using Comfort.Common;
using BepInEx;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
using BSG.CameraEffects;
using System.Xml.Linq;
using EFT.InventoryLogic;
using BepInEx.Configuration;
using static GClass1607;

namespace BorkelRNVG
{
    [BepInPlugin("com.borkel.nvgmasks", "Borkel's Realistic NVGs", "1.3.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //mask textures
        public static Texture2D maskAnvis;
        public static Texture2D maskBino;
        public static Texture2D maskMono;
        public static Texture2D maskThermal;
        public static Texture2D maskPixel; //i don't really know if this one does anything
        public static Shader pixelationShader; //Assets/Systems/Effects/Pixelation/Pixelation.shader
        //player is used to know what nvg is equipped
        public static GameWorld gameWorld;
        public static Player player;
        //global config stuff
        public static ConfigEntry<float> globalMaskSize;
        public static ConfigEntry<float> globalGain;
        //gpnvg18 config stuff
        public static ConfigEntry<float> quadR;
        public static ConfigEntry<float> quadG;
        public static ConfigEntry<float> quadB;
        public static ConfigEntry<float> quadMaskSize;
        public static ConfigEntry<float> quadNoiseIntensity;
        public static ConfigEntry<float> quadNoiseSize;
        public static ConfigEntry<float> quadGain;
        //pvs14 config stuff
        public static ConfigEntry<float> pvsR;
        public static ConfigEntry<float> pvsG;
        public static ConfigEntry<float> pvsB;
        public static ConfigEntry<float> pvsMaskSize;
        public static ConfigEntry<float> pvsNoiseIntensity;
        public static ConfigEntry<float> pvsNoiseSize;
        public static ConfigEntry<float> pvsGain;
        //n15 config stuff
        public static ConfigEntry<float> nR;
        public static ConfigEntry<float> nG;
        public static ConfigEntry<float> nB;
        public static ConfigEntry<float> nMaskSize;
        public static ConfigEntry<float> nNoiseIntensity;
        public static ConfigEntry<float> nNoiseSize;
        public static ConfigEntry<float> nGain;
        //pnv10t config stuff
        public static ConfigEntry<float> pnvR;
        public static ConfigEntry<float> pnvG;
        public static ConfigEntry<float> pnvB;
        public static ConfigEntry<float> pnvMaskSize;
        public static ConfigEntry<float> pnvNoiseIntensity;
        public static ConfigEntry<float> pnvNoiseSize;
        public static ConfigEntry<float> pnvGain;

        private void Awake()
        {
            //############-BEPINEX F12-MENU##############
            //Global multipliers
            globalMaskSize = Config.Bind("0.Globals (changes apply on NVG reactivation)", "Mask size multiplier", 1f, new ConfigDescription("Applies size multiplier to all masks", new AcceptableValueRange<float>(0f, 2f)));
            globalGain = Config.Bind("0.Globals (changes apply on NVG reactivation)", "Gain multiplier", 1f, new ConfigDescription("Applies gain multiplier to all NVGs", new AcceptableValueRange<float>(0f, 5f)));
            //GPNVG-18 config
            quadGain = Config.Bind("1.GPNVG-18", "1.Gain", 2.5f, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            quadNoiseIntensity = Config.Bind("1.GPNVG-18", "2.Noise intensity", 0.035f, new ConfigDescription("", new AcceptableValueRange<float>(0f,0.2f)));
            quadNoiseSize = Config.Bind("1.GPNVG-18", "3.Noise scale", 0.05f, new ConfigDescription("", new AcceptableValueRange<float>(0f,1f)));
            quadMaskSize = Config.Bind("1.GPNVG-18", "4.Mask size", 0.96f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            quadR = Config.Bind("1.GPNVG-18", "5.Red", 152f, new ConfigDescription("", new AcceptableValueRange<float>(0f,255f)));
            quadG = Config.Bind("1.GPNVG-18", "6.Green", 214f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            quadB = Config.Bind("1.GPNVG-18", "7.Blue", 252f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            //PVS-14 config
            pvsGain = Config.Bind("2.PVS-14", "1.Gain", 2.4f, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            pvsNoiseIntensity = Config.Bind("2.PVS-14", "2.Noise intensity", 0.04f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 0.2f)));
            pvsNoiseSize = Config.Bind("2.PVS-14", "3.Noise scale", 0.05f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            pvsMaskSize = Config.Bind("2.PVS-14", "4.Mask size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            pvsR = Config.Bind("2.PVS-14", "5.Red", 95f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            pvsG = Config.Bind("2.PVS-14", "6.Green", 210f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            pvsB = Config.Bind("2.PVS-14", "7.Blue", 255f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            //N-15 config
            nGain = Config.Bind("3.N-15", "1.Gain", 2.1f, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            nNoiseIntensity = Config.Bind("3.N-15", "2.Noise intensity", 0.05f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 0.2f)));
            nNoiseSize = Config.Bind("3.N-15", "3.Noise scale", 0.15f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            nMaskSize = Config.Bind("3.N-15", "4.Mask size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            nR = Config.Bind("3.N-15", "5.Red", 60f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            nG = Config.Bind("3.N-15", "6.Green", 235f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            nB = Config.Bind("3.N-15", "7.Blue", 100f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            //PNV-10T config
            pnvGain = Config.Bind("4.PNV-10T", "1.Gain", 1.8f, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            pnvNoiseIntensity = Config.Bind("4.PNV-10T", "2.Noise intensity", 0.07f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 0.2f)));
            pnvNoiseSize = Config.Bind("4.PNV-10T", "3.Noise scale", 0.2f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            pnvMaskSize = Config.Bind("4.PNV-10T", "4.Mask size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            pnvR = Config.Bind("4.PNV-10T", "5.Red", 60f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            pnvG = Config.Bind("4.PNV-10T", "6.Green", 210f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            pnvB = Config.Bind("4.PNV-10T", "7.Blue", 60f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            //###########################################

            string directory = Plugin.directory;//directory contains string of path where the .dll is located, for me it is C:\SPTarkov3.7.1\BepInEx\plugins
            //loading from PNGs, like Fontaine suggested
            string anvisPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_anvis.png";
            string binoPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_binocular.png";
            string monoPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_old_monocular.png";
            string thermalPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_thermal.png";
            string pixelPath = $"{directory}\\BorkelRNVG\\PNGtextures\\pixel_mask1.png";
            maskAnvis = LoadPNG(anvisPath);
            maskBino = LoadPNG(binoPath);
            maskMono = LoadPNG(monoPath);
            maskThermal = LoadPNG(thermalPath);
            maskPixel = LoadPNG(pixelPath);//might not do anything really
            pixelationShader = LoadShader("Assets/Systems/Effects/Pixelation/Pixelation.shader"); //to pixelate teh T-7
            maskAnvis.wrapMode = TextureWrapMode.Clamp;
            maskBino.wrapMode = TextureWrapMode.Clamp; //otherwise the mask will repeat itself around screen borders
            maskMono.wrapMode = TextureWrapMode.Clamp;
            maskThermal.wrapMode = TextureWrapMode.Clamp;
            if (maskAnvis == null || maskBino == null || maskMono == null || maskThermal == null || maskPixel == null)
            {
                Logger.LogError($"Error loading PNGs.");
                return;
            }
            new SetMaskPatch().Enable();
            new SetColorPatch().Enable();
            if (pixelationShader == null)
            {
                Logger.LogError($"Error loading Shader.");
                return;
            }
            new SetThermalMaskPatch().Enable();
        }
        public static Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }
        public static Shader LoadShader(string shaderName) //for the thermals
        {
            Shader sh = null;
            string directory = Plugin.directory;
            //string bundlePath2 = $"{directory}\\BorkelRNVG\\Shader\\shaders";
            string parent = Directory.GetParent(directory).FullName;
            string parent2 = Directory.GetParent(parent).FullName;
            string bundlePath = $"{parent2}\\EscapeFromTarkov_Data\\StreamingAssets\\Windows\\shaders";
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            sh = assetBundle.LoadAsset<Shader>(shaderName);
            assetBundle.Unload(false);
            return sh;
        }
    }
    public class SetColorPatch : ModulePatch
    { //patches the color, gain and noise using values from the config menu
        protected override MethodBase GetTargetMethod()
        {
            return typeof(NightVision).GetMethod("StartSwitch", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPostfix]
        private static void PatchPostFix(ref NightVision __instance)
        {
            __instance.Color.a = (float)254 / 255; //i think it does nothing
            __instance.MaskSize = 1; //does not affect the t-7 for some reason
            Plugin.gameWorld = Singleton<GameWorld>.Instance;
            Plugin.player = Plugin.gameWorld.MainPlayer;
            if (Plugin.gameWorld != null && Plugin.player != null)
            {
                if(Plugin.player.NightVisionObserver.Component != null && Plugin.player.NightVisionObserver.Component.Item != null &&
                    Plugin.player.NightVisionObserver.Component.Item.TemplateId != null)
                {
                    string nvgID = Plugin.player.NightVisionObserver.Component.Item.TemplateId; //ID of the nvg
                    //gpnvg18 id: 5c0558060db834001b735271
                    if (nvgID == "5c0558060db834001b735271")
                    {
                        //vanilla intensity:2.27
                        __instance.Intensity = Plugin.quadGain.Value * Plugin.globalGain.Value;
                        //vanilla noiseintensity:0.02
                        __instance.NoiseIntensity = Plugin.quadNoiseIntensity.Value;
                        //vanilla noisescale:5 bigger number means smaller noise
                        //__instance.NoiseScale = 0.95F; -> 0.05 in the bepinex menu, smaller number will mean smaller noise (easier for the user)
                        __instance.NoiseScale = 1f - Plugin.quadNoiseSize.Value;
                        __instance.MaskSize = Plugin.quadMaskSize.Value * Plugin.globalMaskSize.Value;
                        __instance.Color.r = Plugin.quadR.Value / 255f;
                        __instance.Color.g = Plugin.quadG.Value / 255f;
                        __instance.Color.b = Plugin.quadB.Value / 255f;
                    }
                    //pvs14 id: 57235b6f24597759bf5a30f1
                    if (nvgID == "57235b6f24597759bf5a30f1")
                    {
                        //vanilla intensity:2.27
                        __instance.Intensity = Plugin.pvsGain.Value * Plugin.globalGain.Value;
                        //vanilla noiseintensity:0.02
                        __instance.NoiseIntensity = Plugin.pvsNoiseIntensity.Value;
                        //vanilla noisescale:5
                        __instance.NoiseScale = 1f - Plugin.pvsNoiseSize.Value;
                        __instance.MaskSize = Plugin.pvsMaskSize.Value * Plugin.globalMaskSize.Value;
                        __instance.Color.r = Plugin.pvsR.Value / 255;
                        __instance.Color.g = Plugin.pvsG.Value / 255;
                        __instance.Color.b = Plugin.pvsB.Value / 255;

                    }
                    //n15 id: 5c066e3a0db834001b7353f0
                    if (nvgID == "5c066e3a0db834001b7353f0")
                    {
                        //vanilla intensity:1.8
                        __instance.Intensity = Plugin.nGain.Value * Plugin.globalGain.Value;
                        //vanilla noiseintensity:0.04
                        __instance.NoiseIntensity = Plugin.nNoiseIntensity.Value;
                        //vanilla noisescale:2
                        __instance.NoiseScale = 1f - Plugin.nNoiseSize.Value;
                        __instance.MaskSize = Plugin.nMaskSize.Value * Plugin.globalMaskSize.Value;
                        __instance.Color.r = Plugin.nR.Value / 255;
                        __instance.Color.g = Plugin.nG.Value / 255;
                        __instance.Color.b = Plugin.nB.Value / 255;
                    }
                    //pnv10t id: 5c0696830db834001d23f5da
                    if (nvgID == "5c0696830db834001d23f5da")
                    {
                        //vanilla intensity:2
                        __instance.Intensity = Plugin.pnvGain.Value * Plugin.globalGain.Value;
                        //vanilla noiseintensity:0.05
                        __instance.NoiseIntensity = Plugin.pnvNoiseIntensity.Value;
                        //vanilla noisescale:1
                        __instance.NoiseScale = 1f - Plugin.pnvNoiseSize.Value;
                        __instance.MaskSize = Plugin.pnvMaskSize.Value * Plugin.globalMaskSize.Value;
                        __instance.Color.r = Plugin.pnvR.Value / 255;
                        __instance.Color.g = Plugin.pnvG.Value / 255;
                        __instance.Color.b = Plugin.pnvB.Value / 255;
                    }
                }
            }
            __instance.ApplySettings();
        }
    }
    public class SetMaskPatch : ModulePatch
    {  //this will patch the instance of the NightVision class, thanks Fontaine, Mirni, Cj, GrooveypenguinX, Choccster, kiobu-kouhai, GrakiaXYZ, kiki, Props (sorry if i forget someone)
        protected override MethodBase GetTargetMethod()
        {
            return typeof(NightVision).GetMethod("SetMask", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPostfix]
        private static void PatchPostfix(ref NightVision __instance)
        {

            //replaces the masks in the class NightVision and applies visual changes
            __instance.AnvisMaskTexture = Plugin.maskAnvis;
            __instance.BinocularMaskTexture = Plugin.maskBino;
            __instance.OldMonocularMaskTexture = Plugin.maskMono;
            __instance.ThermalMaskTexture = Plugin.maskMono;
            Plugin.gameWorld = Singleton<GameWorld>.Instance;
            Plugin.player = Plugin.gameWorld.MainPlayer;
            if (Plugin.gameWorld != null && Plugin.player != null)
            {

                if (Plugin.player.NightVisionObserver.Component != null && Plugin.player.NightVisionObserver.Component.Item != null &&
                    Plugin.player.NightVisionObserver.Component.Item.TemplateId != null) //checks if the player has an nvg
                {
                    string nvgID = Plugin.player.NightVisionObserver.Component.Item.TemplateId; //ID of the nvg
                    //n15 id: 5c066e3a0db834001b7353f0
                    if (nvgID == "5c066e3a0db834001b7353f0")
                    {
                        __instance.BinocularMaskTexture = Plugin.maskBino; //makes sure the N-15 is binocular after patching the PNV-10T

                    }
                    //pnv10t id: 5c0696830db834001d23f5da
                    if (nvgID == "5c0696830db834001d23f5da")
                    {
                        __instance.BinocularMaskTexture = Plugin.maskMono; //forces the PNV-10T to use monocular mask

                    }
                }
            }
            __instance.ApplySettings();
        }
    }
    public class SetThermalMaskPatch : ModulePatch
    {  //this will patch the instance of the ThermalVision class to edit the T-7
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ThermalVision).GetMethod("SetMask", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPostfix]
        private static void PatchPostfix(ref ThermalVision __instance)
        {
            if (__instance.IsPixelated == false)
            {
                //this is all for the T7
                //__instance.TextureMask.Size = 1f;
                //__instance.ThermalVisionUtilities.MaskDescription.MaskSize = 1f; //for some reason changing mask size does not work
                __instance.ThermalVisionUtilities.MaskDescription.Mask = Plugin.maskThermal;
                __instance.ThermalVisionUtilities.MaskDescription.Mask.wrapMode = TextureWrapMode.Clamp;
                __instance.ThermalVisionUtilities.MaskDescription.OldMonocularMaskTexture = Plugin.maskThermal;
                __instance.ThermalVisionUtilities.MaskDescription.OldMonocularMaskTexture.wrapMode = TextureWrapMode.Clamp;
                __instance.ThermalVisionUtilities.MaskDescription.ThermalMaskTexture = Plugin.maskThermal;
                __instance.ThermalVisionUtilities.MaskDescription.ThermalMaskTexture.wrapMode = TextureWrapMode.Clamp;
                __instance.IsPixelated = true;
                __instance.IsNoisy = false;
                __instance.IsMotionBlurred = true;
                __instance.PixelationUtilities = new PixelationUtilities();
                __instance.PixelationUtilities.Mode = 0;
                __instance.PixelationUtilities.BlockCount = 320; //doesn't do anything really
                __instance.PixelationUtilities.PixelationMask = Plugin.maskPixel;
                __instance.PixelationUtilities.PixelationShader = Plugin.pixelationShader;
                __instance.StuckFpsUtilities = new StuckFPSUtilities();
                __instance.IsFpsStuck = true;
                __instance.StuckFpsUtilities.MinFramerate = 60;
                __instance.StuckFpsUtilities.MaxFramerate = 60;
            }
        }
    }
}
