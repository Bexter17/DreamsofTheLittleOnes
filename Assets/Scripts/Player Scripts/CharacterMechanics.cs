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

    //Temporary update for placing hammerSmash during key frame. Need to keep this in place for Coroutine until Ali can show me the way he wanted 
    //to accomplish this task next week.
    #region hammerSmashUPDATE

    enum ability{ hammerSmashDown };

    IEnumerator hammerSmashDown()
    {
        if (ib.actionAllowed)
        {
            ib.setBufferFalse();

            #region Debug Log

            if (hammerDebug)
            {
                Debug.Log("hammerSmash: actionAllowed set to false");
            }

            #endregion

            comboCount = 0;

            //Run animation and wait for keyframe to spawn AOE 
            ac.smash();
            yield return new WaitForSeconds(1.2f);
            ControlCameraShake.shakeOn = true;
            hammerSmashTemp = Instantiate(hammerSmashPrefab, hammerSmashSpawn.position, hammerSmashSpawn.transform.rotation, gameObject.transform);

            //Wait for AOE to affect enemies then delete
            Debug.Log("TIMER: 1 Second");
            yield return new WaitForSeconds(1);
            Debug.Log("HammerSmash has been removed");
            Destroy(hammerSmashTemp, 2);
            AttackEnd();
        }

        else
        {
            Debug.Log("action not allowed");
        }
    }

    #endregion

    #region Components

    AnimController ac;

    InputControl ic;

    InputBuffer ib;

    MovementHelper mh;

    AbilitiesCooldown abilities; 

    AimShoot aims;

    GameObject Aimshoot;

    public SimpleCameraShake ControlCameraShake;

    #endregion

    #region Variables

    #region Debug Toggles
    [Header("Debug Settings")]

    [SerializeField] public bool combatDebug;

    [SerializeField] public bool comboDebug;

    [SerializeField] public bool hammerDebug;

    [SerializeField] public bool whirlwindDebug;

    [SerializeField] public bool rangedDebug;

    #endregion

    #region Animator Variables

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
    public bool isAlive = true;

    public bool isPlaying = true;

    private bool isInCombo = false;

    private Vector3 playerSize;

    private float rotationAmount;
//    private int wastedClicks = 0;

    //Tracks player checkpoints and where they will respawn 
    [SerializeField] private GameObject respawnPoint;

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

    public int comboCount;

    [SerializeField] private int attackTimer;

    public bool isAttacking = false;

    Sword_Script sword;

    #endregion

    #region Abilities

    [SerializeField] public GameObject abilitySpawn;

    [SerializeField] public bool IsAimOn = false;

    #region Dash

    [Header("Dash Ability")]

    [SerializeField] private GameObject dashRangePrefab;

    [SerializeField] private int dashDamage;

    [SerializeField] private int dashSpeed;

    //[SerializeField] private Vector3 dashOffset = new Vector3(0.0f, 0.0f, 1.0f);

    private GameObject dashTemp = null;

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

    public bool isSpinning = false;

    #endregion

    #region Ranged

    [Header("Ranged Ability")]

    [SerializeField] private GameObject RangePrefab;

    [SerializeField] private Transform RangedSpawn;

    #endregion

    #endregion

    #region Input Buffer System

    //Queue InputBufferQueue = new Queue[];

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

    [SerializeField] private GameObject healthEffect;

    private GameObject heTemp;

    [SerializeField] private int healthEffectTimer;

#endregion

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region Initialization

        #region Components

        Aimshoot = GameObject.FindGameObjectWithTag("FreeAimer");

        aims = Aimshoot.transform.GetComponent<AimShoot>();

        ac = this.transform.GetComponent<AnimController>();
        
        ib = this.transform.GetComponent<InputBuffer>();

        ic = this.transform.GetComponent<InputControl>();

        mh = this.transform.GetComponent<MovementHelper>();

        abilities = this.transform.GetComponent<AbilitiesCooldown>();

        #endregion

        #region Health

        if (!HealthBar)
            HealthBar = GameObject.FindGameObjectWithTag("Health Bar");

        healthBar = HealthBar.GetComponent<HealthBar>();
       
        currentHealth = maxHealth;

        healthBar.SetMaxHealth(maxHealth);

        healthBar.SetHealth(currentHealth);

        #endregion

        #region Combat

        comboCount = 0;

        sword = GetComponentInChildren<Sword_Script>();

        #endregion

        #region Abilities

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
            dashSpeed = 10;

        if (!Canvas)
            Canvas = GameObject.FindGameObjectWithTag("HUD Canvas").GetComponent<Canvas>();

        if (!playerStats)
            playerStats = Canvas.transform.GetChild(0).GetChild(2).transform.GetComponent<TMP_Text>();

        #endregion

        isPlaying = true;

        try
        {
            isAlive = true;

            #region Debug

            if (!combatDebug)
                combatDebug = false;

            if (!comboDebug)
                comboDebug = false;

            if (!ib.inputBufferDebug)
                ib.inputBufferDebug = false;

            if (!hammerDebug)
                hammerDebug = false;

            if (!whirlwindDebug)
                whirlwindDebug = false;

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

            if (healthEffectTimer <= 0)
                healthEffectTimer = 2;
        }

        finally
        {

        }

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
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

                    ib.actionAllowed = false;

                    comboCount = 0;

                    ac.Die();

                    Invoke("TryAgain", 2);
                }

                #endregion

                #region Update HUD

                ////updateHud();

                #endregion

                #region Check Input Buffer

                //ic.checkKeyboardInput();

                if (ib.actionAllowed)
                {
                    ib.tryBufferedAction();
                }

                #endregion

                // Debug.Log("Grounded: " + controller.isGrounded + " vSpeed: " + vSpeed);
            }

            //if (Input.GetMouseButtonDown(1))
            //    IsAimOn = true;

            //if (Input.GetMouseButtonUp(1))
            //    IsAimOn = false;
        }
    }

    private void LateUpdate()
    {
        //this.transform.rotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y + rotationAmount, this.transform.rotation.z, this.transform.rotation.w);
    }

    //void FunIdle()
    //{
    //    if(Random.Range(1, 5) == 1)
    //    {
    //        animator.SetFloat("Idle_Fun", 2);
    //    }
    //}

    #region HUD

    //private void updateHud()
    //{
    //    playerStats.text = "Lives: " + Lives;

    //    healthBar.SetHealth(currentHealth);

    //    healthBar.SetMaxHealth(maxHealth);
    //}

    #endregion

    #region Collision and Trigger Handling

    //Tracks player collision 

    //Tracks triggers / pickups

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Max Health Pickup")
        {
            Destroy(collision.gameObject);
            Debug.Log("Health Pickup Worked");
        }

        if(collision.gameObject.tag == "Killbox")   //For Testing Purposes, Can also be implemented in full game as bug failsafe. Can use die() to take away a players life if they fall off or in water.
        {
            gameObject.transform.position = respawnPoint.transform.position;
            ac.respawn();
            //die();
        }
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

    void createHealthEffect()
    {
        heTemp = Instantiate(healthEffect, abilitySpawn.transform.position, abilitySpawn.transform.rotation, gameObject.transform);

        Destroy(heTemp, healthEffectTimer);
    }

    private void pickupHealth()
    {
        currentHealth = maxHealth;

        //updateHud();

        createHealthEffect();
    }

    private void pickupMaxHealth()
    {
        maxHealth += 50;

       // updateHud();
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

    public void takeDamage(Transform dmgSource,  int dmgDealt)
    {
        #region Debug Log

        if (combatDebug)
        {
            Debug.Log("Combat System: takeDamage called");

            Debug.Log("Combat System: damage dealt = " + dmgDealt);

            Debug.Log("Combat System: damage source = " + dmgSource.name);
        }

        #endregion

        comboCount = 0;

        ac.takeDamage();

        currentHealth -= dmgDealt;

        healthBar.SetHealth(currentHealth);

        if (combatDebug)
        {
            Debug.Log("Combat System: remaining health = " + currentHealth);
        }
    }
    

    #endregion

  

    #endregion

    #region Basic Attack 
    
    public void comboAttack1()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboAttack1 called");
        }

        #endregion

        if (ib.actionAllowed)
            ib.setBufferFalse();

        comboCount = 1;

        isAttacking = true;

        ac.attack(comboCount);

        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("comboCount set to 1 via comboAttack1()");

            Debug.Log("Combo System: animator set");

            Debug.Log("Combo System: comboCount: " + comboCount);

            Debug.Log("actionAllowed = " + ib.actionAllowed);
        }

        #endregion
    }

    public void comboAttack2()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboAttack2 called");
        }

        #endregion

        if (ib.actionAllowed)
            ib.setBufferTrue();

        comboCount = 2;

        isAttacking = true;

        ac.attack(comboCount);

        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("comboCount set to 2 via comboAttack2()");

            Debug.Log("Combo System: animator set");

            Debug.Log("Combo System: comboCount: " + comboCount);

            Debug.Log("actionAllowed = " + ib.actionAllowed);
        }

        #endregion
    }

    public void comboAttack3()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboAttack3 called");
        }

        #endregion

        if (ib.actionAllowed)
            ib.setBufferFalse();

        comboCount = 3;

        isAttacking = true;

        ac.attack(comboCount);


        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("comboCount set to 3 via comboAttack3()");

            Debug.Log("Combo System: animator set");

            Debug.Log("Combo System: comboCount: " + comboCount);

            Debug.Log("actionAllowed = " + ib.actionAllowed);
        }

        #endregion
    }

    public void AttackBegins()
    {
        isInCombo = true;

        if (ib.actionAllowed)
            ib.setBufferFalse();

        if (!isAttacking)
            isAttacking = true;

        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: AttackBegins called");
            
            Debug.Log("Combo System: isInCombo = " + isInCombo);

            Debug.Log("actionAllowed = " + ib.actionAllowed);
        }

        #endregion
    }

    public void AttackEnd()
    {
        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combat System: AttackEnd called");

            Debug.Log("Combat System: actionAllowed = " + ib.actionAllowed);
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

            ac.setComboCount(comboCount);
        //}

        ib.setBufferTrue();

        isAttacking = false;

        #region Debug Log

        if (comboDebug)
        {
            Debug.Log("Combo System: comboCount set to " + comboCount + " by AttackEnd()");

            Debug.Log("Input Buffer System: inputbuffer count = " + ib.inputBuffer.Count);

            Debug.Log("actionAllowed = " + ib.actionAllowed);
        }

        #endregion

        ib.tryBufferedAction();

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

        if (ib.actionAllowed)
        {
            comboCount = 0;

            if(comboDebug)
            Debug.Log("Combo System: comboCount set to 0 by comboReset()");

            ac.setComboCount(comboCount);
        }
        else if (!ib.actionAllowed)
        {
            if (!ic.isJumping)
            {
                ib.setBufferTrue();

                #region Debug Log

                if (ib.inputBufferDebug)
                {
                    Debug.Log("Input Buffer System: comboReset Ran, actionAllowed and isJumping = false, setting actionAllowed to true");
                }

                #endregion
            }

            else
            {
                #region Debug Log

                if (ib.inputBufferDebug)
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

        if(ib.actionAllowed)
        { 
        comboCount = 0;

        if (dashRangePrefab && abilitySpawn)
            dashTemp = Instantiate(dashRangePrefab, abilitySpawn.transform.position, abilitySpawn.transform.rotation, abilitySpawn.transform);

        else
            Debug.LogError("Missing Object reference" + "dashRangePrefab: " + dashRangePrefab + "abilitySpawn: " + abilitySpawn);

        ic.dash();
        
        ac.dash();

        abilities.activateAbility1();

        ib.setBufferFalse();
        }

        else
        {
            Debug.Log("action not allowed");
        }
    }
   
    public void dashEnds()
    {
        #region Debug Log

        if (combatDebug)
        {
            Debug.Log("Combat System: dash complete");
        }

        #endregion

        Destroy(dashTemp);

        dashTemp = null;

        ib.setBufferTrue();

        AttackEnd();
    }

    public void hammerSmash()
    {
        #region Debug Log

        if (hammerDebug)
        {
            Debug.Log("hammerSmash has been pressed");
        }

        #endregion

        abilities.activateAbility3();

        StartCoroutine(hammerSmashDown());
    }

    public void whirlwind()
    {
        #region Debug Log
        
        if (whirlwindDebug)
        {
            Debug.Log("whirlwind has been called");
        }

        #endregion

        if (ib.actionAllowed)
        {
            ib.setBufferFalse();

            abilities.activateAbility2();

            comboCount = 0;

            ac.spin();

            //whirlwindTemp = Instantiate(whirlwindRangePrefab, whirlwindSpawn.position, whirlwindSpawn.transform.rotation, gameObject.transform);

            isSpinning = true;

            //Destroy(whirlwindTemp, 2);

            AttackEnd();
        }

        else
        {
            Debug.Log("action not allowed");
        }
    }

    public void whirlwindEnd()
    {
        #region Debug Log

        if (whirlwindDebug)
        {
            Debug.Log("whirlwindEnd has been called");

            Destroy(whirlwindTemp);
        }

        AttackEnd();

        #endregion

        ib.setBufferTrue();

        isSpinning = false;

        //Destroy(whirlwindTemp);
    }

    public void hammerSmashEnd()
    {
        #region Debug Log

        if (hammerDebug)
        {
            Debug.Log("HammerSmash has been called");

            Destroy(hammerSmashTemp);
        }

        #endregion

        AttackEnd();

        ib.setBufferTrue();

        Destroy(hammerSmashTemp);
    }

    public void ranged()
    {
        #region Debug Log

        if (rangedDebug)
        {
            Debug.Log("ranged ability: ranged() has been called");
        }

        #endregion

        //&& aims.isCooldown1 == false
         if (!IsAimOn )
        {

        #region Debug Log

        if (rangedDebug)
            {
                Debug.Log("ranged ability: IsAimOn = " + IsAimOn + " aims.isCooldown1 = " + aims.isCooldown1);
            }

            #endregion

            if (ib.actionAllowed)
            {
                #region Debug Log

                if (rangedDebug)
                {
                    Debug.Log("ranged ability: action allowed");
                }

                #endregion

                ib.setBufferFalse();

                abilities.activateAbility4();

                ac.throw_();

                GameObject bullet = Instantiate(RangePrefab, RangedSpawn.transform.position, RangedSpawn.transform.rotation) as GameObject;

                bullet.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);

                Destroy(bullet, 2);

                AttackEnd();
            }

            else
            {
                #region Debug Log

                Debug.Log("action not allowed");

                #endregion
            }
        }
    }


    public void setAimTrue()
    {
        IsAimOn = true;
    }

    public void setAImFalse()
    {
        IsAimOn = false;
    }

    public void rangedEnd()
    {
        ib.setBufferTrue();
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

            ac.respawn();
        }
    }

    private void respawn()
    {
        currentHealth = maxHealth;

        isAlive = true;

        ib.setBufferTrue();

        comboCount = 0;
    }

    public void kill()
    { 
        Lives -= 1;
        gameObject.transform.position = respawnPoint.transform.position;

        ac.respawn();
    }

    #endregion

    public void TryAgain()
    {
        SceneManager.LoadScene("EndScene");
    }

    public void rotatePlayer(Vector2 input)
    {
        Debug.Log("Player rotated by " + input.x);
        rotationAmount = input.x;
    }
}
