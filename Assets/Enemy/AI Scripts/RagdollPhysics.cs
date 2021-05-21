using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollPhysics : MonoBehaviour
{
    [SerializeField] private Rigidbody ragdollRB;

    
    private void Awake()
    {
        ragdollRB = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        SetVelocity();
    }

    // Update is called once per frame
    void Update()
    {
        //ragdollRB.AddForce(0, 5000, 0, ForceMode.Impulse);
        //ragdollRB.velocity = new Vector3(0, 5000, 0);
        
    }
    private void SetVelocity()
    {

    }
    
    public void GetVelocity(Vector3 givenVelocity)
    {
        Debug.LogWarning("Ragdoll Given Velocity: " + givenVelocity);
        if(givenVelocity == new Vector3(0, 0,0))
        {
            givenVelocity = new Vector3(0, 0, 2);
        }
        givenVelocity = Vector3.Normalize(givenVelocity);
        //givenVelocity *= 5000;
        //ragdollRB.velocity.Set(givenVelocity.x, givenVelocity.y, givenVelocity.z);
        givenVelocity *= 1000;
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.AddForce(givenVelocity);
        }

    }
}
