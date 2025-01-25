using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System.Reflection;
using System;
using BorkelRNVG.Helpers.Enum;

namespace BorkelRNVG.Helpers
{
    public class AssetHelper
    {
        public static readonly string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string assetsDirectory = $"{directory}\\Assets";

        public static Shader pixelationShader; // Assets/Systems/Effects/Pixelation/Pixelation.shader
        public static Shader nightVisionShader; // Assets/Shaders/CustomNightVision.shader

        public static Texture2D noiseTexture;

        public static Dictionary<string, AudioClip> LoadedAudioClips = new Dictionary<string, AudioClip>();

        // could probably use some changes still ill keep it like that because whatever lol :^)
        public static Dictionary<ENVGTexture, string> MaskTexturePaths = new Dictionary<ENVGTexture, string>()
        {
            { ENVGTexture.Anvis, "MaskTextures\\mask_anvis.png" },
            { ENVGTexture.Binocular, "MaskTextures\\mask_binocular.png" },
            { ENVGTexture.Monocular, "MaskTextures\\mask_old_monocular.png" },
            { ENVGTexture.Pnv, "MaskTextures\\mask_pnv.png" },
            { ENVGTexture.Thermal, "MaskTextures\\mask_thermal.png" },
            { ENVGTexture.Pixel, "MaskTextures\\pixel_mask1.png" },
            { ENVGTexture.Noise, "MaskTextures\\Noise.png" }
        };

        public static Dictionary<ENVGTexture, string> LensTexturePaths = new Dictionary<ENVGTexture, string>
        {
            { ENVGTexture.Anvis, "LensTextures\\lens_anvis.png" },
            { ENVGTexture.Binocular, "LensTextures\\lens_binocular.png" },
            { ENVGTexture.Monocular, "LensTextures\\lens_old_monocular.png" },
            { ENVGTexture.Pnv, "LensTextures\\lens_pnv.png" }
        };

        public static Dictionary<ENVGTexture, Texture2D> MaskTextures = new Dictionary<ENVGTexture, Texture2D>();
        public static Dictionary<ENVGTexture, Texture2D> LensTextures = new Dictionary<ENVGTexture, Texture2D>();
        public static Dictionary<Texture, Texture> MaskToLens = new Dictionary<Texture, Texture>();

        public static void LoadTextures()
        {
            foreach (KeyValuePair<ENVGTexture, string> lens in LensTexturePaths)
            {
                string pngPath = $"{assetsDirectory}\\{lens.Value}";
                Texture2D texture = LoadPNG(pngPath);
                if (texture == null) continue;

                LensTextures[lens.Key] = texture;
            }

            foreach (KeyValuePair<ENVGTexture, string> mask in MaskTexturePaths)
            {
                string pngPath = $"{assetsDirectory}\\{mask.Value}";
                Texture2D texture = LoadPNG(pngPath);
                if (texture == null) continue;

                MaskTextures[mask.Key] = texture;

                if (mask.Key == ENVGTexture.Noise) // umm.... weird.
                {
                    noiseTexture = texture;
                    noiseTexture.wrapMode = TextureWrapMode.Repeat;
                }

                if (LensTextures.TryGetValue(mask.Key, out var lensTexture))
                {
                    MaskToLens[texture] = lensTexture;
                }
            }
        }

        public static void LoadShaders()
        {
            string eftShaderPath = Path.Combine(Environment.CurrentDirectory, "EscapeFromTarkov_Data", "StreamingAssets", "Windows", "shaders");
            string nightVisionShaderPath = $"{assetsDirectory}\\Shaders\\borkel_realisticnvg_shaders";

            pixelationShader = LoadShader("Assets/Systems/Effects/Pixelation/Pixelation.shader", eftShaderPath); // T-7 pixelation
            nightVisionShader = LoadShader("Assets/Shaders/CustomNightVision.shader", nightVisionShaderPath);
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
                tex.wrapMode = TextureWrapMode.Clamp; //otherwise the mask will repeat itself around screen borders
            }

            return tex;
        }

        public static Shader LoadShader(string shaderName, string bundlePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            Shader shader = assetBundle.LoadAsset<Shader>(shaderName);
            assetBundle.Unload(false);
            return shader;
        }

        public static ComputeShader LoadComputeShader(string shaderName, string bundlePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            ComputeShader computeShader = assetBundle.LoadAsset<ComputeShader>(shaderName);
            assetBundle.Unload(false);
            return computeShader;
        }

        public static void LoadAudioClips()
        {
            string[] audioFilesDir = Directory.GetFiles($"{assetsDirectory}\\Sounds");
            LoadedAudioClips.Clear();

            foreach (string fileDir in audioFilesDir)
            {
                LoadAudioClip(fileDir);
            }
        }

        public static async void LoadAudioClip(string path)
        {
            LoadedAudioClips[Path.GetFileName(path)] = await RequestAudioClip(path);
        }

        public static async Task<AudioClip> RequestAudioClip(string path)
        {
            string extension = Path.GetExtension(path);
            AudioType audioType = AudioType.WAV;

            switch (extension)
            {
                case ".wav":
                    audioType = AudioType.WAV;
                    break;
                case ".ogg":
                    audioType = AudioType.OGGVORBIS;
                    break;
            }

            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            UnityWebRequestAsyncOperation sendWeb = uwr.SendWebRequest();

            while (!sendWeb.isDone)
                await Task.Yield();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Plugin.Log.LogWarning("BRNVG Mod: Failed To Fetch Audio Clip");
                return null;
            }
            else
            {
                AudioClip audioclip = DownloadHandlerAudioClip.GetContent(uwr);
                return audioclip;
            }
        }
    }
}
