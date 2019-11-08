using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushSettings : MonoBehaviour
{
    public void SetMarkerColour(Color new_color)
    {
        Texture2DEditor.penColor = new_color;
    }
    public void SetMarkerWidth(int new_width)
    {
        Texture2DEditor.penRadius = new_width;
    }
    public void SetMarkerWidth(float new_width)
    {
        SetMarkerWidth((int)new_width);
    }
    public void SetBrushType(int index)
    {
        switch (index)
        {
            case 0:
                {
                    Texture2DEditor.brush = Texture2DEditor.BrushType.Circle;
                    break;
                }
            case 1:
                {
                    Texture2DEditor.brush = Texture2DEditor.BrushType.Square;
                    break;
                }
            case 2:
                {
                    Texture2DEditor.brush = Texture2DEditor.BrushType.Triangle;
                    break;
                }
            case 3:
                {
                    Texture2DEditor.brush = Texture2DEditor.BrushType.Line;
                    break;
                }
        }
    }
}