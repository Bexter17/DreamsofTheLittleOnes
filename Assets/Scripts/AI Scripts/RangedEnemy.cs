using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class RangedEnemy : MonoBehaviour
{
    #region Variables

    [SerializeField] int hp = 5;
    private int maxHP;
    public Rigidbody rb;
    public Transform target;
    [SerializeField] Rigidbody projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float attackTimer = 0;
    [SerializeField] float callingRange;
    [SerializeField] float knockDistanceModifier;
    [SerializeField] float knockHeightModifier;
    [SerializeField] float knockDuration;
    [SerializeField] float knockPause;

    NavMeshAgent agent;

    [SerializeField] private Image hpBar;

    //Death
    public bool death = false;

    CharacterMechanics cm;

    //used to track the player for giveDamage function 
    private GameObject Player;
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
    bool getCalled = false;


    public Transform waypoint1;
    public Transform waypoint2;


    public float rotationSpeed;
    #endregion



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Player = GameObject.FindGameObjectWithTag("Player");

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

        #region default values

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
        if (enemyRunMultiplier <= 0)
        {
            enemyRunMultiplier = 4;
        }
        if (callingRange <= 0)
        {
            callingRange = 10f;
        }
        Patrol();
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("t"))
        {
            Debug.Log("Enemy has lost 1 hp");
            takeDamage(1);
        }
        #region AI States
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
            if (Vector3.Distance(target.position, gameObject.transform.position) < attackRange)
            {
                agent.isStopped = true;
                myEnemy = EnemyState.Attack;
                if (Time.time - attackTimer > 2.0f)
                {
                    eAnim.SetTrigger("isPunching");
                    attackTimer = Time.time;
                    if (projectilePrefab)
                    {
                        transform.LookAt(Player.transform.position);
                        Rigidbody rb = Instantiate(projectilePrefab,
                            projectileSpawnPoint.position,
                            projectileSpawnPoint.rotation) as Rigidbody;

                        rb.AddForce(transform.forward * projectilePrefab.GetComponent<Projectiles>().projectileSpeed, ForceMode.Impulse);
                    }
                }
            }
            else if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange)
            {
                Chase();
                LookingForAllies();
                agent.isStopped = false;
                myEnemy = EnemyState.Chase;
                if(Vector3.Distance(target.position, gameObject.transform.position) < chaseRange - (enemyMovement * enemyRunMultiplier * 0.5))
                {
                    getCalled = false;
                }
            }
            else if (!isPatrolling && !getCalled)
            {
                Patrol();
                agent.isStopped = false;
                myEnemy = EnemyState.Patrol;
            }
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            float str = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
            //transform.LookAt(targetPosition);
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
        #endregion
    }
    public void takeDamage(int dmg)
    {
        //Debug.Log(dmg + "Damage Taken");
        agent.isStopped = true;
        hp -= dmg;
        if (hp <= 0)
        {
            death = true;
            Debug.Log("Enemy has been killed");
            enemyMovement = 0;
            rotationSpeed = 0;
            AgentStop();
            // so that enemy doesn't move after dying
            //eAnim.SetTrigger("IsPunching");
            eAnim.SetBool("IsDying", true);
            eAnim.SetTrigger("IsDead");
            Destroy(gameObject, 4);

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
        if (collision.gameObject.tag=="HammerSmashAOE")
        {
            #region Debug Log
            Debug.Log("Ranged enemy has been hit by hammer smash!");
            #endregion
            rb.velocity = Vector3.zero;
            agent.isStopped = true;
            //Stop attacking
            AgentStop();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "HammerSmashAOE")
        {
            #region Debug Log
            Debug.Log("Ranged enemy has regained it's speed!");
            #endregion
            //Give enemies back their speed after hammer smash AOE
            enemyMovement += 2;
        }
    }
    #region init States
    public void Chase()
    {
        //agent.isStopped = false;
        //Debug.Log("CHASE");
        isPatrolling = false;
        myEnemy = EnemyState.Chase;
        // Sets player as destination
        //agent.SetDestination(target.transform.position);
        //UpdateCirclePoints();
        agent.SetDestination(Player.transform.position);
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
    private void LookingForAllies()
    {
        GameObject[] Enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for(int i = 0; i < Enemies.Length; i++)
        {
            if(Enemies[i] != gameObject)
            {
                if(Vector3.Distance(Enemies[i].transform.position,gameObject.transform.position) < callingRange)
                {
                    Enemies[i].GetComponent<EnemyCarny>().Called();
                    Debug.Log("calling1");
                }
            }
        }
    }

    // Used for enemy animations and patrolling between waypoints
    private void OnTriggerEnter(Collider other)
    {

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
    #endregion
}
