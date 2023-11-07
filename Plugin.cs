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
        private void Awake()
        {
            //directory contains string of path where the .dll is located, for me it is C:\SPTarkov3.7.1\BepInEx\plugins
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //loading from PNGs, like Fontaine suggested
            string bundlePath = $"{directory}\\BorkelRNVG\\realmasks";
            string anvisPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_anvis.png";
            string binoPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_binocular.png";
            string monoPath = $"{directory}\\BorkelRNVG\\PNGtextures\\mask_old_monocular.png";
            maskAnvis = LoadPNG(anvisPath);
            maskBino = LoadPNG(binoPath);
            maskMono = LoadPNG(monoPath);
            maskAnvis.wrapMode = TextureWrapMode.Clamp;
            maskBino.wrapMode = TextureWrapMode.Clamp; //otherwise the mask will repeat itself around screen borders
            maskMono.wrapMode = TextureWrapMode.Clamp;
            Logger.LogMessage($"Texture2D 0: {maskAnvis}"); //mask 0: mask_anvis
            Logger.LogMessage($"Texture2D 1: {maskBino}"); //mask 1: mask_binocular
            Logger.LogMessage($"Texture2D 2: {maskMono}"); //mask 2: mask_old_monocular
            if(maskAnvis == null || maskBino == null || maskMono==null)
            {
                Logger.LogError($"Error loading PNGs.");
                return;
            }
            new SetMaskPatch().Enable();
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
            Logger.LogMessage($"After Mask name: {__instance.Mask.name}");
            Logger.LogMessage($"After Anvismask name: {__instance.AnvisMaskTexture.name}");
            Logger.LogMessage($"After Binosmask name: {__instance.BinocularMaskTexture.name}");
            Logger.LogMessage($"After Monomask name: {__instance.OldMonocularMaskTexture.name}");
            //return false; //prevents original method from running, so we can fully override it //not needed

        }
    }

}
