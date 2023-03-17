using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    [Range(0,360)]
    [Tooltip("Should be divisible by 10 to work best")]
    public float viewAngle;
    public float rayPer10Degree;

    [HideInInspector]
    public RaycastHit2D[] hits;

    private int rays;
    private float degreeBetween;
    private float sideView;

    void Start()
    {
        // calculate how many rays needs given field of view and density or rays
        rays = (int) ((viewAngle / 10) * rayPer10Degree) + 1;
        degreeBetween = 10 / rayPer10Degree;
        sideView = viewAngle / 2;

        // initialize the list storing the raycast hit data in
        hits = new RaycastHit2D[rays];
    }



    void FixedUpdate()
    {
        // calculate what the forwards vector is given rotation of the eye
        Vector3 forward = transform.TransformDirection(Vector3.up);

        float angleInDegrees;
        float angleInRadians;

        for(int i = 0; i < rays; i++)
        {
            angleInDegrees = sideView - (degreeBetween * i);
            angleInRadians = (Mathf.PI / 180) * angleInDegrees;
            // calculate a new vector based on the forward vector and a specific given angle
            Vector3 expanded = new Vector3(forward[0] * Mathf.Cos(angleInRadians) + forward[1] * Mathf.Sin(angleInRadians), 
                -forward[0] * Mathf.Sin(angleInRadians) + forward[1] * Mathf.Cos(angleInRadians), 0);

            // draw the new ray
            Debug.DrawRay(transform.position, Vector3.Normalize(expanded) * viewRadius, Color.green);

            // create the new ray and store the result
            hits[i] = Physics2D.Raycast(transform.position, Vector3.Normalize(expanded), viewRadius);

        }


    }

}
