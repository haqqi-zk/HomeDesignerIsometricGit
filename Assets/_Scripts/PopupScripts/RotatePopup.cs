using Exoa.Designer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotatePopup : BaseFloatingPopup
{
    public static RotatePopup Instance;
    public Button ClockwiseButton;
    public Button CounterClockwiseButton;

    private GameObject currentObject;

    override protected void Awake()
    {
        Instance = this;

        base.Awake();
    }
    private void Start()
    {
        ClockwiseButton?.onClick.AddListener(RotateClockwise);
        CounterClockwiseButton?.onClick.AddListener(RotateCounterClockwise);
    }
    public void ShowMode(GameObject target)
    {
        _BackgroundColorController.Instance.content.SetActive(false);
        currentObject = target;
        Show();
        MovePopup();
    }
    private void RotateClockwise()
    {
        Vector3 rotationToAdd = new Vector3(0, 45, 0);
        currentObject.transform.Rotate(rotationToAdd);
    }
    private void RotateCounterClockwise()
    {
        Vector3 rotationToAdd = new Vector3(0, -45, 0);
        currentObject.transform.Rotate(rotationToAdd);
    }
}
