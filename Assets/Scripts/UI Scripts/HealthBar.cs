using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
	//CharacterMechanics cs;

	GameObject canvas;
	public Slider slider;
	public Image fill;

    public void Start()
    {
		//cs = GetComponent<CharacterMechanics>();

		canvas = GameObject.FindGameObjectWithTag("HUD Canvas");

		slider = canvas.transform.GetChild(0).GetChild(0).GetComponent<Slider>();
	}
    public void SetMaxHealth(int health)
	{
		
		slider.maxValue = health;
		slider.value = health;

	}

	public void SetHealth(int health)
	{
		Debug.Log("SetHealth called");
		slider.value = health;
	}

}