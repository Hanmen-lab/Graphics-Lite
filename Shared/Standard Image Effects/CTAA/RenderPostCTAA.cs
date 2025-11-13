//LIVENDA CTAA CINEMATIC TEMPORAL ANTI ALIASING
//Copyright Livenda Labs 2019
//CTAA-NXT V2.0
//Original Author: Livenda Labs
////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    public class RenderPostCTAA : MonoBehaviour
    {
        public CTAA_PC ctaaPC;
        public Transform ctaaCamTransform;
        //Masking feature. Seems useless
        //public Camera MaskRenderCam;    
        //public RenderTexture maskTexRT;
        //public Shader maskRenderShader;
        //public Camera MaskRenderCam;    
        //public bool layerMaskingEnabled;
        //public Material layerPostMat;    

        void LateUpdate()
        {
            this.transform.position = ctaaCamTransform.position;
            this.transform.rotation = ctaaCamTransform.rotation;
            //Masking feature. Seems useless
            //MaskRenderCam.transform.position = ctaaCamTransform.position;
            //MaskRenderCam.transform.rotation = ctaaCamTransform.rotation;
        }

        //Masking feature. Seems useless
        /*private void OnDisable()
        {
            if (maskTexRT != null) DestroyImmediate(maskTexRT); maskTexRT = null;
            if(MaskRenderCam !=null) MaskRenderCam.targetTexture = null;
        }*/
        /*void OnPreCull()
        {
            ctaaPC.MainCamera.targetTexture = ctaaPC.getCTAA_Render();
        }*/

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //Masking feature. Seems useless
            /* if (((maskTexRT == null) || (maskTexRT.width != source.width)) || (maskTexRT.height != source.height))
             {
                 DestroyImmediate(maskTexRT);
                 maskTexRT = new RenderTexture(source.width, source.height, 16, source.format);
                 maskTexRT.hideFlags = HideFlags.HideAndDontSave;
                 maskTexRT.filterMode = FilterMode.Bilinear;
                 maskTexRT.wrapMode = TextureWrapMode.Repeat;
                 MaskRenderCam.targetTexture = maskTexRT;
             }
             if (layerMaskingEnabled)
                 MaskRenderCam.RenderWithShader(maskRenderShader, "");*/
            RenderTexture tmprt = ctaaPC.GetCTAA_Render();

            if (tmprt != null)
            {
                //Masking feature. Seems useless
                /*layerPostMat.SetTexture("_CTAA_RENDER", tmprt);
                layerPostMat.SetTexture("_maskTexRT", maskTexRT);

                if(layerMaskingEnabled)
                    Graphics.Blit(source, destination, layerPostMat);
                else*/
                UnityEngine.Graphics.Blit(tmprt, destination);
            }
            else
            {
                UnityEngine.Graphics.Blit(source, destination);
            }
        }
    }
}
