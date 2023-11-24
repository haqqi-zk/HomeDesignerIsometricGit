using Exoa.Designer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupColor : BaseFloatingPopup
{
    public static PopupColor Instance;

    public ColorPicker colorPicker;
    public _ColorController colorController;

    private GameObject currentObject;
    public Button applyToAllWalls;
    public List<GameObject> allWalls;
    public List<GameObject> allFloors;
    override protected void Awake()
    {
        Instance = this;

        base.Awake();
    }
    private void Start()
    {
        colorPicker?.colorPickerPublicEvent.AddListener(ChangeColor);
        applyToAllWalls?.onClick.AddListener(ApplyToAllWalls);
    }
    public void ShowMode(GameObject target, bool isWall = false)
    {
        _BackgroundColorController.Instance.content.SetActive(false);
        applyToAllWalls.gameObject.SetActive(isWall);
        currentObject = target;
        colorController = currentObject.GetComponent<_ColorController>();
        if(colorController != null)
        {
            Show();
            MovePopup();
        }
    }
    private void ChangeColor()
    {
        colorController.SetColor(colorPicker.ColorPickerResult);
    }
    public void SetWallFloorDefaultColor()
    {
        Color defaultColor = new Color(0.486f, 0.798f, 1, 1);
        if(allWalls != null)
        {
            foreach(GameObject wall in allWalls)
            {
                _ColorController wallColor = wall.GetComponent<_ColorController>();
                wallColor.SetColor(defaultColor);
            }
        }
        defaultColor = new Color(1, 1, 1, 1);
        if (allFloors != null)
        {
            foreach(GameObject floor in allFloors)
            {
                _ColorController floorColor = floor.GetComponent<_ColorController>();
                floorColor.SetColor(defaultColor);
            }
        }
    }
    public void ApplyToAllWalls()
    {
        Color colorToSet = currentObject.GetComponent<_ColorController>().GetColor();
        if(allWalls != null)
        {
            foreach (GameObject wall in allWalls)
            {
                _ColorController wallColor = wall.GetComponent<_ColorController>();
                wallColor.SetColor(colorToSet);
            }
        }
    }
    public List<string> GetStringData(string type)
    {
        List<string> colors = new List<string>();
        if(type == "Wall")
        {
            foreach(GameObject wall in allWalls)
            {
                _ColorController colorController = wall.GetComponent<_ColorController>();
                string color = ColorToString(colorController.GetColor());
                colors.Add(color);
            }
        }
        else if(type == "Floor")
        {
            foreach (GameObject floor in allFloors)
            {
                _ColorController colorController = floor.GetComponent<_ColorController>();
                string color = ColorToString(colorController.GetColor());
                colors.Add(color);
            }
        }
        return colors;
    }
    public void SetBaseSceneColor(List<string> walls, List<string> floors)
    {
        Color errorColor = new Color(0, 0, 0, 0);
        if(walls != null)
        {
            int i = 0;
            foreach(string wall in walls)
            {
                Color newColor = StringToColor(wall);
                if(newColor != errorColor)
                {
                    allWalls[i].GetComponent<_ColorController>().SetColor(newColor);
                }
                i++;
            }

            i = 0;
            foreach(string floor in floors)
            {
                Color newColor = StringToColor(floor);
                if(newColor != errorColor)
                {
                    allFloors[i].GetComponent<_ColorController>().SetColor(newColor);
                }
                i++;
            }
        }

    }
    public static string ColorToString(Color color)
    {
        return color.r + "," + color.g + "," + color.b + "," + color.a;
    }
    public static Color StringToColor(string colorString)
    {
        try
        {
            string[] colors = colorString.Split(',');
            return new Color(float.Parse(colors[0]), float.Parse(colors[1]), float.Parse(colors[2]), float.Parse(colors[3]));
        }
        catch
        {
            return Color.white;
        }
    }
}
