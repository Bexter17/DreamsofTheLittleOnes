using Cinemachine;
using UnityEngine;
//using UnityEngine.InputSystem;

/*  Cinemachine State Switcher by Ross Curry
 *  2021 - 03 - 24
 *  System must use the new unity input system. I commented out all of the new input system and replaced it with functionality for the old input system.
 */
public class CinemachineStateSwitcher : MonoBehaviour
{
   // [SerializeField]
   // private InputAction action;

    private Animator animator;

    private bool ThirdPersonCamera = true;
    private bool FreeLookCamera = false;

    [SerializeField]
    private CinemachineVirtualCamera vCam1; //Third Person Camera
    [SerializeField]
    private CinemachineVirtualCamera vCam2; //Free Look Camera




    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
      //  action.Enable();
    }

    private void OnDisable()
    {
      //  action.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        //action.performed += _ => SwitchState();
        //action.performed += _ => SwitchPriority();
        //SwitchPriority();
    }

    private void SwitchState()
    {
     //   if(ThirdPersonCamera)
     //   {
    //        animator.Play("FreeLookCamera");
     //   }
     //   else
      //  {
      //      animator.Play("ThirdPersonCamera");
     //   }
     //   FreeLookCamera = !FreeLookCamera;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            animator.SetTrigger("CameraSwitch");
            Debug.Log("Right Mouse Button Clicked");
            ThirdPersonCamera = true;
            FreeLookCamera = false;
            SwitchPriority();
        }

        if (Input.GetMouseButtonUp(1))
        {
            animator.SetTrigger("CameraSwitch");
            Debug.Log("Right Mouse Button Let go");
            ThirdPersonCamera = false;
            FreeLookCamera = true;
            SwitchPriority();
        }
    }

    private void SwitchPriority()
    {
        Debug.Log("Switch State Running");
        if (ThirdPersonCamera)
        {
            vCam1.Priority = 1;
            vCam2.Priority = 0;
        }
        else
        {
            vCam2.Priority = 1;
            vCam1.Priority = 0;
        }
        FreeLookCamera = !FreeLookCamera;
        
    }
}
