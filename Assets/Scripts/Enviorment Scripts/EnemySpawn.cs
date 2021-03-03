using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemySpawn : MonoBehaviour
{
    public GameObject EnemyPrefab;
    GameObject Enemy;
    // Start is called before the first frame update
    void Start()
    {
        if (!Enemy)
        {
            Enemy = Instantiate(EnemyPrefab, transform.position, transform.rotation);
            // I commented these lines out because on the enemy waypoints are now automatically gotten
            //Enemy.GetComponent<EnemyCarny>().waypoint1 = GameObject.FindGameObjectWithTag("WayPoint1");
            //Enemy.GetComponent<EnemyCarny>().waypoint2 = GameObject.FindGameObjectWithTag("WayPoint2");
            Enemy.GetComponent<EnemyCarny>().chaseRange = 30;
            Enemy.GetComponent<EnemyCarny>().changeStackrange(Enemy.GetComponent<EnemyCarny>().chaseRange);
            Enemy.GetComponent<EnemyCarny>().Chase();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
