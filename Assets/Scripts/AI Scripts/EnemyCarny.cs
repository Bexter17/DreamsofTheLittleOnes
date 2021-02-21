using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

//Combat Script
public class EnemyCarny : MonoBehaviour
{
    #region Variables

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

    //used to track the player for giveDamage function 
    private GameObject Player;

    [SerializeField] enum EnemyState { Start, Patrol, Chase, Attack, Stun };
    EnemyState myEnemy;

    [SerializeField] GameObject attackBoxRange;

    //Used to stun the enemy, wait a few seconds for AOE then return to normal function. 
    //Had to use IEnumerator because I couldn't get yield return new WaitForSeconds() to work anywhere else in script. Would like to 
    //plug this into an official state as you can see I have inerted Stun into the enemyState and began the other necessary fields.
    IEnumerator Stun()
    {
        Debug.Log("ENEMY HAS BEEN STUNNED FOR 6 SECONDS BY HAMMER SMASH");
        //Stop enemy movement
        enemyMovement = 0;
        //Stop enemy attack
        agent.isStopped = true;
        //Damage enemy
        hp -= 2;
        //takeDamage(1); - Had to change the hp variable alone because takeDamage was applying knockback.
        //> There's a BUG where this method seems to stack effect and instant kill or send enemy flying.
        AgentStop();
        //WAIT for AOE
        yield return new WaitForSeconds(6);
        //Return enemy movement and attack to normal
        AgentStart();
        enemyMovement = 5;
    }


    //Animations
    Animator eAnim;

    // How fast enemy moves
    [SerializeField] float enemyMovement;
    // multiplies by walk enemyMovement speed for chasing speed
    [SerializeField] int enemyRunMultiplier;

    // The distance the enemy will begin to chase player
    public float punchRange;
    public float chaseRange;
    public float attackRange;
    private float checkStackRange = 6;


    //bool isMoving = true;
    bool isPatrolling = false;
    bool getCalled = false;


    public Transform waypoint1;
    public Transform waypoint2;

    // Amount of damage done by enemy to player
    public int dmgDealt = 2;

    public float rotationSpeed;
    // Start is called before the first frame update
    #endregion
    #region EncircleVariables
    [SerializeField] GameObject[] circlePoints;
    private int encircleNum;
    [SerializeField] float circleDist;
    private bool onStack = false;
    EnemyStack stackTracker;
    
    #endregion
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
        if (punchRange <= 0)
        {
            punchRange = 2;
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
        #endregion

        circlePoints[0] = GameObject.FindGameObjectWithTag("Enemy Slot 1");
        circlePoints[1] = GameObject.FindGameObjectWithTag("Enemy Slot 2");
        circlePoints[2] = GameObject.FindGameObjectWithTag("Enemy Slot 3");
        circlePoints[3] = GameObject.FindGameObjectWithTag("Enemy Slot 4");
        // circle points 4 different points around the player where the enemies will go to attack
        //circlePoints = new Vector3[4];

        stackTracker = GameObject.Find("Enemy Stack Tracker").GetComponent<EnemyStack>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawRay()
        //Sets hp text to change based on players perspective
        //So it's not backwards to the player
        //Vector3 textDirection = transform.position - target.transform.position;

        //Used for testing enemy death
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

            // If enemy within attackrange stop moving and attack
            // If enemy within chaserange chase player
            // else go back to patrol route
            if (Vector3.Distance(target.position, gameObject.transform.position) < punchRange)
            {
                eAnim.SetBool("isAttacking", true);
            }
            else
            {
                eAnim.SetBool("isAttacking", false);
            }
            if (onStack)
            {
                ResetMovement();
                agent.SetDestination(target.transform.position);
                myEnemy = EnemyState.Attack;
            }
            if (Vector3.Distance(target.position, gameObject.transform.position) < checkStackRange)
            {
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
                    enemyMovement = 0;
                }

            }
            else if (Vector3.Distance(target.position, gameObject.transform.position) < chaseRange)
            {
                Chase();
                agent.isStopped = false;
                myEnemy = EnemyState.Chase;
                if(Vector3.Distance(target.position, gameObject.transform.position) < chaseRange - (enemyMovement*enemyRunMultiplier*0.5))
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
                //Debug.Log("Run");
            }
            //else if (myEnemy == EnemyState.Stun)
            //{
            //Stun();
            //}
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
            stackTracker.RemoveStack(gameObject);
            agent.speed = 0;
            death = true;
            Debug.Log("Enemy has been killed");
            enemyMovement = 0;
            rotationSpeed = 0;
            AgentStop();
            // so that enemy doesn't move after dying
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

        //Debug.Log("Knockback");
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
        if (collision.gameObject.tag =="HammerSmashAOE")
        {
            #region Debug Log
            Debug.Log("Enemy has been hit by hammer smash!");
            #endregion
            //Slow down enemies in contact with hammer smash AOE 
            //enemyMovement = 0;
            //Stop attacking                                        -> Moved to IEnumerator for WaitForSeconds function
            //AgentStop();
            //yield return new WaitForSeconds(5);
            //enemyMovement = 5;
            StartCoroutine(Stun());
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
            //enemyMovement = 5;
        }
    }
    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        Debug.LogWarning("ENEMY STARTED ATTACKING");
    //        isPunching = true;
    //        //punches++;
    //        //enemyMovement = 0;
    //        //if (punches % punchCooldown == 0)
    //        //{
    //        //    eAnim.SetTrigger("isPunching");
    //        //    Invoke("ResetMovement", 1);
    //        //}
    //    }
    //}

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
        if (encircleNum == 5)
        {
            //Idle
        }
        else
        {
            ResetMovement();
            agent.SetDestination(circlePoints[encircleNum].transform.position);
        }

    }
    public void Called()
    {
        getCalled = true;
        Chase();
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
        //if (other.CompareTag("Player"))
        //    eAnim.SetTrigger("isPunching");

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
        //else
        //{
        //    if (other.CompareTag("Player"))
        //    {
        //        Debug.LogWarning("ENEMY STARTED ATTACKING");
        //        isPunching = true;
        //    }
        //}
    }

    //private void Stun()
    //{
        //myEnemy = EnemyState.Stun;
        //enemyMovement = 0;
        //yield return new WaitForSeconds(4);
        //enemyMovement = 5;
        //Chase();
    //}

    #endregion

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

    private void UpdateCirclePoints()
    {
        //circlePoints[0] = target.position + new Vector3(circleDist, 0, 0);
        //circlePoints[1] = target.position + new Vector3(0, 0, circleDist);
        //circlePoints[2] = target.position + new Vector3(-circleDist, 0, 0);
        //circlePoints[3] = target.position + new Vector3(0, 0, -circleDist);
    }
    private void ResetMovement()
    {
        enemyMovement = 3;
    }

    private void giveDamage()
    {
        Player.SendMessage("takeDamage", dmgDealt);
    }

}
