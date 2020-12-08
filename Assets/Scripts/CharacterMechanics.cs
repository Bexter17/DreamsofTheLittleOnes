using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;


// Character Mechanics Prototype #6
//Made By Craig Walker

//Changes:
//fixed combo system, implemented IK foot system

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class CharacterMechanics : MonoBehaviour
{
    #region Variables

    #region PlayerStats

    [Header("Player Stats")]
    //HealthBar for Player
    public HealthBar healthBar;

    //Creates a charactercontoller variable named "controller"
    CharacterController controller;

    //Tracks player health
    //[SerializeField] private
    public int currentHealth = 5;

    //Max health
    //[SerializeField] private
    public int maxHealth = 50;

    //Tracks incoming damage
    private int Damage = 0;

    //Tracks what is damaging the player
    private GameObject damageSource;

    //Tracks player's lives
    [SerializeField] private int Lives = 1;

    //Tracks if the player is currently alive or not
    private bool isAlive = true;

    private bool isInCombo = false;

    private int wastedClicks = 0;

    //Determines how fast the character moves
    //[SerializeField] private 
    public float speed;

    //Variable for how high the character will jump
    [SerializeField ] private float jumpSpeed;

    private float vSpeed = 0;

    //Rotation speed of the character
    [SerializeField] private float rotationSpeed; // Used when not using MouseLook.CS to rotate character

    //Amount of gravity set on the player
    [SerializeField] private float gravity;

    //Allows you to toggle hold to crouch or press to crouch
    [SerializeField] private bool crouchIsToggle;

    //Tracks if the player is actively hold crouch key
    [SerializeField] private bool isCrouched = false;

    //Tracks if player is too busy to attack
    [SerializeField] private bool isBusy = false;

    //Boolean to track if the player is on the ground or in the air
    [SerializeField] private bool isGrounded;

    //Variable used to add force or direction to the character
    Vector3 moveDirection;

    //creates a script accessable variable of the Animator component
    Animator animator;

    AnimatorClipInfo[] CurrentClipInfo;

    private string animName;

    //Tracks player checkpoints and where they will respawn 
    [SerializeField] private GameObject respawnPoint;

#endregion

    #region AttackSystem

    [Header("Attack System")]
    //holds the box collider for the attack range
    [SerializeField] private GameObject attackRangePrefab;

    [SerializeField] private GameObject DashRangePrefab;

    //creates atemporary, destructable version of the prefab
    private GameObject attackTemp;

    private GameObject dashTemp;

    private int comboCount;

    private int queuedAttack1 = -1;

    private int queuedAttack2 = -1;

    private int currentAttack;

    //determines how long the attack lasts
    [SerializeField] private Transform attackSpawn;

    [SerializeField] private Transform dashSpawn;

    [SerializeField] private int attackTimer;

    [SerializeField] private int dashDamage;

    [SerializeField] private int dashSpeed;

    Sword_Script sword;

    #endregion

    #region PickupSystem

    [Header("Pick up System")]
    //Tracks if GodMode is Active
    [SerializeField] private bool isGodMode;

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

    [SerializeField] private LayerMask environmentLayer;

    [SerializeField] private float pelvisOffset = 0f;

    [Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.2f;

    [Range(0, 1)] [SerializeField] private float feetToIKPosSpeed = 0.5f;

    [SerializeField] private string leftFootVariableName = "Left Foot Curve";

    [SerializeField] private string rightFootVariableName = "Right Foot Curve";

    public bool useProIKFeatures = true;

    public bool showSolverDebug = true;

    #endregion

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region Initialization

        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        comboCount = 0;

        sword = GetComponentInChildren<Sword_Script>();

        try
        {
            //Accesses the CharacterController component on the character object 
            controller = GetComponent<CharacterController>();

            isAlive = true;

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

            //Sets variables to a defualt value incase not set in Unity inspector
            if (speed <= 0)
            {
                speed = 6.0f;
            }

            if (jumpSpeed <= 0)
            {
                jumpSpeed = 10.0f;
            }

            if (rotationSpeed <= 0)
            {
                rotationSpeed = 4.0f;
            }

            if (gravity <= 0)
            {
                gravity = 9.81f;
            }

            if (respawnPoint == null)
            {
                respawnPoint = GameObject.FindGameObjectWithTag("Starting Respawn Point");
            }

            if (attackRangePrefab == null)
            {
                attackRangePrefab = GameObject.FindGameObjectWithTag("Attack Zone");
            }

            if (attackSpawn == null)
            {
                attackSpawn = GameObject.FindGameObjectWithTag("Attack Spawn").transform;
            }

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
            #region CheckPlayerHealth
            //If health drops to or below zero, the player dies
            if (currentHealth <= 0)
            {

                animator.SetTrigger("Die");
                isAlive = false;
            }
            #endregion

            #region PlayerMovement

            //Assign "moveDirection" to track vertical movement
            moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));

            //Character rotation
            transform.Rotate(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);

            //Character movement
            Vector3 forward = transform.TransformDirection(Vector3.forward);

            //Movement speed
            float curSpeed = Input.GetAxis("Vertical") * speed;

            //Character controller movement
            controller.SimpleMove(transform.forward * (Input.GetAxis("Vertical") * speed));

            #endregion

            #region SetAnim

            animator.SetFloat("Speed", Input.GetAxis("Vertical"));
            animator.SetBool("isGrounded", controller.isGrounded);

            #endregion

            #region HandleInput

            //Enables the player to use Ability 1
            if (Input.GetButtonDown("Fire1"))
            {
                attack();
            }

            //Enables the player to use Ability 2
            if (Input.GetButtonDown("Fire2"))
            {
                Debug.Log("Ability2 has been pressed");
                Dash();
            }

            //Enables the player to use Ability 3
            if (Input.GetButtonDown("Fire3"))
            {
                Debug.Log("Ability3 has been pressed");
                Ability3();
            }

            //Enables the player to jump
            // Jumping is not working with the player when in game
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                //  Debug.Log("Jump has been pressed");
                vSpeed = jumpSpeed;
                animator.SetTrigger("Jump");
            }

            #endregion

            #region ApplyGravity
            vSpeed -= gravity * Time.deltaTime;
            moveDirection.y = vSpeed;
            controller.Move(moveDirection * Time.deltaTime);
            // Debug.Log("Grounded: " + controller.isGrounded + " vSpeed: " + vSpeed);
            #endregion
        }
    }

    #region FeetGrounding

    private void FixedUpdate()
    {
        if(enableFeetIK == false)
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
   
        if(Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
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


    //Tracks player collision 
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
       // Debug.Log("OnControllerColliderHit: " + hit.gameObject.name);

        if(hit.gameObject.tag == "Floor")
        {
            isGrounded = true;
        }

        if (hit.gameObject.tag == "Checkpoint")
        {
            respawnPoint = hit.gameObject;
        }

        if (hit.gameObject.tag == "Enemy")
        {
            //animator.SetTrigger("Got Hit");
            //takeDamage(1);
        }

        if (hit.gameObject.tag == "Projectile")
        {

        }
    }

    //Tracks triggers / pickups
    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.tag == "Teleport Potion")
        {
            speed *= speedBoost;
            Destroy(c.gameObject);
          //  Debug.Log("Drank Teleportation Potion");
            StartCoroutine(stopSpeedBoost());
        }

        if (c.gameObject.tag == "Jump Boost")
        {
            jumpSpeed += jumpBoost;
            Destroy(c.gameObject);
           // Debug.Log("Jump Boost Applied");
            StartCoroutine(stopJumpBoost());
        }

        if (c.gameObject.tag == "GodMode Pickup")
        {
            isGodMode = true;

            GetComponentInChildren<Renderer>().material.color = Color.blue;

            Destroy(c.gameObject);

            StartCoroutine(stopGodmode());
        }

        if (c.gameObject.tag == "Speed Boost")
        {
            speed *= speedBoost;
            Destroy(c.gameObject);
           // Debug.Log("Speed Boost Applied");
            StartCoroutine(stopSpeedBoost());
        }
    }

    //Trigger after death animation to fully kill player object
    public void die()
    {
        Lives--;
       // gameObject.transform.position = respawnPoint.transform.position;
        isAlive = true;
        if (Lives == 0)
        {
            SceneManager.LoadScene("EndScene");
        }
    }

    IEnumerator stopGodmode()
    {
        yield return new WaitForSeconds(timerGodMode);

        GetComponentInChildren<Renderer>().material.color = Color.white;

        isGodMode = false;
    }

    IEnumerator stopJumpBoost()
    {
        yield return new WaitForSeconds(timerJumpBoost);

        jumpSpeed -= jumpBoost;
    }

    IEnumerator stopSpeedBoost()
    {
        yield return new WaitForSeconds(timerSpeedBoost);

        speed -= speedBoost;
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }

    public void queueAttack()
    {
        int incomingAttack = comboCount;

        if (incomingAttack != queuedAttack1)
        {
            queuedAttack1 = incomingAttack;
        }

        if (incomingAttack != queuedAttack1 && incomingAttack != queuedAttack2)
        {

        }

        else
        {
            wastedClicks += 1;
        }
    }

    public void checkQueue()
    {
        isBusy = false;

        if (queuedAttack1 > -1)
        {
            comboCount = queuedAttack1;
            queuedAttack1 = -1;

            if(queuedAttack2 > -1)
            {
                queuedAttack1 = queuedAttack2;
                queuedAttack2 = -1;
            }

            attack();
        }
    }
    public void attack()
    {
        //Debug.LogWarning(isBusy);

        //if (!isBusy || isInCombo)
        //{
        Debug.Log("Attack has been pressed");

        isBusy = true;

        isInCombo = true;

        currentAttack = comboCount;

        CurrentClipInfo = this.animator.GetCurrentAnimatorClipInfo(0);

        animName = CurrentClipInfo[0].clip.name;

        Debug.LogWarning("animName =" + animName);

        Debug.LogError("comboCount =" + comboCount);

        switch (comboCount)
        {
            case 0:

                //if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
                if (animName == ("Idle") || animName == ("Run") || animName == ("Walk"))
                {
                    Debug.Log("Attack 1 Start");
                    comboCount = 1;
                    Debug.LogWarning(comboCount);
                    animator.SetInteger("Counter", comboCount);
                    animator.SetTrigger("Attack");
                }

                else if (animName != ("Idle") && animName != ("Run") && animName != ("Walk") && animName != ("Attack 1"))
                {
                    Debug.LogWarning("Error in combo case 0");

                    Debug.LogWarning("comboCount = " + comboCount);

                    Debug.LogWarning("Animation: " + animName);

                    //Debug.Log(animator.GetCurrentAnimatorStateInfo(0).nameHash);

                    Debug.LogWarning("comboCount resetting to 0");

                    comboCount = 0;

                    isBusy = false;
                }

                else if (animName == ("Attack 1"))
                {
                    comboCount = 1;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName == ("Attack 2"))
                {
                    comboCount = 2;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName == ("Attack 3"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                break;

            case 1:
                if (animName == ("Attack 1"))
                {
                    Debug.Log("attack 2 start");
                    comboCount = 2;
                    Debug.LogWarning(comboCount);
                    animator.SetInteger("Counter", comboCount);
                    animator.SetTrigger("Attack");
                }

                else if (animName == ("Idle") || animName == ("Run") || animName == ("Walk"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName == ("attack 2"))
                {
                    comboCount = 3;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName == ("attack 3"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName != ("attack 1"))
                {
                    Debug.LogWarning("Error in combo case 1");

                    Debug.LogWarning("comboCount = " + comboCount);

                    Debug.LogWarning("Animation: " + animName);

                    //Debug.Log(animator.GetCurrentAnimatorStateInfo(0).nameHash);

                    Debug.LogWarning("comboCount resetting to 0");

                    comboCount = 0;

                    isBusy = false;
                }

                break;

            case 2:
                if (animName == ("attack 2"))
                {
                    Debug.Log("attack 3 start");
                    comboCount = 3;
                    Debug.LogWarning(comboCount);
                    animator.SetInteger("Counter", comboCount);
                    animator.SetTrigger("attack");
                    //isInCombo = false;
                }

                else if (animName == ("Idle") || animName == ("Run") || animName == ("Walk"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                //else if (animName == ("attack 1"))
                //{
                //    comboCount = 1;
                //    Debug.LogWarning(comboCount);
                //    isBusy = false;
                //    attack();
                //}

                else if (animName == ("attack 3"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName != ("attack 2") && animName != ("attack 1"))
                {
                    Debug.LogWarning("Error in combo case 2");

                    Debug.LogWarning("comboCount = " + comboCount);

                    Debug.LogWarning("Animation: " + animName);

                    Debug.LogWarning("comboCount resetting to 0");

                    comboCount = 0;

                    isBusy = false;
                }

                break;

            case 3:

                if (animName == ("attack 3"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName == ("Idle") || animName == ("Run") || animName == ("Walk"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName == ("attack 1"))
                {
                    comboCount = 1;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName == ("attack 2"))
                {
                    comboCount = 2;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    attack();
                }

                else if (animName != ("attack 3"))
                {
                    Debug.LogWarning("Error in combo case 3");

                    Debug.LogWarning("comboCount = " + comboCount);

                    Debug.LogWarning("Animation: " + animName);

                    //Debug.Log(animator.GetCurrentAnimatorStateInfo(0).nameHash);

                    Debug.LogWarning("comboCount resetting to 0");

                    comboCount = 0;

                    isBusy = false;
                }

                break;
        }
    

//        else if (isBusy)
//{
//            Debug.Log("attack has been pressed, you are too busy to attack");

//            if (comboCount != currentattack)
//            {
//                Debug.Log("attack Queued");
//                queueAttack();
//            }
//        }
    }
    public void attackBegins()
    {
        isInCombo = true;
        sword.SendMessage("activateattack");
        //sends message to the players sword script to start dealing damage on collision

    }
    public void attackEnd()
    { 
        //sends message to the players sword script to stop dealing damage on collision
        sword.SendMessage("deactivateattack");

        if (animator.GetInteger("Counter") == comboCount)
        {
            //Debug.LogWarning(comboCount);

            //comboCount = 0;
            
            isInCombo = false;
            //animator.SetInteger("Counter", comboCount);
        }

        animator.SetInteger("Counter", comboCount);
        
        //checkQueue();
    }

    // Dash now has animation tested and animation plays when hitting the left alt, spawns the dashTemp but player doesnt move forward.
    public void Dash()
    {
        Debug.Log("Dash has been triggered");

        dashTemp = Instantiate(DashRangePrefab, dashSpawn.transform.position, dashSpawn.transform.rotation);

        animator.SetTrigger("Dash");

        controller.SimpleMove(transform.forward * (Input.GetAxis("Vertical") * dashSpeed));

        //Rigidbody.addforce();
    }
    public void comboReset()
    {
        Debug.LogWarning("comboReset Ran");
        comboCount =  0;
        animator.SetInteger("Counter", comboCount);

    }

    private void DashEnds()
    {
        Destroy(dashTemp);
        Debug.Log("Dash complete");
    }

    public void Ability2()
    {
        Debug.Log("Ability 2 has been pressed");
    }

    public void Ability3()
    {
        Debug.Log("Ability 3 has been pressed");
    }
}
