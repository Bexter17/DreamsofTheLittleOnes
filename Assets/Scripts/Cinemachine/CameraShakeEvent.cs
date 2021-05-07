using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraShakeEvent : MonoBehaviour
{

    public UnityEvent shake;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ShakeEvent", 3f, 4f);
    }

    private void ShakeEvent()
    {
        shake.Invoke();
    }
}
