using System;
using Gameplay.Ragdoll.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Ragdoll.Core
{
    public class RagdollCamera : RagdollCore
    {

        public Transform lookAt;
        public float     lookSensitivity   = 1;
        public float     scrollSensitivity = 1;

        public bool invertX = false;
        public bool invertY = false;

        public GameObject cameraObject;

        private Vector2 _rotation;
        private Vector2 _inputDelta;

        [Header("Smoothing")]
        public float smoothSpeed = 5;
        public bool smooth = true;

        private Vector3 _smoothedLookPoint, _startDirection;

        [Header("Blocking")]
        public LayerMask dontBlockCamera; // camera won't reposition for the layer mask
        public float cameraRepositionOffset = 0.15f; // how far to reposition the camera from an obstacle

        [Header("Steep inclination")]
        // Allows the camera to make a crane movement over the head when looking down,
        // increasing visibility downwards.
        public bool improveSteepInclinations = true;

        public float inclinationAngle    = 30;
        public float inclinationDistance = 1.5f;


        [Header("Distance")]
        public float minDistance = 1.2f;
        public float maxDistance     = 3.5f;
        public float initialDistance = 3f;

        private float _currentDistance;


        [Header("Look limits")]
        public float minVerticalAngle = -20; // look down
        public float maxVerticalAngle = 60; // look up

        public void Start()
        {
            // create camera object in runtime
            if (cameraObject == null)
            {
                cameraObject = new GameObject("Active Ragdoll Camera", typeof(Camera));
                cameraObject.transform.parent = transform;
            }

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
            // not always working
            if (lookAt == null) lookAt = ragdoll.ragdollBody.GetPhysicalBone(HumanBodyBones.Head).gameObject.transform;
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
                                   * MathHelper.GetProjectionOnGround(cameraObject.transform.forward)) * currentDistance;
            }

            // Smooth
            _smoothedLookPoint =
                Vector3.Lerp(_smoothedLookPoint, movedLookPoint, smooth ? smoothSpeed * Time.deltaTime : 1);

            cameraObject.transform.position = _smoothedLookPoint - (_startDirection * _currentDistance);
            // camera rotates around the look point, about the axis right/up,  by degree of _cameraRotation
            cameraObject.transform.RotateAround(_smoothedLookPoint, Vector3.right, _rotation.y);
            cameraObject.transform.RotateAround(_smoothedLookPoint, Vector3.up, _rotation.x);
            cameraObject.transform.LookAt(_smoothedLookPoint);
        }

        private void AvoidObstacles()
        {
            Vector3 cameraPos = cameraObject.transform.position;
            Vector3 lookAtPos = lookAt.position;
            Ray cameraRay = new Ray(lookAtPos, cameraPos - lookAtPos);
            bool isHit = Physics.Raycast(cameraRay, out RaycastHit hit,
                Vector3.Distance(cameraPos, lookAtPos), ~dontBlockCamera);

            if (isHit)
            {
                cameraObject.transform.position = hit.point + (hit.normal * cameraRepositionOffset);
                cameraObject.transform.LookAt(_smoothedLookPoint);
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
