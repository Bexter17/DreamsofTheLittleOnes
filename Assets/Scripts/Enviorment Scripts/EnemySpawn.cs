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
    GameObject minigame;
    float spawntimer = 0;
    public int status;
    // Start is called before the first frame update
    void Start()
    {
        if (!Enemy)
        {
            minigame = GameObject.Find("MovingBox");
            Enemy = Instantiate(EnemyPrefab, transform.position, transform.rotation);
            Enemy.GetComponent<EnemyCarny>().chaseRange = 300;
            Enemy.GetComponent<EnemyCarny>().editChase();
            // I commented these lines out because on the enemy waypoints are now automatically gotten
            //Enemy.GetComponent<EnemyCarny>().waypoint1 = GameObject.FindGameObjectWithTag("WayPoint1");
            //Enemy.GetComponent<EnemyCarny>().waypoint2 = GameObject.FindGameObjectWithTag("WayPoint2");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Enemy)
        {
            status = Enemy.GetComponent<EnemyCarny>().getstatus();
            if (status == 1)
            {
                Enemy.GetComponent<EnemyCarny>().editChase();
            }
        }
        if (minigame.GetComponent<HammerGame>().isHit == false)
        {

            if (!Enemy && (Time.time - spawntimer > 10))
            {
                spawntimer = Time.time;
                Enemy = Instantiate(EnemyPrefab, transform.position, transform.rotation);
                Enemy.GetComponent<EnemyCarny>().chaseRange = 300;
                Enemy.GetComponent<EnemyCarny>().editChase();
                // I commented these lines out because on the enemy waypoints are now automatically gotten
                //Enemy.GetComponent<EnemyCarny>().waypoint1 = GameObject.FindGameObjectWithTag("WayPoint1");
                //Enemy.GetComponent<EnemyCarny>().waypoint2 = GameObject.FindGameObjectWithTag("WayPoint2");
            }
        }
    }
}
