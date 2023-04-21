using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float speed; 
    public float rotationSpeed;
    Rigidbody2D rgbd;

    [HideInInspector]
    public float currSpeed;

    private Vector3 prevLocation;

    // Start is called before the first frame update
    void Start()
    {
        rgbd = GetComponent<Rigidbody2D>();
        prevLocation = rgbd.transform.position;
        currSpeed = 0;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // if (Input.GetKey(KeyCode.UpArrow))
        // {
        //     moveAgent(0);
        // }
        // if (Input.GetKey(KeyCode.DownArrow))
        // {
        //     moveAgent(1);
        // }
        // if (Input.GetKey(KeyCode.LeftArrow))
        // {
        //     rotateAgent(0);
        // }
        // if (Input.GetKey(KeyCode.RightArrow))
        // {
        //     rotateAgent(1);
        // }
        // if (Input.GetKey(KeyCode.A))
        // {
        //     moveAgent(2);
        // }
        // if (Input.GetKey(KeyCode.D))
        // {
        //     moveAgent(3);
        // }

        currSpeed = Vector3.Distance(rgbd.transform.position, prevLocation);
        prevLocation = rgbd.transform.position;
    }

    public void moveAgent(int dir)
    {
        if (dir == 0)
        {
            rgbd.transform.Translate(new Vector3(0, speed, 0) * Time.deltaTime);
        }
        else if (dir == 1)
        {
            rgbd.transform.Translate(new Vector3(0, -speed, 0) * Time.deltaTime);
        }
        else if (dir == 2)
        {
            rgbd.transform.Translate(new Vector3(-speed, 0, 0) * Time.deltaTime);
        }
        else if (dir == 3)
        {
            rgbd.transform.Translate(new Vector3(speed, 0, 0) * Time.deltaTime);
        }
    } 

    public void rotateAgent(int dir)
    {
        if (dir == 0)
        {
            rgbd.transform.Rotate(new Vector3(0, 0, rotationSpeed) * Time.deltaTime);
        }
        if (dir == 1)
        {
            rgbd.transform.Rotate(new Vector3(0, 0, -rotationSpeed) * Time.deltaTime);
        }
    }   
}
