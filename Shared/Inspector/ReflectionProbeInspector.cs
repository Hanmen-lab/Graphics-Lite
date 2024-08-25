using Graphics.Settings;
using Graphics.Textures;
using System.Linq;
using UnityEngine;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class ReflectionProbeInspector
    {
        private static Vector2 probeSettingsScrollView;
        private static int selectedProbe = 0;
        //private static bool inspectReflectionProbes = true;

        internal static void Draw(LightingSettings lightingSettings, SkyboxManager skyboxManager, LightManager lightmanager, bool showAdvanced)
        {
            GUIStyle SmallBox = new GUIStyle(GUIStyles.tabcontent);
            SmallBox.padding = new RectOffset(0,0, 0, 0);
            SmallBox.margin = new RectOffset(0, 0, 0, 0);
            SmallBox.normal.background = null;

            GUILayout.BeginVertical(GUIStyles.tabcontent);
            {
                ReflectionProbe[] rps = skyboxManager.GetReflectinProbes();
                if (0 < rps.Length)
                {
                    if (selectedProbe >= rps.Length)
                        selectedProbe = 0;

                    string[] probeNames = rps.Select(probe => probe.name).ToArray();
                    selectedProbe = GUILayout.SelectionGrid(selectedProbe, probeNames, 3, GUIStyles.toolbarbutton);
                    ReflectionProbe rp = rps[selectedProbe];
                    GUILayout.Space(1);
                    probeSettingsScrollView = GUILayout.BeginScrollView(probeSettingsScrollView);
                    GUILayout.BeginVertical(SmallBox);

                    GUILayout.Space(10);
                    {
                        Label("Type", rp.mode.ToString());
                        Label("Runtime settings", "");
                        Slider("Importance", rp.importance, 0, 1000, importance => rp.importance = importance);
                        Slider("Intensity", rp.intensity, 0, 10, "N2", intensity => rp.intensity = intensity);
                        Toggle("Box Projection", rp.boxProjection, false, box => rp.boxProjection = box);
                        Text("Blend Distance", rp.blendDistance, "N2", distance => rp.blendDistance = distance);
                        Dimension("Box Size", rp.size, size => rp.size = size);
                        Dimension("Box Offset", rp.center, size => rp.center = size);
                        GUILayout.Space(10);
                        Label("Cubemap capture settings", "");
                        Selection("Resolution", rp.resolution, LightingSettings.ReflectionResolutions, resolution => rp.resolution = resolution);
                        Toggle("HDR", rp.hdr, false, hdr => rp.hdr = hdr);
                        Text("Shadow Distance", rp.shadowDistance, "N2", distance => rp.shadowDistance = distance);
                        Selection("Clear Flags", rp.clearFlags, flag => rp.clearFlags = flag);
                        if (showAdvanced)
                        {
                            SelectionMask("Culling Mask", rp.cullingMask, mask => rp.cullingMask = mask);
                        }
                        Text("Clipping Planes - Near", rp.nearClipPlane, "N2", plane => rp.nearClipPlane = plane);
                        Text("Clipping Planes - Far", rp.farClipPlane, "N2", plane => rp.farClipPlane = plane);
                        SliderColor("Background", rp.backgroundColor, colour => { rp.backgroundColor = colour; });
                        Selection("Time Slicing Mode", rp.timeSlicingMode, mode => rp.timeSlicingMode = mode);
                        GUILayout.Space(25);
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }

            }
            GUILayout.EndVertical();
        }
    }
}
