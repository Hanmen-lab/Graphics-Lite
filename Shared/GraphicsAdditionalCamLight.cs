using Studio;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics
{
    public class GraphicsAdditionalCamLight : MonoBehaviour
    {
        public Camera Camera { get; set; }
        public Light Light { get; set; }
        public OCILight OCILight { get; set; }
        public Vector3 OriginalPosition { get; set; }
        public Quaternion OriginalRotation { get; set; }

        private void LateUpdate()
        {
            if (Light && OCILight != null && Camera)
            {
                Light.transform.rotation = Quaternion.Euler(Camera.transform.rotation.eulerAngles - OCILight.guideObject.changeAmount.rot);
                if (Light.type == LightType.Spot)
                    Light.transform.position = Camera.transform.position + OCILight.guideObject.changeAmount.pos;
            }
            else if (Light && Camera && OCILight == null)
            {
                Light.transform.rotation = Quaternion.Euler(Camera.transform.rotation.eulerAngles - OriginalRotation.eulerAngles);
                Light.transform.position = Camera.transform.position + OriginalPosition;
            }
        }

        private void OnEnable()
        {
            if (gameObject)
            {
                OriginalPosition = gameObject.transform.position;
                OriginalRotation = gameObject.transform.rotation;
            }
        }

        private void OnDisable()
        {
            if (gameObject)
            {
                gameObject.transform.position = OriginalPosition;
                gameObject.transform.rotation = OriginalRotation;
            }
        }

    }
}
