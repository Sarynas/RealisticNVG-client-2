using BorkelRNVG.Patches;
using BSG.CameraEffects;
using Comfort.Common;
using EFT.UI;
using EFT.Vaulting;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BorkelRNVG
{
    public class AutoGatingController : MonoBehaviour
    {
        public static AutoGatingController Instance;

        // randomass variables
        public Camera mainCamera;
        //private Camera _rtCamera;
        public NightVision nightVision;

        // shader stuff
        public ComputeShader computeShader;
        public ComputeBuffer brightnessBuffer;
        public RenderTexture renderTexture;

        public CommandBuffer commandBuffer;
        public int nvInt_6;
        public RenderTargetIdentifier nvTargetIdentifier;
        public Material nvMaterial_0;

        private int _currentFrame = 0;
        private float _currentBrightness = 1.0f;
        private CameraEvent _cameraEvent = CameraEvent.BeforeImageEffects;

        private const float BRIGHTNESS_SCALE = 10000f;
        private int kernel;
        private int textureWidth;
        private int textureHeight;
        private int _renderInterval = 60;
        private float _gateSpeed = 0.02f;
        private float _lerp1 = 1f;
        private float _lerp2 = 0.2f;
        private float minInput = 0.08f;
        private float maxInput = 0.1f;

        // more randomass vars
        private int _brightnessExponent = 1;

        // screen was upside down.. very funny but not playable
        private Vector2 _uvScale = new Vector2(1, -1);
        private Vector2 _uvOffset = new Vector2(0, 1);

        // public stuff
        public float GatingMultiplier { get; private set; } = 1.0f;

        public static AutoGatingController Create()
        {
            GameObject camera = CameraClass.Instance.Camera.gameObject;
            AutoGatingController autoGatingController = camera.AddComponent<AutoGatingController>();
            return autoGatingController;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            textureWidth = Screen.width;
            textureHeight = Screen.height;

            mainCamera = CameraClass.Instance.Camera;
            nightVision = CameraClass.Instance.NightVision;

            // everything beyond this point makes my head hurt

            renderTexture = CreateRenderTexture();

            commandBuffer = new CommandBuffer();
            commandBuffer.SetRenderTarget(renderTexture);
            commandBuffer.Blit(null, renderTexture);
            mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);

            /*
            GameObject rtCameraGameObject = new GameObject("RTCamera");
            _rtCamera = rtCameraGameObject.AddComponent<Camera>();
            _rtCamera.CopyFrom(_mainCamera);
            _rtCamera.targetTexture = renderTexture;
            _rtCamera.nearClipPlane = 2f;
            _rtCamera.enabled = false;
            DontDestroyOnLoad(rtCameraGameObject);*/

            brightnessBuffer = new ComputeBuffer(1, sizeof(uint));

            //computeShader = AssetBundle.LoadFromFile($"{Plugin.assetsDirectory}\\Shaders\\pein_shaders").LoadAsset("BrightnessShader") as ComputeShader;
            computeShader = AssetBundle.LoadFromFile($"{Plugin.assetsDirectory}\\Shaders\\pein_shaders").LoadAsset("AverageBrightnessShader") as ComputeShader;
            kernel = computeShader.FindKernel("CSReduceBrightness");
            computeShader.SetInt("_Width", textureWidth);
            computeShader.SetInt("_Height", textureHeight);
            computeShader.SetTexture(kernel, "_InputTexture", renderTexture);
            computeShader.SetBuffer(kernel, "_BrightnessBuffer", brightnessBuffer);

            // temp rt debug
            var canvas = new GameObject("Canvas", typeof(Canvas)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var rawImage = new GameObject("RawImage", typeof(RawImage)).GetComponent<RawImage>();
            rawImage.transform.SetParent(canvas.transform);

            RectTransform rectTransform = rawImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);

            rectTransform.anchoredPosition = new Vector2(-100, -100);

            rawImage.texture = renderTexture;
        }

        private RenderTexture CreateRenderTexture()
        {
            RenderTexture rt = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32);
            rt.enableRandomWrite = true;
            rt.useMipMap = false;
            rt.autoGenerateMips = false;
            rt.filterMode = FilterMode.Point;
            rt.Create();

            return rt;
        }

        private void Update()
        {
            /*
            // Ensure the renderTexture is set up properly
            if (renderTexture == null || !renderTexture.IsCreated())
            {
                renderTexture = CreateRT();
            }

            //_rtCamera.transform.position = _mainCamera.transform.position;
            //_rtCamera.transform.rotation = _mainCamera.transform.rotation;
            //_rtCamera.Render();

            uint[] brightnessSum = new uint[1] { 0 };
            brightnessBuffer.SetData(brightnessSum);

            computeShader.Dispatch(kernel, 8, 8, 1);

            brightnessBuffer.GetData(brightnessSum);

            float totalPixels = textureWidth * textureHeight;

            _currentBrightness = brightnessSum[0] / totalPixels;

            float normalizedBrightness = Mathf.Clamp((_currentBrightness - minInput) / (maxInput - minInput), 0.0f, 1.0f);
            float gatingTarget = Mathf.Lerp(_lerp1, _lerp2, normalizedBrightness);

            GatingMultiplier = Mathf.Lerp(GatingMultiplier, gatingTarget, _gateSpeed);

            _nightVision.ApplySettings();*/
        }

        private void OnPreCull()
        {
            //Graphics.ExecuteCommandBuffer(commandBuffer);
            mainCamera.GetComponent<UltimateBloom>()?.method_23(null, renderTexture, 1f);
        }

        /*
        private void OnPreCull()
        {
            commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, renderTexture);

            Singleton<LevelSettings>.Instance.AmbientType = AmbientType.NightVision;
            nvCommandBuffer.Clear();
            nvCommandBuffer.GetTemporaryRT(nvInt_6, -1, -1);
            nvCommandBuffer.Blit(nvTargetIdentifier, nvInt_6, _nightVision.Material_0);
            nvCommandBuffer.Blit(nvInt_6, nvTargetIdentifier);
            nvCommandBuffer.ReleaseTemporaryRT(nvInt_6);
        }

        private void OnPostRender()
        {
            Singleton<LevelSettings>.Instance.AmbientType = AmbientType.Default;
        }*/

        private void OnDestroy()
        {
            // get rid of this shit...
            renderTexture?.Release();
            brightnessBuffer?.Release();
        }
    }
}