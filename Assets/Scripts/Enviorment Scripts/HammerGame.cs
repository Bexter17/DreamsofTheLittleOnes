using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerGame : MonoBehaviour
{
    /*When player uses their hammer smash ability on the Test Your Strength Game, the box will fly up and hit the bell!
    This will open a new area for the player to explore.
    Target will be the bell, this script is attached to the moving part of the game at bottom. (Square) For now.*/
    public GameObject target;
    public bool isHit = false;

    /*
     * Put the wall opening from hammer game code below here 
     */
    public bool wallOpen = false;
    public GameObject gate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isHit == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 10f * Time.deltaTime);
        }

        if (wallOpen == true)
        {
            #region Debug Log
            Debug.Log("Gate is being opened to the DARK CARNIVAL!");
                #endregion
            Destroy(gate);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isHit = true;
        Debug.Log("Trigger has been hit!");
        //Check if player has used the hammer smash to play game
        if (other.gameObject.CompareTag("HammerSmashAOE")) {
            Debug.Log("Box is heading to bell");
            #region Debug Log
            Debug.Log("Hit Hammer Game with hammer smashAOE!");
            #endregion
            isHit = true;
        }

        //Check if player has won the game and opened the new area
        if (other.gameObject.CompareTag("Bell")) {
            #region Debug Log
            Debug.Log("Player has won Hammer Game!");
            #endregion
            wallOpen = true;
        }
    }
}
