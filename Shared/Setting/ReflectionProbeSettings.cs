using MessagePack;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Graphics.Settings
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ReflectionProbeSettings
    {
        public enum AIReflectionProbeClearFlags 
        { 
            Skybox = ReflectionProbeClearFlags.Skybox,
            SolidColor = ReflectionProbeClearFlags.SolidColor
        }

        public enum AIReflectionProbeTimeSlicingMode
        {
            AllFacesAtOnce = ReflectionProbeTimeSlicingMode.AllFacesAtOnce,
            IndividualFaces = ReflectionProbeTimeSlicingMode.IndividualFaces,
            NoTimeSlicing = ReflectionProbeTimeSlicingMode.NoTimeSlicing
        }

        public int Importance { get; set; }

        public float Intensity { get; set; }

        public bool BoxProjection { get; set; }

        public float BlendDistance { get; set; }

        public Vector3 BoxSize { get; set; }

        public Vector3 BoxOffset { get; set; }

        public int ReflectionResolutions { get; set; }
        
        public bool HDR { get; set; }

        public float ShadowDistance { get; set; }

        public AIReflectionProbeClearFlags ClearFlags { get; set; }

        public int CullingMask { get; set; }

        public float NearClipPlane { get; set; }

        public float FarClipPlane { get; set; }

        public Color BackgroundColor { get; set; }

        public AIReflectionProbeTimeSlicingMode TimeSlicingMode { get; set; }

        public string Name { get; set; }

        public PathElement HierarchyPath { get; set; }

        public void ApplySettings(ReflectionProbe probe)
        {
            probe.backgroundColor = BackgroundColor;
            probe.blendDistance = BlendDistance;
            probe.boxProjection = BoxProjection;
            probe.center = BoxOffset;
            probe.clearFlags = (ReflectionProbeClearFlags)ClearFlags;
            probe.cullingMask = CullingMask;
            probe.farClipPlane = FarClipPlane;
            probe.hdr = HDR;
            probe.importance = Importance;
            probe.intensity = Intensity;
            probe.nearClipPlane = NearClipPlane;
            probe.resolution = ReflectionResolutions;
            probe.shadowDistance = ShadowDistance;
            probe.size = BoxSize;
            probe.timeSlicingMode = (ReflectionProbeTimeSlicingMode)TimeSlicingMode;

        }

        public void FillSettings(ReflectionProbe probe)
        {
            if (probe == null || (probe != null && !probe.isActiveAndEnabled))
                return;

            try
            {
                BackgroundColor = probe.backgroundColor;
                BlendDistance = probe.blendDistance;
                BoxProjection = probe.boxProjection;
                BoxOffset = probe.center;
                ClearFlags = (AIReflectionProbeClearFlags)probe.clearFlags;
                CullingMask = probe.cullingMask;
                FarClipPlane = probe.farClipPlane;
                HDR = probe.hdr;
                Importance = probe.importance;
                Intensity = probe.intensity;
                NearClipPlane = probe.nearClipPlane;
                ReflectionResolutions = probe.resolution;
                ShadowDistance = probe.shadowDistance;
                BoxSize = probe.size;
                TimeSlicingMode = (AIReflectionProbeTimeSlicingMode)probe.timeSlicingMode;
                Name = probe.name;

                HierarchyPath = PathElement.Build(probe.gameObject.transform);
            }
            catch (Exception err)
            {
                Graphics.Instance.Log.LogInfo($"{err.Message} {err.StackTrace}");
            }
        }     
    }
}
