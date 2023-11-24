using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotDelegate : MonoBehaviour {

    void setSprite(Texture2D tex)
    {
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        this.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    private void OnEnable()
    {
        GameObject.Find("UniShare-GameObject").GetComponent<UniShare>().OnScreenShotReady += setSprite;
    }


    private void OnDisable()
    {
        GameObject.Find("UniShare-GameObject").GetComponent<UniShare>().OnScreenShotReady -= setSprite;
    }

    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
