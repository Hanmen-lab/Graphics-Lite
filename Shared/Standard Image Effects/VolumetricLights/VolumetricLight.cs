// From Graphics (Shared/Standard Image Effects/VolumetricLights), itself based on Unity's Volumetric
// Lights by Michal Skalsky (BSD). Namespace and resource loading changed for this plugin; otherwise as
// Graphics ships it.
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System;

namespace Graphics.VolumetricLights
{
    [RequireComponent(typeof(Light))]
    public class VolumetricLight : MonoBehaviour
    {
        public event Action<VolumetricLightRenderer, VolumetricLight, CommandBuffer, Matrix4x4> CustomRenderEvent;

        private Light _light;
        private Material _material;
        private CommandBuffer _commandBuffer;
        private CommandBuffer _cascadeShadowCommandBuffer;

        // Кэширование компонентов
        private Camera _camera;
        private Transform _transform;
        private Transform _lightTransform;

        [SerializeField]
        [Range(1, 24)]
        public int SampleCount = 8;
        [SerializeField]
        [Range(0.0f, 10.0f)]
        public float ScatteringCoef = 0.1f;
        [SerializeField]
        [Range(0.0f, 0.1f)]
        public float ExtinctionCoef = 0.0f;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        public float SkyboxExtinctionCoef = 0.0f;
        [SerializeField]
        [Range(0.0f, 0.999f)]
        public float MieG = 0.1f;
        public bool HeightFog = false;
        [SerializeField]
        [Range(0, 0.5f)]
        public float HeightScale = 0.10f;
        public float GroundLevel = 0;
        public bool Noise = false;
        public float NoiseScale = 0.015f;
        public float NoiseIntensity = 1.0f;
        public float NoiseIntensityOffset = 0.3f;
        public Vector2 NoiseVelocity = new Vector2(3.0f, 3.0f);

        [SerializeField]
        public float MaxRayLength = 400.0f;

        public Light Light { get { return _light; } }
        public Material VolumetricMaterial { get { return _material; } }

        // Кэши для матриц и векторов
        private Matrix4x4 _cachedWorldMatrix;
        private Matrix4x4 _cachedWorldViewMatrix;
        private Matrix4x4 _cachedWorldViewProj;
        private Matrix4x4 _cachedViewMatrix;
        private Matrix4x4 _cachedMyLightMatrix0;
        private Matrix4x4 _cachedMyWorld2Shadow;

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Vector3 _lastScale;
        private int _lastFrameUpdate = -1;

        // Кэш для Camera matrices
        private Matrix4x4 _lastCameraWorldToLocal;
        private int _lastCameraMatrixFrame = -1;

        // Предвычисленные значения
        private Vector4 _cachedMieG;
        private Vector4 _cachedNoiseVelocity;
        private Vector4 _cachedNoiseData;
        private Vector4 _cachedVolumetricLight;
        private Vector4 _cachedHeightFog;
        private float _lastMieG = -1f;
        private float _lastScattering = -1f;
        private float _lastExtinction = -1f;
        private float _lastSkyboxExtinction = -1f;

        // Кэш для frustum corners
        private readonly Vector4[] _frustumCorners = new Vector4[4];
        private int _lastFrustumFrame = -1;

        // Кэш для проверок расстояний
        private float _shadowDistanceSqr;
        private int _lastShadowDistanceFrame = -1;

        private bool _reversedZ = true;

        readonly struct ID
        {
            public static readonly int NoiseVelocity = Shader.PropertyToID("_NoiseVelocity");
            public static readonly int MieG = Shader.PropertyToID("_MieG");
            public static readonly int SampleCount = Shader.PropertyToID("_SampleCount");
            public static readonly int CameraForward = Shader.PropertyToID("_CameraForward");
            public static readonly int NoiseData = Shader.PropertyToID("_NoiseData");
            public static readonly int VolumetricLight = Shader.PropertyToID("_VolumetricLight");
            public static readonly int ZTest = Shader.PropertyToID("_ZTest");
            public static readonly int HeightFog = Shader.PropertyToID("_HeightFog");
            public static readonly int CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
            public static readonly int CascadeShadowMapTexture = Shader.PropertyToID("_CascadeShadowMapTexture");
            public static readonly int LightDir = Shader.PropertyToID("_LightDir");
            public static readonly int LightColor = Shader.PropertyToID("_LightColor");
            public static readonly int LightPos = Shader.PropertyToID("_LightPos");
            public static readonly int MaxRayLength = Shader.PropertyToID("_MaxRayLength");
            public static readonly int LightTexture0 = Shader.PropertyToID("_LightTexture0");
            public static readonly int WorldViewProj = Shader.PropertyToID("_WorldViewProj");
            public static readonly int WorldView = Shader.PropertyToID("_WorldView");
            public static readonly int MyLightMatrix0 = Shader.PropertyToID("_MyLightMatrix0");
            public static readonly int ShadowMapTexture = Shader.PropertyToID("_ShadowMapTexture");
            public static readonly int PlaneD = Shader.PropertyToID("_PlaneD");
            public static readonly int CosAngle = Shader.PropertyToID("_CosAngle");
            public static readonly int ConeApex = Shader.PropertyToID("_ConeApex");
            public static readonly int ConeAxis = Shader.PropertyToID("_ConeAxis");
            public static readonly int MyWorld2Shadow = Shader.PropertyToID("_MyWorld2Shadow");
        }

        private const string LightCommandBuffer = "Light Command Buffer";
        private const string DirLightCommandBuffer = "Dir Light Command Buffer";
        private const string HEIGHT_FOG = "HEIGHT_FOG";
        private const string SHADOWS_CUBE = "SHADOWS_CUBE";
        private const string SHADOWS_DEPTH = "SHADOWS_DEPTH";
        private const string POINT = "POINT";
        private const string POINT_COOKIE = "POINT_COOKIE";
        private const string DIRECTIONAL = "DIRECTIONAL";
        private const string DIRECTIONAL_COOKIE = "DIRECTIONAL_COOKIE";
        private const string SPOT = "SPOT";
        private const string NOISE = "NOISE";

        void Awake()
        {            
            _light = GetComponent<Light>();
            _transform = transform;
            _lightTransform = _light.transform;
            _commandBuffer = new CommandBuffer { name = LightCommandBuffer };
            _cascadeShadowCommandBuffer = new CommandBuffer { name = DirLightCommandBuffer };
            _cascadeShadowCommandBuffer.SetGlobalTexture(ID.CascadeShadowMapTexture, new RenderTargetIdentifier(UnityEngine.Rendering.BuiltinRenderTextureType.CurrentActive));
            this.enabled = false;
        }

        void Start()
        {
            //Cause problems with copy lighting settings to the volumetric light material, and it's not really needed to have it enabled at start
            //this.enabled = false;
            // Before the renderer has loaded its material (a scene's lights start first) it is made on the first render instead.
            if (_material == null && VolumetricLightRenderer.GetLightMaterial() != null)
                _material = new Material(VolumetricLightRenderer.GetLightMaterial());
        }

        private void Reset()
        {
            SampleCount = 8;
            ScatteringCoef = 0.5f;
            ExtinctionCoef = 0.0f;
            SkyboxExtinctionCoef = 0.0f;
            MieG = 0.1f;
            HeightFog = false;
            HeightScale = 0.10f;
            GroundLevel = 0;
            Noise = false;
            NoiseScale = 0.015f;
            NoiseIntensity = 1.0f;
            NoiseIntensityOffset = 0.3f;
            NoiseVelocity = new Vector2(3.0f, 3.0f);
            MaxRayLength = 400.0f;
        }

        void OnEnable()
        {
            VolumetricLightRenderer.PreRenderEvent += VolumetricLightRenderer_PreRenderEvent;

            if (_light.type == LightType.Directional)
            {
                _light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _commandBuffer);
                _light.AddCommandBuffer(LightEvent.AfterShadowMap, _cascadeShadowCommandBuffer);
            }
            else
                _light.AddCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);
        }

        void OnDisable()
        {
            VolumetricLightRenderer.PreRenderEvent -= VolumetricLightRenderer_PreRenderEvent;
            if (_light != null)
            {
                if (_light.type == LightType.Directional)
                {
                    _light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, _commandBuffer);
                    _light.RemoveCommandBuffer(LightEvent.AfterShadowMap, _cascadeShadowCommandBuffer);
                }
                else
                {
                    _light.RemoveCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);
                }
            }
        }

        public void OnDestroy()
        {
            Destroy(_material);
        }

        /// <summary>
        /// Получить матрицу World с кэшированием
        /// </summary>
        private Matrix4x4 GetWorldMatrix(Vector3 scale)
        {
            int currentFrame = Time.frameCount;
            Vector3 pos = _transform.position;
            Quaternion rot = _transform.rotation;

            if (_lastFrameUpdate != currentFrame || _lastPosition != pos || _lastRotation != rot || _lastScale != scale)
            {
                _lastPosition = pos;
                _lastRotation = rot;
                _lastScale = scale;
                _lastFrameUpdate = currentFrame;

                _cachedWorldMatrix = Matrix4x4.TRS(pos, rot, scale);
            }

            return _cachedWorldMatrix;
        }

        /// <summary>
        /// Получить Camera WorldToCameraMatrix с кэшированием
        /// </summary>
        private Matrix4x4 GetCameraWorldToCameraMatrix()
        {
            int currentFrame = Time.frameCount;
            if (_lastCameraMatrixFrame != currentFrame)
            {
                _lastCameraWorldToLocal = _camera.worldToCameraMatrix;
                _lastCameraMatrixFrame = currentFrame;
            }
            return _lastCameraWorldToLocal;
        }

        /// <summary>
        /// Предвычислить константы MieG
        /// </summary>
        private void UpdateMieG()
        {
            if (Mathf.Abs(_lastMieG - MieG) > 0.0001f)
            {
                _lastMieG = MieG;
                float mieGSqr = MieG * MieG;
                _cachedMieG = new Vector4(1 - mieGSqr, 1 + mieGSqr, 2 * MieG, 1.0f / (4.0f * Mathf.PI));
            }
        }

        /// <summary>
        /// Предвычислить VolumetricLight вектор
        /// </summary>
        private void UpdateVolumetricLight()
        {
            if (Mathf.Abs(_lastScattering - ScatteringCoef) > 0.0001f ||
                Mathf.Abs(_lastExtinction - ExtinctionCoef) > 0.0001f ||
                Mathf.Abs(_lastSkyboxExtinction - SkyboxExtinctionCoef) > 0.0001f)
            {
                _lastScattering = ScatteringCoef;
                _lastExtinction = ExtinctionCoef;
                _lastSkyboxExtinction = SkyboxExtinctionCoef;

                _cachedVolumetricLight = new Vector4(ScatteringCoef, ExtinctionCoef, _light.range, 1.0f - SkyboxExtinctionCoef);
            }
        }

        /// <summary>
        /// Проверка на теневую дистанцию (кэшированная)
        /// </summary>
        private bool IsInShadowDistance(Vector3 lightPos, Vector3 cameraPos)
        {
            int currentFrame = Time.frameCount;
            if (_lastShadowDistanceFrame != currentFrame)
            {
                float shadowDist = QualitySettings.shadowDistance;
                _shadowDistanceSqr = shadowDist * shadowDist;
                _lastShadowDistanceFrame = currentFrame;
            }

            return (lightPos - cameraPos).sqrMagnitude < _shadowDistanceSqr;
        }

        private void VolumetricLightRenderer_PreRenderEvent(VolumetricLightRenderer renderer, Matrix4x4 viewProj)
        {
            if (_light == null || _light.gameObject == null)
            {
                VolumetricLightRenderer.PreRenderEvent -= VolumetricLightRenderer_PreRenderEvent;
                return;
            }

            if (!_light.gameObject.activeInHierarchy || _light.enabled == false)
                return;

            _camera = Camera.current;
            // The material is made in Start, which a light switched on this frame has not had yet.
            if (_material == null)
            {
                Material light = VolumetricLightRenderer.GetLightMaterial();
                if (light == null)
                    return;
                _material = new Material(light);
            }
            if (_camera == null || renderer == null)
                return;

            // Предвычисления констант
            UpdateMieG();
            UpdateVolumetricLight();

            // Установка общих параметров материала
            _material.SetVector(ID.CameraForward, _camera.transform.forward);
            _material.SetInt(ID.SampleCount, SampleCount);

            _cachedNoiseVelocity.x = NoiseVelocity.x * NoiseScale;
            _cachedNoiseVelocity.y = NoiseVelocity.y * NoiseScale;
            _material.SetVector(ID.NoiseVelocity, _cachedNoiseVelocity);

            _cachedNoiseData.x = NoiseScale;
            _cachedNoiseData.y = NoiseIntensity;
            _cachedNoiseData.z = NoiseIntensityOffset;
            _material.SetVector(ID.NoiseData, _cachedNoiseData);

            _material.SetVector(ID.MieG, _cachedMieG);
            _material.SetVector(ID.VolumetricLight, _cachedVolumetricLight);
            _material.SetTexture(ID.CameraDepthTexture, renderer.GetVolumeLightDepthBuffer());
            _material.SetFloat(ID.ZTest, (int)UnityEngine.Rendering.CompareFunction.Always);

            // Height Fog
            if (HeightFog)
            {
                _material.EnableKeyword(HEIGHT_FOG);
                _cachedHeightFog.x = GroundLevel;
                _cachedHeightFog.y = HeightScale;
                _material.SetVector(ID.HeightFog, _cachedHeightFog);
            }
            else
            {
                _material.DisableKeyword(HEIGHT_FOG);
            }

            // Обработка типов источников света
            if (_light.type == LightType.Point)
            {
                SetupPointLight(renderer, viewProj);
            }
            else if (_light.type == LightType.Spot)
            {
                SetupSpotLight(renderer, viewProj);
            }
            else if (_light.type == LightType.Directional)
            {
                SetupDirectionalLight(renderer, viewProj);
            }
        }

        private void SetupPointLight(VolumetricLightRenderer renderer, Matrix4x4 viewProj)
        {
            _commandBuffer.Clear();

            int pass = IsCameraInPointLightBounds() ? 0 : 2;
            _material.SetPass(pass);

            Mesh mesh = VolumetricLightRenderer.GetPointLightMesh();

            float scale = _light.range * 2.0f;
            Vector3 scaleVec = new Vector3(scale, scale, scale);
            Matrix4x4 world = GetWorldMatrix(scaleVec);

            // Кэшированное умножение матриц
            _cachedWorldViewProj = viewProj * world;
            _material.SetMatrix(ID.WorldViewProj, _cachedWorldViewProj);

            _cachedWorldViewMatrix = GetCameraWorldToCameraMatrix() * world;
            _material.SetMatrix(ID.WorldView, _cachedWorldViewMatrix);

            // Noise
            if (Noise)
                _material.EnableKeyword(NOISE);
            else
                _material.DisableKeyword(NOISE);

            Vector3 lightPos = _lightTransform.position;
            float invRangeSqr = 1.0f / (_light.range * _light.range);
            _material.SetVector(ID.LightPos, new Vector4(lightPos.x, lightPos.y, lightPos.z, invRangeSqr));
            _material.SetColor(ID.LightColor, _light.color);

            // Cookie
            if (_light.cookie == null)
            {
                _material.EnableKeyword(POINT);
                _material.DisableKeyword(POINT_COOKIE);
            }
            else
            {
                _cachedMyLightMatrix0 = Matrix4x4.TRS(lightPos, _lightTransform.rotation, Vector3.one).inverse;
                _material.SetMatrix(ID.MyLightMatrix0, _cachedMyLightMatrix0);

                _material.EnableKeyword(POINT_COOKIE);
                _material.DisableKeyword(POINT);
                _material.SetTexture(ID.LightTexture0, _light.cookie);
            }

            // Shadows
            bool useShadows = _light.shadows != LightShadows.None && IsInShadowDistance(lightPos, _camera.transform.position);

            if (useShadows)
            {
                _material.EnableKeyword(SHADOWS_CUBE);
                _commandBuffer.SetGlobalTexture(ID.ShadowMapTexture, BuiltinRenderTextureType.CurrentActive);
                _commandBuffer.SetRenderTarget(renderer.GetVolumeLightBuffer());
                _commandBuffer.DrawMesh(mesh, world, _material, 0, pass);

                CustomRenderEvent?.Invoke(renderer, this, _commandBuffer, viewProj);
            }
            else
            {
                _material.DisableKeyword(SHADOWS_CUBE);
                renderer.GlobalCommandBuffer.DrawMesh(mesh, world, _material, 0, pass);

                CustomRenderEvent?.Invoke(renderer, this, renderer.GlobalCommandBuffer, viewProj);
            }
        }

        private void SetupSpotLight(VolumetricLightRenderer renderer, Matrix4x4 viewProj)
        {
            _commandBuffer.Clear();

            int pass = IsCameraInSpotLightBounds() ? 1 : 3;

            Mesh mesh = VolumetricLightRenderer.GetSpotLightMesh();

            float range = _light.range;
            float angleScale = Mathf.Tan((_light.spotAngle + 1) * 0.5f * Mathf.Deg2Rad) * range;
            Vector3 scaleVec = new Vector3(angleScale, angleScale, range);

            Matrix4x4 world = GetWorldMatrix(scaleVec);

            Vector3 lightPos = _lightTransform.position;
            Quaternion lightRot = _lightTransform.rotation;

            Matrix4x4 view = Matrix4x4.TRS(lightPos, lightRot, Vector3.one).inverse;
            Matrix4x4 proj = Matrix4x4.Perspective(_light.spotAngle, 1, 0, 1);

            // Статический clip matrix
            const float clipVal = 0.5f;
            const float clipScaleVal = -0.5f;
            Matrix4x4 clip = Matrix4x4.TRS(new Vector3(clipVal, clipVal, 0.0f), Quaternion.identity, new Vector3(clipScaleVal, clipScaleVal, 1.0f));

            _cachedMyLightMatrix0 = clip * proj * view;
            _material.SetMatrix(ID.MyLightMatrix0, _cachedMyLightMatrix0);

            _cachedWorldViewProj = viewProj * world;
            _material.SetMatrix(ID.WorldViewProj, _cachedWorldViewProj);

            float invRangeSqr = 1.0f / (range * range);
            _material.SetVector(ID.LightPos, new Vector4(lightPos.x, lightPos.y, lightPos.z, invRangeSqr));
            _material.SetColor(ID.LightColor, _light.color);

            Vector3 axis = _transform.forward;
            Vector3 center = lightPos + axis * range;
            float d = -Vector3.Dot(center, axis);

            _material.SetFloat(ID.PlaneD, d);
            _material.SetFloat(ID.CosAngle, Mathf.Cos((_light.spotAngle + 1) * 0.5f * Mathf.Deg2Rad));
            _material.SetVector(ID.ConeApex, new Vector4(lightPos.x, lightPos.y, lightPos.z));
            _material.SetVector(ID.ConeAxis, new Vector4(axis.x, axis.y, axis.z));

            _material.EnableKeyword(SPOT);

            if (Noise)
                _material.EnableKeyword(NOISE);
            else
                _material.DisableKeyword(NOISE);

            if (_light.cookie == null)
                _material.SetTexture(ID.LightTexture0, VolumetricLightRenderer.GetDefaultSpotCookie());
            else
                _material.SetTexture(ID.LightTexture0, _light.cookie);

            bool useShadows = _light.shadows != LightShadows.None && IsInShadowDistance(lightPos, _camera.transform.position);

            if (useShadows)
            {
                Matrix4x4 clipShadow = Matrix4x4.TRS(new Vector3(clipVal, clipVal, clipVal), Quaternion.identity, new Vector3(clipVal, clipVal, clipVal));

                if (_reversedZ)
                    proj = Matrix4x4.Perspective(_light.spotAngle, 1, range, _light.shadowNearPlane);
                else
                    proj = Matrix4x4.Perspective(_light.spotAngle, 1, _light.shadowNearPlane, range);

                Matrix4x4 m = clipShadow * proj;
                m.m02 *= -1; m.m12 *= -1; m.m22 *= -1; m.m32 *= -1;

                _cachedMyWorld2Shadow = m * view;
                _material.SetMatrix(ID.MyWorld2Shadow, _cachedMyWorld2Shadow);
                _material.SetMatrix(ID.WorldView, _cachedMyWorld2Shadow);

                _material.EnableKeyword(SHADOWS_DEPTH);
                _commandBuffer.SetGlobalTexture(ID.ShadowMapTexture, BuiltinRenderTextureType.CurrentActive);
                _commandBuffer.SetRenderTarget(renderer.GetVolumeLightBuffer());
                _commandBuffer.DrawMesh(mesh, world, _material, 0, pass);

                CustomRenderEvent?.Invoke(renderer, this, _commandBuffer, viewProj);
            }
            else
            {
                _material.DisableKeyword(SHADOWS_DEPTH);
                renderer.GlobalCommandBuffer.DrawMesh(mesh, world, _material, 0, pass);

                CustomRenderEvent?.Invoke(renderer, this, renderer.GlobalCommandBuffer, viewProj);
            }
        }

        private void LateUpdate()
        {
            _commandBuffer?.Clear();

            //_cascadeShadowCommandBuffer?.Clear();
        }

        private void SetupDirectionalLight(VolumetricLightRenderer renderer, Matrix4x4 viewProj)
        {
            _commandBuffer.Clear();

            const int pass = 4;
            _material.SetPass(pass);

            if (Noise)
                _material.EnableKeyword(NOISE);
            else
                _material.DisableKeyword(NOISE);

            Vector3 forward = _lightTransform.forward;
            float invRangeSqr = 1.0f / (_light.range * _light.range);
            _material.SetVector(ID.LightDir, new Vector4(forward.x, forward.y, forward.z, invRangeSqr));
            _material.SetColor(ID.LightColor, _light.color);
            _material.SetFloat(ID.MaxRayLength, MaxRayLength);

            if (_light.cookie == null)
            {
                _material.EnableKeyword(DIRECTIONAL);
                _material.DisableKeyword(DIRECTIONAL_COOKIE);
            }
            else
            {
                _material.EnableKeyword(DIRECTIONAL_COOKIE);
                _material.DisableKeyword(DIRECTIONAL);
                _material.SetTexture(ID.LightTexture0, _light.cookie);
            }

            // Frustum corners с кэшированием
            int currentFrame = Time.frameCount;
            if (_lastFrustumFrame != currentFrame)
            {
                float farClip = _camera.farClipPlane;
                _frustumCorners[0] = _camera.ViewportToWorldPoint(new Vector3(0, 0, farClip));
                _frustumCorners[2] = _camera.ViewportToWorldPoint(new Vector3(0, 1, farClip));
                _frustumCorners[3] = _camera.ViewportToWorldPoint(new Vector3(1, 1, farClip));
                _frustumCorners[1] = _camera.ViewportToWorldPoint(new Vector3(1, 0, farClip));
                _lastFrustumFrame = currentFrame;
            }

            _material.SetVectorArray("_FrustumCorners", _frustumCorners);

            Texture nullTexture = null;
            if (_light.shadows != LightShadows.None)
            {
                _material.EnableKeyword(SHADOWS_DEPTH);
                _commandBuffer.Blit(nullTexture, renderer.GetVolumeLightBuffer(), _material, pass);

                CustomRenderEvent?.Invoke(renderer, this, _commandBuffer, viewProj);
            }
            else
            {
                _material.DisableKeyword(SHADOWS_DEPTH);
                renderer.GlobalCommandBuffer.Blit(nullTexture, renderer.GetVolumeLightBuffer(), _material, pass);

                CustomRenderEvent?.Invoke(renderer, this, renderer.GlobalCommandBuffer, viewProj);
            }
        }

        private bool IsCameraInPointLightBounds()
        {
            float distanceSqr = (_lightTransform.position - _camera.transform.position).sqrMagnitude;
            float extendedRange = _light.range + 1;
            return distanceSqr < (extendedRange * extendedRange);
        }

        private bool IsCameraInSpotLightBounds()
        {
            Vector3 cameraPos = _camera.transform.position;
            Vector3 lightPos = _lightTransform.position;
            Vector3 forward = _transform.forward;

            float distance = Vector3.Dot(forward, (cameraPos - lightPos));
            float extendedRange = _light.range + 1;
            if (distance > extendedRange)
                return false;

            float cosAngle = Vector3.Dot(forward, (cameraPos - lightPos).normalized);
            return (Mathf.Acos(cosAngle) * Mathf.Rad2Deg) <= (_light.spotAngle + 3) * 0.5f;
        }
    }
}