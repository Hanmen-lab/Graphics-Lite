using MessagePack;
using PlaceholderSoftware.WetStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Knife.DeferredDecals;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class DeferredDecalsSettings
    {
        public bool enabled = false;
        public BoolValue LockRebuild = new BoolValue(false, false);
        //public TerrainDecalsType TerrainDecals = TerrainDecalsType.None;
        public IntValue TerrainHeightMapSize = new IntValue(1024, false);
        public BoolValue UseExclusionMask = new BoolValue(false, false);
        public LayerMask ExclusionMask = new LayerMask();
        public BoolValue FrustumCulling = new BoolValue(true, false);
        public BoolValue DistanceCulling = new BoolValue(true, false);
        public FloatValue StartFadeDistance = new FloatValue(50, false);
        public FloatValue FadeLength = new FloatValue(2, false);
        public BoolValue DrawDecalGizmos = new BoolValue(true, false);

        public enum TerrainDecalsType
        {
            None,
            OneTerrain,
            MultiTerrain
        }

        public void Load(DeferredDecalsSystem decalLayer)
        {
            if (decalLayer == null)
                return;

            decalLayer.enabled = enabled;
            decalLayer.LockRebuild = LockRebuild.value;
            //decalLayer.TerrainDecals = (DeferredDecalsSystem.TerrainDecalsType)TerrainDecals;

            if (TerrainHeightMapSize.overrideState)
                decalLayer.TerrainHeightMapSize = TerrainHeightMapSize.value;
            else
                decalLayer.TerrainHeightMapSize = 1024;

            decalLayer.UseExclusionMask = UseExclusionMask.value;
            decalLayer.ExclusionMask = ExclusionMask;
            decalLayer.FrustumCulling = FrustumCulling.value;
            decalLayer.DistanceCulling = DistanceCulling.value;

            if (StartFadeDistance.overrideState)
                decalLayer.StartFadeDistance = StartFadeDistance.value;
            else
                decalLayer.StartFadeDistance = 50f;

            if (FadeLength.overrideState)
                decalLayer.FadeLength = FadeLength.value;
            else
                decalLayer.FadeLength = 2f;

            //decalLayer.DrawDecalGizmos = DrawDecalGizmos.value;
        }

        public void Save(DeferredDecalsSystem decalLayer)
        {
            if (decalLayer == null)
                return;

            enabled = decalLayer.enabled;
            LockRebuild.value = decalLayer.LockRebuild;
            //TerrainDecals = (TerrainDecalsType)decalLayer.TerrainDecals;
            TerrainHeightMapSize.value = decalLayer.TerrainHeightMapSize;
            UseExclusionMask.value = decalLayer.UseExclusionMask;
            ExclusionMask = decalLayer.ExclusionMask;
            FrustumCulling.value = decalLayer.FrustumCulling;
            DistanceCulling.value = decalLayer.DistanceCulling;
            StartFadeDistance.value = decalLayer.StartFadeDistance;
            FadeLength.value = decalLayer.FadeLength;
            //DrawDecalGizmos.value = decalLayer.DrawDecalGizmos;
        }
    }
}
