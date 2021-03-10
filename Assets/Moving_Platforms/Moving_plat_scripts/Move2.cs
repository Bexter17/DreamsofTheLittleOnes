using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move2 : MonoBehaviour
{
    bool flag = true;

    [SerializeField]
    Transform platform;

    [SerializeField]
    Transform endTransform;

    [SerializeField]
    float platformSpeed;

    Vector3 direction;
    Transform destination;
    
    void Update()
    {

        if (GameObject.Find("ball2") == null)
        {

            Debug.Log("Its destroyed");
            flag = false;
        }

    }

    void FixedUpdate()
    {

        if (flag == false)
        {
            platform.GetComponent<Rigidbody>().MovePosition(platform.position + direction * platformSpeed * Time.fixedDeltaTime);

            SetDestination(endTransform);
        }

    }

    void OnDrawGizmos()
    {
        //draw primitives
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(endTransform.position, platform.localScale);
    }

    void SetDestination(Transform dest)
    {
        destination = dest;

        direction = (destination.position - platform.position);
    }
}
