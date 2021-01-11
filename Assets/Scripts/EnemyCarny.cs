using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

//Combat Script
public class EnemyCarny : MonoBehaviour
{
    //Connect EnemyAI1 script with EnemyCombat script
    EnemyAI1 EnemyAI1Script;

    [SerializeField] int hp = 5;
    private int maxHP;
    public Rigidbody rb;
    public Transform target;
    [SerializeField] float knockDistanceModifier;
    [SerializeField] float knockHeightModifier;
    [SerializeField] float knockDuration;
    [SerializeField] float knockPause;

    NavMeshAgent agent;

    [SerializeField] private Image hpBar;

    //Death
    public bool death = false;

    CharacterMechanics cm;

    [SerializeField] enum EnemyState { Start, Patrol, Chase, Attack };
    EnemyState myEnemy;
    // The player that the enemy will chase
    //public Vector3 initialPos;
    //bool isInitPos = true;

    //Animations
    Animator eAnim;

    // How fast enemy moves
    [SerializeField] float enemyMovement;
    // multiplies by walk enemyMovement speed for chasing speed
    [SerializeField] int enemyRunMultiplier;

    // The distance the enemy will begin to chase player
    public float chaseRange;
    public float attackRange;

    //bool isMoving = true;
    bool isPatrolling = false;


    public Transform waypoint1;
    public Transform waypoint2;

    // Amount of damage done by enemy to player
    public int dmgDealt;

    public float rotationSpeed;
    // Start is called before the first frame update
    void Start()
    {
        EnemyAI1Script = gameObject.GetComponent<EnemyAI1>();

        rb = GetComponent<Rigidbody>();

        //sets maxHP to beginning hp in order to get the correct fill amount for hpbar
        int maxHP = hp;
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.Find("Player").transform;

        cm = GameObject.Find("Player").GetComponent<CharacterMechanics>();




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
        //Sets hp text to change based on players perspective
        //So it's not backwards to the player
        Vector3 textDirection = transform.position - target.transform.position;

        if (agent.enabled)
        {
            //hpBar.transform.rotation = Quaternion.LookRotation(textDirection);
            //transform.rotation = Quaternion.LookRotation(textDirection);
        }

        //Detect when there is no HP to kill enemy and play death animation
        if (hp <= 0)
        {
            death = true;
            Debug.Log("Enemy has been killed");
            agent.isStopped = true;
            AgentStop();
        }

        //Used for testing enemy death
        if (Input.GetKeyDown("t"))
        {
            Debug.Log("Enemy has lost 1 hp");
            hp -= 1;
        }


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
            else if (Vector3.Distance(target.position, gameObject.transform.position) < attackRange && (death != true))
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

            if (death == true)
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
            if (myEnemy == EnemyState.Patrol)
            {
                Patrol();
                agent.speed = enemyMovement;
            }
            else if (myEnemy == EnemyState.Chase)
            {
                Chase();
                agent.speed = enemyRunMultiplier * enemyMovement;
                Debug.Log("Run");
            }


        }
    }
    public void takeDamage(int dmg)
    {
        //Debug.Log(dmg + "Damage Taken");
        agent.isStopped = true;
        hp -= dmg;
        if (hp <= 0)
        {
            death = true;
            //Destroy(gameObject);   Destroy object is called in EnemyAI1 when the death animation is played
        }
        hpBar.fillAmount = (float)(hp * 0.2);

        //KNOCKBACK
        // Gets the difference between enemy and player position
        // To knockback enemy away from player
        rb.isKinematic = false;
        //agent.enabled = false;
        rb.AddForce(-transform.forward * knockDistanceModifier);
        //rb.AddForce(transform.up * knockHeightModifier);

        Debug.Log("Knockback");
        //Invokes once enemy is no longer being knocked back and pauses movement
        Invoke("AgentStop", knockDuration);
    }
    private void AgentStop()
    {
        //Enemy briefly pauses after being knocked back
        //Where it's velocity is 0
        Invoke("AgentStart", knockPause);
    }
    private void AgentStart()
    {
        rb.isKinematic = true;
        //Enemy continues moving
        //agent.enabled = true;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector3.zero;
            agent.isStopped = true;
            AgentStop();
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
