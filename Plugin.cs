using BepInEx;
using BorkelRNVG.Patches;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BorkelRNVG
{
    [BepInPlugin("com.borkel.nvgmasks", "my very humble attempt at replacing the damn nvg masks", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        //public static Texture2D[] masks; //my modded masks will be loaded here
        //public AssetBundle bundle; //grabs the bundle with my masks
        private static readonly string Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static Texture2D MaskAnvis;
        internal static Texture2D MaskBino;
        internal static Texture2D MaskMono;
        internal static Texture2D MaskPixel;
        internal static Texture2D MaskThermal;
        internal static Shader PixelationShader;

        private void Awake()
        {
            //directory contains string of path where the .dll is located, for me it is C:\SPTarkov3.7.1\BepInEx\plugins
            //loading from PNGs, like Fontaine suggested
            string anvisPath = $"{Directory}\\BorkelRNVG\\PNGtextures\\mask_anvis.png";
            string binoPath = $"{Directory}\\BorkelRNVG\\PNGtextures\\mask_binocular.png";
            string monoPath = $"{Directory}\\BorkelRNVG\\PNGtextures\\mask_old_monocular.png";
            string thermalPath = $"{Directory}\\BorkelRNVG\\PNGtextures\\mask_thermal.png";
            string pixelPath = $"{Directory}\\BorkelRNVG\\PNGtextures\\pixel_mask1.png";

            MaskAnvis = LoadPNG(anvisPath);
            MaskBino = LoadPNG(binoPath);
            MaskMono = LoadPNG(monoPath);
            MaskPixel = LoadPNG(pixelPath);
            MaskThermal = LoadPNG(thermalPath);
            if (MaskAnvis == null || MaskBino == null || MaskMono == null || MaskThermal == null || MaskPixel == null)
            {
                Logger.LogError($"Error loading PNGs. Patches will be disabled.");
                return;
            }

            MaskAnvis.wrapMode = TextureWrapMode.Clamp;
            MaskBino.wrapMode = TextureWrapMode.Clamp; //otherwise the mask will repeat itself around screen borders
            MaskMono.wrapMode = TextureWrapMode.Clamp;
            MaskThermal.wrapMode = TextureWrapMode.Clamp;

            Logger.LogMessage($"Texture2D 0: {MaskAnvis}"); //mask 0: mask_anvis
            Logger.LogMessage($"Texture2D 1: {MaskBino}"); //mask 1: mask_binocular
            Logger.LogMessage($"Texture2D 2: {MaskMono}"); //mask 2: mask_old_monocular
            Logger.LogMessage($"Texture2D 3: {MaskPixel}"); //mask 3:

            PixelationShader = LoadShader("Assets/Systems/Effects/Pixelation/Pixelation.shader");
            if (PixelationShader == null)
            {
                Logger.LogError($"Error loading pixelation shader. Patches will be disabled.");
                return;
            }
            
            new NightVisionSetMaskPatch().Enable();
            new ThermalVisionSetMaskPatch().Enable();
        }

        private static Texture2D LoadPNG(string filePath)
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

        private static Shader LoadShader(string shaderName) //for the thermals
        {
            //string bundlePath2 = $"{directory}\\BorkelRNVG\\Shader\\shaders";
            string parent = System.IO.Directory.GetParent(Directory).FullName;
            string parent2 = System.IO.Directory.GetParent(parent).FullName;
            string bundlePath = $"{parent2}\\EscapeFromTarkov_Data\\StreamingAssets\\Windows\\shaders";
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            Shader sh = assetBundle.LoadAsset<Shader>(shaderName);
            assetBundle.Unload(false);
            return sh;
        }
    }
}
