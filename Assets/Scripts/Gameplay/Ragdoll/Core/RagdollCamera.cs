using System;
using Gameplay.Ragdoll.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Ragdoll.Core
{
    public class RagdollCamera : RagdollCore
    {

        public Transform lookAt;
        public float lookSensitivity = 1,
            scrollSensitivity        = 1;

        public bool invertX = false,
            invertY         = false;

        public GameObject ragdollCamera;

        private Vector2 _rotation;
        private Vector2 _inputDelta;

        [Header("--- SMOOTHING ---")] public float smoothSpeed = 5;
        public                               bool  smooth      = true;

        private Vector3 _smoothedLookPoint, _startDirection;

        [Header("--- STEEP INCLINATIONS ---")]
        [Tooltip("Allows the camera to make a crane movement over the head when looking down," +
                 " increasing visibility downwards.")]
        public bool improveSteepInclinations = true;

        public float inclinationAngle = 30, inclinationDistance = 1.5f;


        [Header("--- DISTANCES ---")] public float minDistance = 2;
        public                               float maxDistance = 5, initialDistance = 3.5f;

        private float _currentDistance;


        [Header("--- LIMITS ---")]
        public float minVerticalAngle = -30; // How far can the camera look down."

        [Tooltip("How far can the camera look up.")]
        public float maxVerticalAngle = 60;

        [Tooltip("Which layers don't make the camera reposition.")]
        public LayerMask dontBlockCamera;

        [Tooltip("How far to reposition the camera from an obstacle.")]
        public float cameraRepositionOffset = 0.15f;

        public void Start()
        {
            // create camera object in runtime
            ragdollCamera = new GameObject("Active Ragdoll Camera", typeof(Camera));
            ragdollCamera.transform.parent = transform;

            _smoothedLookPoint = lookAt.position;
            _currentDistance = initialDistance;

            _startDirection = lookAt.forward;
        }
        public void Update()
        {
            UpdateCamera();
            AvoidObstacles();
        }

        private void OnValidate()
        {
            if (lookAt == null) lookAt = ragdoll.ragdollBody.GetPhysicalBone(HumanBodyBones.Head);
        }

        private void UpdateCamera()
        {
            // input
            _rotation.x = Mathf.Repeat(_rotation.x + _inputDelta.x * (invertX ? -1 : 1) * lookSensitivity,
                360);
            _rotation.y = Mathf.Clamp(_rotation.y + _inputDelta.y * (invertY ? 1 : -1) * lookSensitivity,
                minVerticalAngle, maxVerticalAngle);

            // Improve steep inclinations
            Vector3 movedLookPoint = lookAt.position;
            if (improveSteepInclinations)
            {
                float anglePercent = (_rotation.y - minVerticalAngle) / (maxVerticalAngle - minVerticalAngle);
                float currentDistance = ((anglePercent * inclinationDistance) - inclinationDistance / 2);
                movedLookPoint += (Quaternion.Euler(inclinationAngle, 0, 0)
                                   * MathHelper.GetProjectionOnGround(ragdollCamera.transform.forward)) * currentDistance;
            }

            // Smooth
            _smoothedLookPoint =
                Vector3.Lerp(_smoothedLookPoint, movedLookPoint, smooth ? smoothSpeed * Time.deltaTime : 1);

            ragdollCamera.transform.position = _smoothedLookPoint - (_startDirection * _currentDistance);
            // camera rotates around the look point, about the axis right/up,  by degree of _cameraRotation
            ragdollCamera.transform.RotateAround(_smoothedLookPoint, Vector3.right, _rotation.y);
            ragdollCamera.transform.RotateAround(_smoothedLookPoint, Vector3.up, _rotation.x);
            ragdollCamera.transform.LookAt(_smoothedLookPoint);
        }

        private void AvoidObstacles()
        {
            Vector3 cameraPos = ragdollCamera.transform.position;
            Vector3 lookAtPos = lookAt.position;
            Ray cameraRay = new Ray(lookAtPos, cameraPos - lookAtPos);
            bool isHit = Physics.Raycast(cameraRay, out RaycastHit hit,
                Vector3.Distance(cameraPos, lookAtPos), ~dontBlockCamera);

            if (isHit)
            {
                ragdollCamera.transform.position = hit.point + (hit.normal * cameraRepositionOffset);
                ragdollCamera.transform.LookAt(_smoothedLookPoint);
            }
        }

        // Player inputs
        public void OnLook(InputValue value)
        {
            _inputDelta = value.Get<Vector2>() / 10;
        }

        public void OnScrollWheel(InputValue value)
        {
            var scrollValue = value.Get<Vector2>();
            _currentDistance = Mathf.Clamp(_currentDistance + scrollValue.y / 1200 * -scrollSensitivity,
                minDistance, maxDistance);
        }

    }
}
