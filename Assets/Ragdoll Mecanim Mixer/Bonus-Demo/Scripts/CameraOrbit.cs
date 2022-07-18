using UnityEngine;

namespace FightDemo {
    public class CameraOrbit : MonoBehaviour {

        public HitController target;
        public HitController secondTarget;
        public float speed = 1;
        public float radius = 5;
        public float height = 0.5f;

        public float minRotX = -20;
        public float maxRotX = 80;

        private float rotX;
        private float rotY;

        private Camera cam;

        private float timeScale = 1;
        public float targetTimeScale = 1;

        public AudioSource audioSource;

        private void Start() {
            cam = Camera.main;
        }
        private void Update() {
            targetTimeScale -= Input.GetAxis("Mouse ScrollWheel");
            timeScale = Mathf.Lerp(timeScale, Mathf.Clamp01(targetTimeScale), 0.1f);
            Time.timeScale = timeScale;
            radius = (1 - timeScale) * 6 + 5;
            cam.fieldOfView = timeScale * 22 + 8;

            if(audioSource != null)
                audioSource.volume = timeScale * 0.7f + 0.3f;


            if (targetTimeScale > 0.8f && secondTarget != null)
                secondTarget = null;

            if (Input.GetButtonDown("Jump")) {
                secondTarget = null;
                targetTimeScale = 1;
            }
        }

        // Update is called once per frame
        void LateUpdate() {
            rotX -= Input.GetAxis("Mouse Y");
            rotX = Mathf.Clamp(rotX, minRotX, maxRotX);
            rotY += Input.GetAxis("Mouse X");
            if (rotY >= 360) rotY -= 360;
            if (rotY <= -360) rotY += 360;

            Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
            Vector3 tp = new Vector3(target.transform.position.x, target.head.position.y, target.transform.position.z);
            if (secondTarget != null)
                tp = (tp + new Vector3(secondTarget.transform.position.x, secondTarget.head.position.y, secondTarget.transform.position.z)) / 2;
            Vector3 pos = tp + Vector3.up * height + rot * new Vector3(0, 0, -radius);
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * speed);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * speed);
        }
    }
}