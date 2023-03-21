using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyAgent : MonoBehaviour
{
    private bool stickyOn;
    SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        stickyOn = false;
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            stickyOn = !stickyOn;

            // colour the outline to either be bright red or match base colour
            if (stickyOn)
            {
                sr.color = Color.red;
            } else 
            {
                sr.color = new Color32(0xCF, 0x4E, 0x20, 0xFF);
            }
        }
    }
}
