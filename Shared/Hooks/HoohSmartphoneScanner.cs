using Graphics.GTAO;
using System.Collections;
using UnityEngine;

namespace Graphics
{
    public class HoohSmartphoneScanner : MonoBehaviour
    {
        private bool running;
        private Coroutine scanCoroutine;

        private void OnEnable()
        {
            running = true;
            scanCoroutine = StartCoroutine(ScanForSmartPhones());
        }

        private IEnumerator ScanForSmartPhones()
        {
            while (running)
            {
                GameObject commonSpace = GameObject.Find("CommonSpace");
                if (commonSpace != null)
                {
                    RecurseScan(commonSpace.transform);
                }

                yield return new WaitForSecondsRealtime(5);
            }
        }

        private void RecurseScan(Transform transform)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.name == "camera_colorable")
                {
                    AddSSSCam(child);
                }
                RecurseScan(child);
            }
        }

        private void AddSSSCam(Transform child)
        {
            Camera cam = child.gameObject.GetComponentInChildren<Camera>();
            if (cam != null && cam.GetComponent<SSS>() == null)
            {
                SSS camSSS = cam.gameObject.AddComponent<SSS>();
                camSSS.enabled = true;
                camSSS.Enabled = true;
                SSSManager.RegisterAdditionalInstance(camSSS);
                Graphics.Instance.Log.LogInfo($"Adding SSS Component to Camera: {cam.name} GO: {cam.gameObject.name}");
            }
            if (cam.gameObject.GetComponent<GroundTruthAmbientOcclusion>() == null)
            {
                GroundTruthAmbientOcclusion gtao = cam.gameObject.AddComponent<GroundTruthAmbientOcclusion>();
                GTAOManager.RegisterAdditionalInstance(gtao);
            }
            if (cam.gameObject.GetComponent<VAO.VAOEffectCommandBuffer>() == null && cam.gameObject.GetComponent<VAO.VAOEffect>() ==  null)
            {
                VAO.VAOEffect vao = cam.gameObject.AddComponent<VAO.VAOEffect>();
                VAO.VAOManager.RegisterAdditionalInstance(vao);
            }
        }
        

        private void OnDisable()
        {
            running = false;
            StopCoroutine(scanCoroutine);
        }

    }
}
