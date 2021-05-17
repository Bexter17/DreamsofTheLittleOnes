using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    AimShoot aim;

    #endregion

    string[] controllerList;

    #region Movement

    //Determines how fast the character moves
    [SerializeField] private float movementSpeed;

    [SerializeField] private float currentSpeed;

    //Rotation speed of the character
    [SerializeField] private float rotationSpeed;

    Vector2 inputVec;

    //Variable used to add force or direction to the character
    Vector3 moveDirection;

    Vector3 lookDirection;

    Vector3 strafeDirection;

    //How much we are boosting the speed by
    [SerializeField] private float speedBoost;

    [SerializeField] private int dashSpeed;

    #endregion

    #region Jumping and Falling

    RaycastHit groundHit;

    public float groundSearchLength = 0.6f;

    Vector3 characterSize;

    //Variable for how high the character will jump
    [SerializeField] private float minimumJumpPower;

    [SerializeField] private float currentJumpPower;

    [SerializeField] private float maxJumpPower;

    [SerializeField] private float jumpAmplifier;

    private float vSpeed = 0;

    //Amount of gravity set on the player
    [SerializeField] private float gravity;

    //Boolean to track if the player is on the ground or in the air
    public bool isGrounded;

    public bool isJumping;

    public bool isFalling;

    [SerializeField] bool jumpDebug;

    GameObject raycastSpawn;

    #endregion

    #region Camera 

    public float mouseHorizontalRotationSpeed = 0.65f;

    public float mouseVerticalRotationSpeed = 0.65f;

    public float controllerHorizontalRotationSpeed = 0.65f;

    public float controllerVerticalRotationSpeed = 0.65f;

    GameObject thirdPersonCam;

    public Transform Target, Player;

    Vector2 mouseVec;

    float mouseX, mouseY;

    #endregion 

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            #region Components

            controller = this.transform.GetComponent<CharacterController>();

            //       controllerList = Input.GetJoystickNames();

            cooldown = GameObject.FindGameObjectWithTag("Abilities").GetComponent<AbilitiesCooldown>();

            ib = this.transform.GetComponent<InputBuffer>();

            cm = this.transform.GetComponent<CharacterMechanics>();

            ac = this.transform.GetComponent<AnimController>();

            aim = this.transform.GetComponent<AimShoot>();

            #endregion

            #region Movement

            if (movementSpeed <= 0)
                movementSpeed = 6.0f;

            if (maxJumpPower <= 0)
                maxJumpPower = 4.0f;

            if (minimumJumpPower == 0)
                minimumJumpPower = 2;

            currentJumpPower = 0;

            if (rotationSpeed <= 0)
                rotationSpeed = 2.0f;     //4.0f was original

            if (gravity <= 0)
                gravity = 9.81f;

            if (dashSpeed == 0)
                dashSpeed = 10;

            if (jumpAmplifier == 0)
                jumpAmplifier = 10;

            //Assigns a value to the variable
            moveDirection = Vector3.zero;

            characterSize = this.transform.localScale;

            raycastSpawn = GameObject.FindGameObjectWithTag("Raycast Spawn");

            raycastSpawn.transform.parent = this.transform;

            raycastSpawn.transform.localPosition = new Vector3(0.0f, characterSize.y * 0.5f, 0.0f);

            groundSearchLength = raycastSpawn.transform.position.y + 0.2f;

            //groundSearchLength = (characterSize.y * 0.5f);

            #endregion

            #region Camera

            thirdPersonCam = GameObject.FindGameObjectWithTag("Third Person Cam");

            Target = thirdPersonCam.transform;

            Player = this.transform;

            #endregion
        }

        catch (MissingReferenceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void FixedUpdate()
    {
        isGrounded = groundCheck(isGrounded);

        if (jumpDebug)
            Debug.Log("jumpDebug: groundCheck returns = " + isGrounded);

        ac.setGrounded(isGrounded);

        currentSpeed = movementSpeed + speedBoost;

        if (!isJumping)
        {
            if (!isGrounded)
            {
                if (isGrounded)
                {
                    land();

                    ac.setFalling(isFalling);
                    ac.setJumping(isJumping);
                }

                if (!isJumping)
                {
                    if (!isFalling)
                    {
                        fall();

                        ac.setFalling(isFalling);
                    }
                }
            }
        }

        #region Apply Gravity

        if (isJumping)
        {
            isFalling = false;

            if (jumpDebug)
                Debug.Log("isJumping true");

            if (currentJumpPower < maxJumpPower)
            {
                if (jumpDebug)
                    Debug.Log("jumpPower increased");
                currentJumpPower++;
            }

            else if (currentJumpPower >= maxJumpPower)
            {
                if (jumpDebug)
                    Debug.Log("reached maxJumpPower");
                JumpEnd();
            }

            if (jumpDebug)
                Debug.Log("Jump force applied by JumpPower");

            vSpeed += currentJumpPower * Time.deltaTime * jumpAmplifier;

            if (jumpDebug)
                Debug.Log("vSpeed = " + vSpeed);
        }

        if (!isGrounded && !isJumping)
            isFalling = true;

        if (isFalling)
        {
            if (jumpDebug)
                Debug.Log("isFalling = true");

            vSpeed -= gravity * Time.deltaTime;

            if (jumpDebug)
            {
                Debug.Log("vSpeed reduced by gravity");
                Debug.Log("vSpeed = " + vSpeed);
            }
        }

        moveDirection.y = vSpeed;

        if (controller)
            controller.Move(moveDirection * Time.deltaTime * currentSpeed);

        else
            Debug.LogError("controller not assigned!");

        Debug.Log("moved controller by " + moveDirection * Time.deltaTime * currentSpeed);
        Debug.Log("moveDirection = " + moveDirection);

        #endregion

        if (jumpDebug)
        {
            Debug.Log("jumpDebug: isGrounded =" + isGrounded);

            Debug.Log("jumpDebug: isFalling =" + isFalling);

            Debug.Log("jumpDebug: isJumping =" + isJumping);

            Debug.Log("Player transform position = " + Player.transform.position);

            try
            {
                Debug.Log("Model transform position = " + Player.GetChild(7).position);
            }

            catch (MissingReferenceException e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (cm)
        {
            if (cm.isPlaying)
            {
                //controller.center = new Vector3(0, raycastSpawn.transform.position.y, 0);

                //Assign "moveDirection" to track vertical movement
                //   moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));

                //   strafeDirection = new Vector3(0, Input.GetAxis("Horizontal"), 0);

                //Character rotation
                //transform.Rotate(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);

                //track any applied speed boosts

                //Character movement
                //Vector3 forward = transform.TransformDirection(Vector3.forward);

                //Movement speed
                //float curSpeed = Input.GetAxis("Vertical") * currentSpeed;

                //Character controller movement
                //   controller.SimpleMove(transform.forward * (Input.GetAxis("Vertical") * currentSpeed));

                //   controller.SimpleMove(transform.right * (Input.GetAxis("Horizontal") * currentSpeed));

            }
        }
    }
    #region Camera

    private void LateUpdate()
    {
        CamControl();
        // changeDirection();
        resetMovement();

        if(!isJumping && isFalling && isGrounded)
            land();
    }

    void fall()
    {
        if (jumpDebug)
            Debug.Log("fall() Called");

        if (isGrounded)
            isGrounded = false;

        if (isJumping)
            isJumping = false;

        if (!isFalling)
            isFalling = true;
    }

    void land()
    {
        if (jumpDebug)
            Debug.Log("jumpDebug: land() Called");

        if (isJumping)
            isJumping = false;

        isFalling = false;

        isGrounded = true;

        vSpeed = 0;

        if (!ib.actionAllowed)
            ib.setBufferTrue();
    }

    void CamControl()
    {
        mouseX += mouseVec.x;// * HorizontalRotationSpeed;

        mouseY -= mouseVec.y;// * VerticalRotationSpeed;

        mouseY = Mathf.Clamp(mouseY, -35, 60);

        thirdPersonCam.transform.LookAt(Target);

        Target.rotation = Quaternion.Euler(mouseY, mouseX, 0);

        Player.rotation = Quaternion.Euler(0, mouseX, 0);

        //Player.transform.localEulerAngles = new Vector3(0, mouseX, 0);
        //changeDirection();
        //lookDirection = transform.TransformDirection(lookDirection);

        moveDirection = new Vector3(inputVec.x, 0, inputVec.y);

        moveDirection = transform.TransformDirection(moveDirection);
    }

    #endregion

    #region Input System Commands
    public void OnMouseCamera(InputValue input)
    {
        if (cm)
        {
            if (cm.isPlaying)
            {
                mouseVec = input.Get<Vector2>();

                mouseVec.x *= mouseHorizontalRotationSpeed;

                mouseVec.x *= mouseHorizontalRotationSpeed;

                cm.rotatePlayer(mouseVec);
            }
        }
    }

    public void OnControllerCamera(InputValue input)
    {
        if (cm)
        {
            if (cm.isPlaying)
            {
                mouseVec = input.Get<Vector2>();

                mouseVec.x *= controllerHorizontalRotationSpeed;

                mouseVec.y *= controllerVerticalRotationSpeed;

                cm.rotatePlayer(mouseVec);
            }
        }
    }

    public void OnMove(InputValue input)
    {
        if (cm.isPlaying)
        {
            inputVec = Vector2.zero;
            inputVec = input.Get<Vector2>();

            //moveDirection = Vector3.zero;
        }
    }

    void resetMovement()
    {

    }

    /*
     Godmode - G, Select
     Killswitch - K
     Helped with bearicade
     Tried to help with github issue
     Fixed jump
     Fixed grounding raycast
    */

    void changeDirection(Vector3 direction)
    {

    }

    //  public void checkKeyboardInput()
    // {
    //float x = Input.GetAxisRaw("Horizontal");

    //float z = Input.GetAxisRaw("Vertical");

    //if (x > 0.01f || x < -0.01f)
    //{

    //}

    //if (z > 0.01f || z < -0.01f)
    //{

    //}

    //if (Input.GetButtonDown("Jump") && isGrounded)
    //{

    public void OnJump()
    {
        if (cm.isPlaying)
        {
            #region Debug Log

            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: Jump has been pressed");
            }

            #endregion

            ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Jump, Time.time));
        }
    }
    public void OnJumpEnd()
    {
        if (jumpDebug)
            Debug.Log("OnJmpEnd() called");

        JumpEnd();
    }

    public Vector2 getMouseData(Vector2 input)
    {
        input = mouseVec;

        return input;
    }


    //Enables the player to use Ability 1
    //if (Input.GetButtonDown("Fire1"))
    //{
    public void OnBasicAttack()
    {
        if (cm.isPlaying)
        {
            Attack();
        }
    }

    public void OnMouseAttack()
    {
        if (cm.isPlaying)
        {
            if (!cm.IsAimOn)
            {
                Attack();
            }

            else
                Throw();
        }
    }

    public void OnDash()
    {
        if (cm.isPlaying)
        {
            //Enables the player to use Ability 2
            //if (Input.GetButtonDown("Fire2") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown1 == false)
            //{
            #region Debug Log

            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: dash has been pressed");
            }

            #endregion
            if (!cooldown.isCooldown1)
                ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Dash, Time.time));
            //cm.AttackEnd();
        }
    }

    void OnHammerSmash()
    {
        if (cm.isPlaying)
        {
            //Enables the player to use Ability 3
            //if (Input.GetButtonDown("Fire3") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown3 == false)
            //{
            #region Debug Log
            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: hammerSmash has been pressed");
            }

            #endregion
            if (!cooldown.isCooldown3)
                ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.HammerSmash, Time.time));
            //cm.AttackEnd();
        }
    }

    //if (Input.GetButtonDown("Fire4") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown2 == false)
    //{
    public void OnWhirlwind()
    {
        if (cm.isPlaying)
        {
            #region Debug Log
            if (ib.inputBufferDebug)
            {
                Debug.Log("Input Buffer System: whirlwind has been pressed");
            }
            #endregion
            if (!cooldown.isCooldown2)
                ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Whirlwind, Time.time));
            //cm.AttackEnd();
        }
    }

    public void OnAimIn()
    {
        if (cm.isPlaying)
        {
            cm.setAimTrue();
        }
    }

    public void OnAimOut()
    {
        if (cm.isPlaying)
        {
            cm.setAImFalse();
        }
    }

    public void OnThrow()
    {
        if (cm.isPlaying)
        {
            Throw();
        }
    }

    #endregion

    //if (Input.GetButtonDown("Fire5") && cooldown.GetComponent<AbilitiesCooldown>().isCooldown4 == false)
    //{
    void Throw()
    {
        #region Debug Log

        if (ib.inputBufferDebug)
        {
            Debug.Log("Input Buffer System: ranged attack has been pressed");
        }

        #endregion
        if (!cooldown.isCooldown4)
            ib.inputBuffer.Add(new ActionItem(ActionItem.InputAction.Ranged, Time.time));
        //cm.AttackEnd();
    }

    void Attack()
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


    public void dash()
    {
        Debug.Log("input control dash called");
        if (!cooldown.isCooldown1)
        {
            if (ib.actionAllowed)
                controller.Move(transform.TransformDirection(Vector3.forward) * inputVec.y * dashSpeed);
        }
    }

    public bool groundCheck(bool isGrounded)
    {
        Vector3 lineStart = raycastSpawn.transform.position;
        Vector3 vectorToSearch = new Vector3(lineStart.x, lineStart.y - groundSearchLength, lineStart.z);

        Debug.DrawLine(lineStart, vectorToSearch, Color.cyan);

        if (Physics.Linecast(lineStart, vectorToSearch, out groundHit))
        {
            if (groundHit.transform.tag == "Floor" ||
                groundHit.transform.tag == "Box" ||
                groundHit.transform.tag == "Picnic Table" ||
                groundHit.transform.tag == "Train Car" ||
                groundHit.transform.tag == "Trash Can" ||
                groundHit.transform.tag == "Test Of Strength")
            {
                if (this.transform.parent == groundHit.transform)
                    this.transform.parent = null;

                return true;
            }

            else if (groundHit.transform.tag == "Platform")
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

        else
            return false;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("OnControllerColliderHit: " + hit.gameObject.name);

        if (hit.gameObject.tag == "Floor")
        {
            //isGrounded = true;

            //isFalling = false;

    //        land();

            if (isJumping)
            {
      //          isJumping = false;

        //        ib.setBufferTrue();
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

            currentJumpPower = minimumJumpPower;

            isGrounded = false;

            isJumping = true;

            isFalling = false;

            ac.jump(isGrounded, isJumping, isFalling);

            ib.setBufferFalse();

            vSpeed = currentJumpPower * jumpAmplifier * Time.deltaTime;

            #region Debug Log


            if (jumpDebug)
            {
                Debug.Log("vSpeed: " + vSpeed);

                Debug.Log("isGrounded = " + isGrounded);

                Debug.Log("isJumping = " + isJumping);

                Debug.Log("actionAllowed = " + ib.actionAllowed);
            }

        }

        #endregion
    }

    private void JumpEnd()
    {
        if (jumpDebug)
            Debug.Log("JumpEnd Called");

        isJumping = false;

        currentJumpPower = 0;

        if (!groundCheck(isGrounded))
        {
            fall();
        }
    }

    #endregion

    public void updateValues()
    {
        ac.updateValues(isGrounded, isJumping, isFalling, inputVec.y, inputVec.x);
    }
}
