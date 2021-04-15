using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementHelper : MonoBehaviour
{
    GameObject Player;

    static private float maxWanderDuration = 2.0f;
    static private float wanderCounter = 0.0f;
    static private System.Random r = new System.Random();

    void Start()
    {
        if (!Player)
            Player = GameObject.FindGameObjectWithTag("Player");
    }

    public class MovementResult
    {
        public Vector3 newPosition = Vector3.zero;
        public Vector3 newOrientation = Vector3.zero;
    }

    public class InputParameters
    {
        public InputParameters(Transform current, Transform target, float updateDelta, float speed)
        {
            currentTransform = current;
            targetTransform = target;
            currentUpdateDuration = updateDelta;
            maxSpeed = speed;
        }

        public InputParameters(InputParameters o)
        {
            currentTransform = o.currentTransform;
            targetTransform = o.targetTransform;
            currentUpdateDuration = o.currentUpdateDuration;
            maxSpeed = o.maxSpeed;
        }

        public InputParameters()
        {
            currentUpdateDuration = 0.0f;
            maxSpeed = 1.0f;
        }

        public Transform currentTransform;
        public Transform targetTransform;
        public float currentUpdateDuration;
        public float maxSpeed;
    }

    public enum MovementBehaviors
    {
        Idle,
        Moving
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal static void findPosition(InputParameters inputData, ref MovementResult result)
    {
        // TODO: Implement logic to write the new desired position that moves closer to the target into result.newPosition

        if (inputData.targetTransform != inputData.currentTransform)
        {
            Vector3 movementDirection = Vector3.Normalize(inputData.targetTransform.position - inputData.currentTransform.position);

            result.newPosition = inputData.currentTransform.position + (movementDirection * inputData.maxSpeed * inputData.currentUpdateDuration);
        }
    }
}
