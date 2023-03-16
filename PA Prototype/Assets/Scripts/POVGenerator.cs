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
    private float pixelWidth;
    private float pixelHeight;

    // Start is called before the first frame update
    void Start()
    {
        // extract the number of rays from the variables of the eye
        eyeScript = eye.GetComponent<FieldOfView>();
        int rays = (int) ((eyeScript.viewAngle / 10) * eyeScript.rayPer10Degree) + 1;

        // calculate the size of the pixels representing
        // the info from each raycast
        pixelWidth = xPOVSize / rays;
        pixelHeight = yPOVSize - 8;



        // create the POV bar that will hold all the pixels
        GameObject imgObject = new GameObject("POV");
        RectTransform trans = imgObject.AddComponent<RectTransform>();
        trans.transform.SetParent(canvas.transform); // setting parent
        trans.localScale = Vector3.one;
        trans.anchoredPosition = new Vector2(xPOVLocation, yPOVLocation); // setting position, will be on center
        trans.sizeDelta= new Vector2(xPOVSize, yPOVSize); // custom size

        Image image = imgObject.AddComponent<Image>();
        image.raycastTarget = false; // ensure this image won't trigger the raycast
        image.color = Color.blue; // might change this to something more specific to look good



        GameObject imgView;
        RectTransform transLoop;
        Image imageLoop;
        float posX;
        for (int i = 1; i <= rays; i++) 
        {
            // creating object to hold information of one ray
            imgView = new GameObject("Ray_" + i.ToString());

            // setting that object to be relative to POV bar
            transLoop = imgView.AddComponent<RectTransform>();
            transLoop.transform.SetParent(imgObject.transform); // setting parent
            transLoop.localScale = Vector3.one;

            // setting size of pixel
            transLoop.sizeDelta= new Vector2(pixelWidth, pixelHeight); // custom size

            // positioning the pixel on the POV bar
            posX = (-xPOVSize / 2) + (i * pixelWidth) - (pixelWidth / 2);
            transLoop.anchoredPosition = new Vector2(posX, 0f); // setting position, will be on center

            // creating actual image which will hold the visual information for the pixel
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
