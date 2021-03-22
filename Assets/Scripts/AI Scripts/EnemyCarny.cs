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
    //TODO removed most of serializefields to a minimum

    //ESSENTIALS
    private Rigidbody rb;
    private Transform target;
    NavMeshAgent agent;
    Animator eAnim;
    CharacterMechanics cm;
    //used to track the player for giveDamage function
    private GameObject Player;

    //HP
    public int hp = 5;
    private int maxHP;
    private Image hpBar;
    public bool death = false;

    //STATES
    [SerializeField] enum EnemyState { Start, Patrol, Chase, Attack, Stun, lockChase };
    EnemyState myEnemy;
    //TODO remove
    private float knockDistanceModifier = 400;
    private float knockDuration = .3f;
    private float knockPause = 1;

    // The distance the enemy will begin to chase player
    private float punchRange = 3;
    public float chaseRange = 10;
    private float checkStackRange = 6;

    bool isPatrolling = false;
    bool getCalled = false;

    private GameObject waypoint1;
    private GameObject waypoint2;
    private GameObject[] potentialWaypoints;
    public GameObject[] waypoints;
    public bool advancedPatrol;
    private int patrolNumber = 0;
    private int patrolIterator = 1;

    // How fast enemy moves
    private float enemyMovement = 3;
    // multiplies by walk enemyMovement speed for chasing speed
    private int enemyRunMultiplier = 2;
    private float rotationSpeed = 3;

    // Amount of damage done by enemy to player
    public int dmgDealt = 2;

    //Used to stun the enemy, wait a few seconds for AOE then return to normal function. 
    //Had to use IEnumerator because I couldn't get yield return new WaitForSeconds() to work anywhere else in script. Would like to 
    //plug this into an official state as you can see I have inerted Stun into the enemyState and began the other necessary fields.
    //TODO does enumerator need to be here?
    IEnumerator Stun()
    {
        Debug.Log("ENEMY HAS BEEN STUNNED FOR 6 SECONDS BY HAMMER SMASH");
        //Stop enemy movement
        enemyMovement = 0;
        //Stop enemy attack
        //TODO check between agent.isStopped and setting enemyMovement = 0 see which one works and put it in an easily accessible function
        agent.isStopped = true;
        //Damage enemy
        hp -= 2;
        //takeDamage(1); - Had to change the hp variable alone because takeDamage was applying knockback.
        //> There's a BUG where this method seems to stack effect and instant kill or send enemy flying.
        //TODO rename functions to be more descriptive
        AgentStop();
        //WAIT for AOE
        yield return new WaitForSeconds(6);
        //Return enemy movement and attack to normal
        AgentStart();
        enemyMovement = 5;
    }

    // Start is called before the first frame update
    #endregion
    #region EncircleVariables
    [SerializeField] GameObject[] circlePoints;
    private int encircleNum = -1;
    private float circleDist = .5f;
    private bool onStack = false;
    EnemyStack stackTracker;
    
    #endregion
    void Start()
    {
        //TODO reorder for aesthetics
        //ESSENTIALS
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        Player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.Find("Player").transform;
        hpBar = transform.Find("vampire/Canvas/Enemy HP Bar").GetComponent<Image>();
        cm = GameObject.Find("Player").GetComponent<CharacterMechanics>();

        //sets maxHP to beginning hp in order to get the correct fill amount for hpbar
        int maxHP = hp;
        myEnemy = EnemyState.Start;
        eAnim = gameObject.GetComponent<Animator>();

        //STATES
        #region SetWaypoints
        if (advancedPatrol)
        {
            for(int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                {
                    Debug.Log(gameObject + "Missing PatrolNode:" + i);
                }
            }
            for(int i = 0; i < waypoints.Length - 1; i++)
            {
                Debug.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position, Color.cyan, 999);
            }
        }
        else
        {
            potentialWaypoints = GameObject.FindGameObjectsWithTag("WayPoint1");
            waypoint1 = potentialWaypoints[0];
            for (int i = 0; i < potentialWaypoints.Length; i++)
            {
                if (Vector3.Distance(transform.position, potentialWaypoints[i].transform.position) < Vector3.Distance(transform.position, waypoint1.transform.position))
                {
                    waypoint1 = potentialWaypoints[i];
                }
            }
            potentialWaypoints = GameObject.FindGameObjectsWithTag("WayPoint2");
            waypoint2 = potentialWaypoints[0];
            for (int i = 0; i < potentialWaypoints.Length; i++)
            {
                if (Vector3.Distance(transform.position, potentialWaypoints[i].transform.position) < Vector3.Distance(transform.position, waypoint2.transform.position))
                {
                    waypoint2 = potentialWaypoints[i];
                }
            }
            //waypoint1 = GameObject.FindGameObjectWithTag("WayPoint1");
            //waypoint2 = GameObject.FindGameObjectWithTag("WayPoint2");
        }

        #endregion
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
        if (agent.enabled && !death)
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
            else if (myEnemy == EnemyState.lockChase)
            {
                editChase();
                Debug.Log("1111111111111111111111111111111111111");
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
    //COLLISIONS
    #region Collisions
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector3.zero;
            agent.isStopped = true;
            AgentStop();
        }
        else if (collision.gameObject.tag == "HammerSmashAOE")
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
        else if (collision.gameObject.tag == "WhirlwindAOE")
        {
            //Deals small knockback from takeDamage function
            #region Debug Log
            Debug.Log("Enemy has been hit by whirlwind!");
            #endregion
            takeDamage(3);
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
    private void OnTriggerEnter(Collider other)
    {
        // During patrol alternate going between Waypoint1 and Waypoint2
        // On colliding with waypoint sets other as destination
        // Patrolling now works regardless of what order waypoints are in
        if (isPatrolling)
        {
            if (advancedPatrol)
            {
                if(other.gameObject.transform == waypoints[patrolNumber].transform)
                {
                    if(patrolNumber == waypoints.Length - 1 && patrolIterator == 1)
                    {
                        patrolIterator = -1;
                    }
                    else if(patrolNumber == 0 && patrolIterator == -1)
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
            if (other.CompareTag("Player"))
            {
                //Debug.LogWarning("Enemy Start Collision With Player");
                rb.isKinematic = false;
            }
        }

        //TODO + parameter to take damage to edit knockback
        //So that ranged attack doesn't knockback as much as melee
        if (other.gameObject.tag == "PlayerRanged")
        {
            Debug.Log("Hit with Ranged");
            takeDamage(1);
        }
    }
    #endregion
    public void takeDamage(int dmg)
    {
        //Debug.Log(dmg + "Damage Taken");
        agent.isStopped = true;
        hp -= dmg;
        if (hp <= 0)
        {
            death = true;
            stackTracker.RemoveStack(gameObject);
            agent.speed = 0;
            Debug.Log("Enemy has been killed");
            enemyMovement = 0;
            rotationSpeed = 0;
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
        agent.isStopped = false;
        agent.SetDestination(target.position);
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

        // doesn't work if stack call returns 5 which means not on stack
        // or -1 which means still not changed
        if (encircleNum < 4 && encircleNum >= 0)
        {
            ResetMovement();
            agent.SetDestination(circlePoints[encircleNum].transform.position);
        }

    }
    //using this funciton to set a chase destination to spawned enemy
    public void editChase()
    {
        isPatrolling = false;
        agent.isStopped = false;
        myEnemy = EnemyState.lockChase;
        agent.SetDestination(Player.transform.position);

    }
    public int getstatus()
    {
        if (myEnemy == EnemyState.Patrol)
        {
            return 1;
        }
        else if (myEnemy == EnemyState.Chase)
        {
            return 2;
        }
        else if (myEnemy == EnemyState.lockChase)
        {
            return 3;
        }
        else if (myEnemy == EnemyState.Attack)
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
        if (advancedPatrol)
        {
            if (myEnemy != EnemyState.Patrol)
            {
                myEnemy = EnemyState.Patrol;
                agent.SetDestination(waypoints[0].transform.position);
                isPatrolling = true;
            }
        }
        else
        {
            if (myEnemy != EnemyState.Patrol)
            {
                myEnemy = EnemyState.Patrol;
                agent.SetDestination(waypoint1.transform.position);
                isPatrolling = true;
            }
        }

    }

    // Used for enemy animations and patrolling between waypoints
    #endregion


    private void ResetMovement()
    {
        enemyMovement = 3;
    }

    private void giveDamage()
    {   
        Player.SendMessage("takeDamage", dmgDealt);
    }
    public void changeStackrange(float i)
    {
        checkStackRange = i;
    }
    #region depreciated
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
    //private void UpdateCirclePoints()
    //{
    //    circlePoints[0] = target.position + new Vector3(circleDist, 0, 0);
    //    circlePoints[1] = target.position + new Vector3(0, 0, circleDist);
    //    circlePoints[2] = target.position + new Vector3(-circleDist, 0, 0);
    //    circlePoints[3] = target.position + new Vector3(0, 0, -circleDist);
    //}
    //private void Stun()
    //{
    //myEnemy = EnemyState.Stun;
    //enemyMovement = 0;
    //yield return new WaitForSeconds(4);
    //enemyMovement = 5;
    //Chase();
    //}
    #endregion
}
