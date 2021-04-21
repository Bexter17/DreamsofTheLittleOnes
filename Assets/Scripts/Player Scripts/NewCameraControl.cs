using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCameraControl : MonoBehaviour
{
    /*
    public float RotationSpeed = 1;
    public Transform Target, Player;
    float inputX, mouseY;

    InputControl ic;

    GameObject PlayerObj;

    // Start is called before the first frame update
    void Start()
    {
        PlayerObj = GameObject.FindGameObjectWithTag("Player");

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        CamControl();
    }

    void CamControl()
    {
       mouseX += Input.GetAxis("Mouse X") * RotationSpeed;
       mouseY -= Input.GetAxis("Mouse Y") * RotationSpeed;
       mouseY = Mathf.Clamp(mouseY, -35, 60);

        transform.LookAt(Target);

        Target.rotation = Quaternion.Euler(inputvec.y, mouseX, 0);
        Player.rotation = Quaternion.Euler(0, mouseX, 0);
    }
    */
}
