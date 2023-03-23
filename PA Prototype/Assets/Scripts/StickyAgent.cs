using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyAgent : MonoBehaviour
{
    private bool stickyOn;
    private bool touching;
    private List<Transform> touchingObj;
    SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        stickyOn = false;
        sr = GetComponent<SpriteRenderer>();
        touchingObj = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // when change status, change outline to indicate
        // and update relationship to any other objects already touching as needed
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Changed sticky status");
            stickyOn = !stickyOn;

            if (stickyOn)
            {
                sr.color = Color.white;
                // any touching objects should become child so moves together
                foreach (Transform obj in touchingObj)
                {
                    obj.parent = transform;
                }
            } else 
            {
                sr.color = new Color32(0xCF, 0x4E, 0x20, 0xFF);
                // the touching objects should no longer be children
                foreach (Transform obj in touchingObj)
                {
                    obj.parent = null;
                }
            }
        }

    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // only register moveable objects, aka ignore the ones in the unmoveable layer
        if (other.gameObject.layer != 6)
        {
            // case where already sticky should stick immediately on collision
            if (stickyOn)
            {
                other.transform.parent = transform;
            } 
            // update list of objects agent is touching and index
            Transform copyOther = other.transform;
            touchingObj.Add(copyOther);


            Debug.Log("current list");
            foreach (Transform obj in touchingObj)
            {
                Debug.Log(obj);
            }
        }

    }

    void OnCollisionExit2D(Collision2D other)
    {
        touchingObj.Remove(other.transform);
    }
}
