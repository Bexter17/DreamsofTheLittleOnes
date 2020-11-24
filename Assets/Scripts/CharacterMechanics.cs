using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;


// Character Mechanics Prototype #5
//Made By Craig Walker
//Changed ability 1 to instantiate a damage block

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class CharacterMechanics : MonoBehaviour
{
    //HealthBar for Player
    public HealthBar healthBar;

    //Creates a charactercontoller variable named "controller"
    CharacterController controller;

    //Tracks player health
    public int currentHealth = 5;

    //Max health
    public int maxHealth = 50;

    //Tracks incoming damage
    int Damage = 0;

    //Tracks what is damaging the player
    private GameObject damageSource;

    //Tracks player's lives
    int Lives = 1;

    //Tracks if the player is currently alive or not
    private bool isAlive = true;

    private bool isInCombo = false;

    private int wastedClicks = 0; 

    //Tracks if GodMode is Active
    public bool isGodMode;

    //Sets the length of the Mode
    public float timerGodMode;

    //Variable to amplify the jumpBoost
    public float jumpBoost;

    //How long the jump boost lasts
    public float timerJumpBoost;

    //How much we are boosting the speed by
    public float speedBoost;

    //How long the speed boost lasts
    public float timerSpeedBoost;

    //Determines how fast the character moves
    public float speed;

    //Variable for how high the character will jump
    public float jumpSpeed;

    private float vSpeed = 0;

    //Rotation speed of the character
    public float rotationSpeed; // Used when not using MouseLook.CS to rotate character
    
    //Amount of gravity set on the player
    public float gravity;

    //Allows you to toggle hold to crouch or press to crouch
    public bool crouchIsToggle;

    //Tracks if the player is actively hold crouch key
    public bool isCrouched = false;

    //Tracks if player is too busy to attack
    public bool isBusy = false;

    //Boolean to track if the player is on the ground or in the air
    public bool isGrounded;

    //holds the box collider for the attack range
    public GameObject attackRangePrefab;

    public GameObject DashRangePrefab;

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

    //Variable used to add force or direction to the character
    Vector3 moveDirection;

    //creates a script accessable variable of the Animator component
    Animator animator;

    AnimatorClipInfo[] CurrentClipInfo;

    private string animName;

    //Tracks player checkpoints and where they will respawn 
    public GameObject respawnPoint;

    Sword_Script sword;

    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            //If health drops to or below zero, the player dies
            if (currentHealth <= 0)
            {
                
                animator.SetTrigger("Die");
                isAlive = false;
            }

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

            animator.SetFloat("Speed", Input.GetAxis("Vertical"));
            animator.SetBool("isGrounded", controller.isGrounded);

            //Enables the player to use Ability 1
            if (Input.GetButtonDown("Fire1"))
            {
                Attack();
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

            vSpeed -= gravity * Time.deltaTime;
            moveDirection.y = vSpeed;
            controller.Move(moveDirection * Time.deltaTime);
           // Debug.Log("Grounded: " + controller.isGrounded + " vSpeed: " + vSpeed);
        }
    }

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
            animator.SetTrigger("Got Hit");
            TakeDamage(1);
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
    public void Die()
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }

    public void QueueAttack()
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

            Attack();
        }
    }
    public void Attack()
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
                    Attack();
                }

                else if (animName == ("Attack 2"))
                {
                    comboCount = 2;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                else if (animName == ("Attack 3"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
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
                    Attack();
                }

                else if (animName == ("Attack 2"))
                {
                    comboCount = 3;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                else if (animName == ("Attack 3"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                else if (animName != ("Attack 1"))
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
                if (animName == ("Attack 2"))
                {
                    Debug.Log("attack 3 start");
                    comboCount = 3;
                    Debug.LogWarning(comboCount);
                    animator.SetInteger("Counter", comboCount);
                    animator.SetTrigger("Attack");
                    //isInCombo = false;
                }

                else if (animName == ("Idle") || animName == ("Run") || animName == ("Walk"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                //else if (animName == ("Attack 1"))
                //{
                //    comboCount = 1;
                //    Debug.LogWarning(comboCount);
                //    isBusy = false;
                //    Attack();
                //}

                else if (animName == ("Attack 3"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                else if (animName != ("Attack 2") && animName != ("Attack 1"))
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

                if (animName == ("Attack 3"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                else if (animName == ("Idle") || animName == ("Run") || animName == ("Walk"))
                {
                    comboCount = 0;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                else if (animName == ("Attack 1"))
                {
                    comboCount = 1;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                else if (animName == ("Attack 2"))
                {
                    comboCount = 2;
                    Debug.LogWarning(comboCount);
                    isBusy = false;
                    Attack();
                }

                else if (animName != ("Attack 3"))
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
//            Debug.Log("Attack has been pressed, you are too busy to attack");

//            if (comboCount != currentAttack)
//            {
//                Debug.Log("Attack Queued");
//                QueueAttack();
//            }
//        }
    }
    public void AttackBegins()
    {
        isInCombo = true;
        sword.SendMessage("activateAttack");
        //sends message to the players sword script to start dealing damage on collision

    }
    public void AttackEnd()
    { 
        //sends message to the players sword script to stop dealing damage on collision
        sword.SendMessage("deactivateAttack");

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
