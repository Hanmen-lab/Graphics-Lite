using Graphics;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
public class SSS_convolution : MonoBehaviour
{
    private float initFOV;
    [HideInInspector] public bool AllowMSAA;
    [HideInInspector][Range(0, 1)] public float BlurRadius = 1;
    [HideInInspector] public Shader BlurShader = null;
    private Camera ThisCamera;
    [HideInInspector] public RenderTextureFormat rtFormat;
    [HideInInspector] public BlurMaterials BlurMaterial = null;
    [HideInInspector][Range(0, 10)] public int iterations = 3;
    [HideInInspector] public RenderTexture blurred;

    private void OnEnable()
    {
        ThisCamera = gameObject.GetComponent<Camera>();
        Camera parentCamera;
        try
        {
            parentCamera = transform.parent.GetComponent<Camera>();
        }
        catch
        {
            parentCamera = FindObjectOfType<SSS>().GetComponent<Camera>();
        }
        initFOV = parentCamera.fieldOfView;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float Pitagoras(int x, int y)
    {
        return Mathf.Sqrt(x * x + y * y);
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (iterations > 0)
        {
            const float Reciprocal_Pitagoras_1920_1080 = 0.0004539455922523694f;

            int rtW = source.width;
            int rtH = source.height;
            BlurRadius *= initFOV / ThisCamera.fieldOfView;
            BlurRadius *= Pitagoras(rtW, rtH) * Reciprocal_Pitagoras_1920_1080;
            Vector4 offset1 = new Vector4(0, BlurRadius, 0, 0);
            Vector4 offset2 = new Vector4(BlurRadius, 0, 0, 0);
            int AA = QualitySettings.antiAliasing;
            if (!ThisCamera.allowMSAA || AA <= 0 || !AllowMSAA)
            {
                AA = 1;
            }
            RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0, rtFormat, RenderTextureReadWrite.Linear, AA);
            BlurMaterial.SetVector(Shader.PropertyToID("_TexelOffsetScale"), offset1, offset2);
            UnityEngine.Graphics.Blit(source, buffer1, BlurMaterial[0]);
            int i = iterations;
            if (--i != 0)
            {
                RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0, rtFormat, RenderTextureReadWrite.Linear, AA);
                do
                {
                    UnityEngine.Graphics.Blit(buffer1, buffer2, BlurMaterial[1]);
                    UnityEngine.Graphics.Blit(buffer2, buffer1, BlurMaterial[0]);
                } while (--i != 0);
                RenderTexture.ReleaseTemporary(buffer2);
            }
            UnityEngine.Graphics.Blit(buffer1, blurred, BlurMaterial[1]);
            RenderTexture.ReleaseTemporary(buffer1);
        }
        else
        {
            UnityEngine.Graphics.Blit(source, blurred);
        }
        UnityEngine.Graphics.Blit(source, destination);
    }

    public class BlurMaterials
    {
        private Material[] materials = new Material[2];

        public Material this[int i] => materials[i];

        public HideFlags hideFlags
        {
            set
            {
                materials[0].hideFlags = value;
                materials[1].hideFlags = value;
            }
        }

        public BlurMaterials(Shader shader)
        {
            materials[0] = new Material(shader);
            materials[1] = new Material(shader);
        }

        public void SetFloat(int nameID, float value)
        {
            materials[0].SetFloat(nameID, value);
            materials[1].SetFloat(nameID, value);
        }

        public void SetInt(int nameID, int value)
        {
            materials[0].SetInt(nameID, value);
            materials[1].SetInt(nameID, value);
        }

        public void SetColor(int nameID, Color value)
        {
            materials[0].SetColor(nameID, value);
            materials[1].SetColor(nameID, value);
        }

        public void SetVector(int nameID, Vector4 value1, Vector4 value2)
        {
            materials[0].SetVector(nameID, value1);
            materials[1].SetVector(nameID, value2);
        }

        public void SetTexture(int nameID, Texture value)
        {
            materials[0].SetTexture(nameID, value);
            materials[1].SetTexture(nameID, value);
        }

        public void EnableKeyword(string keyword, bool enable)
        {
            if (enable)
            {
                materials[0].EnableKeyword(keyword);
                materials[1].EnableKeyword(keyword);
            }
            else
            {
                materials[0].DisableKeyword(keyword);
                materials[1].DisableKeyword(keyword);
            }
        }
    }
}
