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

    public float minTransparencyValue;

    [Tooltip("The number of different transparencies to represent distance from the agent")]
    public int distanceBins;


    private FieldOfView eyeScript;
    private List <Image> imgPixels;
    private int rays;
    private Color defaultColour;
    private float distBinSize;
    private float transBinSize;
    private float transValue;
    private IDictionary<string, float[]> colourDict;


    // Start is called before the first frame update
    void Start()
    {
        // extract the number of rays from the variables of the eye
        eyeScript = eye.GetComponent<FieldOfView>();
        rays = (int) ((eyeScript.viewAngle / 10) * eyeScript.rayPer10Degree) + 1;

        // calculate the size of the pixels representing
        // the info from each raycast
        float pixelWidth = xPOVSize / rays;
        float pixelHeight = yPOVSize - 8;


        // set the colour for the POV bar when it sees nothing
        defaultColour = Color.white;

        // calculate the size of the distance along the raycast and the corresponding
        // transparency value if divided into a set number of bins
        distBinSize = eyeScript.viewRadius / distanceBins;
        transBinSize = (1 - minTransparencyValue) / distanceBins;

        // set up the possible colours to be found on scene and store the correct values
        colourDict = new Dictionary<string, float[]>();
        colourDict.Add("red", new float[] {1, 0, 0});
        colourDict.Add("green", new float[] {0, 1, 0});
        colourDict.Add("blue", new float[] {0, 0, 1});
        colourDict.Add("yellow", new float[] {1, 1, 0});
        colourDict.Add("pink", new float[] {1, 0, 1});
        colourDict.Add("cyan", new float[] {0, 1, 1});
        colourDict.Add("white", new float[] {1, 1, 1});
        colourDict.Add("black", new float[] {0, 0, 0});
        colourDict.Add("brown", new float[] {0.65f, 0.4f, 0.16f});
        colourDict.Add("purple", new float[] {0.6f, 0.2f, 0.7f});
        colourDict.Add("orange", new float[] {1f, 0.5f, 0.1f});



        // create the POV bar that will hold all the pixels
        GameObject imgObject = new GameObject("POV");
        RectTransform trans = imgObject.AddComponent<RectTransform>();
        trans.transform.SetParent(canvas.transform); // setting parent
        trans.localScale = Vector3.one;
        trans.anchoredPosition = new Vector2(xPOVLocation, yPOVLocation); // setting position, will be on center
        trans.sizeDelta= new Vector2(xPOVSize, yPOVSize); // custom size

        Image image = imgObject.AddComponent<Image>();
        image.raycastTarget = false; // ensure this image won't trigger the raycast
        image.color = defaultColour; // might change this to something more specific to look good


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
        // POV bar with correct colour and transparency
        for (int i = 0; i < rays; i++) 
        {
            if (eyeScript.hits[i].collider != null)
            {
                // find the bin the distance belongs in, and then assign it the corresponding trans value
                for (float j = distBinSize; j <= eyeScript.viewRadius; j += distBinSize) 
                {
                    if (eyeScript.hits[i].distance <= j) 
                    {
                        // converting the bin sized j value into a counter
                        // then the further the bin the lower the transparency value
                        transValue = (transBinSize * (distanceBins - (j / distBinSize))) + minTransparencyValue;
                        break;
                    }
                }

                // find the colour based on tag
                // and assign colour and transparency to relevant pixel
                foreach(KeyValuePair<string, float[]> entry in colourDict)
                {
                    if (eyeScript.hits[i].collider.transform.tag == entry.Key)
                    {
                        imgPixels[rays - 1 - i].color = new Color(entry.Value[0], entry.Value[1], entry.Value[2], transValue);
                        break;
                    } else 
                    {
                        // default to black if no tagged colour
                        imgPixels[rays - 1 - i].color = new Color(0, 0, 0, transValue);
                    }
                }


            } else {
                imgPixels[rays - 1 - i].color = defaultColour;
            }
        }
    }
}
