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

namespace BorkelRNVG
{
    [BepInPlugin("com.borkel.nvgmasks", "my very humble attempt at replacing the damn nvg masks", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        //public static Texture2D[] masks; //my modded masks will be loaded here
        //public AssetBundle bundle; //grabs the bundle with my masks
        public static Texture2D maskAnvis; 
        public static Texture2D maskBino; 
        public static Texture2D maskMono; 
        public static Texture2D maskThermal;
        public static Texture2D maskPixel;
        public static Shader pixelationShader; //Assets/Systems/Effects/Pixelation/Pixelation.shader
        public static string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private void Awake()
        {
            string directory = Plugin.directory;
            //directory contains string of path where the .dll is located, for me it is C:\SPTarkov3.7.1\BepInEx\plugins
            //loading from PNGs, like Fontaine suggested
            string anvisPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_anvis.png";
            string binoPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_binocular.png";
            string monoPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_old_monocular.png";
            string thermalPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_thermal.png";
            string pixelPath = $"{directory}\\BorkelRNVG\\PNGtextures\\pixel_mask1.png";
            maskAnvis = LoadPNG(anvisPath);
            maskBino = LoadPNG(binoPath);
            maskMono = LoadPNG(monoPath);
            maskPixel = LoadPNG(pixelPath);
            maskThermal = LoadPNG(thermalPath);
            pixelationShader = LoadShader("Assets/Systems/Effects/Pixelation/Pixelation.shader");
            maskAnvis.wrapMode = TextureWrapMode.Clamp;
            maskBino.wrapMode = TextureWrapMode.Clamp; //otherwise the mask will repeat itself around screen borders
            maskMono.wrapMode = TextureWrapMode.Clamp;
            maskThermal.wrapMode = TextureWrapMode.Clamp;
            Logger.LogMessage($"Texture2D 0: {maskAnvis}"); //mask 0: mask_anvis
            Logger.LogMessage($"Texture2D 1: {maskBino}"); //mask 1: mask_binocular
            Logger.LogMessage($"Texture2D 2: {maskMono}"); //mask 2: mask_old_monocular
            Logger.LogMessage($"Texture2D 3: {maskPixel}"); //mask 3:
            if (maskAnvis == null || maskBino == null || maskMono==null || maskThermal==null || maskPixel==null)
            {
                Logger.LogError($"Error loading PNGs.");
                return;
            }
            new SetMaskPatch().Enable();
            if(pixelationShader == null)
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
        public static Shader LoadShader(string shaderName)
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
    public class SetMaskPatch : ModulePatch {  //this will patch the instance of the NightVision class, thanks Fontaine, Mirni, Cj, GrooveypenguinX, Choccster, kiobu-kouhai, GrakiaXYZ, kiki, Props (sorry if i forget someone)
        protected override MethodBase GetTargetMethod()
        {
            return typeof(NightVision).GetMethod("SetMask", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPrefix]
        private static void Prefix(ref NightVision __instance)
        {
            //code goes here
            //just to check all the masks are what they are supposed to be
            Logger.LogMessage($"Mask name: {__instance.Mask.name}");
            Logger.LogMessage($"Anvismask name: {__instance.AnvisMaskTexture.name}");
            Logger.LogMessage($"Binosmask name: {__instance.BinocularMaskTexture.name}");
            Logger.LogMessage($"Monomask name: {__instance.OldMonocularMaskTexture.name}");
            //replaces the masks in the class NightVision
            __instance.AnvisMaskTexture = Plugin.maskAnvis;
            __instance.BinocularMaskTexture = Plugin.maskBino;
            __instance.OldMonocularMaskTexture = Plugin.maskMono;
            __instance.ThermalMaskTexture = Plugin.maskMono;
            Logger.LogMessage($"After Mask name: {__instance.Mask.name}");
            Logger.LogMessage($"After Anvismask name: {__instance.AnvisMaskTexture.name}");
            Logger.LogMessage($"After Binosmask name: {__instance.BinocularMaskTexture.name}");
            Logger.LogMessage($"After Monomask name: {__instance.OldMonocularMaskTexture.name}");
            //return false; //prevents original method from running, so we can fully override it //not needed

        }
    }
    public class SetThermalMaskPatch : ModulePatch
    {  //this will patch the instance of the NightVision class, thanks Fontaine, Mirni, Cj, GrooveypenguinX, Choccster, kiobu-kouhai, GrakiaXYZ, kiki, Props (sorry if i forget someone)
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ThermalVision).GetMethod("SetMask", BindingFlags.Instance | BindingFlags.Public);
        }
        [PatchPrefix]
        private static void Prefix(ref ThermalVision __instance)
        {
            if (__instance.IsPixelated==false)
            {
                //this is all for the T7
                __instance.ThermalVisionUtilities.MaskDescription.Mask = Plugin.maskThermal;
                __instance.ThermalVisionUtilities.MaskDescription.Mask.wrapMode = TextureWrapMode.Clamp;
                __instance.ThermalVisionUtilities.MaskDescription.OldMonocularMaskTexture = Plugin.maskThermal;
                __instance.ThermalVisionUtilities.MaskDescription.OldMonocularMaskTexture.wrapMode = TextureWrapMode.Clamp;
                __instance.ThermalVisionUtilities.MaskDescription.ThermalMaskTexture = Plugin.maskThermal;
                __instance.ThermalVisionUtilities.MaskDescription.ThermalMaskTexture.wrapMode = TextureWrapMode.Clamp;
                __instance.IsPixelated=true;
                __instance.IsNoisy=false;
                __instance.IsMotionBlurred=true;
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
