using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public Slider R;
    public Slider G;
    public Slider B;
    public Slider AlphaChannel;
    public RawImage colorPreview;
    public void Refresh()
    {
        Color32 b = new Color32((byte)R.value, (byte)G.value, (byte)B.value, 255);
        colorPreview.color = b;
        Texture2DEditor.penColor = b;
    }
}