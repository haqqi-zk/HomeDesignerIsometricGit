using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class _ColorController : MonoBehaviour
{
    public MeshRenderer[] meshRenderer;
    [HideInInspector]
    public List<Material> materials = new List<Material>();
    public void SetColor(Color color)
    {
        if(meshRenderer.Length > 1)
        {
            for(int i = 0; i < meshRenderer.Length; i++)
            {
                Material newMaterial = meshRenderer[i].material;
                newMaterial.color = color;
            }
        }
        else if(meshRenderer.Length == 1)
        {
            Material newMaterial = meshRenderer[0].material;
            newMaterial.color = color;
        }
        else
        {
            return;
        }
    }
    public Color GetColor()
    {
        Material newMaterial = meshRenderer[0].material;
        return newMaterial.color;
    }
}
