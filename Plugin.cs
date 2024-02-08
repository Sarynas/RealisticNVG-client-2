using BepInEx;
using BepInEx.Configuration;
using BorkelRNVG.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BorkelRNVG
{
    [BepInPlugin("com.borkel.nvgmasks", "Borkel's Realistic NVGs", "1.4.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static readonly string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //mask textures
        public static Texture2D maskAnvis;
        public static Texture2D maskBino;
        public static Texture2D maskMono;
        public static Texture2D maskPnv;
        public static Texture2D maskThermal;
        public static Texture2D maskPixel; //i don't really know if this one does anything
        public static Shader pixelationShader; //Assets/Systems/Effects/Pixelation/Pixelation.shader
        public static Shader nightVisionShader; // Assets/Shaders/CustomNightVision.shader
        //lens textures
        public static Texture2D lensAnvis;
        public static Texture2D lensBino;
        public static Texture2D lensMono;
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
        //sprint patch stuff
        public static bool isSprinting;
        public static bool wasSprinting = false;
        public static Dictionary<string, bool> LightDictionary = new Dictionary<string, bool>();
        //UltimateBloom stuff
        public static UltimateBloom UltimateBloomInstance;
        
        private static readonly Dictionary<Texture, Texture> maskToLens = new Dictionary<Texture, Texture>();


        private void Awake()
        {
            //############-BEPINEX F12-MENU##############
            //Global multipliers
            globalMaskSize = Config.Bind("0.Globals", "Mask size multiplier", 1f, new ConfigDescription("Applies size multiplier to all masks", new AcceptableValueRange<float>(0f, 2f)));
            globalGain = Config.Bind("0.Globals", "Gain multiplier", 1f, new ConfigDescription("Applies gain multiplier to all NVGs", new AcceptableValueRange<float>(0f, 5f)));
            //GPNVG-18 config. Mask size should be 0.96 times of the rest
            quadGain = Config.Bind("1.GPNVG-18", "1.Gain", 2.5f, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            quadNoiseIntensity = Config.Bind("1.GPNVG-18", "2.Noise intensity", 0.2f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            quadNoiseSize = Config.Bind("1.GPNVG-18", "3.Noise scale", 0.1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 0.99f)));
            quadMaskSize = Config.Bind("1.GPNVG-18", "4.Mask size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 2f)));
            quadR = Config.Bind("1.GPNVG-18", "5.Red", 152f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            quadG = Config.Bind("1.GPNVG-18", "6.Green", 214f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            quadB = Config.Bind("1.GPNVG-18", "7.Blue", 252f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            //PVS-14 config
            pvsGain = Config.Bind("2.PVS-14", "1.Gain", 2.4f, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            pvsNoiseIntensity = Config.Bind("2.PVS-14", "2.Noise intensity", 0.2f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            pvsNoiseSize = Config.Bind("2.PVS-14", "3.Noise scale", 0.1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 0.99f)));
            pvsMaskSize = Config.Bind("2.PVS-14", "4.Mask size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 2f)));
            pvsR = Config.Bind("2.PVS-14", "5.Red", 95f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            pvsG = Config.Bind("2.PVS-14", "6.Green", 210f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            pvsB = Config.Bind("2.PVS-14", "7.Blue", 255f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            //N-15 config
            nGain = Config.Bind("3.N-15", "1.Gain", 2.1f, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            nNoiseIntensity = Config.Bind("3.N-15", "2.Noise intensity", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            nNoiseSize = Config.Bind("3.N-15", "3.Noise scale", 0.15f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 0.99f)));
            nMaskSize = Config.Bind("3.N-15", "4.Mask size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 2f)));
            nR = Config.Bind("3.N-15", "5.Red", 60f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            nG = Config.Bind("3.N-15", "6.Green", 235f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            nB = Config.Bind("3.N-15", "7.Blue", 100f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            //PNV-10T config
            pnvGain = Config.Bind("4.PNV-10T", "1.Gain", 1.8f, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            pnvNoiseIntensity = Config.Bind("4.PNV-10T", "2.Noise intensity", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            pnvNoiseSize = Config.Bind("4.PNV-10T", "3.Noise scale", 0.2f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 0.99f)));
            pnvMaskSize = Config.Bind("4.PNV-10T", "4.Mask size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 2f)));
            pnvR = Config.Bind("4.PNV-10T", "5.Red", 60f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            pnvG = Config.Bind("4.PNV-10T", "6.Green", 210f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            pnvB = Config.Bind("4.PNV-10T", "7.Blue", 60f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 255f)));
            //###########################################

            string pluginDirectory = $"{directory}\\BorkelRNVG";//plugin folder
            string eftShaderPath = Path.Combine(Environment.CurrentDirectory, "EscapeFromTarkov_Data", "StreamingAssets", "Windows", "shaders");
            //loading from PNGs, like Fontaine suggested
            string anvisPath = $"{pluginDirectory}\\MaskTextures\\mask_anvis.png";
            string binoPath = $"{pluginDirectory}\\MaskTextures\\mask_binocular.png";
            string monoPath = $"{pluginDirectory}\\MaskTextures\\mask_old_monocular.png";
            string pnvPath = $"{pluginDirectory}\\MaskTextures\\mask_pnv.png";
            string thermalPath = $"{pluginDirectory}\\MaskTextures\\mask_thermal.png";
            string pixelPath = $"{pluginDirectory}\\MaskTextures\\pixel_mask1.png";
            string lensAnvisPath = $"{pluginDirectory}\\LensTextures\\lens_anvis.png";
            string lensBinoPath = $"{pluginDirectory}\\LensTextures\\lens_binocular.png";
            string lensMonoPath = $"{pluginDirectory}\\LensTextures\\lens_old_monocular.png";
            maskAnvis = LoadPNG(anvisPath);
            maskBino = LoadPNG(binoPath);
            maskMono = LoadPNG(monoPath);
            maskPnv = LoadPNG(pnvPath);
            maskThermal = LoadPNG(thermalPath);
            maskPixel = LoadPNG(pixelPath);//might not do anything really
            lensAnvis= LoadPNG(lensAnvisPath);
            lensBino= LoadPNG(lensBinoPath);
            lensMono= LoadPNG(lensMonoPath);
            if (maskAnvis == null || maskBino == null || maskMono == null || maskPnv == null || maskThermal == null || maskPixel == null
                || lensAnvis == null || lensBino == null || lensMono == null)
            {
                Logger.LogError($"Error loading PNGs. Patches will be disabled.");
                return;
            }

            maskToLens.Add(maskAnvis, lensAnvis);
            maskToLens.Add(maskBino, lensBino);
            maskToLens.Add(maskMono, lensMono);
            maskToLens.Add(maskPnv, lensMono);

            string nightVisionShaderPath = $"{pluginDirectory}\\borkel_realisticnvg_shaders";
            pixelationShader = LoadShader("Assets/Systems/Effects/Pixelation/Pixelation.shader", eftShaderPath); //to pixelate the T-7
            nightVisionShader = LoadShader("Assets/Shaders/CustomNightVision.shader", nightVisionShaderPath);
            if (pixelationShader == null || nightVisionShader == null)
            {
                Logger.LogError($"Error loading shaders. Patches will be disabled.");
                return;
            }

            new NightVisionAwakePatch().Enable();
            new NightVisionApplySettingsPatch().Enable();
            new NightVisionSetMaskPatch().Enable();
            new ThermalVisionSetMaskPatch().Enable();
            new SprintPatch().Enable();
            new NightVisionMethod_1().Enable(); //not working

            var controller = new GameObject("BorkelRNVG").AddComponent<BorkelRNVGController>();
            DontDestroyOnLoad(controller.gameObject);
        }

        public static Texture GetMatchingLensMask(Texture mask)
        {
            if (maskToLens.TryGetValue(mask, out var lens))
            {
                return lens;
            }
            return null;
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
                tex.wrapMode = TextureWrapMode.Clamp; //otherwise the mask will repeat itself around screen borders
            }
            return tex;
        }

        private static Shader LoadShader(string shaderName, string bundlePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            Shader sh = assetBundle.LoadAsset<Shader>(shaderName);
            assetBundle.Unload(false);
            return sh;
        }
    }
}
