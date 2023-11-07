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
        public static Texture2D[] masks; //my modded masks will be loaded here
        public AssetBundle bundle; //grabs the bundle with my masks
        private void Awake()
        {
            //directory contains string of path where the .dll is located, for me it is C:\SPTarkov3.7.1\BepInEx\plugins
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //realmasks is the bundle, Fontaine suggests to load the masks from PNGs isntead of a bundle, should change it
            string bundlePath = $"{directory}\\BorkelRNVG\\realmasks";
            Logger.LogMessage($"directory {directory} bundlepath {bundlePath}");
            //Load all assets from the bundle realmasks located in C:\SPTarkov3.7.1\BepInEx\plugins\BorkelRNVG
            if (File.Exists(bundlePath))
            {
                bundle = AssetBundle.LoadFromFile(bundlePath); //loads the modified bundle resources.assets
                if (bundle != null)
                {
                    Logger.LogMessage($"---BUNDLE LOADED---");
                    masks = bundle.LoadAllAssets<Texture2D>(); //loads all assets, including the important ones: mask_anvis, mask_binocular, mask_old_monocular
                    masks[0].wrapMode = TextureWrapMode.Clamp; 
                    masks[1].wrapMode = TextureWrapMode.Clamp; //otherwise the mask will repeat itself around screen borders
                    masks[2].wrapMode = TextureWrapMode.Clamp;
                    Logger.LogMessage($"Texture2D 0: {masks[0].name}"); //mask 0: mask_anvis
                    Logger.LogMessage($"Texture2D 1: {masks[1].name}"); //mask 1: mask_binocular
                    Logger.LogMessage($"Texture2D 2: {masks[2].name}"); //mask 2: mask_old_monocular
                    new SetMaskPatch().Enable();
                }
                else
                {
                    Logger.LogError("Failed to load the asset bundle.");
                }
            }
            else
            {
                Logger.LogError("Asset bundle not found.");
            }
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
            __instance.AnvisMaskTexture = Plugin.masks[0];
            __instance.BinocularMaskTexture = Plugin.masks[1];
            __instance.OldMonocularMaskTexture = Plugin.masks[2];
            Logger.LogMessage($"After Mask name: {__instance.Mask.name}");
            Logger.LogMessage($"After Anvismask name: {__instance.AnvisMaskTexture.name}");
            Logger.LogMessage($"After Binosmask name: {__instance.BinocularMaskTexture.name}");
            Logger.LogMessage($"After Monomask name: {__instance.OldMonocularMaskTexture.name}");
            //return false; //prevents original method from running, so we can fully override it //not needed

        }
    }

}
