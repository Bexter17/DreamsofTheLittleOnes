using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    public GameObject destroyedVersion;
    public GameObject[] pickUps;
    public int index;
    public float waitTime = 2f;

    private void Start()
    {
      
    }

    private void Update()
    {
        index = Random.Range(0, pickUps.Length);

      

    }
    private void OnMouseDown()
    {
      
        
        GameObject randomPickup = Instantiate(pickUps[index], transform.position, transform.rotation);
        GameObject brokenVersion = Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
        brokenVersion.transform.localScale = new Vector3(2, 2, 2);
        Object.Destroy(brokenVersion, 5f);
  
        //randomPickup.transform.localScale = new Vector3(1, 1, 1);

       // Instantiate(destroyedVersion, transform.position, transform.rotation, destroyedVersion.transform.parent);
        Destroy(gameObject);
    }



}
