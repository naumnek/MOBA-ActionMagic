using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorTypeConverter
{
    public static string ToRGBHex(Color c)
    {
        return string.Format("<#{0:X2}{1:X2}{2:X2}>", ToByte(c.r), ToByte(c.g), ToByte(c.b));
    }
 
    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
}
