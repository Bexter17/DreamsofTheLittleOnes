using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;


//Animation Script
public class EnemyAI1 : MonoBehaviour
{
    //Connect EnemyAI1 script and EnemyCombat script
    EnemyCombat EnemyCombatScript;

    [SerializeField] enum EnemyState {Start, Patrol, Chase, Attack};
    EnemyState myEnemy;
    // The player that the enemy will chase
    public Transform target;
    //public Vector3 initialPos;
    //bool isInitPos = true;

    //Animations
    Animator eAnim;

    // How fast enemy moves
    [SerializeField] float enemyMovement;
    // multiplies by walk enemyMovement speed for chasing speed
    [SerializeField] int enemyRunMultiplier;
    public Rigidbody rb;

    // The distance the enemy will begin to chase player
    public float chaseRange;
    public float attackRange;

    //bool isMoving = true;
    bool isPatrolling = false;

    NavMeshAgent agent;

    public Transform waypoint1;
    public Transform waypoint2;

    // Amount of damage done by enemy to player
    public int dmgDealt;

    public float rotationSpeed;
    // Start is called before the first frame update
    void Start()
    {
        EnemyCombatScript = gameObject.GetComponent<EnemyCombat>();

        myEnemy = EnemyState.Start;

        eAnim = gameObject.GetComponent<Animator>();

        target = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        agent = GetComponent<NavMeshAgent>();

        // Default values
        if (enemyMovement <= 0)
        {
            enemyMovement = 3f;
        }
        if (chaseRange <= 0)
        {
            chaseRange = 5f;
        }
        if (attackRange <= 0)
        {
            attackRange = 3f;
        }
        if (dmgDealt <= 0)
        {
            dmgDealt = 1;
        }
        if (enemyRunMultiplier <= 0)
        {
            enemyRunMultiplier = 4;
        }
        Patrol();
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.enabled)
        {
            Vector3 targetPosition = agent.destination;
            targetPosition.y = transform.position.y;
            // Corrects rotation for punch to better connect
            //if (myEnemy == EnemyState.Attack)
            //    targetPosition.x -= 100;
            
            // If enemy within attackrange stop moving and attack
            // If enemy within chaserange chase player
            // else go back to patrol route
            if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange && Vector3.Distance(target.position, gameObject.transform.position) > attackRange)
            {
                Chase();
                agent.isStopped = false;
                myEnemy = EnemyState.Chase;
            }
            else if (Vector3.Distance(target.position, gameObject.transform.position) < attackRange && (EnemyCombatScript.death != true))
            {
                agent.isStopped = true;
                myEnemy = EnemyState.Attack;
            }

            else if (!isPatrolling)
            {
                Patrol();
                agent.isStopped = false;
                myEnemy = EnemyState.Patrol;
            }

            if (EnemyCombatScript.death == true)
            {
                // so that enemy doesn't move after dying
                agent.isStopped = true;
                //eAnim.SetTrigger("IsPunching");
                eAnim.SetBool("IsDying", true);
                eAnim.SetTrigger("IsDead");
                Destroy(gameObject, 5);
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
                float str = rotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
                //transform.LookAt(targetPosition);
            }
            if(myEnemy == EnemyState.Patrol)
            {
                Patrol();
                agent.speed = enemyMovement;
            }
            else if(myEnemy == EnemyState.Chase)
            {
                Chase();
                agent.speed = enemyRunMultiplier * enemyMovement;
                Debug.Log("Run");
            }

            
        }
    }
    public void Chase()
    {
        //agent.isStopped = false;
        //Debug.Log("CHASE");
        isPatrolling = false;
        myEnemy = EnemyState.Chase;
        // Sets player as destination
        agent.SetDestination(target.transform.position);
    }

    // Calls Chase() for all enemies
    // Currently not being used
    //private void Honk()
    //{
    //    GameObject[] enemies;
    //    enemies = GameObject.FindGameObjectsWithTag("Enemy");
    //    for(int i = 0; i < enemies.Length; i++)
    //    {
    //        enemies[i].GetComponent<EnemyAI1>().Chase();
    //    }
    //}

    // Turns around and continues
    private void Patrol()
    {
        //Debug.Log("PATROL");
        // At the beginning of patrolling sets first patrol destination
        if (myEnemy != EnemyState.Patrol)
        {
            myEnemy = EnemyState.Patrol;
            agent.SetDestination(waypoint1.position);
            isPatrolling = true;
        }
    }
    
    // Used for enemy animations and patrolling between waypoints
    private void OnTriggerEnter(Collider other)
    {
        // Plays punching animation when player enters collision
        if (other.CompareTag("Player"))
            eAnim.SetTrigger("isPunching");

        // During patrol alternate going between Waypoint1 and Waypoint2
        // On colliding with waypoint sets other as destination
        if (isPatrolling)
        {
            if (other.CompareTag("WayPoint1"))
            {
                //Debug.Log("Waypoint");
                agent.SetDestination(waypoint2.position);
            }
            else if (other.CompareTag("WayPoint2"))
            { 
                agent.SetDestination(waypoint1.position);
            }
        }
}

    //Sets enemy to walking animation if player leaves collision
    private void OnTriggerExit(Collider other)
    {
       // eAnim.ResetTrigger("isPunching");
    }

    

    /*private void OnCollisionEnter(Collision collision)
    {
        if (!isPatrolling)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                //Debug.Log("Player Hit");
            }
        }
    }*/
}

//Go in circles
//transform.Rotate(0, 1, 0);
//transform.position += transform.forward* enemyMovement * Time.deltaTime;