using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestColor : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Material material;

    public bool setColor = false;

    private void Start()
    {
        material = meshRenderer.material;
    }
    private void Update()
    {
        Debug.Log(material.color.ToString());
    }
}
