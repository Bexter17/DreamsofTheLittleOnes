using UnityEngine;
using System.Collections;
using Cinemachine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    private bool ThirdPersonCamera = true;

    public Image Aimer;

    [SerializeField]
    public CinemachineVirtualCamera vCam1; //Third Person Camera
    [SerializeField]
    public CinemachineVirtualCamera vCam2; //Free Look Camera

    void Awake()
    {
        Aimer.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Right Mouse Button Clicked");
            ThirdPersonCamera = false;
            Aimer.enabled = true;
            SwitchPriority();
        }

        if (Input.GetMouseButtonUp(1))
        {
            Debug.Log("Right Mouse Button Let go");
            ThirdPersonCamera = true;
            Aimer.enabled = false;
            SwitchPriority();
        }
    }

    private void SwitchPriority()
    {
        Debug.Log("Switch State Running");
        if (ThirdPersonCamera)
        {
            vCam1.Priority = 3;
        }
        else
        {
            vCam2.Priority = 1;
        }
    }

}