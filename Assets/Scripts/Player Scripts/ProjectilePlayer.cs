using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlayer : MonoBehaviour
{
    EnemyCarny enemy;
    RangedEnemy rangedEnemy;
    // Start is called before the first frame update
    void Start()
    {
        rangedEnemy.GetComponent<RangedEnemy>();
        enemy.GetComponent<EnemyCarny>();
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Balloon"))
        {
            Destroy();
        }

    }

    public void Destroy()
    {
        //Instantiate(balloonPopParticles, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            enemy.takeDamage(1);
        }

        if (other.gameObject.tag == "RangedEnemy")
        {
            rangedEnemy.takeDamage(1);
        }
    }
}
