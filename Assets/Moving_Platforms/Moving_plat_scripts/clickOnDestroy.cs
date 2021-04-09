using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickOnDestroy : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                //Only destroy spheres 
                SphereCollider bc = hit.collider as SphereCollider;
                if (bc != null)
                {
                    Destroy(bc.gameObject);
                }
            }
        }

        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag (""))
        {

        }



    }
}
