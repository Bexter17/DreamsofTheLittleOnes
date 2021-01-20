using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStack : MonoBehaviour
{
    GameObject playerTarget;

    private GameObject[] EnemyQueue;
    // Start is called before the first frame update

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = playerTarget.transform.position;
    }
}
