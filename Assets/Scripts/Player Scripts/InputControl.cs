using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputControl : MonoBehaviour
{
    #region Components

    CharacterController controller;

    #endregion

    #region Scripts

    AnimController ac;

    CharacterMechanics cm;

    InputBuffer ib;

    AbilitiesCooldown cooldown;

    #endregion

    GameObject Player;

    string[] controllerList;

    #region Movement

    //Determines how fast the character moves
    [SerializeField] private float movementSpeed;

    [SerializeField] private float currentSpeed;

    //Rotation speed of the character
    [SerializeField] private float rotationSpeed;

    //Variable used to add force or direction to the character
    Vector3 moveDirection;

    //How much we are boosting the speed by
    [SerializeField] private float speedBoost;

    [SerializeField] private int dashSpeed;

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
    public bool isGrounded;

    public bool isJumping;

    public bool isFalling;

    [SerializeField] bool jumpDebug;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        #region Components

        controller = GetComponent<CharacterController>();

        controllerList = Input.GetJoystickNames();

        cooldown = this.transform.GetComponent<AbilitiesCooldown>();

        ib = this.transform.GetComponent<InputBuffer>();

        cm = this.transform.GetComponent<CharacterMechanics>();

        ac = this.transform.GetComponent<AnimController>();

        #endregion

        #region Movement

        if (movementSpeed <= 0)
            movementSpeed = 6.0f;

        if (jumpSpeed <= 0)
            jumpSpeed = 10.0f;

        if (rotationSpeed <= 0)
            rotationSpeed = 2.0f;     //4.0f was original

        if (gravity <= 0)
            gravity = 9.81f;

        if (dashSpeed == 0)
            dashSpeed = 10;

        //Assigns a value to the variable
        moveDirection = Vector3.zero;

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
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

        if (isGrounded)
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

        #region Apply Gravity

        vSpeed -= gravity * Time.deltaTime;

        moveDirection.y = vSpeed;

        controller.Move(moveDirection * Time.deltaTime);

        if (!isGrounded && !isJumping)
            isFalling = true;

        #endregion
    }

    public void checkKeyboardInput()
    {
        float x = Input.GetAxisRaw("Horizontal");

        float z = Input.GetAxisRaw("Vertical");

        if (x > 0.01f || x < -0.01f)
        {

        }

        if (z > 0.01f || z < -0.01f)
        {

        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            #region Debug Log

            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: Jump has been pressed");
            }

            #endregion

            ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Jump, Time.time));
        }


        //Enables the player to use Ability 1
        if (Input.GetButtonDown("Fire1"))
        {
            #region Debug Log

            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: Attack has been pressed");
            }

            #endregion

            ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Attack, Time.time));
            //AttackEnd();
        }

        //Enables the player to use Ability 2
        if (Input.GetButtonDown("Fire2") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown1 == false)
        {
            #region Debug Log

            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: dash has been pressed");
            }

            #endregion

            ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Dash, Time.time));
            //cm.AttackEnd();
        }

        //Enables the player to use Ability 3
        if (Input.GetButtonDown("Fire3") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown3 == false)
        {
            #region Debug Log
            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: hammerSmash has been pressed");
            }

            #endregion

            ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.HammerSmash, Time.time));
            //cm.AttackEnd();
        }

        if (Input.GetButtonDown("Fire4") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown2 == false)
        {
            #region Debug Log
            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: whirlwind has been pressed");
            }
            #endregion

            ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Whirlwind, Time.time));
            //cm.AttackEnd();
        }

        if (Input.GetButtonDown("Fire5") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown4 == false)
        {
            #region Debug Log

            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: ranged attack has been pressed");
            }

            #endregion

            ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Ranged, Time.time));
            //cm.AttackEnd();
        }
    }

    public void dash()
    {
        Debug.Log("input control dash called");

        if(ib.actionAllowed)
        controller.Move(transform.TransformDirection(Vector3.forward) * Input.GetAxis("Vertical") * dashSpeed);
    }

    public bool groundCheck(bool isGrounded)
    {
        Vector3 lineStart = transform.position;
        Vector3 vectorToSearch = new Vector3(lineStart.x, lineStart.y - groundSearchLength, lineStart.z);

        Debug.DrawLine(lineStart, vectorToSearch);

        Physics.Linecast(lineStart, vectorToSearch, out groundHit);

        if(groundHit.transform.tag == "Floor" || groundHit.transform.tag == "Box" || groundHit.transform.tag == "Picnic Table" || groundHit.transform.tag == "Train Car" || groundHit.transform.tag == "Trash Can" || groundHit.transform.tag == "Test Of Strength")
        {
            if (this.transform.parent == groundHit.transform)
                this.transform.parent = null;

            return true;
        }

        else if(groundHit.transform.tag == "Platform")
        {
            this.transform.parent = groundHit.transform;

            return true;
        }

        else
        {
            if (this.transform.parent == groundHit.transform)
                this.transform.parent = null;

            return false;
        }

    }

    private void JumpEnd()
    {
        if (jumpDebug)
            Debug.Log("JumpEnd Called");

        isJumping = false;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("OnControllerColliderHit: " + hit.gameObject.name);

        if (hit.gameObject.tag == "Floor")
        {
            isGrounded = true;

            isFalling = false;

            if (isJumping)
            {
                isJumping = false;

                ib.setBufferTrue();
            }

            ac.hitGround();
        }

        else if (hit.gameObject.tag == "Killbox")
        {
            cm.kill();
        }
    }

    #region Jump

    public void jump()
    {
        #region Debug Log

        if (jumpDebug)
        {
            Debug.Log("jump has been called");
        }

        #endregion

        if (isGrounded)
        {

            cm.comboReset();

            vSpeed = jumpSpeed;

            isGrounded = false;

            isJumping = true;

            isJumping = true;

            ac.jump(isGrounded, isJumping, isFalling);

            ib.setBufferFalse();

            #region Debug Log

            if (jumpDebug)
            {
                Debug.Log("jump power: " + vSpeed);

                Debug.Log("isGrounded = " + isGrounded);

                Debug.Log("isJumping = " + isJumping);

                Debug.Log("actionAllowed = " + ib.actionAllowed);
            }

        }

        #endregion
    }

    #endregion

    public void updateValues()
    {
        ac.updateValues(isGrounded, isJumping, isFalling, Input.GetAxis("Vertical"));
    }
}
