using UnityEngine;

namespace Graphics.VolumetricLights
{
    /// <summary>
    /// A stand-in for the noise the volumetric lights drift through when a light's Noise is on, used
    /// only if FogWindNoise cannot be read from the embedded height-fog bundle: a tileable 3D fractal
    /// noise made once for the session (the same value in every channel, wrapping on every axis).
    /// </summary>
    internal static class NoiseVolume
    {
        private const int Size = 32;
        private static Texture3D _texture;

        public static Texture3D Get()
        {
            if (_texture != null)
                return _texture;

            float[] lattice = new float[Size * Size * Size];
            System.Random random = new System.Random(1337);
            for (int i = 0; i < lattice.Length; ++i)
                lattice[i] = (float)random.NextDouble();

            Color32[] pixels = new Color32[Size * Size * Size];
            for (int z = 0; z < Size; ++z)
                for (int y = 0; y < Size; ++y)
                    for (int x = 0; x < Size; ++x)
                    {
                        float v = 0f, amplitude = 0.5f, total = 0f;
                        int period = 8;
                        for (int octave = 0; octave < 3; ++octave)
                        {
                            float scale = (float)period / Size;
                            v += Sample(lattice, x * scale, y * scale, z * scale, period) * amplitude;
                            total += amplitude;
                            amplitude *= 0.5f;
                            period *= 2;
                        }
                        byte b = (byte)Mathf.RoundToInt(Mathf.Clamp01(v / total) * 255f);
                        pixels[(z * Size + y) * Size + x] = new Color32(b, b, b, b);
                    }

            _texture = new Texture3D(Size, Size, Size, TextureFormat.RGBA32, false)
            {
                name = "VolumetricNoise",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };
            _texture.SetPixels32(pixels);
            _texture.Apply(false, true);
            return _texture;
        }

        /// <summary>Smooth value noise on a lattice that wraps every <paramref name="period"/> cells.</summary>
        private static float Sample(float[] lattice, float x, float y, float z, int period)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y), z0 = Mathf.FloorToInt(z);
            float fx = Smooth(x - x0), fy = Smooth(y - y0), fz = Smooth(z - z0);
            float c000 = At(lattice, x0, y0, z0, period), c100 = At(lattice, x0 + 1, y0, z0, period);
            float c010 = At(lattice, x0, y0 + 1, z0, period), c110 = At(lattice, x0 + 1, y0 + 1, z0, period);
            float c001 = At(lattice, x0, y0, z0 + 1, period), c101 = At(lattice, x0 + 1, y0, z0 + 1, period);
            float c011 = At(lattice, x0, y0 + 1, z0 + 1, period), c111 = At(lattice, x0 + 1, y0 + 1, z0 + 1, period);
            float a = Mathf.Lerp(Mathf.Lerp(c000, c100, fx), Mathf.Lerp(c010, c110, fx), fy);
            float b = Mathf.Lerp(Mathf.Lerp(c001, c101, fx), Mathf.Lerp(c011, c111, fx), fy);
            return Mathf.Lerp(a, b, fz);
        }

        private static float At(float[] lattice, int x, int y, int z, int period)
        {
            x = ((x % period) + period) % period;
            y = ((y % period) + period) % period;
            z = ((z % period) + period) % period;
            return lattice[(z * Size + y) * Size + x];
        }

        private static float Smooth(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}
