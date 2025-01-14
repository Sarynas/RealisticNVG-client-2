using BSG.CameraEffects;
using System.Management.Instrumentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace BorkelRNVG
{
    public class AutoGatingController : MonoBehaviour
    {
        public static AutoGatingController Instance;

        // randomass variables
        private Camera _mainCamera;
        private Camera _rtCamera;
        private NightVision _nightVision;

        // shader stuff
        public ComputeShader computeShader;
        public ComputeBuffer brightnessBuffer;
        public RenderTexture renderTexture;

        private int kernel;
        private int textureWidth;
        private int textureHeight;

        private const float BRIGHTNESS_SCALE = 10000f;

        // more randomass vars
        private int _currentFrame = 0;
        private float _currentBrightness = 1.0f;

        private int _renderInterval = 60;
        private float _gateSpeed = 0.02f;
        private int _brightnessExponent = 1;

        private float _lerp1 = 0.2f;
        private float _lerp2 = 1f;

        // screen was upside down.. very funny but not playable
        private Vector2 _uvScale = new Vector2(1, -1);
        private Vector2 _uvOffset = new Vector2(0, 1);

        private float minInput = 0.08f;
        private float maxInput = 0.1f;

        // public stuff
        public float GatingMultiplier { get; private set; } = 1.0f;

        public static AutoGatingController Create()
        {
            GameObject camera = Camera.main.gameObject;
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

            _mainCamera = CameraClass.Instance.Camera;
            _nightVision = CameraClass.Instance.NightVision;

            // everything beyond this point makes my head hurt

            computeShader = AssetBundle.LoadFromFile($"{Plugin.assetsDirectory}\\Shaders\\pein_shaders").LoadAsset("BrightnessShader") as ComputeShader;

            textureWidth = 32;
            textureHeight = 32;

            renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.DefaultHDR);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            GameObject rtCameraGameObject = new GameObject("RTCamera");
            _rtCamera = rtCameraGameObject.AddComponent<Camera>();
            _rtCamera.CopyFrom(_mainCamera);
            _rtCamera.targetTexture = renderTexture;
            _rtCamera.nearClipPlane = 2f;
            DontDestroyOnLoad(rtCameraGameObject);

            brightnessBuffer = new ComputeBuffer(1, sizeof(uint));

            kernel = computeShader.FindKernel("CSReduceBrightness");
        }

        void Update()
        {
            // Ensure the renderTexture is set up properly
            if (renderTexture == null || !renderTexture.IsCreated())
            {
                renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.DefaultHDR);
                renderTexture.enableRandomWrite = true;
                renderTexture.Create();
            }

            _rtCamera.transform.position = _mainCamera.transform.position;
            _rtCamera.transform.rotation = _mainCamera.transform.rotation;

            // Reset the buffer value
            uint[] brightnessSum = new uint[1] { 0 };
            brightnessBuffer.SetData(brightnessSum);

            // Set shader parameters
            computeShader.SetTexture(kernel, "_InputTexture", renderTexture);
            computeShader.SetBuffer(kernel, "_BrightnessBuffer", brightnessBuffer);
            computeShader.SetInt("_Width", textureWidth);
            computeShader.SetInt("_Height", textureHeight);

            // Dispatch the compute shader
            int threadGroupsX = Mathf.CeilToInt(textureWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(textureHeight / 8.0f);
            computeShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

            brightnessBuffer.GetData(brightnessSum);

            float totalPixels = textureWidth * textureHeight;

            _currentBrightness = (brightnessSum[0] / (BRIGHTNESS_SCALE * totalPixels));

            float normalizedBrightness = Mathf.Clamp((_currentBrightness - minInput) / (maxInput - minInput), 0.0f, 1.0f);
            float gatingTarget = Mathf.Lerp(_lerp1, _lerp2, normalizedBrightness);

            GatingMultiplier = Mathf.Lerp(GatingMultiplier, gatingTarget, _gateSpeed);

            _nightVision.ApplySettings();
        }

        private void OnDestroy()
        {
            // get rid of this shit...
            renderTexture?.Release();
            brightnessBuffer?.Release();
        }
    }
}