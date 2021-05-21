using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playVFX : MonoBehaviour
{
    public ParticleSystem effect; 
    private void Awake()
    {
      effect.Play();
    }
}
