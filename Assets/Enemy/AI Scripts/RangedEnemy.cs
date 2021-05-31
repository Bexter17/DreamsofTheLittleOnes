using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class RangedEnemy : MonoBehaviour
{
    //May 5, 2021
    #region Variables

    [Header("Essentials")]
    //HP
    public int hp = 5;
    private int maxHP;
    private Image hpBar;

    public Rigidbody rb;
    public Transform target;
    [SerializeField] private GameObject ragdoll;
    [SerializeField] Rigidbody projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] float attackTimer = 0;
    private bool enemyDissolveIn = false;

    [Header("Knockback")]
    [SerializeField] float knockDistanceModifier;
    [SerializeField] float knockDuration;
    [SerializeField] float knockPause;
    [SerializeField] float basicKnockbackForce;
    [SerializeField] float smashKnockbackForce;
    [SerializeField] float dashKnockbackForce;
    [SerializeField] float whirlKnockbackForce;
    [SerializeField] float rangeKnockbackForce;
    private Vector3 knockbackDirection;
    private int staggerCounter = 0;

    [Header("Debugs")]
    [SerializeField] bool stateDebug;
    [SerializeField] bool combatDebug;
    [SerializeField] bool knockbackDebug;

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

    //TakeDamage Cooldown
    private float damageInterval = 0;
    private bool canTakeDamage = true;

    IEnumerator Stun()
    {
        Debug.Log("ENEMY HAS BEEN STUNNED FOR 6 SECONDS BY HAMMER SMASH");
        //Stop enemy movement
        enemyMovement = 0;
        //Stop enemy attack
        AgentStop();
        //Damage Enemy - Had to change hp variable instead of takeDamage because it applies knockback when we want a stun effect
        //hp -= 2;
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
    //Layer used for raycast
    public LayerMask playerLayer;

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


        #endregion
        if (isStationary)
        {
            eAnim.SetBool("Stationary", true);
        }
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Enemy State:" + myEnemyClown);
        //if (Input.GetKeyDown("t"))
        //{
        //    Debug.Log("Enemy has lost 1 hp");
        //    takeDamage(1);
        //}

        if (!enemyDissolveIn && !death)
        {
            gameObject.GetComponent<VFXenemies>().spawnIn = true;
            enemyDissolveIn = true;
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
                rb.isKinematic = true;
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
        //agent.isStopped = true;
        Debug.LogError("CLOWNHP: " + hp);
        if (canTakeDamage)
        {
            hp -= dmg;
            canTakeDamage = false;
        }
        Debug.Log("Enemy Damage Taken" + dmg);
        
        if (hp <= 0 && !death)
        {
            transform.GetComponent<CapsuleCollider>().enabled = false;
            Vector3 ragdollPos = transform.position;
            ragdollPos.y -= 1.5f;
            GameObject temp = Instantiate(ragdoll, ragdollPos, transform.rotation);
            temp.transform.localScale = new Vector3(3.25f, 3.25f, 3.25f);
            RagdollPhysics ragdollPhysics = temp.GetComponent<RagdollPhysics>();
            Vector3 currentVelocity = rb.velocity;
            ragdollPhysics.GetVelocity(currentVelocity);
            Destroy(gameObject);
            death = true;
            //Debug.Log("Enemy has been killed");
            //agent.isStopped = true;
            //eAnim.SetTrigger("Death");
        }
        hpBar.fillAmount = (float)(hp * 0.02);

        //KNOCKBACK
        // Gets the difference between enemy and player position
        // To knockback enemy away from player
        //rb.isKinematic = false;
        //agent.enabled = false;
        //rb.AddForce(-transform.forward * knockDistanceModifier);
        //rb.AddForce(transform.up * knockHeightModifier);

        Debug.Log("Knockback");
        //Invokes once enemy is no longer being knocked back and pauses movement
        //Invoke("AgentStop", knockDuration);
    }
    private void FixedUpdate()
    {
        if(canTakeDamage == false)
        {
            damageInterval += Time.deltaTime;
            if (damageInterval >= 0f)
            {
                damageInterval = 0;
                canTakeDamage = true;
            }
            else
            {
                canTakeDamage = false;
            }
        }

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
        if (collision.gameObject.CompareTag("Player") && agent.isOnNavMesh)
        {
            rb.velocity = Vector3.zero;
            agent.isStopped = true;
            AgentStop();
        }
        else if (collision.gameObject.CompareTag("HammerSmashAOE"))
        {
            #region Debug Log

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Hit by Hammersmash!");
                Debug.Log(this.transform.name + " Damage Applied!");
            }

            #endregion
            //Slow down enemies in contact with hammer smash AOE 
            //movementSpeed = 0;
            //Stop attacking                                        -> Moved to IEnumerator for WaitForSeconds function
            takeDamage(35);
            //StartCoroutine(Stun());

            if (rb)
            {
                eAnim.SetTrigger("staggerBack");
                knockbackDirection = transform.position - collision.transform.position;
                knockbackDirection.y = 0;

                rb.AddForce(knockbackDirection.normalized * smashKnockbackForce, ForceMode.Impulse);
            }

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Knocked Back!");
            }
        }
        else if (collision.gameObject.CompareTag("WhirlwindAOE"))
        {
            //Deals small knockback from takeDamage function
            #region Debug Log
            Debug.Log("Enemy has been hit by whirlwind!");
            #endregion
            takeDamage(20);
        }
        else if (collision.gameObject.CompareTag("Attack Zone"))
        {
            takeDamage(3);
        }

        if (collision.gameObject.CompareTag("Hammer"))
        {
            if (cm.isAttacking)
            {
                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Hit by Basic Attack!");
                    Debug.Log(this.transform.name + " Damage Applied!");
                }
                //Debug.Log("Enemy Basic Attack");
                takeDamage(5);

                if (rb)
                {
                    //Stagger enemy 1/4 hits
                    if (staggerCounter >= 3)
                    {
                        staggerCounter = 0;
                        eAnim.SetTrigger("staggerBack");
                    }
                    else
                    {
                        staggerCounter++;
                    }

                    knockbackDirection = transform.position - collision.transform.position;
                    //knockbackDirection = Vector3.right;
                    knockbackDirection.y = 0;

                    rb.AddForce(knockbackDirection.normalized * basicKnockbackForce, ForceMode.Impulse);
                }

                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Knocked Back!");
                }
            }

            if (cm.isSpinning)
            {
                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Hit by Whirlwind!");
                    Debug.Log(this.transform.name + " Damage Applied!");
                }

                takeDamage(4);

                if (rb)
                {
                    eAnim.SetTrigger("staggerBack");
                    knockbackDirection = transform.position - collision.transform.position;
                    knockbackDirection.y = 0;

                    rb.AddForce(knockbackDirection.normalized * whirlKnockbackForce, ForceMode.Impulse);
                }

                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Knocked Back!");
                }
            }
        }
        if (collision.gameObject.CompareTag("Dash Collider"))
        {
            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Hit by Dash!");
                Debug.Log(this.transform.name + " Damage Applied!");
            }

            takeDamage(7);

            if (rb)
            {
                eAnim.SetTrigger("staggerBack");
                knockbackDirection = transform.position - collision.transform.position;
                knockbackDirection.y = 0;

                rb.AddForce(knockbackDirection.normalized * dashKnockbackForce, ForceMode.Impulse);
            }

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Knocked Back!");
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("HammerSmashAOE"))
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
        RaycastHit hit;
        Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), transform.TransformDirection(Vector3.forward), out hit, playerLayer);
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), transform.TransformDirection(Vector3.forward));
        if (Time.time - attackTimer > 1.0f && hit.collider.tag == "Player")
        {
            eAnim.SetTrigger("Attack");
            attackTimer = Time.time;
        }
    }

    // Used for enemy animations and patrolling between waypoints
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Clown Ontrigger");
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
                //takeDamage(2);
            }

            if (cm.isSpinning)
            {
                Debug.Log("Hit by whirlwind");
                takeDamage(4);
            }
        }
        else if(other.CompareTag("Player"))
        {
            rb.isKinematic = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            rb.isKinematic = true;
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
