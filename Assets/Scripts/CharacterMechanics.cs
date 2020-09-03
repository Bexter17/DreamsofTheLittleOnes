using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Character Mechanics Prototype #5
//Made By Craig Walker
//Fixed Jump
//Removed 1st Person Control 
//Added Abilities 1,2, and 3
//Added respawnPoints and death resetting
//Cleaned up excess code
//Health loss on collision with "Enemy" or "Projectile"

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class CharacterMechanics : MonoBehaviour
{
    //Creates a charactercontoller variable named "controller"
    CharacterController controller;

    //Tracks player health
    int Health = 5;

    //Tracks incoming damage
    int Damage = 0;

    //Tracks what is damaging the player
    private GameObject damageSource;

    //Tracks player's lives
    int Lives = 3;

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

    //Variable used to add force or direction to the character
    Vector3 moveDirection;

    //creates the animator version of the Animator component
    Animator animator;

    //Tracks player checkpoints and where they will respawn 
    public GameObject respawnPoint;

    // Start is called before the first frame update
    void Start()
    {
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
                Debug.Log("Speed not set on " + name + " defaulting to " + speed);
            }

            if (jumpSpeed <= 0)
            {
                jumpSpeed = 8.0f;
                Debug.Log("JumpSpeed not set on " + name + " defaulting to " + jumpSpeed);
            }

            if (rotationSpeed <= 0)
            {
                rotationSpeed = 10.0f;
                Debug.Log("RotationSpeed not set on " + name + " defaulting to " + rotationSpeed);
            }

            if (gravity <= 0)
            {
                gravity = 9.81f;
                Debug.Log("Gravity not set on " + name + " defaulting to " + gravity);
            }

            //Assigns a value to the variable
            moveDirection = Vector3.zero;

            // Manually throw the Exception or the System will throw an Exception
            // throw new ArgumentNullException("Whoops");
        }
        catch (NullReferenceException e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            //If health drops to or below zero, the player dies
            if (Health <= 0)
            {
                animator.SetTrigger("Die");
                isAlive = false;
                Die();
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

            Debug.Log("Speed: " + moveDirection);
            //animator.SetFloat("Speed", transform.TransformDirection(controller.velocity).z);
            //animator.SetFloat("Speed", transform.InverseTransformDirection(controller.velocity).z);
            //animator.SetFloat("Speed", curSpeed);
            animator.SetFloat("Speed", Input.GetAxis("Vertical"));
            animator.SetBool("isGrounded", controller.isGrounded);

            //Enables the player to use Ability 1
            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("Ability1 has been pressed");
                Ability1();
            }

            //Enables the player to use Ability 2
            if (Input.GetButtonDown("Fire2"))
            {
                Debug.Log("Ability2 has been pressed");
                Ability2();
            }

            //Enables the player to use Ability 3
            if (Input.GetButtonDown("Fire3"))
            {
                Debug.Log("Ability3 has been pressed");
                Ability3();
            }

            //Enables the player to jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                Debug.Log("Jump has been pressed");
                vSpeed = jumpSpeed;
                //animator.SetTrigger("Jump");
            }

            vSpeed -= gravity * Time.deltaTime;
            moveDirection.y = vSpeed;
            controller.Move(moveDirection * Time.deltaTime);
            Debug.Log("Grounded: " + controller.isGrounded + " vSpeed: " + vSpeed);
        }
    }
    //Triggers at the end of an animation to turn bool off
    public void AttackEnds()
    {
        isAttacking = false;
    }

    //Tracks player collision 
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("OnControllerColliderHit: " + hit.gameObject.name);

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
            damageSource = hit.gameObject;
            Health--;
            animator.SetTrigger("Got Hit");
            Debug.Log("Player Hit by Enemy");
            takeDamage();
        }

        if (hit.gameObject.tag == "Projectile")
        {
            Health--;
            animator.SetTrigger("Got Hit"); 
            Debug.Log("Player Hit by projectile");
        }
    }

    //Tracks triggers / pickups
    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.tag == "Teleport Potion")
        {
            speed *= speedBoost;
            Destroy(c.gameObject);
            Debug.Log("Drank Teleportation Potion");
            StartCoroutine(stopSpeedBoost());
        }

        if (c.gameObject.tag == "Jump Boost")
        {
            jumpSpeed += jumpBoost;
            Destroy(c.gameObject);
            Debug.Log("Jump Boost Applied");
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
            Debug.Log("Speed Boost Applied");
            StartCoroutine(stopSpeedBoost());
        }
    }

    //Trigger after death animation to fully kill player object
    public void Die()
    {
        Lives--;
        gameObject.transform.position = respawnPoint.transform.position;
        isAlive = true;
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

    public int takeDamage()
    {
        //Damage = damageSource.Damage;
        Health -= Damage;
        return Health;
    }

    public void Ability1()
    {
        Debug.Log("Ability 1 has been pressed");
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
