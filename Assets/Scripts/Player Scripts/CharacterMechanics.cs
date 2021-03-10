using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using Random = UnityEngine.Random;

/*

Character Mechanics Version 8.0
Made By Craig Walker

Features:
Combo Counter
Created an input buffer system
IK Foot Placement

Recent Changes:
Seperated jumping and falling
isGround() check each frame
adjusted attack & combo reseting

*/



#region ActionItem class Creation

public class ActionItem
{

    public enum InputAction { Jump, Attack, Dash, HammerSmash, Whirlwind, Ranged };
    public InputAction Action;
    public float Timestamp;

    public static float TimeBeforeActionsExpire = 2f;

    //Constructor
    public ActionItem(InputAction ia, float stamp)
    {
        Action = ia;
        Timestamp = stamp;
    }

    //returns true if this action hasn't expired due to the timestamp
    public bool CheckIfValid()
    {
        bool returnValue = false;
        if (Timestamp + TimeBeforeActionsExpire >= Time.time)
        {
            returnValue = true;
        }
        return returnValue;
    }
}

#endregion

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class CharacterMechanics : MonoBehaviour
{
    #region Components


    //Creates a charactercontoller variable named "controller"
    CharacterController controller;

    //creates a script accessable variable of the Animator component
    Animator animator;

    #endregion

    #region Variables

    #region Debug Toggles
    [Header("Debug Settings")]

    [SerializeField] private bool movementDebug;

    [SerializeField] private bool jumpDebug;

    [SerializeField] private bool combatDebug;

    [SerializeField] private bool comboDebug;

    [SerializeField] private bool inputBufferDebug;

    [SerializeField] private bool showSolverDebug;

    [SerializeField] private bool hammerDebug;

    [SerializeField] private bool whirlwindDebug;

    [SerializeField] private bool animDebug;

    #endregion

    #region Animator Variables

    AnimatorClipInfo[] currentClipInfo;

    private string animName;

//    float currentAnimLength;

    #endregion

    #region PlayerStats

    [Header("Player Stats")]

    //HealthBar for Player

    [SerializeField] private GameObject HealthBar;

    public HealthBar healthBar;

    //Tracks player health
    public int currentHealth = 5;

  //[SerializeField] private 
    public int maxHealth = 50;

    //Tracks player's lives
    [SerializeField] private int Lives = 3;

    //Tracks if the player is currently alive or not
    private bool isAlive = true;

    private bool isInCombo = false;

    private Vector3 playerSize; 

//    private int wastedClicks = 0;

    //Tracks player checkpoints and where they will respawn 
    [SerializeField] private GameObject respawnPoint;

    #endregion

    #region Movement

    //Determines how fast the character moves
    [SerializeField] private float movementSpeed;

    [SerializeField] private float currentSpeed;

    //Rotation speed of the character
    [SerializeField] private float rotationSpeed;

    //Variable used to add force or direction to the character
    Vector3 moveDirection;

    #endregion

    #region Jumping and Falling

    RaycastHit groundHit;

    public float groundSearchLength = 0.6f;

    //Variable for how high the character will jump
    [SerializeField] private float jumpSpeed;

    private float vSpeed = 0;

    //Amount of gravity set on the player
    [SerializeField] private float gravity;

    //Boolean to track if the player is on the ground or in the air
    [SerializeField] private bool isGrounded;

    private bool isJumping;

    private bool isFalling;

    #endregion

    #region HUD

    Canvas Canvas;

    [SerializeField] TMP_Text playerStats;

    #endregion

    #region Attack System

    [Header("Attack System")]
    //holds the box collider for the attack range
    [SerializeField] private GameObject attackRangePrefab;

    //creates atemporary, destructable version of the prefab
    //private GameObject attackTemp;

    private int comboCount;

    [SerializeField] private int attackTimer;

    Sword_Script sword;

    #endregion

    #region Abilities

    [SerializeField] private GameObject abilitySpawn;

    AbilitiesCooldown cooldown;

    #region Dash

    [Header("Dash Ability")]

    [SerializeField] private GameObject dashRangePrefab;

    [SerializeField] private int dashDamage;

    [SerializeField] private int dashSpeed;

    private GameObject dashTemp;

    #endregion

    #region Hammer Smash

    [Header("Hammer Smash Ability")]

    [SerializeField] private GameObject hammerSmashPrefab;

    [SerializeField] private Transform hammerSmashSpawn;

    [SerializeField] private int hammerSmashDamage;

    private GameObject hammerSmashTemp;

    #endregion

    #region Whirlwind 

    [Header("Whirlwind Ability")]

    [SerializeField] private GameObject whirlwindRangePrefab;

    [SerializeField] private Transform whirlwindSpawn;

    [SerializeField] private int whirlwindDamage;

    private GameObject whirlwindTemp;

    #endregion

    #region Ranged

    [Header("Ranged Ability")]

    [SerializeField] private GameObject RangePrefab;

    [SerializeField] private Transform RangedSpawn;


    #endregion

    #endregion

    #region Input Buffer System

    //Queue InputBufferQueue = new Queue[];

    private List<ActionItem> inputBuffer = new List<ActionItem>();

    bool actionAllowed = true;

    #endregion

    #region Pickup System

    [Header("Pick up System")]

    //Variable to track how much health the health pickup heals
    [SerializeField] private int healthBoost;

    //Tracks if GodMode is Active
    //[SerializeField] private bool isGodMode;

    //Sets the length of the Mode
    [SerializeField] private float timerGodMode;

    //Variable to amplify the jumpBoost
    [SerializeField] private float jumpBoost;

    //How long the jump boost lasts
    [SerializeField] private float timerJumpBoost;

    //How much we are boosting the speed by
    [SerializeField] private float speedBoost;

    //How long the speed boost lasts
    [SerializeField] private float timerSpeedBoost;

#endregion

    #region IKSystem

    [Header("IK System")]

    [SerializeField] private bool enableFeetIK = true;

    private Vector3 rightFootPos, leftFootPos, rightFootIKPos, leftFootIKPos;

    private Quaternion leftFootIKRot, rightFootIKRot;

    private float lastPelvisPosY, lastRightFootPosY, lastLeftFootPosY;

    [Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;

    [Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;

    //[SerializeField] private LayerMask environmentLayer;

    [SerializeField] private float pelvisOffset = 0f;

    [Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.2f;

    [Range(0, 1)] [SerializeField] private float feetToIKPosSpeed = 0.5f;

    [SerializeField] private string leftFootVariableName = "Left Foot Curve";

    [SerializeField] private string rightFootVariableName = "Right Foot Curve";

    public bool useProIKFeatures = true;

    #endregion

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region Initialization

        #region Set Ground Search Length

        playerSize = gameObject.transform.localScale;

        groundSearchLength = 0.5f * playerSize.y;

        #endregion

        #region Health

        if (!HealthBar)
        {
            HealthBar = GameObject.FindGameObjectWithTag("Health Bar");
        }

        currentHealth = maxHealth;

        healthBar.SetMaxHealth(maxHealth);

        healthBar.SetHealth(currentHealth);

        #endregion

        #region Combat

        comboCount = 0;

        sword = GetComponentInChildren<Sword_Script>();

        cooldown = GetComponentInChildren<AbilitiesCooldown>();

        #endregion

        #region Dash

        if (!dashRangePrefab)
            dashRangePrefab = Resources.Load("Dash Zone", typeof(GameObject)) as GameObject;

        if (!abilitySpawn)
            abilitySpawn = GameObject.FindGameObjectWithTag("Attack Spawn");

        if (!hammerSmashSpawn)
            hammerSmashSpawn = abilitySpawn.transform;

        if (!whirlwindSpawn)
            whirlwindSpawn = abilitySpawn.transform;

        if (!RangedSpawn)
            RangedSpawn = gameObject.transform.GetChild(2).transform;

        if (dashSpeed == 0)
            dashSpeed = 5;

        if (!Canvas)
            Canvas = GameObject.FindGameObjectWithTag("HUD Canvas").GetComponent<Canvas>();

        if (!playerStats)
            playerStats = Canvas.transform.GetChild(0).GetChild(2).transform.GetComponent<TMP_Text>();

        #endregion

        try
        {
            //Accesses the CharacterController component on the character object 
            controller = GetComponent<CharacterController>();

            isAlive = true;

            #region Debug

            if (!movementDebug)
                movementDebug = false;

            if (!jumpDebug)
                jumpDebug = false;

            if (!combatDebug)
                combatDebug = false;

            if (!comboDebug)
                comboDebug = false;

            if (!inputBufferDebug)
                inputBufferDebug = false;

            if (!showSolverDebug)
                showSolverDebug = false;

            if (!hammerDebug)
                hammerDebug = false;

            if (!whirlwindDebug)
                whirlwindDebug = false;

            if (!animDebug)
                animDebug = false;

            #endregion

            #region Animation

            //Accesses the Animator component
            animator = GetComponent<Animator>();

            int idleId = Animator.StringToHash("Idle");

            int runId = Animator.StringToHash("Run");

            int attack1Id = Animator.StringToHash("Attack 1");

            int attack2Id = Animator.StringToHash("Attack 2");

            int attack3Id = Animator.StringToHash("Attack 3");

            AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

            //Automatically disables Root Motion (to avoid adding motion twice)
            animator.applyRootMotion = false;

            #endregion

            #region Movement

            if (movementSpeed <= 0)
                movementSpeed = 6.0f;

            if (jumpSpeed <= 0)
                jumpSpeed = 10.0f;

            if (rotationSpeed <= 0)
                rotationSpeed = 4.0f;

            if (gravity <= 0)
                gravity = 9.81f;

            #endregion

            #region Respawn

            if (!respawnPoint)
                respawnPoint = GameObject.FindGameObjectWithTag("Starting Respawn Point");

            #endregion

            #region Combat

            if (!attackRangePrefab)
                attackRangePrefab = GameObject.FindGameObjectWithTag("Attack Zone");
            
            if (!whirlwindRangePrefab)
                whirlwindRangePrefab = Resources.Load("whirlwindAttack", typeof(GameObject)) as GameObject;

            if (!hammerSmashPrefab)
                hammerSmashPrefab = Resources.Load("hammerSmashPrefab", typeof(GameObject)) as GameObject;

            if (!RangePrefab)
                RangePrefab = Resources.Load("ThrowingWeapon", typeof(GameObject)) as GameObject;

            #endregion

            if (attackTimer <= 0)
            {
                attackTimer = 3;
            }

            //Assigns a value to the variable
            moveDirection = Vector3.zero;
        }

        finally
        {

        }

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            #region Check Player Health

            //If health drops to or below zero, the player dies
            if (currentHealth <= 0)
            {
                #region Debug Log

                if (combatDebug)
                {
                    Debug.Log("Combat System: health dropped below 0");
                }

                #endregion

                isAlive = false;

                actionAllowed = false;

                comboCount = 0;

                animator.SetTrigger("Die");
            }

            #endregion

            #region Update HUD

            updateHud();

            #endregion

            #region Check Input Buffer

            checkInput();

            if (actionAllowed)
            {
                tryBufferedAction();
            }

            #endregion

            //Che

            #region Player Movement

            //Assign "moveDirection" to track vertical movement
            moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));

            //Character rotation
            transform.Rotate(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);

            //track any applied speed boosts
            currentSpeed = movementSpeed + speedBoost;

            //Character movement
            Vector3 forward = transform.TransformDirection(Vector3.forward);

            //Movement speed
            float curSpeed = Input.GetAxis("Vertical") * currentSpeed;

            //Character controller movement
            controller.SimpleMove(transform.forward * (Input.GetAxis("Vertical") * currentSpeed));

            isGrounded = groundCheck(isGrounded);

            if(isGrounded)
            {
                if (isFalling)
                    isFalling = false;

                if (isJumping)
                    isJumping = false;
            }


            else if (!isGrounded)
            { 
                if (!isJumping)
                    if (!isFalling)
                        isFalling = true;
            }

            #endregion

            #region Check Anim Status

            currentClipInfo = this.animator.GetCurrentAnimatorClipInfo(0);

            //currentAnimLength = currentClipInfo[0].clip.length;

            animName = currentClipInfo[0].clip.name;

            if (animName == "Male Attack 1" && actionAllowed|| animName == "Male Attack 2" && actionAllowed || animName == "Male Attack 3" && actionAllowed)
            {
                comboCount = 0;

                Debug.Log("Animation System: comboCount reset by update");
            }

            #region Debug Log

            if(animDebug)
            {
                Debug.Log("Animator System: Anim Name" + animName);

                //Debug.Log("Animator System: Anim Length" + currentAnimLength);
            }

            #endregion

            #endregion

            #region Apply Gravity

            vSpeed -= gravity * Time.deltaTime;

            moveDirection.y = vSpeed;

            controller.Move(moveDirection * Time.deltaTime);

            if (!isGrounded && !isJumping)
                isFalling = true;

            // Debug.Log("Grounded: " + controller.isGrounded + " vSpeed: " + vSpeed);

            #endregion


            #region Set Animator

            animator.SetFloat("Speed", Input.GetAxis("Vertical"));

            animator.SetBool("isGrounded", isGrounded);

            animator.SetBool("isJumping", isJumping);

            animator.SetBool("isFalling", isFalling);

            #endregion
        }
    }

    //void FunIdle()
    //{
    //    if(Random.Range(1, 5) == 1)
    //    {
    //        animator.SetFloat("Idle_Fun", 1);
    //    }
    //}

    #region FeetGrounding

    private void FixedUpdate()
    {
        #region IK System Check

        if (enableFeetIK == false)
        {
            return;
        }

        if(animator == null)
        {
            return;
        }

        adjustFeetTarget(ref rightFootPos, HumanBodyBones.RightFoot);

        adjustFeetTarget(ref leftFootPos, HumanBodyBones.LeftFoot);

        feetPositionSolver(rightFootPos, ref rightFootIKPos, ref rightFootIKRot); //Handles the solver for the right foot

        feetPositionSolver(leftFootPos, ref leftFootIKPos, ref leftFootIKRot); //Handles the solver for the left foot

        #endregion
    }

    private void updateHud()
    {
        playerStats.text = "Lives: " + Lives;

        healthBar.SetHealth(currentHealth);

        healthBar.SetMaxHealth(maxHealth);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (enableFeetIK == false)
        {
            return;
        }

        if (animator == null)
        {
            return;
        }

        movePelvisHeight();

        //right foot ik position & rotation -- utilize pro features here
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        if(useProIKFeatures)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(rightFootVariableName));
        }

        moveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPos, rightFootIKRot, ref lastRightFootPosY);

        //left foot ik position & rotation -- utilize pro features here
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

        if (useProIKFeatures)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(leftFootVariableName));
        }

        moveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPos, leftFootIKRot, ref lastLeftFootPosY);
    }

    #endregion

    #region FeetGroundingMethods

    private void moveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
    {
        Vector3 targetIKPosition = animator.GetIKPosition(foot);

        if(positionIKHolder != Vector3.zero)
        {
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);

            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPosSpeed);

            targetIKPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIKPosition = transform.TransformPoint(targetIKPosition);

            animator.SetIKPosition(foot, targetIKPosition);
        }

        animator.SetIKPosition(foot, targetIKPosition);
    }

    private void movePelvisHeight()
    {
        if(rightFootIKPos == Vector3.zero || leftFootIKPos == Vector3.zero || lastPelvisPosY == 0)
        {
            lastPelvisPosY = animator.bodyPosition.y;
            return;
        }

        float lOffsetPos = leftFootIKPos.y - transform.position.y;

        float rOffsetPos = rightFootIKPos.y - transform.position.y;

        float totalOffset = (lOffsetPos < rOffsetPos) ? lOffsetPos : rOffsetPos;

        Vector3 newPelvisPos = animator.bodyPosition + Vector3.up * totalOffset;

        newPelvisPos.y = Mathf.Lerp(lastPelvisPosY, newPelvisPos.y, pelvisUpAndDownSpeed);

        animator.bodyPosition = newPelvisPos;

        lastPelvisPosY = animator.bodyPosition.y;
    }

    private void feetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
    {
        //raycast handling section
        RaycastHit feetOutHit;

        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.blue);
   
        if(Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast))
        {
            feetIKPositions = fromSkyPosition;

            feetIKPositions.y = feetOutHit.point.y + pelvisOffset;

            feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

            return;
        }

        feetIKPositions = Vector3.zero;
    }

    private void adjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = animator.GetBoneTransform(foot).position;

        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }

    #endregion

    #region Collision and Trigger Handling

    //Tracks player collision 
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
       Debug.Log("OnControllerColliderHit: " + hit.gameObject.name);

        if(hit.gameObject.tag == "Floor")
        {
            isGrounded = true;

            isFalling = false;

            animator.SetBool("isGrounded", isGrounded);

            animator.SetBool("isFalling", isFalling);

            if (isJumping)
            {
                isJumping = false;

                animator.SetBool("isJumping", isJumping);

                actionAllowed = true;
            }
        }
        else if(hit.gameObject.tag =="Killbox")
        {
            Killbox();
        }

        //if (hit.gameObject.tag == "Checkpoint")
        //{
        //    respawnPoint = hit.gameObject;
        //}

        //if (hit.gameObject.tag == "Projectile")
        //{

        //}
    }

    //Tracks triggers / pickups

    private bool groundCheck(bool isGrounded)
    {
        Vector3 lineStart = transform.position;
        Vector3 vectorToSearch = new Vector3(lineStart.x, lineStart.y - groundSearchLength, lineStart.z);

        Debug.DrawLine(lineStart, vectorToSearch);

        return Physics.Linecast(lineStart, vectorToSearch, out groundHit);
    }

    private void JumpEnd()
    {
        if (jumpDebug)
            Debug.Log("JumpEnd Called");

        isJumping = false;
    }

    private void OnTriggerEnter(Collider c)
    {
        #region Pickups

        //if (c.gameObject.tag == "Teleport Potion")
        //{
        //    //speed *= speedBoost;
        //    Destroy(c.gameObject);
        //  //  Debug.Log("Drank Teleportation Potion");
        //    StartCoroutine(stopSpeedBoost());
        //}

        //if (c.gameObject.tag == "Jump Boost")
        //{
        //    jumpSpeed += jumpBoost;
        //    Destroy(c.gameObject);
        //   // Debug.Log("Jump Boost Applied");
        //    StartCoroutine(stopJumpBoost());
        //}

        //if (c.gameObject.tag == "GodMode Pickup")
        //{
        //    isGodMode = true;

        //    GetComponentInChildren<Renderer>().material.color = Color.blue;

        //    Destroy(c.gameObject);

        //    StartCoroutine(stopGodmode());
        //}

        if (c.gameObject.tag == "Speed Pickup")
        {
            //speed *= speedBoost;
            Destroy(c.gameObject);
            // Debug.Log("Speed Boost Applied");
            //StartCoroutine(stopSpeedBoost());
            pickupSpeed();
        }

        if (c.gameObject.tag == "Health Pickup")
        {
            Destroy(c.gameObject);
          
            pickupHealth();
        }

        if (c.gameObject.tag == "Max Health Pickup")
        {
            Destroy(c.gameObject);

            pickupMaxHealth();
        }

        #endregion
    }

    #endregion

    #region Pickup Functions

    private void pickupHealth()
    {
        currentHealth = maxHealth;

        updateHud();
    }

    private void pickupMaxHealth()
    {
        maxHealth += 50;

        updateHud();
    }

    private void pickupSpeed()
    {
        speedBoost += 2;
    }

    #endregion

    #region Pickup Coroutines

    //IEnumerator stopGodmode()
    //{
    //    yield return new WaitForSeconds(timerGodMode);

    //    GetComponentInChildren<Renderer>().material.color = Color.white;

    //    isGodMode = false;
    //}

    //IEnumerator stopJumpBoost()
    //{
    //    yield return new WaitForSeconds(timerJumpBoost);

    //    jumpSpeed -= jumpBoost;
    //}

   // IEnumerator stopSpeedBoost()
   // {
   //     yield return new WaitForSeconds(timerSpeedBoost);

   ////     speed -= speedBoost;
   // }

    #endregion

    #region Combat System

    #region Take Damage

    public void takeDamage(int dmgDealt)
    {
        #region Debug Log

        if (combatDebug)
        {
            Debug.Log("Combat System: takeDamage called");
        }

        #endregion

        comboCount = 0;

        animator.SetInteger("Counter", comboCount);

        if(actionAllowed)
        animator.SetTrigger("Got Hit");

        currentHealth -= dmgDealt;
        
        healthBar.SetHealth(currentHealth);
    }

    #endregion

    #region Input Buffer

    private void checkInput()
    {
        float x = Input.GetAxisRaw("Horizontal");

        float z = Input.GetAxisRaw("Vertical");

        if (x > 0.01f || x < -0.01f )
        {

        }

        if(z > 0.01f || z < -0.01f)
        {

        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            #region Debug Log

            if (inputBufferDebug)
            {
                Debug.Log("Input Buffer System: Jump has been pressed");
            }

            #endregion

            inputBuffer.Add(new ActionItem(ActionItem.InputAction.Jump, Time.time));
        }


        //Enables the player to use Ability 1
        if (Input.GetButtonDown("Fire1") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown1 == false)
        {
            #region Debug Log

            if (inputBufferDebug)
            {
                Debug.Log("Input Buffer System: Attack has been pressed");
            }

            #endregion

            inputBuffer.Add(new ActionItem(ActionItem.InputAction.Attack, Time.time));
            AttackEnd();
        }

        //Enables the player to use Ability 2
        if (Input.GetButtonDown("Fire2") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown2 == false)
        {
            #region Debug Log

            if (inputBufferDebug)
            {
                Debug.Log("Input Buffer System: dash has been pressed");
            }

            #endregion

            inputBuffer.Add(new ActionItem(ActionItem.InputAction.Dash, Time.time));
            AttackEnd();
        }

        //Enables the player to use Ability 3
        //if (Input.GetButtonDown("Fire3") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown3 == false)
        if (Input.GetButtonDown("Fire3"))
        {
            #region Debug Log
            if (inputBufferDebug)
            {
                Debug.Log("Input Buffer System: hammerSmash has been pressed");
            }

            #endregion

            inputBuffer.Add(new ActionItem(ActionItem.InputAction.HammerSmash, Time.time));
            AttackEnd();
        }

        if (Input.GetButtonDown("Fire4") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown4 == false)
        {
            #region Debug Log
            if (inputBufferDebug)
            {
                Debug.Log("Input Buffer System: whirlwind has been pressed");
            }
            #endregion

            inputBuffer.Add(new ActionItem(ActionItem.InputAction.Whirlwind, Time.time));
            AttackEnd();
        }

        if (Input.GetButtonDown("Fire5"))
        {
            if (inputBufferDebug)
            {
                Debug.Log("Input Buffer System: ranged attack has been pressed");
            }

            inputBuffer.Add(new ActionItem(ActionItem.InputAction.Ranged, Time.time));
        }
    }

    private void tryBufferedAction()
    {
        if (inputBuffer.Count > 0)
        {
            foreach (ActionItem ai in inputBuffer.ToArray()) 
            {
                inputBuffer.Remove(ai);
                if (ai.CheckIfValid())
                {
                    doAction(ai);
                    break;
                }
            }
        }

        else
        {
            comboCount = 0;

            animator.SetInteger("Counter", comboCount);

            Debug.Log("comboCount set to 0 by tryBufferedAction()");
        }
    }

    private void doAction(ActionItem ai)
    {
        #region Debug.Log

        if(inputBufferDebug)
        {
            Debug.Log("doAction called");

            Debug.Log(ai.Action);
        }

        #endregion

        if (ai.Action == ActionItem.InputAction.Jump)
        {
            jump();
        }

        if (ai.Action == ActionItem.InputAction.Attack)
        {
            #region Debug.Log

            if (inputBufferDebug)
            {
                Debug.Log("Input Buffer System: Attack Input Detected");

                Debug.Log("Input Buffer System: comboCount during input = " + comboCount);
            }

            #endregion

            if (comboCount == 0)
            {
                comboAttack1();
            }

            else if (comboCount == 1)
            {
                comboAttack2();
            }

            else if (comboCount == 2)
            {
                comboAttack3();
            }

            else if(comboCount < 0 || comboCount >= 3)
            {
                comboCount = 0;

                if(comboDebug)
                Debug.Log("Combo System: comboCount reset to 0 because combo was either < 0 or >= 3");

                comboAttack1();
            }
        }

        if (ai.Action == ActionItem.InputAction.Dash)
        {
            dash();
        }

        if (ai.Action == ActionItem.InputAction.HammerSmash)
        {
            hammerSmash();
        }

        if (ai.Action == ActionItem.InputAction.Whirlwind)
        {
            whirlwind();
        }

        if (ai.Action == ActionItem.InputAction.Ranged)
        {
            ranged();
        }

        actionAllowed = false; 
    }

    #endregion

    #region Jump

    void jump()
    {
        #region Debug Log

        if (jumpDebug)
        {
            Debug.Log("jump has been called");
        }

        #endregion

        if (isGrounded)
        {

            comboCount = 0;

            vSpeed = jumpSpeed;

            animator.SetTrigger("Jump");

            isGrounded = false;

            animator.SetBool("isGrounded", isGrounded);

            isJumping = true;

            animator.SetBool("isJumping", isJumping);

            isJumping = true;

            animator.SetBool("isFalling", isFalling);

            actionAllowed = false;

            #region Debug Log

            if (jumpDebug)
            {
                Debug.Log("jump power: " + vSpeed);

                Debug.Log("isGrounded = " + isGrounded);

                Debug.Log("isJumping = " + isJumping);

                Debug.Log("actionAllowed = " + actionAllowed);
            }

        }

        #endregion
    }

    #endregion

    #region Basic Attack 
    
    private void comboAttack1()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboAttack1 called");
        }

        #endregion

        if (actionAllowed)
            actionAllowed = false;

        comboCount = 1;

        animator.SetInteger("Counter", comboCount);

        animator.SetTrigger("Attack");

        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("comboCount set to 1 via comboAttack1()");

            Debug.Log("Combo System: animator set");

            Debug.Log("Combo System: comboCount: " + comboCount);

            Debug.Log("actionAllowed = " + actionAllowed);
        }

        #endregion
    }

    private void comboAttack2()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboAttack2 called");
        }

        #endregion

        if (actionAllowed)
            actionAllowed = false;

        comboCount = 2;

        animator.SetInteger("Counter", comboCount);

        animator.SetTrigger("Attack");

        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("comboCount set to 2 via comboAttack2()");

            Debug.Log("Combo System: animator set");

            Debug.Log("Combo System: comboCount: " + comboCount);

            Debug.Log("actionAllowed = " + actionAllowed);
        }

        #endregion
    }

    private void comboAttack3()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboAttack3 called");
        }

        #endregion

        if (actionAllowed)
            actionAllowed = false;

        comboCount = 3;

        animator.SetInteger("Counter", comboCount);

        animator.SetTrigger("Attack");

        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("comboCount set to 3 via comboAttack3()");

            Debug.Log("Combo System: animator set");

            Debug.Log("Combo System: comboCount: " + comboCount);

            Debug.Log("actionAllowed = " + actionAllowed);
        }

        #endregion
    }

    public void AttackBegins()
    {
        isInCombo = true;

        sword.SendMessage("activateAttack");
        //sends message to the players sword script to start dealing damage on collision

        if (actionAllowed)
            actionAllowed = false;

        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: AttackBegins called");
            
            Debug.Log("Combo System: isInCombo = " + isInCombo);

            Debug.Log("actionAllowed = " + actionAllowed);
        }

        #endregion
    }

    public void AttackEnd()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combat System: AttackEnd called");

            Debug.Log("Combat System: actionAllowed = " + actionAllowed);
        }

        #endregion

        //sends message to the players sword script to stop dealing damage on collision
     //   sword.SendMessage("deactivateAttack");

        //if (animator.GetInteger("Counter") == comboCount)
        //{
        //    //Debug.Log(comboCount);

        //    //comboCount = 0;

        //    isInCombo = false;
        //    //animator.SetInteger("Counter", comboCount);
        //}

        //if (inputBuffer.Count > 0)
        //{
        //    if (comboCount == 1)
        //    {
        //        comboCount = 2;

        //        animator.SetInteger("Counter", comboCount);
        //    }

        //    if (comboCount == 2)
        //    {
        //        comboCount = 3;

        //        animator.SetInteger("Counter", comboCount);
        //    }

        //    if (comboCount == 3)
        //    {
        //        comboCount = 0;

        //        animator.SetInteger("Counter", comboCount);
        //    }

            animator.SetInteger("Counter", comboCount);
        //}

        actionAllowed = true;


        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboCount set to " + comboCount + " by AttackEnd()");

            Debug.Log("Input Buffer System: inputbuffer count = " + inputBuffer.Count);

            Debug.Log("actionAllowed = " + actionAllowed);
        }

        #endregion

        tryBufferedAction();

        //comboCount = 0;

        //animator.SetInteger("Counter", comboCount);

        //checkQueue();
    }

    public void comboReset()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboReset Ran");
        }

        #endregion

        if (actionAllowed)
        {
            comboCount = 0;

            if(comboDebug)
            Debug.Log("Combo System: comboCount set to 0 by comboReset()");

            animator.SetInteger("Counter", comboCount);
        }
        if (!actionAllowed)
        {
            if (!isJumping)
            {
                actionAllowed = true;

                #region Debug Log

                if (inputBufferDebug)
                {
                    Debug.Log("Input Buffer System: comboReset Ran, actionAllowed and isJumping = false, setting actionAllowed to true");
                }

                #endregion
            }

            else
            {
                #region Debug Log

                if (inputBufferDebug)
                {
                    Debug.Log("Input Buffer System: comboReset Ran, actionAllowed = false, isJumping = true");
                }

                #endregion
            }
        }
    }

    #endregion

    #region Abilities

    


    // Dash now has animation tested and animation plays when hitting the left alt, spawns the dashTemp but player doesnt move forward.
    public void dash()
    {
        #region Debug Log

        if (combatDebug)
        {
            Debug.Log("dash has been triggered");
        }

        #endregion

        comboCount = 0;

        dashTemp = Instantiate(dashRangePrefab, abilitySpawn.transform.position, abilitySpawn.transform.rotation);

        animator.SetTrigger("dash");

        controller.SimpleMove(transform.forward * (Input.GetAxis("Vertical") * dashSpeed));

        //Rigidbody.addforce();
        actionAllowed = true;
    }
   
    private void dashEnds()
    {
        #region Debug Log

        if (combatDebug)
        {
            Debug.Log("Combat System: dash complete");
        }

        #endregion

        Destroy(dashTemp);

        actionAllowed = true;
    }

    private void hammerSmash()
    {
        #region Debug Log

        if (hammerDebug)
        {
            Debug.Log("hammerSmash has been pressed");
        }

        #endregion

        comboCount = 0;

        animator.SetTrigger("HammerSmash");

        hammerSmashTemp = Instantiate(hammerSmashPrefab, hammerSmashSpawn.position, hammerSmashSpawn.transform.rotation, gameObject.transform);
        Destroy(hammerSmashTemp, 2);
        AttackEnd();
        actionAllowed = true;
        //hammerSmashEnd();
    }

    private void whirlwind()
    {
        #region Debug Log
        
        if (whirlwindDebug)
        {
            Debug.Log("whirlwind has been called");
        }

        #endregion

        comboCount = 0;

        animator.SetTrigger("Spin");

        whirlwindTemp = Instantiate(whirlwindRangePrefab, whirlwindSpawn.position, whirlwindSpawn.transform.rotation, gameObject.transform);
        Destroy(whirlwindTemp, 2);
        AttackEnd();
        //whirlwindEnd();
    }

    private void whirlwindEnd()
    {
        #region Debug Log

        if (whirlwindDebug)
        {
            Debug.Log("whirlwindEnd has been called");

            Destroy(whirlwindTemp);
        }

        AttackEnd();

        #endregion

        Destroy(whirlwindTemp);
    }

    private void hammerSmashEnd()
    {
        #region Debug Log

        if (whirlwindDebug)
        {
            Debug.Log("HammerSmash has been called");

            Destroy(hammerSmashTemp);
        }

        AttackEnd();

        #endregion

        Destroy(hammerSmashTemp);
    }


    private void ranged()
    {
        #region Debug Log

        if (whirlwindDebug)
        {
            Debug.Log("ranged() has been called");
        }

        #endregion

        animator.SetTrigger("Throw");

        GameObject bullet = Instantiate(RangePrefab, RangedSpawn.transform.position, RangedSpawn.transform.rotation) as GameObject;

        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
    }

    #endregion

    #region Die
    //Trigger after death animation to fully kill player object
    public void die()
    {
        Lives--;
        
        if (Lives <= 0)
        {
            SceneManager.LoadScene("EndScene");
        }

        else
        {
            gameObject.transform.position = respawnPoint.transform.position;

            animator.SetTrigger("Respawn");
        }
    }

    private void respawn()
    {
        currentHealth = maxHealth;

        isAlive = true;

        actionAllowed = true;

        comboCount = 0;
    }

    private void Killbox()
    {
        Lives -= 1;
        gameObject.transform.position = respawnPoint.transform.position;

        animator.SetTrigger("Respawn");
    }

    #endregion

    #endregion
}
