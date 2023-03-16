using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class POVGenerator : MonoBehaviour
{
    public GameObject eye;
    public GameObject canvas;
    public Sprite testSprite;

    public float xPOVLocation;
    public float yPOVLocation;

    public float xPOVSize;
    public float yPOVSize;

    private FieldOfView eyeScript;

    // Start is called before the first frame update
    void Start()
    {
        eyeScript = eye.GetComponent<FieldOfView>();



        GameObject imgObject = new GameObject("POV");

        RectTransform trans = imgObject.AddComponent<RectTransform>();
        trans.transform.SetParent(canvas.transform); // setting parent
        trans.localScale = Vector3.one;
        trans.anchoredPosition = new Vector2(xPOVLocation, yPOVLocation); // setting position, will be on center
        trans.sizeDelta= new Vector2(xPOVSize, yPOVSize); // custom size

        Image image = imgObject.AddComponent<Image>();
        image.raycastTarget = false; // ensure this image won't trigger the raycast
        // image.sprite = testSprite; // assign a sprite to the image
        image.color = Color.blue;



        GameObject imgView;
        RectTransform transLoop;
        Image imageLoop;
        for (int i = 0; i < 2; i++) 
        {
            imgView = new GameObject("Ray_" + i.ToString());

            transLoop = imgView.AddComponent<RectTransform>();
            transLoop.transform.SetParent(imgObject.transform); // setting parent
            transLoop.localScale = Vector3.one;
            transLoop.anchoredPosition = new Vector2(0f, 0f); // setting position, will be on center
            transLoop.sizeDelta= new Vector2(20, 30); // custom size

            imageLoop = imgView.AddComponent<Image>();
            imageLoop.raycastTarget = false; // ensure this image won't trigger the raycast

            // colour chosen will be dependent on what the raycast sees
            imageLoop.color = Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(eyeScript.rays);
        
    }
}
