using UnityEngine;
using UnityEngine.Rendering;

namespace Graphics
{
    /// <summary>
    /// The one deferred shading pass Unity has room for, shared by the effects that bring their own:
    /// HSSSS (its skin lighting is done in that pass) and Dithered Shadows (its point and spot light
    /// shadows are read there). Each says which shader it wants, or none, and the slot is set from all
    /// of them at once, so neither overwrites the other or puts back a stale shader when it stops.
    /// HSSSS goes first: without its pass skin is not lit at all, while dithering only softens shadows.
    /// Dithered Shadows' directional shadows use the screen-space shadows pass, which is not shared.
    /// </summary>
    internal static class DeferredShadingSlot
    {
        private static Shader _hssss, _dither;

        public static Shader Hssss
        {
            get { return _hssss; }
            set
            {
                if (_hssss == value)
                    return;
                _hssss = value;
                Apply();
            }
        }

        public static Shader Dither
        {
            get { return _dither; }
            set
            {
                if (_dither == value)
                    return;
                _dither = value;
                Apply();
            }
        }

        /// <summary>Whether Dithered Shadows wants the pass while HSSSS has it.</summary>
        public static bool DitherDisplaced => _hssss != null && _dither != null;

        private static void Apply()
        {
            Shader shader = _hssss != null ? _hssss : _dither;
            if (shader != null)
            {
                GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.UseCustom);
                GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, shader);
            }
            else
            {
                GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.UseBuiltin);
                GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, null);
            }
        }
    }
}
