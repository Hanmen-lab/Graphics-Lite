using Graphics.Settings;
using Graphics.Textures;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class ReflectionProbeInspector
    {
        private static Vector2 probeSettingsScrollView;
        private static int selectedProbe = 0;
        private static bool inspectReflectionProbes = true;

        private static int cachedFontSize = -1;
        private static int paddingL, paddingR;
        private static GUIStyle SmallBox;

        static void UpdateCachedValues(GlobalSettings renderSettings)
        {
            if (cachedFontSize == renderSettings.FontSize) return;

            cachedFontSize = renderSettings.FontSize;

            paddingL = Mathf.RoundToInt(renderSettings.FontSize * 2f);
            paddingR = Mathf.RoundToInt(renderSettings.FontSize * 2.9f);

            SmallBox = new GUIStyle(GUIStyles.tabcontent);
            SmallBox.padding = new RectOffset(paddingL, paddingR, paddingL, 0);
            SmallBox.margin = new RectOffset(0, 0, 0, 5);
        }

        internal static void Draw(LightingSettings lightingSettings, SkyboxManager skyboxManager, GlobalSettings renderSettings, bool showAdvanced)
        {
            UpdateCachedValues(renderSettings);

            probeSettingsScrollView = GUILayout.BeginScrollView(probeSettingsScrollView);
            GUILayout.BeginVertical(SmallBox);
            {
                Label("REFLECTION PROBES:", "", true);
                ReflectionProbe[] rps = skyboxManager.GetReflectinProbes();
                if (0 < rps.Length)
                {
                    if (selectedProbe >= rps.Length)
                        selectedProbe = 0;

                    string[] probeNames = rps.Select(probe => probe.name).ToArray();
                    selectedProbe = GUILayout.SelectionGrid(selectedProbe, probeNames, 3);
                    ReflectionProbe rp = rps[selectedProbe];
                    GUILayout.Space(1);

                    //GUILayout.BeginVertical(SmallBox);

                    GUILayout.Space(10);
                    {
                        Label("Type", rp.mode.ToString());
                        Selection("Refresh Mode", rp.refreshMode, mode => { rp.refreshMode = mode; UpdateProbeNextFrame(rp); });
                        Label("Runtime settings", "");
                        Slider("Importance", rp.importance, 0, 1000, importance => { rp.importance = importance; UpdateProbeNextFrame(rp); });
                        Slider("Intensity", rp.intensity, 0, 10, "N1", intensity => { rp.intensity = intensity; UpdateProbeNextFrame(rp); });
                        Toggle("Box Projection", rp.boxProjection, false, box => { rp.boxProjection = box; UpdateProbeNextFrame(rp); });
                        Text("Blend Distance", rp.blendDistance, "N0", distance => { rp.blendDistance = distance; UpdateProbeNextFrame(rp); });
                        Dimension("Box Size", rp.size, size => { rp.size = size; UpdateProbeNextFrame(rp); });
                        Dimension("Box Offset", rp.center, size => { rp.center = size; UpdateProbeNextFrame(rp); });
                        GUILayout.Space(10);
                        Label("Cubemap capture settings", "");
                        Selection("Resolution", rp.resolution, LightingSettings.ReflectionResolutions, resolution => { rp.resolution = resolution; UpdateProbeNextFrame(rp); });
                        Toggle("HDR", rp.hdr, false, hdr => { rp.hdr = hdr; UpdateProbeNextFrame(rp); });
                        Text("Shadow Distance", rp.shadowDistance, "N0", distance => { rp.shadowDistance = distance; UpdateProbeNextFrame(rp); });
                        Selection("Clear Flags", rp.clearFlags, flag => { rp.clearFlags = flag; UpdateProbeNextFrame(rp); });
                        SelectionMask("Culling Mask", rp.cullingMask, mask => { rp.cullingMask = mask; UpdateProbeNextFrame(rp); });
                        Text("Clipping Planes - Near", rp.nearClipPlane, "N2", plane => { rp.nearClipPlane = plane; UpdateProbeNextFrame(rp); });
                        Text("Clipping Planes - Far", rp.farClipPlane, "N0", plane => { rp.farClipPlane = plane; UpdateProbeNextFrame(rp); });
                        SliderColor("Background", rp.backgroundColor, colour => { rp.backgroundColor = colour; UpdateProbeNextFrame(rp); });
                        Selection("Time Slicing Mode", rp.timeSlicingMode, mode => { rp.timeSlicingMode = mode; UpdateProbeNextFrame(rp); });
                        GUILayout.Space(25);
                    }
                    //GUILayout.EndScrollView();
                    //GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical(SmallBox);
            //GUILayout.BeginVertical(SmallBox);
            //GUILayout.Space(10);
            Label("UPDATE SETTINGS", "", true);
            GUILayout.Space(10);
            Toggle("Realtime Reflection Probes", renderSettings.RealtimeReflectionProbes, false, realtime => renderSettings.RealtimeReflectionProbes = realtime);
            Toggle("Pulse Realtime Reflection Probes", renderSettings.PulseReflectionProbes, false, pulse => renderSettings.PulseReflectionProbes = pulse);
            if (renderSettings.PulseReflectionProbes)
                Slider("Pulse Timing (Secs)", renderSettings.PulseReflectionTimer, .25f, 10f, "N1", prt => { renderSettings.PulseReflectionTimer = prt; });
            GUILayout.Space(30);
            GUILayout.EndVertical();
            //GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private static IEnumerator UpdateProbeNextFrame(ReflectionProbe probe)
        {
            if (probe.refreshMode == ReflectionProbeRefreshMode.OnAwake || probe.refreshMode == ReflectionProbeRefreshMode.ViaScripting)
            {
                yield return null; // Wait 1 frame before rendering the probe
                probe.RenderProbe();
            }

        }
    }
}
