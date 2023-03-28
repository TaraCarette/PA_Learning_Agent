using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeInBin : MonoBehaviour
{
    public GameObject receivedObject;

    private Vector3 goalBase;
    private Vector3 side1;
    private Vector3 side2;

    private Vector3 receivedPosition;
    private Rect rect;

    // Start is called before the first frame update
    void Start()
    {
        goalBase = transform.GetChild(0).position;
        // // Transform s1 = transform.GetChild(1);
        // // Transform s2 = transform.GetChild(2);
        // // side1 = s1.parent.TransformPosition(s1.localPosition);
        // // side2 = s2.parent.TransformPosition(s2.localPosition);
        // // transform.GetChild(1).position;
        side1 = transform.GetChild(1).position;
        side2 = transform.GetChild(2).position;
        // Debug.Log(transform.GetChild(1).localScale);
        // Debug.Log(side1);
        // Debug.Log(side2);


        // issue is depending on how flipped gonna be different

        Vector3 pos = transform.position;
        Vector3 scale = transform.localScale;

        // if (goalBase[0] > side1)
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
        rect = new Rect(topX, topY, xLength, yLength);  
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
                Destroy(receivedObject);
            }
        }

    }
}
