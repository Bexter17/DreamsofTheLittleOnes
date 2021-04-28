using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    void Start()
    {
    }

    void OnTriggerEnter(Collider other)

    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("checkpoint activated");
            GameManager.Instance.UpdateCheckpoint(gameObject);
        }
    }
}
