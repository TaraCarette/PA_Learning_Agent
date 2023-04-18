using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeInBin : MonoBehaviour
{
    public GameObject receivedObject;
    public GameObject stickyObject;

    private List<Transform> touchingObj;

    private Vector3 side1;
    private Vector3 side2;

    private Vector3 receivedPosition;
    private Rect rect;

    // Start is called before the first frame update
    void Start()
    {
        // get the list of objects that are touchy the sticky agent so can delete object safely
        touchingObj = stickyObject.GetComponent<StickyAgent>().touchingObj;


        // the rectange will be slightly in the box, forcing the agent to properly shove whole thing in
        rect = drawBinRect(transform);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (receivedObject != null)
        {
            receivedPosition = receivedObject.transform.position;
            // if object has been moved inside the goal container, destory it
            if (rect.Contains(receivedPosition))
            {
                // remove it from list of objects stuck to agent
                touchingObj.Remove(receivedObject.transform);
                Destroy(receivedObject);
            }
        }
    }

    public Rect drawBinRect(Transform goalBoxTransform)
    {
        // get the sides so can tell orientation of box
        side1 = goalBoxTransform.GetChild(1).position;
        side2 = goalBoxTransform.GetChild(2).position;

        // get information about size of box we want detected
        Vector3 pos = goalBoxTransform.position;
        Vector3 scale = goalBoxTransform.localScale;

        // variables we need to draw detection rect
        float topX;
        float topY;
        float xLength;
        float yLength;

        // checking the orientation
        // this one is sideways
        if (side1[0] - side2[0] < 0.001)
        {
            xLength = scale[0];
            yLength = scale[1];
            topX = pos[0] - (scale[0] / 2);
            topY = pos[1] - (scale[1] / 2);
        // this one is up and down
        } else if (side1[1] - side2[1] < 0.001)
        {
            xLength = scale[1];
            yLength = scale[0];
            topX = pos[0] - (scale[1] / 2);
            topY = pos[1] - (scale[0] / 2);
        // rect cannot be rotated so if not a precise match this will fail
        } else {
            Debug.Log("Error in the goal, needs to be at a 90 degree angle to work");
            xLength = 0;
            yLength = 0;
            topX = 0;
            topY = 0;
        }

        // the rectange will be slightly in the box, forcing the agent to properly shove whole thing in
        Rect newRect = new Rect(topX, topY, xLength, yLength); 

        return newRect;
    } 
}
