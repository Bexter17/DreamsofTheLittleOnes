using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBuffer : MonoBehaviour
{
    GameObject Player;

    InputControl ic;

    InputBuffer ib;

    CharacterMechanics cm;

    AbilitiesCooldown cooldown;

    AnimController ac;

  

    public List<ActionItem> inputBuffer = new List<ActionItem>();

    public bool actionAllowed = true;

    [SerializeField] public bool inputBufferDebug; 
    // Start is called before the first frame update
    void Start()
    {
        cooldown = this.transform.GetComponent<AbilitiesCooldown>();

        ib = this.transform.GetComponent<InputBuffer>();

        ic = this.transform.GetComponent<InputControl>();

        cm = this.transform.GetComponent<CharacterMechanics>();

        ac = this.transform.GetComponent<AnimController>();

       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void tryBufferedAction()
    {
        if (inputBuffer.Count > 0)
        {
            foreach (ActionItem ai in inputBuffer.ToArray())
            {
                inputBuffer.Remove(ai);
                if (ai.CheckIfValid())
                {
                    doAction(ai);
                    break;
                }
            }
        }

        else
        {
            cm.comboReset();

            ac.resetCounter();

            //Debug.Log("comboCount set to 0 by tryBufferedAction()");
        }
    }

    private void doAction(ActionItem ai)
    {
        #region Debug.Log

        if (inputBufferDebug)
        {
            Debug.Log("doAction called");

            Debug.Log(ai.Action);
        }

        #endregion

        if (ai.Action == ActionItem.InputAction.Jump)
        {
            ic.jump();
        }

        if (ai.Action == ActionItem.InputAction.Attack)
        {
            #region Debug.Log

            if (inputBufferDebug)
            {
                Debug.Log("Input Buffer System: Attack Input Detected");

                Debug.Log("Input Buffer System: comboCount during input = " + cm.comboCount);
            }

            #endregion

            if (cm.comboCount == 0)
            {
                cm.comboAttack1();
            }

            else if (cm.comboCount == 1)
            {
                cm.comboAttack2();
            }

            else if (cm.comboCount == 2)
            {
                cm.comboAttack3();
            }

            else if (cm.comboCount < 0 || cm.comboCount >= 3)
            {
                cm.comboCount = 0;

                if (cm.comboDebug)
                    Debug.Log("Combo System: comboCount reset to 0 because combo was either < 0 or >= 3");

                cm.comboAttack1();
            }
        }

        if (ai.Action == ActionItem.InputAction.Dash)
        {
            cm.dash();
        }

        if (ai.Action == ActionItem.InputAction.HammerSmash)
        {
            Debug.Log("HAMMER TIME");
         
            cm.hammerSmash();

        }

        if (ai.Action == ActionItem.InputAction.Whirlwind)
        {
            cm.whirlwind();
        }

        if (ai.Action == ActionItem.InputAction.Ranged)
        {
            cm.ranged();
        }

        actionAllowed = false;
    }

    public void setBufferFalse()
    {
        if (inputBufferDebug)
            Debug.Log("action allowed set to false");

        actionAllowed = false;
    }

    public void setBufferTrue()
    {
        if (inputBufferDebug)
            Debug.Log("action allowed set to true");

        actionAllowed = true;
    }
}
