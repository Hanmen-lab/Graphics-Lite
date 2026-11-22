// From Graphics (Shared/Standard Image Effects/VolumetricLights), itself based on Unity's Volumetric
// Lights by Michal Skalsky (BSD). Namespace and resource loading changed for this plugin; otherwise as
// Graphics ships it.
//  Copyright(c) 2016, Michal Skalsky
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:
//
//  1. Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//  2. Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//  3. Neither the name of the copyright holder nor the names of its contributors
//     may be used to endorse or promote products derived from this software without
//     specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT
//  SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
//  TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.



using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System;

namespace Graphics.VolumetricLights
{
    [RequireComponent(typeof(Camera))]
    public class VolumetricLightRenderer : MonoBehaviour
    {
        public enum VolumetricResolution
        {
            Full,
            Half,
            Quarter
        };

        public bool UseBilateralBlur = true;
        public bool VisualizeLightTexture = false;

        public static event Action<VolumetricLightRenderer, Matrix4x4> PreRenderEvent;

        int frameCounter = 0;
        private static Mesh _pointLightMesh;
        private static Mesh _spotLightMesh;
        private static Material _lightMaterial;

        private Camera _camera;
        private CommandBuffer _preLightPass;

        private Matrix4x4 _viewProj;
        private Material _blitAddMaterial;
        private Material _bilateralBlurMaterial;

        private RenderTexture _volumeLightTexture;
        private RenderTexture _halfVolumeLightTexture;
        private RenderTexture _quarterVolumeLightTexture;
        private static Texture _defaultSpotCookie;

        private RenderTexture _halfDepthBuffer;
        private RenderTexture _quarterDepthBuffer;
        private VolumetricResolution _currentResolution = VolumetricResolution.Half;
        private Texture2D _ditheringTexture;
        private Texture3D _noiseTexture;

        private Vector2 currentResolution;

        public VolumetricResolution Resolution = VolumetricResolution.Half;
        public Texture DefaultSpotCookie;

        public CommandBuffer GlobalCommandBuffer { get { return _preLightPass; } }

        AssetBundle assetBundle;
        Shader volumetricshader, blitshader, blurshader;

        //Shader IDs
        static readonly int HalfResDepthBuffer = Shader.PropertyToID("_HalfResDepthBuffer");
        static readonly int HalfResColor = Shader.PropertyToID("_HalfResColor");
        static readonly int QuarterResDepthBuffer = Shader.PropertyToID("_QuarterResDepthBuffer");
        static readonly int QuarterResColor = Shader.PropertyToID("_QuarterResColor");
        static readonly int DitherTexture = Shader.PropertyToID("_DitherTexture");
        static readonly int NoiseTexture = Shader.PropertyToID("_NoiseTexture");
        static readonly int Source = Shader.PropertyToID("_Source");
        static readonly int FrameSwitch = Shader.PropertyToID("FrameSwitch");
        static readonly int CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        static readonly int ViewProj = Shader.PropertyToID("_ViewProj");

        public static Material GetLightMaterial()
        {
            return _lightMaterial;
        }

        public static Mesh GetPointLightMesh()
        {
            return _pointLightMesh;
        }

        public static Mesh GetSpotLightMesh()
        {
            return _spotLightMesh;
        }

        public RenderTexture GetVolumeLightBuffer()
        {
            if (Resolution == VolumetricResolution.Quarter)
                return _quarterVolumeLightTexture;
            else if (Resolution == VolumetricResolution.Half)
                return _halfVolumeLightTexture;
            else
                return _volumeLightTexture;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RenderTexture GetVolumeLightDepthBuffer()
        {
            if (Resolution == VolumetricResolution.Quarter)
                return _quarterDepthBuffer;
            else if (Resolution == VolumetricResolution.Half)
                return _halfDepthBuffer;
            else
                return null;
        }

        public static Texture GetDefaultSpotCookie()
        {
            return _defaultSpotCookie;
        }

        void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera.actualRenderingPath == RenderingPath.Forward)
                _camera.depthTextureMode |= DepthTextureMode.Depth; // not =: TAA and CTAA need the motion vectors

            _currentResolution = Resolution;

            // Graphics' volumetrics.unity3d, embedded in the plugin.

            blitshader = EmbeddedBundles.Load<Shader>("volumetrics.unity3d", "Assets/VolumetricLights/Shaders/BlitAdd.shader");
            if (!blitshader) Graphics.Instance.Log.LogError("failed to load BlitAdd.shader");

            blurshader = EmbeddedBundles.Load<Shader>("volumetrics.unity3d", "Assets/VolumetricLights/Shaders/BilateralBlur.shader");
            if (!blurshader) Graphics.Instance.Log.LogError("failed to load BilateralBlur.shader");

            _blitAddMaterial = new Material(blitshader);
            _bilateralBlurMaterial = new Material(blurshader);

            _preLightPass = new CommandBuffer();
            _preLightPass.name = "PreLight";

            ChangeResolution();

            if (_pointLightMesh == null)
            {
                _pointLightMesh = CreatePrimitiveMesh(PrimitiveType.Sphere);
            }

            if (_spotLightMesh == null)
            {
                _spotLightMesh = CreateSpotLightMesh();
            }

            if (_lightMaterial == null)
            {
                volumetricshader = EmbeddedBundles.Load<Shader>("volumetrics.unity3d", "Assets/VolumetricLights/Shaders/VolumetricLight.shader");
                _lightMaterial = new Material(volumetricshader);
            }

            if (_defaultSpotCookie == null)
            {
                _defaultSpotCookie = EmbeddedBundles.Load<Texture2D>("volumetrics.unity3d", "Assets/VolumetricLights/Shaders/spot.png");
            }
        }

        void OnEnable()
        {
            //_camera.RemoveAllCommandBuffers();
            if (_camera.actualRenderingPath == RenderingPath.Forward)
                _camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _preLightPass);
            else
                _camera.AddCommandBuffer(CameraEvent.BeforeLighting, _preLightPass);

            //foreach (VolumetricLight volumetricLight in Resources.FindObjectsOfTypeAll<VolumetricLight>())
            //    volumetricLight.enabled = true;

            ChangeResolution();
            LoadTextures();
        }

        void OnDisable()
        {
            //_camera.RemoveAllCommandBuffers();
            if (_camera.actualRenderingPath == RenderingPath.Forward)
                _camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _preLightPass);
            else
                _camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, _preLightPass);

            //foreach (VolumetricLight volumetricLight in Resources.FindObjectsOfTypeAll<VolumetricLight>())
            //    volumetricLight.enabled = false;

            CleanupTextures();
        }

        void LoadTextures()
        {
            if (!_noiseTexture)
                _noiseTexture = EmbeddedBundles.Load<Texture3D>("ultrafog.unity3d", "Assets/HeightFog/Resources/FogWindNoise.asset", false) ?? NoiseVolume.Get();

            GenerateDitherTexture();
            Shader.SetGlobalTexture(DitherTexture, _ditheringTexture);
            Shader.SetGlobalTexture(NoiseTexture, _noiseTexture);
        }

        void CleanupTextures()
        {
            // The noise volume is shared and kept for the session (see EmbeddedBundles).
            if (_ditheringTexture != null)
                Destroy(_ditheringTexture);
            if (_volumeLightTexture != null)
                Destroy(_volumeLightTexture);
            if (_halfDepthBuffer != null)
                Destroy(_halfDepthBuffer);
            if (_halfVolumeLightTexture != null)
                Destroy(_halfVolumeLightTexture);
            if (_quarterVolumeLightTexture != null)
                Destroy(_quarterVolumeLightTexture);
            if (_quarterDepthBuffer != null)
                Destroy(_quarterDepthBuffer);

            _volumeLightTexture = null;
            _ditheringTexture = null;
            _halfDepthBuffer = null;
            _halfVolumeLightTexture = null;
            _quarterVolumeLightTexture = null;
            _quarterDepthBuffer = null;
        }

        public void ChangeResolution()
        {

            //currentResolution = new Vector2(_camera.pixelWidth, _camera.pixelHeight);
            //var targetRT = _camera.targetTexture;
            //if (targetRT != null)
            //{
            //    currentResolution = new Vector2(targetRT.width, targetRT.height);
            //}


            // HS2PostProcessing: Awake and OnEnable call this before OnPreRender has read the camera's
            // size, which made 0x0 textures ("RenderTexture.Create failed"). Read it here when it is not
            // known yet, and wait for a frame with a size (FSR3 shrinks the viewport in between).
            if (currentResolution.x < 1f || currentResolution.y < 1f)
            {
                RenderTexture target = _camera != null ? _camera.targetTexture : null;
                currentResolution = target != null ? new Vector2(target.width, target.height)
                    : _camera != null ? new Vector2(_camera.pixelWidth, _camera.pixelHeight) : Vector2.zero;
                if (currentResolution.x < 4f || currentResolution.y < 4f)
                {
                    currentResolution = Vector2.zero;
                    return;
                }
            }

            if (_volumeLightTexture != null)
                Destroy(_volumeLightTexture);

            _volumeLightTexture = new RenderTexture((int)currentResolution.x, (int)currentResolution.y, 0, RenderTextureFormat.ARGBHalf);
            _volumeLightTexture.name = "VolumeLightBuffer";
            _volumeLightTexture.filterMode = FilterMode.Bilinear;

            if (_halfDepthBuffer != null)
                Destroy(_halfDepthBuffer);
            if (_halfVolumeLightTexture != null)
                Destroy(_halfVolumeLightTexture);

            if (Resolution == VolumetricResolution.Half || Resolution == VolumetricResolution.Quarter)
            {
                _halfVolumeLightTexture = new RenderTexture((int)currentResolution.x / 2, (int)currentResolution.y / 2, 0, RenderTextureFormat.ARGBHalf);
                _halfVolumeLightTexture.name = "VolumeLightBufferHalf";
                _halfVolumeLightTexture.filterMode = FilterMode.Bilinear;

                _halfDepthBuffer = new RenderTexture((int)currentResolution.x / 2, (int)currentResolution.y / 2, 0, RenderTextureFormat.RFloat);
                _halfDepthBuffer.name = "VolumeLightHalfDepth";
                _halfDepthBuffer.Create();
                _halfDepthBuffer.filterMode = FilterMode.Point;
            }

            if (_quarterVolumeLightTexture != null)
                Destroy(_quarterVolumeLightTexture);
            if (_quarterDepthBuffer != null)
                Destroy(_quarterDepthBuffer);

            if (Resolution == VolumetricResolution.Quarter)
            {
                _quarterVolumeLightTexture = new RenderTexture((int)currentResolution.x / 4, (int)currentResolution.y / 4, 0, RenderTextureFormat.ARGBHalf);
                _quarterVolumeLightTexture.name = "VolumeLightBufferQuarter";
                _quarterVolumeLightTexture.filterMode = FilterMode.Bilinear;

                _quarterDepthBuffer = new RenderTexture((int)currentResolution.x / 4, (int)currentResolution.y / 4, 0, RenderTextureFormat.RFloat);
                _quarterDepthBuffer.name = "VolumeLightQuarterDepth";
                _quarterDepthBuffer.Create();
                _quarterDepthBuffer.filterMode = FilterMode.Point;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnPreRender()
        {
            Matrix4x4 proj = Matrix4x4.Perspective(_camera.fieldOfView, _camera.aspect, 0.01f, _camera.farClipPlane);

            if (UnityEngine.XR.XRSettings.enabled)
            {
                proj = Camera.current.projectionMatrix;
            }

            proj = GL.GetGPUProjectionMatrix(proj, true);
            _viewProj = proj * _camera.worldToCameraMatrix;

            _preLightPass.Clear();

            bool dx11 = SystemInfo.graphicsShaderLevel > 40;

            // Оптимизированная проверка разрешения
            int currentWidth = _camera.pixelWidth;
            int currentHeight = _camera.pixelHeight;

            var targetRT = _camera.targetTexture;
            if (targetRT)
            {
                currentWidth = targetRT.width;
                currentHeight = targetRT.height;
            }

            // Используем целочисленное сравнение вместо Vector2
            if (currentWidth != (int)currentResolution.x ||
                currentHeight != (int)currentResolution.y ||
                _currentResolution != Resolution)
            {
                currentResolution = new Vector2(currentWidth, currentHeight);
                _currentResolution = Resolution;
                ChangeResolution();
            }
            if (_volumeLightTexture == null)
                return;

            if (Resolution == VolumetricResolution.Quarter)
            {
                Texture nullTexture = null;
                _preLightPass.Blit(nullTexture, _halfDepthBuffer, _bilateralBlurMaterial, dx11 ? 4 : 10);
                _preLightPass.Blit(nullTexture, _quarterDepthBuffer, _bilateralBlurMaterial, dx11 ? 6 : 11);
                _preLightPass.SetRenderTarget(_quarterVolumeLightTexture);
            }
            else if (Resolution == VolumetricResolution.Half)
            {
                Texture nullTexture = null;
                _preLightPass.Blit(nullTexture, _halfDepthBuffer, _bilateralBlurMaterial, dx11 ? 4 : 10);
                _preLightPass.SetRenderTarget(_halfVolumeLightTexture);
            }
            else
            {
                _preLightPass.SetRenderTarget(_volumeLightTexture);
            }

            _preLightPass.ClearRenderTarget(false, true, new Color(0, 0, 0, 1));

            UpdateMaterialParameters();

            PreRenderEvent?.Invoke(this, _viewProj);
        }

        [ImageEffectOpaque]
        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // HS2PostProcessing: no buffers yet (the camera had no size): pass the frame through.
            if (_volumeLightTexture == null)
            {
                UnityEngine.Graphics.Blit(source, destination);
                return;
            }
            RenderTexture activeBuffer;
            int width, height;

            switch (Resolution)
            {
                case VolumetricResolution.Quarter:
                    activeBuffer = _quarterVolumeLightTexture;
                    width = _quarterDepthBuffer.width;
                    height = _quarterDepthBuffer.height;
                    break;
                case VolumetricResolution.Half:
                    activeBuffer = _halfVolumeLightTexture;
                    width = _halfVolumeLightTexture.width;
                    height = _halfVolumeLightTexture.height;
                    break;
                default:
                    activeBuffer = _volumeLightTexture;
                    width = _volumeLightTexture.width;
                    height = _volumeLightTexture.height;
                    break;
            }

            if (UseBilateralBlur && Resolution != VolumetricResolution.Full)
            {
                RenderTexture temp = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBHalf);
                temp.filterMode = FilterMode.Bilinear;

                int horizontalPass = Resolution == VolumetricResolution.Quarter ? 8 : 2;
                int verticalPass = Resolution == VolumetricResolution.Quarter ? 9 : 3;

                UnityEngine.Graphics.Blit(activeBuffer, temp, _bilateralBlurMaterial, horizontalPass);
                UnityEngine.Graphics.Blit(temp, activeBuffer, _bilateralBlurMaterial, verticalPass);

                RenderTexture.ReleaseTemporary(temp);
            }
            else if (UseBilateralBlur)  // Full resolution
            {
                RenderTexture temp = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBHalf);
                temp.filterMode = FilterMode.Bilinear;

                UnityEngine.Graphics.Blit(_volumeLightTexture, temp, _bilateralBlurMaterial, 0);
                UnityEngine.Graphics.Blit(temp, _volumeLightTexture, _bilateralBlurMaterial, 1);

                RenderTexture.ReleaseTemporary(temp);
            }

            // Upscale if needed
            if (Resolution != VolumetricResolution.Full)
            {
                int upscalePass = Resolution == VolumetricResolution.Quarter ? 7 : 5;
                UnityEngine.Graphics.Blit(activeBuffer, _volumeLightTexture, _bilateralBlurMaterial, upscalePass);
            }

            if (VisualizeLightTexture)
            {
                UnityEngine.Graphics.Blit(_volumeLightTexture, destination);
            }
            else
            {
                _blitAddMaterial.SetTexture(Source, source);
                UnityEngine.Graphics.Blit(_volumeLightTexture, destination, _blitAddMaterial, 0);
            }
        }

        private void OnPostRender()
        {
            frameCounter = (frameCounter + 1) % (64);
            Shader.SetGlobalInt(FrameSwitch, frameCounter);
        }

        private void UpdateMaterialParameters()
        {
            if (Resolution == VolumetricResolution.Half || Resolution == VolumetricResolution.Quarter)
            {
                _bilateralBlurMaterial.SetTexture(HalfResDepthBuffer, _halfDepthBuffer);
                _bilateralBlurMaterial.SetTexture(HalfResColor, _halfVolumeLightTexture);
            }

            if (Resolution == VolumetricResolution.Quarter)
            {
                _bilateralBlurMaterial.SetTexture(QuarterResDepthBuffer, _quarterDepthBuffer);
                _bilateralBlurMaterial.SetTexture(QuarterResColor, _quarterVolumeLightTexture);
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            //#if UNITY_EDITOR
            if (_currentResolution != Resolution)
            {
                _currentResolution = Resolution;
                ChangeResolution();
            }

            if (_volumeLightTexture == null || _volumeLightTexture.width != _camera.pixelWidth || _volumeLightTexture.height != _camera.pixelHeight)
                ChangeResolution();
            //#endif
        }

        private void GenerateDitherTexture()
        {
            if (_ditheringTexture != null)
            {
                return;
            }

            int size = 8;
#if DITHER_4_4
        size = 4;
#endif
            // again, I couldn't make it work with Alpha8
            _ditheringTexture = new Texture2D(size, size, TextureFormat.Alpha8, false, true);
            _ditheringTexture.filterMode = FilterMode.Point;
            Color32[] c = new Color32[size * size];

            byte b;
#if DITHER_4_4
        b = (byte)(0.0f / 16.0f * 255); c[0] = new Color32(b, b, b, b);
        b = (byte)(8.0f / 16.0f * 255); c[1] = new Color32(b, b, b, b);
        b = (byte)(2.0f / 16.0f * 255); c[2] = new Color32(b, b, b, b);
        b = (byte)(10.0f / 16.0f * 255); c[3] = new Color32(b, b, b, b);

        b = (byte)(12.0f / 16.0f * 255); c[4] = new Color32(b, b, b, b);
        b = (byte)(4.0f / 16.0f * 255); c[5] = new Color32(b, b, b, b);
        b = (byte)(14.0f / 16.0f * 255); c[6] = new Color32(b, b, b, b);
        b = (byte)(6.0f / 16.0f * 255); c[7] = new Color32(b, b, b, b);

        b = (byte)(3.0f / 16.0f * 255); c[8] = new Color32(b, b, b, b);
        b = (byte)(11.0f / 16.0f * 255); c[9] = new Color32(b, b, b, b);
        b = (byte)(1.0f / 16.0f * 255); c[10] = new Color32(b, b, b, b);
        b = (byte)(9.0f / 16.0f * 255); c[11] = new Color32(b, b, b, b);

        b = (byte)(15.0f / 16.0f * 255); c[12] = new Color32(b, b, b, b);
        b = (byte)(7.0f / 16.0f * 255); c[13] = new Color32(b, b, b, b);
        b = (byte)(13.0f / 16.0f * 255); c[14] = new Color32(b, b, b, b);
        b = (byte)(5.0f / 16.0f * 255); c[15] = new Color32(b, b, b, b);
#else
            int i = 0;
            b = (byte)(1.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(49.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(13.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(61.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(4.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(52.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(16.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(64.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(33.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(17.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(45.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(29.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(36.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(20.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(48.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(32.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(9.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(57.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(5.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(53.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(12.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(60.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(8.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(56.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(41.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(25.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(37.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(21.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(44.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(28.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(40.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(24.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(3.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(51.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(15.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(63.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(2.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(50.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(14.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(62.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(35.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(19.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(47.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(31.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(34.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(18.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(46.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(30.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(11.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(59.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(7.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(55.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(10.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(58.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(6.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(54.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);

            b = (byte)(43.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(27.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(39.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(23.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(42.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(26.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(38.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
            b = (byte)(22.0f / 65.0f * 255); c[i++] = new Color32(b, b, b, b);
#endif

            _ditheringTexture.SetPixels32(c);
            _ditheringTexture.Apply();
        }

        private static Mesh CreatePrimitiveMesh(PrimitiveType primitiveType)
        {
            GameObject tempGO = GameObject.CreatePrimitive(primitiveType);
            Mesh mesh = tempGO.GetComponent<MeshFilter>().sharedMesh;

            if (Application.isPlaying)
                Destroy(tempGO);
            else
                DestroyImmediate(tempGO);

            return mesh;
        }

        private Mesh CreateSpotLightMesh()
        {
            // copy & pasted from other project, the geometry is too complex, should be simplified
            Mesh mesh = new Mesh();

            const int segmentCount = 32;
            Vector3[] vertices = new Vector3[2 + segmentCount * 3];
            Color32[] colors = new Color32[2 + segmentCount * 3];

            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(0, 0, 1);

            float angle = 0;
            float step = Mathf.PI * 2.0f / segmentCount;
            float ratio = 0.9f;

            for (int i = 0; i < segmentCount; ++i)
            {
                vertices[i + 2] = new Vector3(-Mathf.Cos(angle) * ratio, Mathf.Sin(angle) * ratio, ratio);
                colors[i + 2] = new Color32(255, 255, 255, 255);
                vertices[i + 2 + segmentCount] = new Vector3(-Mathf.Cos(angle), Mathf.Sin(angle), 1);
                colors[i + 2 + segmentCount] = new Color32(255, 255, 255, 0);
                vertices[i + 2 + segmentCount * 2] = new Vector3(-Mathf.Cos(angle) * ratio, Mathf.Sin(angle) * ratio, 1);
                colors[i + 2 + segmentCount * 2] = new Color32(255, 255, 255, 255);
                angle += step;
            }

            mesh.vertices = vertices;
            mesh.colors32 = colors;

            int[] indices = new int[segmentCount * 3 * 2 + segmentCount * 6 * 2];
            int index = 0;

            for (int i = 2; i < segmentCount + 1; ++i)
            {
                indices[index++] = 0;
                indices[index++] = i;
                indices[index++] = i + 1;
            }

            indices[index++] = 0;
            indices[index++] = segmentCount + 1;
            indices[index++] = 2;

            for (int i = 2; i < segmentCount + 1; ++i)
            {
                indices[index++] = i;
                indices[index++] = i + segmentCount;
                indices[index++] = i + 1;

                indices[index++] = i + 1;
                indices[index++] = i + segmentCount;
                indices[index++] = i + segmentCount + 1;
            }

            indices[index++] = 2;
            indices[index++] = 1 + segmentCount;
            indices[index++] = 2 + segmentCount;

            indices[index++] = 2 + segmentCount;
            indices[index++] = 1 + segmentCount;
            indices[index++] = 1 + segmentCount + segmentCount;

            //------------
            for (int i = 2 + segmentCount; i < segmentCount + 1 + segmentCount; ++i)
            {
                indices[index++] = i;
                indices[index++] = i + segmentCount;
                indices[index++] = i + 1;

                indices[index++] = i + 1;
                indices[index++] = i + segmentCount;
                indices[index++] = i + segmentCount + 1;
            }

            indices[index++] = 2 + segmentCount;
            indices[index++] = 1 + segmentCount * 2;
            indices[index++] = 2 + segmentCount * 2;

            indices[index++] = 2 + segmentCount * 2;
            indices[index++] = 1 + segmentCount * 2;
            indices[index++] = 1 + segmentCount * 3;

            ////-------------------------------------
            for (int i = 2 + segmentCount * 2; i < segmentCount * 3 + 1; ++i)
            {
                indices[index++] = 1;
                indices[index++] = i + 1;
                indices[index++] = i;
            }

            indices[index++] = 1;
            indices[index++] = 2 + segmentCount * 2;
            indices[index++] = segmentCount * 3 + 1;

            mesh.triangles = indices;
            mesh.RecalculateBounds();

            return mesh;
        }
    }

}