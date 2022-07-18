using UnityEngine;

namespace FightDemo {
    public class Hand : MonoBehaviour {

        private Redirector redirector;
        private HitController hitController;

        public Vector3 Velocity {
            set {
                for (int i = 0; i < velocities.Length - 1; i++) {
                    velocities[i + 1] = velocities[i];
                }
                velocities[0] = value;
            }
            get {
                Vector3 avg = Vector3.zero;
                foreach (Vector3 vel in velocities) {
                    avg += vel;
                }
                avg /= velocities.Length;
                return avg;
            }
        }
        public Vector3[] velocities = new Vector3[2];
        private Vector3 oldPos;

        void OnCollisionEnter(Collision col) {
            //if (col.impulse == Vector3.zero) return;
            Transform targetParent = col.transform.parent;
            if (targetParent == null || targetParent.name != "Ragdoll" || targetParent == transform.parent) return;
            Redirector targetRedirector = col.transform.root.GetComponent<Redirector>();
            HitController targetHitController = targetRedirector.hitController;
            if (!hitController.CanHitThisTarget(targetHitController)) return;
            if (targetHitController.TakeHit(col.gameObject, col.contacts[0].point, Velocity))
                hitController.GiveHit(targetHitController);
        }

        // Use this for initialization
        void Start() {
            redirector = transform.root.GetComponent<Redirector>();
            hitController = redirector.hitController;
        }

        // Update is called once per frame
        void FixedUpdate() {
            Velocity = (transform.position - oldPos) / Time.fixedDeltaTime;
            oldPos = transform.position;
        }
    }
}