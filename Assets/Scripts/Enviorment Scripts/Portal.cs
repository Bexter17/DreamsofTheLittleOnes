using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public int targetscene = -1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (targetscene > 0)
        {
            if (other.gameObject.tag == "Player")
            {
                SceneManager.LoadScene(targetscene);
            }
        }
    }
}
