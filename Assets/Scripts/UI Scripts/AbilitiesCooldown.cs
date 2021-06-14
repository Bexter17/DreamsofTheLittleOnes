using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitiesCooldown : MonoBehaviour
{
    [Header("Ability 1")]
    GameObject abilityObject1;
    public Image abilityImage1;
    public float cooldown1 = 5;
    public bool isCooldown1 = false;
    public KeyCode ability1;

    [Header("Ability 2")]
    GameObject abilityObject2;
    public Image abilityImage2;
    public float cooldown2 = 5;
    public bool isCooldown2 = false;
    public KeyCode ability2;

    [Header("Ability 3")]
    GameObject abilityObject3;
    public Image abilityImage3;
    public float cooldown3 = 5;
    public bool isCooldown3 = false;
    public KeyCode ability3;

    [Header("Ability 4")]
    GameObject abilityObject4;
    public Image abilityImage4;
    public float cooldown4 = 5;
    public bool isCooldown4 = false;
    public KeyCode ability4;

    void Start()
    {
        abilityObject1 = GameObject.FindGameObjectWithTag("Ability 1");
        abilityImage1 = abilityObject1.GetComponent<Image>();
        abilityImage1.fillAmount = 0;

        abilityObject2 = GameObject.FindGameObjectWithTag("Ability 2");
        abilityImage2 = abilityObject2.GetComponent<Image>();
        abilityImage2.fillAmount = 0;

        abilityObject3 = GameObject.FindGameObjectWithTag("Ability 3");
        abilityImage3 = abilityObject3.GetComponent<Image>();
        abilityImage3.fillAmount = 0;

        abilityObject4 = GameObject.FindGameObjectWithTag("Ability 4");
        abilityImage4 = abilityObject4.GetComponent<Image>();
        abilityImage4.fillAmount = 1;
    }

    void Update()
    {
        Ability1();
        Ability2();
        Ability3();
        Ability4();
    }

    public void activateAbility1()
    {
        isCooldown1 = true;
        abilityImage1.fillAmount = 1;
    }

    void Ability1()
    {
        if (isCooldown1)
        {
            abilityImage1.fillAmount -= 1 / cooldown1 * Time.deltaTime;

            if(abilityImage1.fillAmount <= 0)
            {
                abilityImage1.fillAmount = 0;
                isCooldown1 = false;
            }
        }
    }

    public void activateAbility2()
    {
        isCooldown2 = true;
        abilityImage2.fillAmount = 1;
    }

    void Ability2()
    {
        if (isCooldown2)
        {
            abilityImage2.fillAmount -= 1 / cooldown2 * Time.deltaTime;

            if (abilityImage2.fillAmount <= 0)
            {
                abilityImage2.fillAmount = 0;
                isCooldown2 = false;
            }
        }
    }

    public void activateAbility3()
    {
        isCooldown3 = true;
        abilityImage3.fillAmount = 1;
    }

    void Ability3()
    {
        if (isCooldown3)
        {
            abilityImage3.fillAmount -= 1 / cooldown3 * Time.deltaTime;

            if (abilityImage3.fillAmount <= 0)
            {
                abilityImage3.fillAmount = 0;
                isCooldown3 = false;
            }
        }
    }

    public void activateAbility4()
    {
        isCooldown4 = true;
        abilityImage4.fillAmount = 1;
    }

    void Ability4()
    {
        if (isCooldown4)
        {
            abilityImage4.fillAmount -= 1 / cooldown4 * Time.deltaTime;

            if (abilityImage4.fillAmount <= 0)
            {
                abilityImage4.fillAmount = 0;
                isCooldown4 = false;
            }
        }
    }

    public void setRangedActive()
    {
        abilityImage4.fillAmount = 0;
    }
}
