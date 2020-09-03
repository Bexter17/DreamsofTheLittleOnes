using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	CharacterMechanics cs;

	public Slider slider;
	public Image fill;

    public void Start()
    {
		cs = GetComponent<CharacterMechanics>();
	}
    public void SetMaxHealth(int health)
	{
		
		slider.maxValue = health;
		slider.value = health;

	}

	public void SetHealth(int health)
	{
		slider.value = health;

	}

}