using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class POVGenerator : MonoBehaviour
{
    public GameObject eye; // give it the eye you want the POV of
    public GameObject canvas;

    // give it the location you want the POV bar to appear
    // will be relative to the center of the screen
    public float xPOVLocation;
    public float yPOVLocation;

    // the size of the POV bar
    public float xPOVSize;
    public float yPOVSize;


    private FieldOfView eyeScript;
    private List <Image> imgPixels;
    private int rays;
    private Color defaultColour;

    // Start is called before the first frame update
    void Start()
    {
        // set the colour for the POV bar when it sees nothing
        defaultColour = Color.blue;

        // extract the number of rays from the variables of the eye
        eyeScript = eye.GetComponent<FieldOfView>();
        rays = (int) ((eyeScript.viewAngle / 10) * eyeScript.rayPer10Degree) + 1;

        // calculate the size of the pixels representing
        // the info from each raycast
        float pixelWidth = xPOVSize / rays;
        float pixelHeight = yPOVSize - 8;



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


        // creating objects and images for each pixel
        GameObject imgView;
        RectTransform transLoop;
        float posX;
        imgPixels = new List<Image>(); // initialize empty list
        for (int i = 1; i <= rays; i++) 
        {
            // creating object to hold information of one ray
            imgView = new GameObject("Ray_" + i.ToString());

            // setting that object to be relative to POV bar
            transLoop = imgView.AddComponent<RectTransform>();
            transLoop.transform.SetParent(imgObject.transform); // setting parent
            transLoop.localScale = Vector3.one;

            // setting size of pixel
            transLoop.sizeDelta = new Vector2(pixelWidth, pixelHeight); // custom size

            // positioning the pixel on the POV bar
            posX = (-xPOVSize / 2) + (i * pixelWidth) - (pixelWidth / 2);
            transLoop.anchoredPosition = new Vector2(posX, 0f); // setting position, will be on center

            // creating actual image which will hold the visual information for the pixel
            imgPixels.Add(imgView.AddComponent<Image>());

            // start colour should match bar colour
            imgPixels[i - 1].color = defaultColour;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // access results of raycasts from eye in order to update the 
        // POV bar with
        for (int i = 0; i < rays; i++) 
        {
            if (eyeScript.hits[i].collider != null)
            {
                imgPixels[rays - 1 - i].color = Color.green;
            } else {
                imgPixels[rays - 1 - i].color = defaultColour;
            }
        }
    }
}
