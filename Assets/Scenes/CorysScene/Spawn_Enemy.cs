using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;

public class Spawn_Enemy : MonoBehaviour
{
    public GameObject Enemy;
    
    public int xPos;
    public int ZPos;
    public int enemyCount;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EnemyDrop());
    }
       
    
        
        IEnumerator EnemyDrop()
        {
            while (enemyCount < 2)
            {
                xPos = Random.Range(1, 50);
                ZPos = Random.Range(1, 31);
                Instantiate(Enemy, new Vector3(xPos, 5, ZPos), Quaternion.identity);
                yield return new WaitForSeconds(0.1f);
                enemyCount += 1;
            }

        }
    }

   

