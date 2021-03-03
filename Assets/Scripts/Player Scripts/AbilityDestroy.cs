using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDestroy : MonoBehaviour
{
    public GameObject ability;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(ability, 1);
    }
}
