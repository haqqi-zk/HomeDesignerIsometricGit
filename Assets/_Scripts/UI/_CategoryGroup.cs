using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _CategoryGroup : MonoBehaviour 
{
    [HideInInspector] public List<_CategoryButton> categoryButtons;
    [HideInInspector] public _CategoryButton selectedCategory;
    public _CategoryButton ArmChair;

    #region HIGHLIGHT CATEGORY
    public void Subscribe(_CategoryButton button)
    {
        if (categoryButtons == null)
        {
            categoryButtons = new List<_CategoryButton>();
        }
        categoryButtons.Add(button);
    }
    public void OnCategoryEnter(_CategoryButton button)
    {
        ResetCategory();
        if (selectedCategory == null || button != selectedCategory)
        {
            button.child2.SetActive(true);
        }
    }
    public void OnCategoryExit(_CategoryButton button)
    {
        ResetCategory();
    }
    public void OnCategorySelected(_CategoryButton button)
    {
        selectedCategory = button;
        ResetCategory();
        button.child.SetActive(true);
        button.child2.SetActive(false);
    }
    public void StartingCategory()
    {
        selectedCategory = ArmChair;
        ArmChair.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void ResetCategory()
    {
        foreach (_CategoryButton button in categoryButtons)
        {
            if (selectedCategory != null && button == selectedCategory) continue;
            button.child.SetActive(false);
            button.child2.SetActive(false);
        }
    }
    #endregion HIGHLIGHTCATEGORY
}
