using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using EFT.CameraControl;
using GPUInstancer;
using System;
using UnityEngine;

namespace BorkelRNVG.Helpers
{
    public class Util
    {
        private static GameWorld _gameWorld;
        private static CameraClass _fpsCamera;
        private static NightVision _nightVision;
        private static Player _mainPlayer;

        public static void InitializeVars()
        {
            PlayerCameraController.OnPlayerCameraControllerCreated += OnCameraCreated;
            PlayerCameraController.OnPlayerCameraControllerDestroyed += OnCameraDestroyed;
        }

        private static void OnCameraCreated(PlayerCameraController controller, Camera cam)
        {
            if (!CameraClass.Exist)
            {
                return;
            }

            _fpsCamera = CameraClass.Instance;
            if (_fpsCamera.NightVision != null)
            {
                _nightVision = _fpsCamera.NightVision;
            }

            AutoGatingController.Create();
        }

        private static void OnCameraDestroyed()
        {
            _fpsCamera = null;
            _nightVision = null;
            GameObject.Destroy(AutoGatingController.Instance.gameObject);
        }

        private static bool CheckFpsCameraExist()
        {
            if (_fpsCamera != null)
            {
                return true;
            }
            return false;
        }

        public static void ApplyNightVisionSettings(object sender, EventArgs eventArgs)
        {
            if (_nightVision == null)
            {
                if (!CheckFpsCameraExist())
                {
                    return;
                }
                _nightVision = _fpsCamera.NightVision;
            }

            _nightVision.ApplySettings();
        }

        public static void ApplyGatingSettings(object sender, EventArgs eventArgs)
        {
            AutoGatingController.Instance?.ApplySettings();
        }
    }
}
