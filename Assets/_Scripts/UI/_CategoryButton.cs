using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class _CategoryButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public _CategoryGroup categoryGroup;
    [HideInInspector] public GameObject child;
    [HideInInspector] public GameObject child2;

    public void OnPointerClick(PointerEventData eventData)
    {
        categoryGroup.OnCategorySelected(this);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        categoryGroup.OnCategoryEnter(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        categoryGroup.OnCategoryExit(this);
    }
    private void Awake()
    {
        child = transform.GetChild(0).gameObject;
        child2 = transform.GetChild(1).gameObject;
    }
    public void Subscribe()
    {
        categoryGroup.Subscribe(this);
    }
}
