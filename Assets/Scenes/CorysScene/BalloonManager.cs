using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BalloonManager : MonoBehaviour
{
    public GameObject redBalloon;
    public GameObject blueBalloon;
    public static bool GameFinished = false;
    public ForceMode forceMode;
    public static int endingGame = 0;
    protected Rigidbody rgbd;

    public Vector3 startDirection;

    // Start is called before the first frame update
    void Start()
    {
        int initialBalloonCount = (int)Random.Range(0, 5.0f);
        for (int i = 0; i < initialBalloonCount; i++)
        {
            SpawnBalloon();
        }
    }

    // Update is called once per frame
    public void Push(Vector3 direction, float magnitude)
    {
        Vector3 dir = direction.normalized;
        rgbd.AddForce(dir * magnitude, forceMode);
    }

    private void SpawnBalloon()
    {
        // Boundaries for the Balloon to spawn within the screen
        float xBoundary = 5.5f;
        float yMin = -6.4f;
        Vector3 spawnPos = new Vector3(Random.Range(-xBoundary, xBoundary), yMin);

        // Only spawn if balloon does not overlap with another balloon
        if (IsSpawnValid(spawnPos))
        {
            GameObject balloon;
            if (Random.Range(0, 1.0f) < 0.5)
            {
                balloon = blueBalloon;
            }
            else
            {
                balloon = redBalloon;
            }
            Instantiate(balloon, spawnPos, Quaternion.identity);
        }
    }

    private bool IsSpawnValid(Vector3 spawnPosition)
    {
        return Physics2D.OverlapCircle(spawnPosition, 0.1f) == null;
    }
}
