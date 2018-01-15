using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ColorHelper
{
    public static Color FindComplementaryColor(Color startColor)
    {
        float h = 0;
        float s = 0;
        float v = 0;
        Color.RGBToHSV(startColor, out h, out s, out v);
        h = (h + 0.5f) % 1;

        Color complementaryColor = Color.HSVToRGB(h, s, v);

        return complementaryColor;
    }

    public static void SetTextColor(Text textComponent, Color environmentColor, bool changeShadowColor = false)
    {
        textComponent.color = FindComplementaryColor(environmentColor);

        if (changeShadowColor)
        {
            Shadow shadowComponent = textComponent.GetComponent<Shadow>();

            if (shadowComponent != null)
            {
                shadowComponent.effectColor = environmentColor;
            }
        }
    }
}
