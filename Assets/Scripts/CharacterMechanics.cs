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

    //Tracks if the playe is currently alive or not
    private bool isAlive = true;

    //tracks if the player is mid attack
    public bool isAttacking = false;

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

    //Boolean to track if the player is on the ground or in the air
    public bool isGrounded;

    //holds the box collider for the attack range
    public GameObject attackRangePrefab;

    public GameObject DashRangePrefab;

    //creates atemporary, destructable version of the prefab
    private GameObject attackTemp;

    private GameObject dashTemp;

    private int comboCount;

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

    //Tracks player checkpoints and where they will respawn 
    public GameObject respawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        try
        {
            //Accesses the CharacterController component on the character object 
            controller = GetComponent<CharacterController>();

            isAlive = true;

            //Accesses the Animator component
            animator = GetComponent<Animator>();

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
        catch (NullReferenceException e)
        {
           // Debug.LogWarning(e.Message);
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

          //  Debug.Log("Speed: " + moveDirection);
            //animator.SetFloat("Speed", transform.TransformDirection(controller.velocity).z);
            //animator.SetFloat("Speed", transform.InverseTransformDirection(controller.velocity).z);
            //animator.SetFloat("Speed", curSpeed);
            animator.SetFloat("Speed", Input.GetAxis("Vertical"));
            animator.SetBool("isGrounded", controller.isGrounded);

            //Enables the player to use Ability 1
            if (Input.GetButtonDown("Fire1"))
            {
                Debug.LogWarning(isAttacking);

                if (!isAttacking)
                {
                    Debug.Log("Attack has been pressed");
                    //animator.SetTrigger("Attack");
                    comboCount = 1;
                    animator.SetInteger("Counter", comboCount);

                    //AttackEnd();
                }
                else
                {
                    switch (comboCount)
                    {
                        case 0:
                            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Male Sword Stance"))
                            {
                                comboCount++;
                            }                            
                            break;
                        case 1:
                            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Male Attack 1"))
                            {                                
                                comboCount++;
                                Debug.Log("attack 2 start");
                                //animator.SetTrigger("Attack");
                            }
                            break;
                        case 2:
                            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Male Attack 2"))
                            {
                                comboCount++;
                                Debug.Log("attack 3 start");
                                //animator.SetTrigger("Attack");
                            }
                            break;
                        case 3:
                            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Male Attack 3"))
                            {
                                comboCount = 0;
                            }
                            break;
                    }
                    animator.SetInteger("Counter", comboCount);

                }
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

    public void AttackBegins()
    {
        Debug.Log("AttackBegins called");
        isAttacking = true;
    }
    public void AttackEnd()
    {
        // not sure where this is working correctly
        Debug.Log("Attack complete");

        if (animator.GetInteger("Counter") == comboCount)
        {
            Debug.LogWarning(comboCount);

            comboCount = 0;
            isAttacking = false;
            animator.SetInteger("Counter", comboCount);
        }

        animator.SetInteger("Counter", comboCount);
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
