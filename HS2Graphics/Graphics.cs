using BepInEx;
using Graphics.Settings;
using KKAPI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Graphics
{
    public partial class Graphics : BaseUnityPlugin
    {
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_isLoaded)
            {
                StartCoroutine(InitializeLight(scene));

                // Reset to known preset for everything except Studio -- this loads after scene data loads, don't want to wipe out saved scenes
                if (KKAPI.KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
                {
                    StartCoroutine("ApplyPresets", scene);
                }
            }
        }

        private IEnumerator ApplyPresets(Scene scene)
        {
            Log.LogInfo($"Checking Apply Presets {scene.isLoaded} {CameraSettings.MainCamera} {scene.name} {KKAPI.Maker.MakerAPI.InsideAndLoaded}");
            yield return new WaitUntil(() => scene.isLoaded && CameraSettings.MainCamera != null &&
                ( (scene.name.Equals("CharaCustom", System.StringComparison.OrdinalIgnoreCase) && KKAPI.Maker.MakerAPI.InsideAndLoaded) || (!scene.name.Equals("CharaCustom", System.StringComparison.OrdinalIgnoreCase)))
            );

            GameMode gameMode = KoikatuAPI.GetCurrentGameMode();
            Log.LogInfo(string.Format("HS Scene Loaded: {0} Game: {1} CAM FOV: {2}", scene.name, gameMode, CameraSettings.MainCamera.fieldOfView));
            
            _ctaaManager?.CheckInstance();
            _sssManager?.CheckInstance();
            _gtaoManager?.CheckInstance();
            _vaoManager?.CheckInstance();
            _amplifyoccManager?.CheckInstance();
            //_shinyssrrManager?.CheckInstance();
            _ditheredshadowsManager?.CheckInstance();
            _focusManager?.CheckInstance();

            if (CameraSettings.MainCamera.stereoEnabled) // VR...use VR
            {
                _presetManager?.LoadDefault(PresetDefaultType.VR_GAME);
            }
            else if (scene.name.Equals("NightPool", System.StringComparison.OrdinalIgnoreCase))
            {
                // Special carve out for lobby -- initial load doesn't have mode MAIN_GAME recognized and the FOV needs special handling
                _presetManager?.LoadDefault(PresetDefaultType.TITLE);
                if (CameraSettings != null)
                {                    
                    CameraSettings.Fov = 40;  // Not sure why this sometimes doesn't work...
                    CameraSettings.MainCamera.fieldOfView = 40; // But this does...
                    Log.LogDebug(string.Format("Overrode CAM FOV: {0}", CameraSettings.MainCamera.fieldOfView));
                }
            }
            else if (gameMode == GameMode.Maker || gameMode == GameMode.Studio)
            {
                _presetManager?.LoadDefaultForCurrentGameMode();
            }
            else
            {
                // For other main games scenes, we need to preserve the original FOV and not replace from preset
                float _fov = CameraSettings.MainCamera.fieldOfView;
                if (CameraSettings.MainCamera.stereoEnabled)
                {
                    _presetManager?.LoadDefault(PresetDefaultType.VR_GAME);
                }
                else
                {
                    _presetManager?.LoadDefaultForCurrentGameMode();
                }
                CameraSettings.Fov = _fov;  // Not sure why this sometimes doesn't work...
                CameraSettings.MainCamera.fieldOfView = _fov; // But this does...
                Log.LogDebug(string.Format("After Load CAM FOV: {0}", CameraSettings.MainCamera.fieldOfView));
            }    
            
            if (gameMode != GameMode.Studio)
            {
                // Check for saved Lights
                if (!_presetManager.LoadMapLights())
                {
                    if (!_presetManager.MapLightOriginalExists())
                    {
                        _presetManager.SaveMapLights(true);
                    }
                }
            }

        }

        private IEnumerator InitializeLight(Scene scene)
        {
            yield return new WaitUntil(() => _lightManager != null);

            if (GameMode.Maker == KoikatuAPI.GetCurrentGameMode() && scene.name == "CharaCustom")
            {
                GameObject lights = GameObject.Find("CharaCustom/CustomControl/CharaCamera/Main Camera/Lights Custom");
                if (lights is null)
                {
                    Transform backLight = lights.transform.Find("Directional Light Back");
                    if (backLight is null)
                    {
                        Light light = backLight.GetComponent<Light>();
                        if (light is null)
                        {
                            light.enabled = false;
                        }
                    }
                }
            }
            LightManager.Light();
        }

        private bool IsLoaded()
        {
            switch (KoikatuAPI.GetCurrentGameMode())
            {                
                case GameMode.Maker:
                    return KKAPI.Maker.MakerAPI.InsideAndLoaded;
                case GameMode.Studio:
                    return KKAPI.Studio.StudioAPI.StudioLoaded;
                case GameMode.MainGame:
                    return (SceneManager.GetActiveScene().name.Equals("Lobby", System.StringComparison.OrdinalIgnoreCase) || SceneManager.GetActiveScene().name.Equals("NightPool", System.StringComparison.OrdinalIgnoreCase)) && SceneManager.GetActiveScene().isLoaded && null != Camera.main; //HS2API doesn't provide an api for in game check 
                case GameMode.Unknown:
                    return (SceneManager.GetActiveScene().name == "VRTitle" || SceneManager.GetActiveScene().name == "Title" ) && SceneManager.GetActiveScene().isLoaded && null != Camera.main;  // Looking for the Official VR Start Menu or the Lobby
                default:
                    return false;
            }
        }
    }
}
