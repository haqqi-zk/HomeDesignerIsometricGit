using Exoa.Designer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupHeight : BaseFloatingPopup
{
    public static PopupHeight Instance;

    private GameObject currentObject;

    public Slider heightSlider;
    override protected void Awake()
    {
        Instance = this;

        base.Awake();
    }
    private void Start()
    {
        heightSlider?.onValueChanged.AddListener(delegate { ChangeHeight(); });
    }
    public void ShowMode(GameObject target)
    {
        currentObject = target;
        heightSlider.value = currentObject.transform.position.y;
        Show();
        MovePopup();
    }
    private void ChangeHeight()
    {
        currentObject.transform.position = new Vector3(currentObject.transform.position.x, heightSlider.value, currentObject.transform.position.z);
    }
}
