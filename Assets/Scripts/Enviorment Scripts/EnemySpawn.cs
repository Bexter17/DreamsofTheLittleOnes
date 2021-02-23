using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public Rigidbody EnemyPrefab;
    Rigidbody Enemy;
    // Start is called before the first frame update
    void Start()
    {
        if (!Enemy)
        {
            Enemy = Instantiate(EnemyPrefab, transform.position, transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
