using LuxWater;
using UnityEngine;


namespace Graphics
{
    public class ConnectSunToUnderwater : MonoBehaviour
    {

        public Transform Sun;
        LuxWater_UnderWaterRendering underwater;

        void Start()
        {
            Connect();
        }

        void OnEnable()
        {
            Connect();
        }

        public void Connect()
        {
            underwater = GetComponent<LuxWater_UnderWaterRendering>();
            underwater.Sun = RenderSettings.sun.transform;
            underwater.SunLight = RenderSettings.sun;
        }

        internal static void ConnectSun()
        {
            if (null == RenderSettings.sun)
                return;

            var underwater = UnityEngine.Object.FindObjectOfType<LuxWater.LuxWater_UnderWaterRendering>();
            if (null == underwater)
                return;

            underwater.Sun = RenderSettings.sun.transform;
            underwater.SunLight = RenderSettings.sun;
        }
    }
}

