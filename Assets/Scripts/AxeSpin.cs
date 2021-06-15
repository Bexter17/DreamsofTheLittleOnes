using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeSpin : MonoBehaviour
{

    public float axeAngle = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(axeAngle, Vector3.forward);
    }
}
