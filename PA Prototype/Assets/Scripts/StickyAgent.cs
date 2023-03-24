using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyAgent : MonoBehaviour
{
    private bool stickyOn;
    private bool touching;
    private List<Transform> touchingObj;
    SpriteRenderer sr;
    private Color currColour;

    // Start is called before the first frame update
    void Start()
    {
        stickyOn = false;
        sr = GetComponent<SpriteRenderer>();
        currColour = sr.color;
        touchingObj = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // when change status, change outline to indicate
        // and update relationship to any other objects already touching as needed
        if (Input.GetKeyDown(KeyCode.S))
        {
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
                // sr.color = new Color32(0xCF, 0x4E, 0x20, 0xFF);
                sr.color = currColour;
                // the touching objects should no longer be children
                foreach (Transform obj in touchingObj)
                {
                    obj.parent = null;
                }
                // removing from list as will newly collide once no
                // longer a child of the agent
                touchingObj = new List<Transform>();
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
        }

    }

    void OnCollisionExit2D(Collision2D other)
    {
        // when sticky and touching, no collision as same body
        // but still want to keep track of object
        if (!stickyOn) {
            touchingObj.Remove(other.transform);
        }
    }
}
