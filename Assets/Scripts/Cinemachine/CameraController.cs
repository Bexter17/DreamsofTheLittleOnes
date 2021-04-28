using UnityEngine;
using System.Collections;
using Cinemachine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    private bool ThirdPersonCamera = true;

    GameObject Player;

    CharacterMechanics cm;

    public Image Aimer;

    public GameObject respawnPoint;

    [SerializeField]
    public CinemachineVirtualCamera vCam1; //Third Person Camera
    [SerializeField]
    public CinemachineVirtualCamera vCam2; //Free Look Camera

    void Awake()
    {
        Aimer.enabled = false;
    }

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        cm = Player.transform.GetComponent<CharacterMechanics>();

        vCam2.transform.position = respawnPoint.transform.position;
    }

    void Update()
    {
        if (cm.IsAimOn)
        {
            Debug.Log("Right Mouse Button Clicked");
            ThirdPersonCamera = false;
            Aimer.enabled = true;
            SwitchPriority();
        }

        if (!cm.IsAimOn)
        {
            vCam2.transform.position = respawnPoint.transform.position;
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