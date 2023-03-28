using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbController : MonoBehaviour
{
    public float speed;
    public GameObject anchor;
    public float maxAngle;
    public float minAngle;
    public GameObject obstacle;
    public GameObject body;

    void Start()
    {
        // turn angles into radians
        maxAngle = (Mathf.PI / 180) * maxAngle;
        minAngle = (Mathf.PI / 180) * minAngle;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(obstacle.GetComponent<Collider2D>().IsTouching(body.GetComponent<Collider2D>()));
        if (Input.GetKey(KeyCode.Z) && transform.rotation.z < maxAngle)
        {
            transform.RotateAround(anchor.transform.position, Vector3.forward, speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.X) && transform.rotation.z > minAngle)
        {
            transform.RotateAround(anchor.transform.position, -Vector3.forward, speed * Time.deltaTime);
        }
    }
}
