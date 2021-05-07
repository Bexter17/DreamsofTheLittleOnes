using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemydespawn : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyMe", 5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void DestroyMe()
    {
        Destroy(gameObject);
    }
}
