using UnityEngine;
using UnityEngine.TestTools;

namespace Gameplay.Fracture.Runtime.Scripts.Slicers
{
    [ExcludeFromCoverage]
    public class PlaneSlicer : MonoBehaviour
    {
        public float RotationSensitivity = 1f;

        public void OnTriggerStay(Collider collider)
        {
            var material = collider.gameObject.GetComponent<MeshRenderer>().material;
            if (material.name.StartsWith("HighlightSlice"))
            {
                material.SetVector("CutPlaneNormal", this.transform.up);
                material.SetVector("CutPlaneOrigin", this.transform.position);
            }
        }

        public void OnTriggerExit(Collider collider)
        {
            var material = collider.gameObject.GetComponent<MeshRenderer>().material;
            if (material.name.StartsWith("HighlightSlice"))
            {
                material.SetVector("CutPlaneOrigin", Vector3.positiveInfinity);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Q))
            {
                this.transform.Rotate(Vector3.forward, RotationSensitivity, Space.Self);
            }
            if (Input.GetKey(KeyCode.E))
            {
                this.transform.Rotate(Vector3.forward, -RotationSensitivity, Space.Self);
            }
        
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                var mesh = this.GetComponent<MeshFilter>().sharedMesh;
                var center = mesh.bounds.center;
                var extents = mesh.bounds.extents;

                var tf = this.transform;
                // +-----+-----+
                // |     |     |
                // +-----+-----+
                // |     |     |
                // +-----+-----+
                // |- z -|
                extents = new Vector3(extents.x * tf.localScale.x,
                    extents.y * tf.localScale.y,
                    extents.z * tf.localScale.z); // to World extents
                                  
                // Cast a ray and find the nearest object
                RaycastHit[] hits = Physics.BoxCastAll(tf.position, extents, tf.forward, tf.rotation, extents.z);
                //         +-----------------+ 
                //        /                 /|
                //       /        +        / |
                //      /                 /  |
                //     +-----------------+   |
                //     |                 |   |
                //     |                 | + | 
                //     |        +        |   +
                //     |                 |  /
                //     |                 | /
                //     +-----------------+ 
                //     |-      2z       -|
                foreach(RaycastHit hit in hits)
                {
                    var obj = hit.collider.gameObject;
                    var sliceObj = obj.GetComponent<Slice>();

                    if (sliceObj != null)
                    {
                        sliceObj.GetComponent<MeshRenderer>()?.material.SetVector("CutPlaneOrigin", Vector3.positiveInfinity);
                        sliceObj.ComputeSlice(this.transform.up, this.transform.position);
                    }
                }
            }
        }
    }
}
