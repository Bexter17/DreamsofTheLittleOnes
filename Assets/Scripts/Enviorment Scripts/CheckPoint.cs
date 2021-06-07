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
            Debug.LogError("checkpoint activated at " + this.transform.name);
            GameManager.Instance.UpdateCheckpoint(gameObject);
        }
    }
}
