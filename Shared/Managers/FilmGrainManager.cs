using ADV.Commands.Object;
using Graphics.Settings;
using Graphics.Textures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Graphics
{
    public class FilmGrainManager
    {
        public static FilmGrainSettings settings;

        internal static FilmGrain FilmGrainInstance;

        // Initialize Components
        internal void Initialize()
        {
            FilmGrainInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<FilmGrain>();
            if (settings == null)
            {
                settings = new FilmGrainSettings();
            }

            settings.Load(FilmGrainInstance);
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new FilmGrainSettings();
            if (FilmGrainInstance != null)
                settings.Load(FilmGrainInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (FilmGrainInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                FilmGrainInstance = camera.GetOrAddComponent<FilmGrain>();
            }
        }

    }
}
