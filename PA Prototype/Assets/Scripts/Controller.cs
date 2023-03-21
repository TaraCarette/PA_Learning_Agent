using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float speed; 
    public float rotationSpeed;
    Rigidbody2D rgbd;

    // Start is called before the first frame update
    void Start()
    {
        rgbd = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rgbd.transform.Translate(new Vector3(0, speed, 0) * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            rgbd.transform.Translate(new Vector3(0, -speed, 0) * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rgbd.transform.Rotate(new Vector3(0, 0, rotationSpeed) * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rgbd.transform.Rotate(new Vector3(0, 0, -rotationSpeed) * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rgbd.transform.Translate(new Vector3(-speed, 0, 0) * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rgbd.transform.Translate(new Vector3(speed, 0, 0) * Time.deltaTime);
        }
    }
}
