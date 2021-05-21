using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

public class TestCarny : MonoBehaviour
{
    /*
    Test AI
    Created May 11th, 2021

    Script created to remove features not needed in testing
    */

    #region Components

    //Essentials
    private Rigidbody rb;
    private Transform target;
    public NavMeshAgent agent;
    Animator animator;
    CombatManager CombatScript;
    CharacterMechanics cm;
    //used to track the player for giveDamage function
    private GameObject Player;

    #endregion

    #region Variables

    [Header("Debugs")]
    [SerializeField] bool stateDebug;
    [SerializeField] bool combatDebug;
    [SerializeField] bool knockbackDebug;

    [Header("Enemy Stats")]
    public int currentHealth;

    private int maxHealth;
    
    private Image hpBar;
    
    public bool death = false;
    
    [SerializeField] private GameObject ragdoll;

    private bool randNumGenerated = false;

    enum EnemyState { Start, Chase, Attack, Stun, lockChase };
    EnemyState testDummy;

    #region Knockback 

    [SerializeField] float knockbackForce;

    [SerializeField] private float knockDistanceModifier = 400;
    
    private float knockDuration = .3f;
    
    private float knockPause = 1;

    #endregion

    #region Combat 

    private float attackRange = 3;
    
    public float chaseRange = 10;

    private float checkStackRange = 10;

    public int attackDmg = 2;
    
    private bool ableToDamage = false;

    #endregion

    //bool isPatrolling = false;
    bool getCalled = false;


    
    [SerializeField] private int numberOfAttacks;

    // How fast enemy moves
    private float movementSpeed = 3;
    // multiplies by walk movementSpeed speed for chasing speed
    private int runSpeed = 2;
    private float rotationSpeed = 3;

    #endregion

#region Stun
    //Used to stun the enemy, wait a few seconds for AOE then return to normal function. 
    //Had to use IEnumerator because I couldn't get yield return new WaitForSeconds() to work anywhere else in script. Would like to 
    //plug this into an official state as you can see I have inerted Stun into the enemyState and began the other necessary fields.
    //TODO does enumerator need to be here?
    IEnumerator Stun()
    {
        Debug.Log("ENEMY HAS BEEN STUNNED FOR 6 SECONDS BY HAMMER SMASH");
        //Stop enemy movement
        //Stop enemy attack
        agent.isStopped = true;
        //Damage enemy
        currentHealth -= 2;
        //takeDamage(1); - Had to change the hp variable alone because takeDamage was applying knockback.
        //> There's a BUG where this method seems to stack effect and instant kill or send enemy flying.
        //TODO rename functions to be more descriptive
        AgentStop();
        //WAIT for AOE
        yield return new WaitForSeconds(6);
        //Return enemy movement and attack to normal
        AgentStart();
        movementSpeed = 5;
    }

    // Start is called before the first frame update
    #endregion

    #region EncircleVariables
    private GameObject[] circlePoints;
    private int encircleNum = -1;
    private float circleDist = .5f;
    private bool onStack = false;
    public EnemyStack stackTracker;
    //Used to ensure onStack only activates once
    private bool hasStacked = true;

    #endregion

    void Start()
    {
        #region Components
        //ESSENTIALS
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;

        hpBar = transform.Find("Carny/Canvas/Enemy HP Bar").GetComponent<Image>();

        cm = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMechanics>();

        agent = GetComponent<NavMeshAgent>();

        animator = gameObject.GetComponent<Animator>();

        Player = GameObject.FindGameObjectWithTag("Player");

        target = GameObject.FindGameObjectWithTag("Player").transform;

        CombatScript = GameObject.Find("GameManager").GetComponent<CombatManager>();

        #endregion

        #region Initialize

if(maxHealth <= 0)
            maxHealth = 5;

        currentHealth = maxHealth;

        testDummy = EnemyState.Start;


        // Default values
        if (movementSpeed <= 0)
            movementSpeed = 3f;

        if (attackRange <= 0)
            attackRange = 2;

        if (chaseRange <= 0)
            chaseRange = 5f;

        if (attackDmg <= 0)
            attackDmg = 2;

        if (runSpeed <= 0)
            runSpeed = 4;

        if (knockbackForce <= 0)
            knockbackForce = 3;

        #endregion

        
         
        #region stackTracker
        stackTracker = GameObject.Find("Enemy Stack Tracker").GetComponent<EnemyStack>();

        if(GameObject.FindGameObjectWithTag("Enemy Slot 1") != null)
        {
            circlePoints[0] = GameObject.FindGameObjectWithTag("Enemy Slot 1");
            circlePoints[1] = GameObject.FindGameObjectWithTag("Enemy Slot 2");
            circlePoints[2] = GameObject.FindGameObjectWithTag("Enemy Slot 3");
            circlePoints[3] = GameObject.FindGameObjectWithTag("Enemy Slot 4");
        }

        //circle points 4 different points around the player where the enemies will go to attack
       // circlePoints = new Vector3[4];

        #endregion

    }
    // Update is called once per frame
    void Update()
    {
        if(stateDebug)
        Debug.Log("Enemy State :" + testDummy);

        /*
        //Kill Switch
        if (Input.GetKeyDown("t"))
        {
            Debug.Log("Enemy has lost 1 hp");
            takeDamage(1);
        }
        */

        #region AI States
        if (agent.enabled && !death)
        {
            Vector3 targetPosition = agent.destination;
            targetPosition.y = transform.position.y;

            if (onStack && !ableToDamage)
            {
                agent.SetDestination(target.transform.position);
            }
            //Only activates once
            if (onStack && hasStacked)
            {
                agent.isStopped = false;
                hasStacked = false;
            }
            NavMeshHit hit;
            if (Vector3.Distance(target.position, gameObject.transform.position) < checkStackRange && !agent.Raycast(target.position, out hit))
            {
                Debug.Log("Enemy To Stack");
                if (!onStack)
                {
                    int stackNum = stackTracker.AddStack(gameObject);
                    if (stackNum == 5)
                    {
                        onStack = false;
                    }
                    else
                    {
                        encircleNum = stackNum;
                        onStack = true;
                    }
                    agent.isStopped = true;
                }

            }
            else if (testDummy == EnemyState.lockChase)
            {
                editChase();
                Debug.Log("1111111111111111111111111111111111111");
            }
            else if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange && !agent.Raycast(target.position, out hit))
            {
                Chase();
                agent.isStopped = false;
                testDummy = EnemyState.Chase;
                if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange - (movementSpeed * runSpeed * 0.5))
                {
                    getCalled = false;
                }
            }

            /*
             
            else if (testDummy != EnemyState.Patrol && !getCalled)
            {
                Patrol();
                agent.isStopped = false;
                testDummy = EnemyState.Patrol;
            }

            */

            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            
            float str = rotationSpeed * Time.deltaTime;
            
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);

            /*
            if (testDummy == EnemyState.Patrol)
            {
                Patrol();
                agent.speed = movementSpeed;
            }
            */

            if (testDummy == EnemyState.Chase)
            {
                Chase();
                agent.speed = runSpeed * movementSpeed;
                //Debug.Log("Run");
            }
            else if (testDummy == EnemyState.Attack)
            {
                //Generates random number once per attack from 1-3 to randomly choose 1 of 3 attacks
                //Will generate number once on the main tree and can do so again after each attack
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Chase Tree") && !randNumGenerated)
                {
                    //1-3
                    //Set to 1, 4 once third animation is added
                    animator.SetInteger("randAttk", Random.Range(1, numberOfAttacks + 1));
                    randNumGenerated = true;
                }
                else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack 1") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack 2") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack 3"))
                {
                    randNumGenerated = false;
                }
                animator.SetBool("isAttacking", true);
                animator.SetTrigger("Attack");
            }
            if (testDummy != EnemyState.Attack)
            {
                animator.SetBool("isAttacking", false);
            }
            //else if (testDummy == EnemyState.Stun)
            //{
            //Stun();
            //}
        }
        #endregion
    }
    //COLLISIONS
    #region Collisions
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && agent.isOnNavMesh)
        {
            rb.velocity = Vector3.zero;
            agent.isStopped = true;
            AgentStop();
        }
        else if (collision.gameObject.tag == "HammerSmashAOE")
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
            //AgentStop();
            //yield return new WaitForSeconds(5);
            //movementSpeed = 5;
            takeDamage(3);
            StartCoroutine(Stun());

            if (rb)
            {
                Vector3 direction = transform.position - collision.transform.position;
                direction.y = 0;

                rb.AddForce(direction.normalized * knockbackForce, ForceMode.Impulse);
            }

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Knocked Back!");
            }
        }
        else if (collision.gameObject.tag == "WhirlwindAOE")
        {
            //Deals small knockback from takeDamage function
            #region Debug Log
            Debug.Log("Enemy has been hit by whirlwind!");
            #endregion
            takeDamage(3);
        }

        else if (collision.gameObject.tag == "Attack Zone")
        {
            takeDamage(1);
        }

        if (collision.gameObject.tag == "Hammer")
        {
            if (cm.isAttacking)
            {
                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Hit by Basic Attack!");
                    Debug.Log(this.transform.name + " Damage Applied!");
                }

                takeDamage(2);

                if (rb)
                {
                    Vector3 direction = transform.position - collision.transform.position;
                    direction.y = 0;

                    rb.AddForce(direction.normalized * knockbackForce, ForceMode.Impulse);
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
                    Vector3 direction = transform.position - collision.transform.position;
                    direction.y = 0;

                    rb.AddForce(direction.normalized * knockbackForce, ForceMode.Impulse);
                }

                if (combatDebug)
                {
                    Debug.Log(this.transform.name + " Knocked Back!");
                }
            }
        }
        if (collision.gameObject.tag == "Dash Collider")
        {
            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Hit by Dash!");
                Debug.Log(this.transform.name + " Damage Applied!");
            }

            takeDamage(1);

            if (rb)
            {
                Vector3 direction = transform.position - collision.transform.position;
                direction.y = 0;

                rb.AddForce(direction.normalized * knockbackForce, ForceMode.Impulse);
            }

            if (combatDebug)
            {
                Debug.Log(this.transform.name + " Knocked Back!");
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "HammerSmashAOE")
        {
            #region Debug Log
            //Debug.Log("Enemy has been hit by hammer smash!");
            #endregion
            //Give enemies back their speed after hammer smash AOE
            //movementSpeed = 5;
        }
    }
    private void OnTriggerEnter(Collider other)
    {

        // During patrol alternate going between Waypoint1 and Waypoint2
        // On colliding with waypoint sets other as destination
        // Patrolling now works regardless of what order waypoints are in
        /*
        if (testDummy == EnemyState.Patrol)
        {
            if (advancedPatrol)
            {
                if (other.gameObject.transform == waypoints[patrolNumber].transform)
                {
                    if (patrolNumber == waypoints.Length - 1 && patrolIterator == 1)
                    {
                        patrolIterator = -1;
                    }
                    else if (patrolNumber == 0 && patrolIterator == -1)
                    {
                        patrolIterator = 1;
                    }
                    patrolNumber += patrolIterator;
                    agent.SetDestination(waypoints[patrolNumber].transform.position);
                }
            }
            else
            {
                if (other.gameObject.transform == waypoint1.transform)
                {
                    agent.SetDestination(waypoint2.transform.position);
                }
                else if (other.gameObject.transform == waypoint2.transform)
                {
                    agent.SetDestination(waypoint1.transform.position);
                }

            }

        }
        else
        {
        */
            if (other.CompareTag("Player"))
            {
                animator.SetBool("cancelAttk", false);
                if (testDummy == EnemyState.Chase && onStack)
                {
                    testDummy = EnemyState.Attack;
                }
                //Debug.LogWarning("Enemy Start Collision With Player");
                rb.isKinematic = false;
                ableToDamage = true;
                agent.isStopped = true;
                animator.SetFloat("Speed", 0);
            }
        //}

        //if(other.CompareTag("Hammer"))
        //{
        //    if(cm.isAttacking)
        //    {
        //        takeDamage(2);
        //    }
        //}

        //TODO + parameter to take damage to edit knockback
        //So that ranged attack doesn't knockback as much as melee
        if (other.gameObject.tag == "PlayerRanged")
        {
            Debug.Log("Hit with Ranged");
            takeDamage(1);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("cancelAttk", true);
            ableToDamage = false;
            agent.isStopped = false;
            animator.SetFloat("Speed", 1);
            testDummy = EnemyState.Chase;
        }
    }
    #endregion
    #region damage
    public void takeDamage(int dmg)
    {
        //Debug.Log(dmg + "Damage Taken");
        agent.isStopped = true;
        currentHealth -= dmg;
        if (currentHealth <= 0 && !death)
        {
            //transform.GetComponent<CapsuleCollider>().enabled = false;
            Vector3 ragdollPos = transform.position;
            ragdollPos.y -= 3.25f;
            GameObject temp = Instantiate(ragdoll, ragdollPos, transform.rotation);
            //temp.transform.localScale = new Vector3(3.25f, 3.25f, 3.25f);
            Destroy(gameObject);
            death = true;
            stackTracker.RemoveStack(gameObject);
            agent.speed = 0;
            Debug.Log("Enemy has been killed");
            movementSpeed = 0;
            rotationSpeed = 0;
            // so that enemy doesn't move after dying
            animator.SetBool("IsDying", true);
            animator.SetTrigger("IsDead");

            //Destroy(gameObject);   Destroy object is called in EnemyAI1 when the death animation is played
        }
        hpBar.fillAmount = (float)(currentHealth * 0.2);

        //KNOCKBACK
        // Gets the difference between enemy and player position
        // To knockback enemy away from player
        rb.isKinematic = false;
        //agent.enabled = false;
        rb.AddForce(-transform.forward * knockDistanceModifier);
        //rb.AddForce(transform.up * knockHeightModifier);

        //Debug.Log("Knockback");
        //Invokes once enemy is no longer being knocked back and pauses movement
        Invoke("AgentStop", knockDuration);
    }
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
    #endregion
    //TODO rename to be more descriptive
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
        movementSpeed = 3;
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    #region init States
    public void Chase()
    {
        //agent.isStopped = false;
        //Debug.Log("CHASE");
        testDummy = EnemyState.Chase;
        // Sets player as destination
        //agent.SetDestination(target.transform.position);
        //UpdateCirclePoints();

        // doesn't work if stack call returns 5 which means not on stack
        // or -1 which means still not changed
        //if (encircleNum < 4 && encircleNum >= 0 && circlePoints[encircleNum] != null)
        //{
        //    agent.isStopped = false;
        //    agent.SetDestination(circlePoints[encircleNum].transform.position);
        //}
        if (encircleNum < 3 && encircleNum >= 0)
        {
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }


    }
    //using this funciton to set a chase destination to spawned enemy
    public void editChase()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        agent.isStopped = false;
        testDummy = EnemyState.lockChase;
        agent.SetDestination(Player.transform.position);

    }
    public int getstatus()
    {
        /*
        if (testDummy == EnemyState.Patrol)
        {
            return 1;
        }
        */
        if (testDummy == EnemyState.Chase)
        {
            return 2;
        }
        else if (testDummy == EnemyState.lockChase)
        {
            return 3;
        }
        else if (testDummy == EnemyState.Attack)
        {
            return 4;
        }
        return 0;
    }
    public void Called()
    {
        getCalled = true;
        Chase();
    }



    // Turns around and continues
    private void Patrol()
    {
        //Debug.Log("PATROL");
        // At the beginning of patrolling sets first patrol destination

        /*
        if (advancedPatrol)
        {
            if (testDummy != EnemyState.Patrol)
            {
                testDummy = EnemyState.Patrol;
                agent.SetDestination(waypoints[0].transform.position);
            }
        }
        else
        {
            if (testDummy != EnemyState.Patrol)
            {
                testDummy = EnemyState.Patrol;
                agent.SetDestination(waypoint1.transform.position);
            }
        }
        */

    }

    // Used for enemy animations and patrolling between waypoints
    #endregion

    public void changeStackrange(float i)
    {
        checkStackRange = i;
    }

    public void Attack()
    {
        if (ableToDamage)
        {
            CombatScript.GivePlayerDamage(this.transform, attackDmg);
        }
    }

    public void DoubleAttack()
    {
        if (ableToDamage)
        {
            CombatScript.GivePlayerDamage(this.transform, attackDmg / 2);
        }
    }
}

