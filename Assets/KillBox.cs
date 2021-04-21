using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Box")
        {
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Health Pickup")
        {
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Speed Pickup")
        {
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Broken Crate")
        {
            Destroy(collision.gameObject);
        }


    }
}
