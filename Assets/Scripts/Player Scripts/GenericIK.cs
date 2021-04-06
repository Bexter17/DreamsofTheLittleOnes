using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericIK : MonoBehaviour
{
    #region Components

    //Creates a charactercontoller variable named "controller"
    CharacterController controller;

    //creates a script accessable variable of the Animator component
    Animator animator;

    #endregion

    #region Variables

    [SerializeField] private bool showSolverDebug;

    [SerializeField] private bool enableFeetIK = true;

    [SerializeField] private Transform rightFoot;

    [SerializeField] private Transform leftFoot;

    private Vector3 rightFootPos, leftFootPos, rightFootIKPos, leftFootIKPos;

    private Quaternion leftFootIKRot, rightFootIKRot;

    private float lastPelvisPosY, lastRightFootPosY, lastLeftFootPosY;

    [Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;

    [Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;

    //[SerializeField] private LayerMask environmentLayer;

    [SerializeField] private float pelvisOffset = 0f;

    [Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.2f;

    [Range(0, 1)] [SerializeField] private float feetToIKPosSpeed = 0.5f;

    [SerializeField] private string leftFootVariableName = "Left Foot Curve";

    [SerializeField] private string rightFootVariableName = "Right Foot Curve";

    public bool useProIKFeatures = true;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Feet Grounding

    private void FixedUpdate()
    {
        #region IK System Check

        if (enableFeetIK == false)
        {
            return;
        }

        if (animator == null)
        {
            return;
        }

        #endregion

        #region Adjust Feet

        adjustFeetTarget(ref rightFootPos, rightFoot);

        adjustFeetTarget(ref leftFootPos, leftFoot);

        feetPositionSolver(rightFootPos, ref rightFootIKPos, ref rightFootIKRot); //Handles the solver for the right foot

        feetPositionSolver(leftFootPos, ref leftFootIKPos, ref leftFootIKRot); //Handles the solver for the left foot

        #endregion
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (enableFeetIK == false)
        {
            return;
        }

        if (animator == null)
        {
            return;
        }

        movePelvisHeight();

        //right foot ik position & rotation -- utilize pro features here
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        if (useProIKFeatures)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(rightFootVariableName));
        }

        moveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPos, rightFootIKRot, ref lastRightFootPosY);

        //left foot ik position & rotation -- utilize pro features here
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

        if (useProIKFeatures)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(leftFootVariableName));
        }

        moveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPos, leftFootIKRot, ref lastLeftFootPosY);
    }

    #endregion

    #region Feet Grounding Methods

    private void moveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
    {
        Vector3 targetIKPosition = animator.GetIKPosition(foot);

        if (positionIKHolder != Vector3.zero)
        {
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);

            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPosSpeed);

            targetIKPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIKPosition = transform.TransformPoint(targetIKPosition);

            animator.SetIKPosition(foot, targetIKPosition);
        }

        animator.SetIKPosition(foot, targetIKPosition);
    }

    private void movePelvisHeight()
    {
        if (rightFootIKPos == Vector3.zero || leftFootIKPos == Vector3.zero || lastPelvisPosY == 0)
        {
            lastPelvisPosY = animator.bodyPosition.y;
            return;
        }

        float lOffsetPos = leftFootIKPos.y - transform.position.y;

        float rOffsetPos = rightFootIKPos.y - transform.position.y;

        float totalOffset = (lOffsetPos < rOffsetPos) ? lOffsetPos : rOffsetPos;

        Vector3 newPelvisPos = animator.bodyPosition + Vector3.up * totalOffset;

        newPelvisPos.y = Mathf.Lerp(lastPelvisPosY, newPelvisPos.y, pelvisUpAndDownSpeed);

        animator.bodyPosition = newPelvisPos;

        lastPelvisPosY = animator.bodyPosition.y;
    }

    private void feetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
    {
        //raycast handling section
        RaycastHit feetOutHit;

        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.blue);

        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast))
        {
            feetIKPositions = fromSkyPosition;

            feetIKPositions.y = feetOutHit.point.y + pelvisOffset;

            feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

            return;
        }

        feetIKPositions = Vector3.zero;
    }

    private void adjustFeetTarget(ref Vector3 feetPositions, Transform foot)
    {
        feetPositions = foot.position;

        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }

    #endregion
}