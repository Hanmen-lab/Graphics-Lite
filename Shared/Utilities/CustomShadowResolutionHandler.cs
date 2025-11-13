using UnityEngine;
using System;
using static Graphics.DebugUtils;

namespace Graphics
{
    [RequireComponent(typeof(Light))]
    public class CustomShadowResolutionHandler : MonoBehaviour
    {
        public enum CustomShadowResolution
        {
            Default = -1,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192,
            _16K = 16384
        }

        public CustomShadowResolution shadowResolutionCustomSelector = CustomShadowResolution.Default;

        private int _targetResolution;
        public int targetResolution
        {
            get => _targetResolution;
            set
            {
                _targetResolution = value;
                // Также обновляем enum selector для консистентности
                if (Enum.IsDefined(typeof(CustomShadowResolution), value))
                {
                    shadowResolutionCustomSelector = (CustomShadowResolution)value;
                }
            }
        }

        private Light lightComponent;

        private void Awake()
        {
            lightComponent = GetComponent<Light>();
            _targetResolution = (int)shadowResolutionCustomSelector;
        }

        private void LateUpdate()
        {
            // Проверяем и восстанавливаем каждый кадр
            if (lightComponent.shadowCustomResolution != _targetResolution)
            {
                lightComponent.shadowCustomResolution = _targetResolution;
            }
        }

        public void ApplyShadowResolution(string name)
        {
            if (lightComponent != null)
            {
                _targetResolution = (int)shadowResolutionCustomSelector;
                lightComponent.shadowCustomResolution = _targetResolution;

                if (_targetResolution == -1)
                    //Graphics.Instance.Log.LogInfo($"{name} ....... Reset shadow resolution to default");
                    LogWithDotsLight($"{name}"+" Shadow Resolution", "Default");
                else
                    //Graphics.Instance.Log.LogInfo($"{name} ....... Applied custom shadow resolution {_targetResolution}");
                    LogWithDotsLight($"{name}" + " Shadow Resolution", $"{_targetResolution}");
            }
        }

        // Метод для установки разрешения из внешнего источника
        public void SetResolution(int resolution)
        {
            targetResolution = resolution;
            ApplyShadowResolution(lightComponent?.name ?? gameObject.name);
        }
    }
}