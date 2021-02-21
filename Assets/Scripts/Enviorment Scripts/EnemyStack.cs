using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStack : MonoBehaviour
{
    GameObject playerTarget;

    public GameObject[] EnemyQueue;
    // Start is called before the first frame update

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player");
        EnemyQueue = new GameObject[4];
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = playerTarget.transform.position;
    }
    public int AddStack(GameObject enemy)
    {
        for (int i = 0; i < EnemyQueue.Length; i++)
        {
            if(EnemyQueue[i] == null)
            {
                EnemyQueue[i] = enemy;
                return i;
            }
        }
        return 5;
    }
    public void RemoveStack(GameObject enemy)
    {
        for (int i = 0; i < EnemyQueue.Length; i++)
        {
            if(EnemyQueue[i] == enemy)
            {
                EnemyQueue[i] = null;
            }
        }
    }
}
