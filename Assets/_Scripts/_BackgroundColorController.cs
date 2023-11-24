using Exoa.Designer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _BackgroundColorController : MonoBehaviour
{
    public static _BackgroundColorController Instance;

    [HideInInspector]
    public Camera mainCamera;
    public ColorPicker colorPicker;
    public string defaultColor;
    public Button defaultButton;
    public GameObject content;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        mainCamera = GameObject.FindObjectOfType<Camera>();
        colorPicker?.colorPickerPublicEvent.AddListener(ChangeBackgroundColor);
        defaultButton?.onClick.AddListener(SetBackgroundColorToDefault);
    }
    private void Update()
    {
        
    }
    public void HideOtherPopups()
    {
        InfoPopup.Instance.Hide();
        RotatePopup.Instance.Hide();
        PopupColor.Instance.Hide();
    }
    public void SetBackgroundColorToDefault()
    {
        ChangeColor(PopupColor.StringToColor(defaultColor));
    }
    private void ChangeBackgroundColor()
    {
        ChangeColor(colorPicker.ColorPickerResult);
    }
    public void ChangeColor(Color color)
    {
        mainCamera.backgroundColor = color;
    }
    public string GetBackgroundColorData()
    {
        return PopupColor.ColorToString(mainCamera.backgroundColor);
    }
    public void SetBackgroundColorData(string colorData)
    {
        ChangeColor(PopupColor.StringToColor(colorData));
    }
}
