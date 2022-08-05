using System;
using System.Collections;
using Gameplay.Fracture.Runtime.Scripts.Fragment;
using Gameplay.Fracture.Runtime.Scripts.Options;
using Gameplay.Fracture.Runtime.Scripts.Projectile;
using UnityEngine;

namespace Gameplay.Fracture.Runtime.Scripts
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    public class Fracture : MonoBehaviour
    {
        public  TriggerOptions    triggerOptions;
        public  FractureOptions   fractureOptions;
        public  RefractureOptions refractureOptions;
        public  CallbackOptions   callbackOptions;
        private ContactPoint      _firstCollidePoint;
        private RaycastHit        _firstRaycastHit;
#if DEBUG
        // public GameObject DebugTestObject;
#endif

        /// <summary>
        /// The number of times this fragment has been re-fractured.
        /// </summary>
        [HideInInspector]
        public int currentRefractureCount = 0;

        /// <summary>
        /// Collector object that stores the produced fragments.
        /// </summary>
        private GameObject _fragmentRoot;

        /// <summary>
        ///  RaycastHit info when a ray hits the fragment.
        /// </summary>
        public RaycastHit FirstRaycastHit
        {
            get
            {
                return _firstRaycastHit;
            }
            set
            {
                _firstRaycastHit = value;
            }
        }

        [ContextMenu("Print Mesh Info")]
        public void PrintMeshInfo()
        {
            var mesh = GetComponent<MeshFilter>().mesh;
            Debug.Log("Positions");

            var positions = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;

            for (int i = 0; i < positions.Length; i++)
            {
                Debug.Log($"Vertex {i}");
                Debug.Log($"POS | X: {positions[i].x} Y: {positions[i].y} Z: {positions[i].z}");
                Debug.Log($"NRM | X: {normals[i].x} Y: {normals[i].y} Z: {normals[i].z} LEN: {normals[i].magnitude}");
                Debug.Log($"UV  | U: {uvs[i].x} V: {uvs[i].y}");
                Debug.Log("");
            }
        }

        void OnValidate()
        {
            if (transform.parent != null)
            {
                // When an object is fractured, the fragments are created as children of that object's parent.
                // Because of this, they inherit the parent transform. If the parent transform is not scaled
                // the same in all axes, the fragments will not be rendered correctly.
                var scale = transform.parent.localScale;
                if ((scale.x != scale.y) || (scale.x != scale.z) || (scale.y != scale.z))
                {
                    Debug.LogWarning($"Warning: Parent transform of fractured object must be uniformly scaled in all axes or fragments will not render correctly.", transform);
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (triggerOptions.triggerType == TriggerType.Collision)
            {
                if (collision.contactCount > 0)
                {
                    // Collision force must exceed the minimum force (F = I / T)
                    var contact = collision.contacts[0];
                    // dI = F * dt
                    float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;

                    // Colliding object tag must be in the set of allowed collision tags if filtering by tag is enabled
                    bool tagAllowed = triggerOptions.IsTagAllowed(contact.otherCollider.gameObject.tag);

                    // Object is unfrozen if the colliding object has the correct tag (if tag filtering is enabled)
                    // and the collision force exceeds the minimum collision force.
                    if (collisionForce > triggerOptions.minimumCollisionForce &&
                        (!triggerOptions.filterCollisionsByTag || tagAllowed))
                    {
                        _firstCollidePoint = contact;
                        ComputeFracture();
                    }
                }
            }
        }

        // when hit, this will be called continuously
        public void RaycastFracture()
        {
            if (triggerOptions.triggerType == TriggerType.RaycastHit)
            {

                // deduct health
                if (triggerOptions.maximumHealth > 0)
                {
                    triggerOptions.maximumHealth -= Time.deltaTime;
                }
                else
                {
                    // burst a invisible projectile
                    if (triggerOptions.burstOnHit)
                    {
                        // foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Projectile"))
                        // {
                        //     obj.SetActive(false);
                        // }
                        // var projectile = Instantiate(triggerOptions.burstProjectile, _firstRaycastHit.point, Quaternion.identity);
                        var projectile = ProjectilePool.PoolInstance.GetProjectile();
                        Instantiate(projectile, _firstRaycastHit.point, Quaternion.identity);
                        projectile.GetComponent<Rigidbody>().AddForce(-_firstRaycastHit.normal * triggerOptions.rayForce, ForceMode.Impulse);
                        // projectile.GetComponent<Rigidbody>().velocity = -_firstRaycastHit.normal * triggerOptions.rayForce;
                    }
                    ComputeFracture();
                }
            }
        }


        void OnTriggerEnter(Collider collider)
        {
            if (triggerOptions.triggerType == TriggerType.Trigger)
            {
                // Colliding object tag must be in the set of allowed collision tags if filtering by tag is enabled
                bool tagAllowed = triggerOptions.IsTagAllowed(collider.gameObject.tag);

                if (!triggerOptions.filterCollisionsByTag || tagAllowed)
                {
                    ComputeFracture();
                }
            }
        }

        void Update()
        {
            if (triggerOptions.triggerType == TriggerType.Keyboard)
            {
                if (Input.GetKeyDown(triggerOptions.triggerKey))
                {
                    ComputeFracture();
                }
            }
        }

        /// <summary>
        /// Compute the fracture and create the fragments
        /// </summary>
        /// <returns></returns>
        private void ComputeFracture() // start fracture once condition is satisfied
        {
            var mesh = GetComponent<MeshFilter>().sharedMesh;

            if (mesh != null)
            {
                // If the fragment root object has not yet been created, create it now
                if (_fragmentRoot == null)
                {
                    // Create a game object to contain the fragments
                    _fragmentRoot = new GameObject($"{name}Fragments");
                    _fragmentRoot.transform.SetParent(transform.parent);

                    // Each fragment will handle its own scale
                    _fragmentRoot.transform.position = transform.position;
                    _fragmentRoot.transform.rotation = transform.rotation;
                    _fragmentRoot.transform.localScale = Vector3.one;
                }

                var fragmentTemplate = CreateFragmentTemplate();

                if (fractureOptions.asynchronous)
                {
                    switch (triggerOptions.triggerType)
                    {
                        case TriggerType.Collision:
                            Fragmenter.FirstHitPoint = _firstCollidePoint.point;
                            Fragmenter.FirstHitNormal = _firstCollidePoint.normal;
                            break;
                        case TriggerType.RaycastHit:
                            Fragmenter.FirstHitPoint = _firstRaycastHit.point;
                            Fragmenter.FirstHitNormal = _firstRaycastHit.normal;
                            break;
                    }
                    StartCoroutine(Fragmenter.FractureAsync(
                        gameObject,
                        fractureOptions,
                        fragmentTemplate,
                        _fragmentRoot.transform,
                        () =>
                        {
                            // Done with template, destroy it
                            GameObject.Destroy(fragmentTemplate);

                            // Deactivate the original object
                            gameObject.SetActive(false);

                            // Fire the completion callback
                            if ((currentRefractureCount == 0) ||
                                (currentRefractureCount > 0 && refractureOptions.invokeCallbacks))
                            {
                                if (callbackOptions.onCompleted != null)
                                {
                                    callbackOptions.onCompleted.Invoke();
                                }
                            }
                        }
                        ));
                }
                else
                {
                    switch (triggerOptions.triggerType)
                    {
                        case TriggerType.Collision:
                            Fragmenter.FirstHitPoint = _firstCollidePoint.point;
                            Fragmenter.FirstHitNormal = _firstCollidePoint.normal;
                            break;
                        case TriggerType.RaycastHit:
                            Fragmenter.FirstHitPoint = _firstRaycastHit.point;
                            Fragmenter.FirstHitNormal = _firstRaycastHit.normal;
                            break;
                    }
#if DEBUG
                    var debugHitPoint = _firstRaycastHit.point;
                    var debugHitNormal = _firstRaycastHit.normal;
                    // Instantiate(DebugTestObject, debugHitPoint, Quaternion.identity);
                    Debug.DrawRay(debugHitPoint, new Vector3(1, 0, 0), Color.red);
                    Debug.DrawRay(debugHitPoint, new Vector3(0, 1, 0), Color.green);
                    Debug.DrawRay(debugHitPoint, new Vector3(0, 0, 1), Color.blue);
                    Debug.DrawLine(debugHitNormal * 20 + debugHitPoint, debugHitPoint, Color.yellow); //
#endif
                    Fragmenter.Fracture(gameObject,
                        fractureOptions,
                        fragmentTemplate,
                        _fragmentRoot.transform);

                    // Done with template, destroy it
                    GameObject.Destroy(fragmentTemplate);

                    // Deactivate the original object
                    gameObject.SetActive(false);

                    // Fire the completion callback
                    if ((currentRefractureCount == 0) ||
                        (currentRefractureCount > 0 && refractureOptions.invokeCallbacks))
                    {
                        if (callbackOptions.onCompleted != null)
                        {
                            callbackOptions.onCompleted.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a template object which each fragment will derive from
        /// </summary>
        /// <param name="preFracture">True if this object is being pre-fractured. This will freeze all of the fragments.</param>
        /// <returns></returns>
        private GameObject CreateFragmentTemplate()
        {
            // If pre-fracturing, make the fragments children of this object so they can easily be unfrozen later.
            // Otherwise, parent to this object's parent
            GameObject obj = new GameObject();
            obj.name = "Fragment";
            obj.tag = tag;

            // Update mesh to the new sliced mesh
            obj.AddComponent<MeshFilter>();

            // Add materials. Normal material goes in slot 1, cut material in slot 2
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = new Material[2]
            {
                GetComponent<MeshRenderer>().sharedMaterial,
                fractureOptions.insideMaterial
            };

            // Copy collider properties to fragment
            var thisCollider = GetComponent<Collider>();
            var fragmentCollider = obj.AddComponent<MeshCollider>();
            fragmentCollider.convex = true;
            fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
            fragmentCollider.isTrigger = thisCollider.isTrigger;

            // Copy rigid body properties to fragment
            var thisRigidBody = GetComponent<Rigidbody>();
            var fragmentRigidBody = obj.AddComponent<Rigidbody>();
            fragmentRigidBody.velocity = thisRigidBody.velocity;
            fragmentRigidBody.angularVelocity = thisRigidBody.angularVelocity;
            fragmentRigidBody.drag = thisRigidBody.drag;
            fragmentRigidBody.angularDrag = thisRigidBody.angularDrag;
            fragmentRigidBody.useGravity = thisRigidBody.useGravity;

            // If refracturing is enabled, create a copy of this component and add it to the template fragment object
            if (refractureOptions.enableRefracturing &&
                (currentRefractureCount < refractureOptions.maxRefractureCount))
            {
                CopyFractureComponent(obj);
            }

            return obj;
        }

        /// <summary>
        /// Convenience method for copying this component to another component
        /// </summary>
        /// <param name="obj">The GameObject to copy the component to</param>
        private void CopyFractureComponent(GameObject obj)
        {
            var fractureComponent = obj.AddComponent<Fracture>();

            fractureComponent.triggerOptions = triggerOptions;
            fractureComponent.fractureOptions = fractureOptions;
            fractureComponent.refractureOptions = refractureOptions;
            fractureComponent.callbackOptions = callbackOptions;
            fractureComponent.currentRefractureCount = currentRefractureCount + 1;
            fractureComponent._fragmentRoot = _fragmentRoot;
        }
    }
}
