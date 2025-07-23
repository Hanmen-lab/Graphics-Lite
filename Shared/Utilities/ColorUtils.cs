using UnityEngine;

public static class ColorUtils
{
    public static Color ColorFromTemperature(float temperature)
    {
        float kelvin = temperature / 100f;

        float red, green, blue;

        if (kelvin <= 66f)
        {
            red = 255f;
            green = kelvin;
            green = 99.4708025861f * Mathf.Log(green) - 161.1195681661f;

            if (kelvin <= 19f)
                blue = 0f;
            else
            {
                blue = kelvin - 10f;
                blue = 138.5177312231f * Mathf.Log(blue) - 305.0447927307f;
            }
        }
        else
        {
            red = kelvin - 60f;
            red = 329.698727446f * Mathf.Pow(red, -0.1332047592f);

            green = kelvin - 60f;
            green = 288.1221695283f * Mathf.Pow(green, -0.0755148492f);

            blue = 255f;
        }

        red = Mathf.Clamp(red, 0f, 255f);
        green = Mathf.Clamp(green, 0f, 255f);
        blue = Mathf.Clamp(blue, 0f, 255f);

        return new Color(red / 255f, green / 255f, blue / 255f);
    }
}