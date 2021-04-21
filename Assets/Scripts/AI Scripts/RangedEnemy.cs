using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class RangedEnemy : MonoBehaviour
{
    #region Variables

    [Header("Essentials")]
    //HP
    public int hp = 5;
    private int maxHP;
    private Image hpBar;

    public Rigidbody rb;
    public Transform target;
    [SerializeField] Rigidbody projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float attackTimer;
    private float timer;

    [Header("Knockback")]
    [SerializeField] float knockDistanceModifier;
    [SerializeField] float knockDuration;
    [SerializeField] float knockPause;
    

    NavMeshAgent agent;

    //Death
    public bool death = false;

    CharacterMechanics cm;

    //used to track the player for giveDamage function 
    private GameObject Player;
    enum EnemyState { Start, Patrol, Attack, Chase};
    EnemyState myEnemyClown;
    // The player that the enemy will chase
    //public Vector3 initialPos;
    //bool isInitPos = true;

    IEnumerator Stun()
    {
        Debug.Log("ENEMY HAS BEEN STUNNED FOR 6 SECONDS BY HAMMER SMASH");
        //Stop enemy movement
        enemyMovement = 0;
        //Stop enemy attack
        AgentStop();
        //Damage Enemy - Had to change hp variable instead of takeDamage because it applies knockback when we want a stun effect
        hp -= 2;
        //takeDamage(1); 
        //-> There's a BUG where this method seems to stack effect and instant kill or send enemy flying.
        //WAIT for AOE
        yield return new WaitForSeconds(6);
        //Return enemy movement and attack to normal
        AgentStart();
        enemyMovement = 5;
    }

    //Animations
    Animator eAnim;
    [Header("Speed")]
    // How fast enemy moves
    [SerializeField] float enemyMovement;
    // multiplies by walk enemyMovement speed for chasing speed
    [SerializeField] int enemyRunMultiplier;
    public float rotationSpeed;

    [Header("Ranges")]
    // The distance the enemy will begin to chase player
    public float chaseRange;
    public float attackRange;

    [Header("Patrol (only need waypoints if !isStationary)")]
    [SerializeField] bool isStationary;
    public Transform waypoint1;
    public Transform waypoint2;

    #endregion



    // Start is called before the first frame update
    void Start()
    {
        #region Get Components
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        eAnim = gameObject.GetComponent<Animator>();
        hpBar = transform.Find("Clown/Canvas/Enemy HP Bar").GetComponent<Image>();

        Player = GameObject.FindGameObjectWithTag("Player");
        target = Player.transform;
        cm = Player.GetComponent<CharacterMechanics>();
        #endregion

        #region default values
        //sets maxHP to beginning hp in order to get the correct fill amount for hpbar
        int maxHP = hp;
        myEnemyClown = EnemyState.Start;
        rb.isKinematic = true;

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
        if (!isStationary)
        {
            Patrol();
        }
        eAnim.SetBool("Stationary", isStationary);

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Enemy State:" + myEnemyClown);
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
            if (!isStationary)
            {
                if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange)
                {
                    myEnemyClown = EnemyState.Chase;
                    Chase();
                }
                // If enemy within attackrange stop moving and attack
                // If enemy within chaserange chase player
                // else go back to patrol route
                if (myEnemyClown == EnemyState.Patrol)
                {
                    Patrol();
                    agent.isStopped = false;
                }
                else
                {
                    eAnim.SetBool("PlayerSpotted", true);
                    if (Vector3.Distance(target.position, gameObject.transform.position) > attackRange)
                    {
                        myEnemyClown = EnemyState.Chase;
                        Chase();
                        //Debug.LogWarning("");

                    }
                    else if (Vector3.Distance(target.position, gameObject.transform.position) <= attackRange)
                    {
                        eAnim.SetBool("Chase", false);
                        agent.isStopped = true;
                        myEnemyClown = EnemyState.Attack;
                        Attack();
                    }
                }
            }
            //IsStationary
            else
            {
                if (Vector3.Distance(target.position, gameObject.transform.position) <= attackRange)
                {
                    eAnim.SetBool("Chase", false);
                    agent.isStopped = true;
                    //Sets Destination to player so that the enemy will turn towards player
                    //Will not move is agent.isStopped = true
                    agent.SetDestination(Player.transform.position);
                    myEnemyClown = EnemyState.Attack;
                    Attack();
                }
            }
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            float str = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
            //transform.LookAt(targetPosition);
        }
        #endregion
    }
    #region health/death
    public void takeDamage(int dmg)
    {
        //Debug.Log(dmg + "Damage Taken");
        agent.isStopped = true;
        hp -= dmg;
        if (hp <= 0 && !death)
        {
            death = true;
            Debug.Log("Enemy has been killed");
            agent.isStopped = true;
            // so that enemy doesn't move after dying
            eAnim.SetTrigger("Death");
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
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
    #endregion

    #region stun/knockback
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
            StartCoroutine(Stun());
            //enemyMovement = 1;
            rb.velocity = Vector3.zero;
            //agent.isStopped = true;
            //Stop attacking
            //AgentStop();
            takeDamage(3);
        }
        if(collision.gameObject.tag== "WhirlwindAOE")
        {
            takeDamage(3);
        }
        if(collision.gameObject.tag== "Attack Zone")
        {
            takeDamage(3);
        }
        
        if(collision.gameObject.tag == "")
        {
            takeDamage(2);
        }
        if (collision.gameObject.tag == "Dash Collider")
        {
            takeDamage(1);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "HammerSmashAOE")
        {
            #region Debug Log
            //Debug.Log("Ranged enemy has regained it's speed!");
            #endregion
            //Give enemies back their speed after hammer smash AOE
            //enemyMovement = 5;
        }
    }

   
    #endregion
    #region States Chase, Patrol, Attack (Ranged)
    public void Chase()
    {
        agent.isStopped = false;
        eAnim.SetBool("Chase", true);
        // Sets player as destination
        //agent.SetDestination(target.transform.position);
        agent.SetDestination(Player.transform.position);
    }

    private void Patrol()
    {
        // At the beginning of patrolling sets first patrol destination
        if (myEnemyClown != EnemyState.Patrol)
        {
            myEnemyClown = EnemyState.Patrol;
            agent.SetDestination(waypoint1.position);
        }
    }
    //Ranged Attack 
    private void Attack()
    {
        timer += Time.deltaTime;
        if (timer > attackTimer)
        {
            timer = 0f;
            eAnim.SetTrigger("Attack");
        }
    }

    // Used for enemy animations and patrolling between waypoints
    private void OnTriggerEnter(Collider other)
    {
        // During patrol alternate going between Waypoint1 and Waypoint2
        // On colliding with waypoint sets other as destination
        if (myEnemyClown == EnemyState.Patrol)
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
        if (other.gameObject.tag == "PlayerRanged")
        {
            Debug.Log("Hit with Ranged");
            takeDamage(1);
        }

        if (other.CompareTag("Hammer2"))
        {
            if (cm.isAttacking)
            {
                takeDamage(2);
            }
        }
    }

    //Gets called by Throw animation on frame 12
    public void SpawnProjectile()
    {
        if (projectilePrefab)
        {
            Rigidbody rb = Instantiate(projectilePrefab,
                projectileSpawnPoint.position,
                projectileSpawnPoint.rotation) as Rigidbody;

            rb.AddForce(transform.forward * projectilePrefab.GetComponent<Projectiles>().projectileSpeed, ForceMode.Impulse);
        }
    }
    #endregion
}
