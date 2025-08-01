using System.Diagnostics.CodeAnalysis;
using System;
using MessagePack;
using UnityEngine;
using static Graphics.SEGI.SEGI;
using static Graphics.LightManager;
using UnityEngine.Experimental.GlobalIllumination;
using System.Reflection;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class SEGISettings
    {
        public bool enabled = false;
        public VoxelResolution voxelResolution = VoxelResolution.high;
        public bool voxelAA = true;
        [Range(0, 2)]
        public int innerOcclusionLayers = 1;
        public bool infiniteBounces = false;
        public bool updateGI = true;
        public LayerMask giCullingMask = (1 << LayerMask.NameToLayer("Chara")) | (1 << LayerMask.NameToLayer("Map"));

        [Range(0.01f, 1.0f)]
        public float temporalBlendWeight = 0.15f;
        public bool useBilateralFiltering = true;
        public bool halfResolution = true;
        public bool stochasticSampling = true;
        public bool doReflections = true;
        public bool reflectionDownsampling = false;

        [Range(1, 128)]
        public int cones = 35;
        [Range(1, 32)]
        public int coneTraceSteps = 8;
        [Range(0.1f, 2.0f)]
        public float coneLength = 1.0f;
        [Range(0.5f, 6.0f)]
        public float coneWidth = 4.0f;
        [Range(0.0f, 4.0f)]
        public float coneTraceBias = 0.63f;
        [Range(0.0f, 4.0f)]
        public float occlusionStrength = 1f;
        [Range(0.0f, 4.0f)]
        public float nearOcclusionStrength = 0.0f;
        [Range(0.001f, 4.0f)]
        public float occlusionPower = 1.0f;
        [Range(0.0f, 4.0f)]
        public float nearLightGain = 1.0f;
        [Range(0.0f, 4.0f)]
        public float giGain = 1f;
        [Range(0.0f, 2.0f)]
        public float secondaryBounceGain = 1.0f;
        [Range(12, 128)]
        public int reflectionSteps = 32;
        [Range(0.001f, 4.0f)]
        public float reflectionOcclusionPower = 0.2f;
        [Range(0.0f, 1.0f)]
        public float skyReflectionIntensity = 1.0f;
        public bool gaussianMipFilter = true;

        [Range(0.1f, 4.0f)]
        public float farOcclusionStrength = 1.0f;
        [Range(0.1f, 4.0f)]
        public float farthestOcclusionStrength = 1.0f;

        [Range(3, 16)]
        public int secondaryCones = 6;
        [Range(0.1f, 4.0f)]
        public float secondaryOcclusionStrength = 1.0f;

        public Color skyColor = Color.grey;
        public float softSunlight = 0.0f;

        [Range(0.0f, 8.0f)]
        public float skyIntensity = 0.7f;

        public float shadowSpaceSize = 50.0f;
        public float voxelSpaceSize = 100.0f;
        public bool sphericalSkylight = true;

        public bool visualizeSunDepthTexture = false;
        public bool visualizeGI = false;
        public bool visualizeVoxels = false;

        public DebugTools debugTools = DebugTools.Off;

        public void Load(SEGI.SEGI instance)
        {
            if (instance == null) return;

            instance.enabled = enabled;
            instance.voxelResolution = voxelResolution;
            instance.voxelAA = voxelAA;
            instance.updateGI = updateGI;
            instance.giCullingMask = giCullingMask;
            instance.innerOcclusionLayers = innerOcclusionLayers;
            instance.infiniteBounces = infiniteBounces;
            instance.temporalBlendWeight = temporalBlendWeight;
            instance.useBilateralFiltering = useBilateralFiltering;
            instance.halfResolution = halfResolution;
            instance.stochasticSampling = stochasticSampling;
            instance.doReflections = doReflections;
            instance.cones = cones;
            instance.coneTraceSteps = coneTraceSteps;
            instance.coneLength = coneLength;
            instance.coneWidth = coneWidth;
            instance.coneTraceBias = coneTraceBias;
            instance.occlusionStrength = occlusionStrength;
            instance.nearOcclusionStrength = nearOcclusionStrength;
            instance.occlusionPower = occlusionPower;
            instance.nearLightGain = nearLightGain;
            instance.giGain = giGain;
            instance.secondaryBounceGain = secondaryBounceGain;
            instance.reflectionSteps = reflectionSteps;
            instance.reflectionOcclusionPower = reflectionOcclusionPower;
            instance.skyReflectionIntensity = skyReflectionIntensity;
            instance.gaussianMipFilter = gaussianMipFilter;
            instance.farOcclusionStrength = farOcclusionStrength;
            instance.farthestOcclusionStrength = farthestOcclusionStrength;
            instance.secondaryCones = secondaryCones;
            instance.secondaryOcclusionStrength = secondaryOcclusionStrength;
            instance.skyColor = skyColor;
            instance.softSunlight = softSunlight;
            instance.skyIntensity = skyIntensity;
            instance.shadowSpaceSize = shadowSpaceSize;
            instance.voxelSpaceSize = voxelSpaceSize;
            instance.sphericalSkylight = sphericalSkylight;
            instance.debugTools = debugTools;
            instance.reflectionDownsampling = reflectionDownsampling;
        }
        public void Save(SEGI.SEGI instance)
        {
            if (instance == null) return;

            enabled = instance.enabled;
            voxelResolution = instance.voxelResolution;
            voxelAA = instance.voxelAA;
            updateGI = instance.updateGI;
            giCullingMask = instance.giCullingMask;   
            innerOcclusionLayers = instance.innerOcclusionLayers;
            infiniteBounces = instance.infiniteBounces;
            temporalBlendWeight = instance.temporalBlendWeight;
            useBilateralFiltering = instance.useBilateralFiltering;
            halfResolution = instance.halfResolution;
            stochasticSampling = instance.stochasticSampling;
            doReflections = instance.doReflections;
            cones = instance.cones;
            coneTraceSteps = instance.coneTraceSteps;
            coneLength = instance.coneLength;
            coneWidth = instance.coneWidth;
            coneTraceBias = instance.coneTraceBias;
            occlusionStrength = instance.occlusionStrength;
            nearOcclusionStrength = instance.nearOcclusionStrength;
            occlusionPower = instance.occlusionPower;
            nearLightGain = instance.nearLightGain;
            giGain = instance.giGain;
            secondaryBounceGain = instance.secondaryBounceGain;
            reflectionSteps = instance.reflectionSteps;
            reflectionOcclusionPower = instance.reflectionOcclusionPower;
            skyReflectionIntensity = instance.skyReflectionIntensity;
            gaussianMipFilter = instance.gaussianMipFilter;
            farOcclusionStrength = instance.farOcclusionStrength;
            farthestOcclusionStrength = instance.farthestOcclusionStrength;
            secondaryCones = instance.secondaryCones;
            secondaryOcclusionStrength = instance.secondaryOcclusionStrength;
            skyColor = instance.skyColor;
            softSunlight = instance.softSunlight;
            skyIntensity = instance.skyIntensity;
            shadowSpaceSize = instance.shadowSpaceSize;
            voxelSpaceSize = instance.voxelSpaceSize;
            sphericalSkylight = instance.sphericalSkylight;
            debugTools = instance.debugTools;
            reflectionDownsampling = instance.reflectionDownsampling;

        }

    }
}