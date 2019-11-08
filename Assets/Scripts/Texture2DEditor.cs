using System;
using System.Collections.Generic;

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Runtime.InteropServices;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Texture2DEditor : MonoBehaviour
{
    /// <summary>
    /// New Version
    /// </summary>

    public static int penRadius = 3;
    public static Texture2DEditor drawable;
    public static Color penColor;

    public enum BrushType
    {
        Circle,
        Square,
        Triangle,
        Line
    }
    public static BrushType brush;

    int numberPoints = (int)Math.Pow((double)penRadius,(double)3);

    public LayerMask Drawing_Layers;
    public LayerMask UI_Layer;
    public Color resetColor = new Color(1, 1, 1, 1);

    public Brush_Function currBrush;
    public bool resetCanvas = true;
    public delegate void Brush_Function(Vector2 world_position);

    Color transparent;
    Color[] cleanColors;
    Color32[] curColors;

    Vector2 prevDrag;
    Sprite drawbleSprite;
    Texture2D drawbleTextr;

    bool currDrawSt = false;
    bool prevMouseHold = false;

    public UIController UIcontroller;

    string mobileDest; 
    string pcDest; 

    public void SetPenBrush()
    {
        currBrush = PenBrush;
    }
    public void PenBrush(Vector2 world_point)
    {
        Vector2 pixel_pos = WorldToPixelCoordinates(world_point);

        curColors = drawbleTextr.GetPixels32();

        if (prevDrag == Vector2.zero)
        {
            MarkPixelsToColor(pixel_pos);
        }
        else
        {
            ColorBetween(prevDrag, pixel_pos);
        }
        ApplyMarkedPixelChanges();
        prevDrag = pixel_pos;
    }
    void Awake()
    {
        brush = BrushType.Square;
        drawable = this;
        currBrush = PenBrush;

        drawbleSprite = this.GetComponent<SpriteRenderer>().sprite;
        drawbleTextr = drawbleSprite.texture;

        cleanColors = new Color[(int)drawbleSprite.rect.width * (int)drawbleSprite.rect.height];
        for (int x = 0; x < cleanColors.Length; x++)
            cleanColors[x] = resetColor;
        
        if (resetCanvas)
            ResetCanvas();
    }
    void Start()
    {
#if UNITY_ANDROID
        mobileDest = Application.persistentDataPath + "/MyFolder/";
#elif UNITY_EDITOR
        pcDest = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/MyFolder/";
#elif UNITY_IOS
        mobileDest = Application.persistentDataPath + "/MyFolder/";
#elif UNITY_STANDALONE
        pcDest = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/MyFolder/";
#endif

        if (mobileDest != null && !Directory.Exists(mobileDest))
        {
            Directory.CreateDirectory(mobileDest);
        }
        if (pcDest != null && !Directory.Exists(pcDest))
        {
            Directory.CreateDirectory(pcDest);
        }
    }
    void Update()
    {
#if UNITY_ANDROID
        ScreenTouch();
#else
        MouseTouch();
#endif
    }

    void MouseTouch()
    {
        bool buttonHold = Input.GetMouseButton(0);

        if (buttonHold && !currDrawSt)
        {
            Vector2 mouseWP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mouseWP, Drawing_Layers.value);
            if (!EventSystem.current.IsPointerOverGameObject() && hit != null && hit.transform != null)
                currBrush(mouseWP);
            else
            {
                prevDrag = Vector2.zero;
                if (!prevMouseHold)
                    currDrawSt = true;
            }
        }
        else if (!buttonHold)
        {
            prevDrag = Vector2.zero;
            currDrawSt = false;
        }
        prevMouseHold = buttonHold;
    }
    void ScreenTouch()
    {
        if (Input.touchCount > 0)
        {
            bool buttonHold = Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(0).phase == TouchPhase.Moved;

            if (buttonHold && !currDrawSt)
            {
                Vector2 touchWP = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                Collider2D hit = Physics2D.OverlapPoint(touchWP, Drawing_Layers.value);
                if (hit != null && hit.transform != null)
                {
                    currBrush(touchWP);
                }
                else
                {
                    prevDrag = Vector2.zero;
                    if (!prevMouseHold)
                        currDrawSt = true;
                }
            }
            else if (!buttonHold)
            {
                prevDrag = Vector2.zero;
                currDrawSt = false;
            }
            prevMouseHold = buttonHold;
        }
    }
    bool CheckUIObjectsInPosition(Vector2 position)
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = position;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0)
        {
            foreach (var go in raycastResults)
            {
                if(go.gameObject.layer == UI_Layer.value)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }


    //Brush Types Algorithms/Calculations
    bool EdgeCheck(int x)
    {
        if (x >= (int)drawbleSprite.rect.width || x < 0) return false;
        return true;
    }
    void BrushType_Circle(int centX, int centY)
    {
        for (int y = -penRadius; y <= penRadius; y++)
            for (int x = -penRadius; x <= penRadius; x++)
                if (x * x + y * y <= penRadius * penRadius)
                    MarkPixelToChange(centX + x, centY + y);
    }
    void BrushType_Square(int centX, int centY)
    {
        for (int x = centX - penRadius; x <= centX + penRadius; x++)
        {
            if (EdgeCheck(x))
            {
                for (int y = centY - penRadius; y <= centY + penRadius; y++)
                {
                    MarkPixelToChange(x, y);
                }
            }
        }
    }
    void BrushType_Line(int centX, int centY)
    {
        for (int x = centX - penRadius,y=centY; x <= centX + penRadius; x++)
        {
            if(EdgeCheck(x))
            MarkPixelToChange(x,y);
        }
    }
    void BrushType_Triangle(int centX, int centY)
    {
        for (int x = centX - penRadius, y = centY; x <= centX + penRadius; x++)
        {
            if (x >= (int)drawbleSprite.rect.width || x < 0)
                continue;

            if (x < centX && x > centX - penRadius) y += 2;
            if (x > centX) y -= 2;
            MarkPixelToChange(x, y);
        }
    }
    void BrushType_SidedTriangle(int centX, int centY)
    {
        int stepValue = 0;
        for (int x = centX - penRadius; x <= centX + penRadius; x++)
        {
            if(EdgeCheck(x))
            for (int y = centY - stepValue; y <= centY + stepValue; y++)
            {
                MarkPixelToChange(x, y);
            }
            stepValue++;
        }
    }
    void BrushType_Rhombus(int centX, int centY)
    {
        int stepValue = 0;
        for (int x = centX - penRadius; x <= centX + penRadius; x++)
        {
            if (x >= (int)drawbleSprite.rect.width || x < 0)
                continue;

            for (int y = centY - stepValue; y <= centY + stepValue; y++)
            {
                MarkPixelToChange(x, y);
            }
            if(x < centX)
            {
                stepValue++;
            }
            else
            {
                stepValue--;
            }
        }
    }

    public Vector2 WorldToPixelCoordinates(Vector2 world_position)
    {
        Vector2 local_pos = transform.InverseTransformPoint(world_position);

        float pixelWidth = drawbleSprite.rect.width;
        float pixelHeight = drawbleSprite.rect.height;
        float unitsToPixels = pixelWidth / drawbleSprite.bounds.size.x * transform.localScale.x;

        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

        return pixel_pos;
    }
    public void ColorBetween(Vector2 p1, Vector2 p2)
    {
        float distance = Vector2.Distance(p1, p2);
        Vector2 direction = (p1 - p2).normalized;

        Vector2 cur_position = p1;

        float lerp_step = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_step)
        {
            cur_position = Vector2.Lerp(p1, p2, lerp);
            MarkPixelsToColor(cur_position);
        }
    }
    public void MarkPixelsToColor(Vector2 clickedPxl)
    {
        int x = (int)clickedPxl.x;
        int y = (int)clickedPxl.y;

        switch (brush)
        {
            case BrushType.Circle:
                {
                    //DrawQuadCurve(clickedPxl);
                    //circleBres(clickedPxl);
                    BrushType_Circle(x, y);
                    break;
                }
            case BrushType.Square:
                {
                    BrushType_Square(x, y);
                    break;
                }
            case BrushType.Triangle:
                {
                    BrushType_SidedTriangle(x, y);
                    break;
                }
            case BrushType.Line:
                {
                    BrushType_Line(x, y);
                    break;
                }
        }
    }
    public void MarkPixelToChange(int x, int y)
    {
        // x and y coordinates to array coordinates
        int arrPos = y * (int)drawbleSprite.rect.width + x;
        if (arrPos <= curColors.Length && arrPos > 0)
        {
            curColors[arrPos] = penColor;
        }
    }
    public void ApplyMarkedPixelChanges()
    {
        drawbleTextr.SetPixels32(curColors);
        drawbleTextr.Apply();
    }
    public void ResetCanvas()
    {
        drawbleTextr.SetPixels(cleanColors);
        drawbleTextr.Apply();
    }
    public void Save()
    {
        var TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var myFilename = "Image-" + TimeStamp + ".png";

#if UNITY_ANDROID
        SaveOn(mobileDest, myFilename);
#elif UNITY_EDITOR
        SaveOn(pcDest, myFilename);
#elif UNITY_IOS
        SaveOn(mobileDest, myFilename);
#elif UNITY_STANDALONE
        SaveOn(pcDest, myFilename);
#endif
    }
    public void Quit()
    {
        Application.Quit();
    }
    //public void SaveOnAndroid(string myFilename)
    //{
    //    var bytes = drawbleTextr.EncodeToPNG();
        
    //    string myFolderLocation = "/storage/emulated/0/DCIM/MyFolder/";
        
    //    if (!Directory.Exists(myFolderLocation))
    //    {
    //        Directory.CreateDirectory(myFolderLocation);
    //    }

    //    string myScreenshotLocation = myFolderLocation + myFilename;

    //    File.WriteAllBytes(myDefaultLocation, bytes);
    //    File.Move(myDefaultLocation, myScreenshotLocation);

    //    if (File.Exists(myDefaultLocation)) UIcontroller.SaveSuccessful(myDefaultLocation);
    //    else if(File.Exists(myScreenshotLocation)) UIcontroller.SaveSuccessful(myScreenshotLocation);
    //    else UIcontroller.SaveFailed();
    //}

    public void SaveOn(string machine, string myFilename)
    {
        var bytes = drawbleTextr.EncodeToPNG();
        var final = machine + myFilename;
        File.WriteAllBytes(final, bytes);

        if (File.Exists(final)) UIcontroller.SaveSuccessful(final);
        else UIcontroller.SaveFailed();
    }
}