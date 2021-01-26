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
    }

    private void OnTriggerEnter(Collision other)
    {
        //Check if player has used the hammer smash to play game
        if (CompareTag("HammerSmash")) {
            Debug.Log("Hit Hammer Game with hammer smash!");
                isHit = true;
        }

        //Check if player has won the game and opened the new area
        if (other.gameObject.CompareTag("Bell")) {
            Debug.Log("Player has won Hammer Game!");
            wallOpen = true;
        }
    }
}
