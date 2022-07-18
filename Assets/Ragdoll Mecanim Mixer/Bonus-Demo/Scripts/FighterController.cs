using UnityEngine;

namespace FightDemo {
    public class FighterController : CharController {
        public Transform target;
        public Vector3 lookAtPos;
        public float lookAtTimer;
        public bool lookAt;

        public GameObject[] targets;

        private Vector3 inputDirection;
        private float inputVelocity;
        private Rigidbody rb;
        private Animator animator;
        private HitController hitController;
        private Transform cam;

        private float findTargetTimer;
        private bool isFocused;
        private bool isRun;

        // Use this for initialization
        void Start() {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            hitController = GetComponent<HitController>();
            cam = Camera.main.transform;
            //Time.timeScale = 0.3f;

            targets = GameObject.FindGameObjectsWithTag("Enemy");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }

        void FixedUpdate() {
        }

        public override void Die() {

        }

        // Update is called once per frame
        void Update() {
            inputDirection = Quaternion.Euler(0, cam.rotation.eulerAngles.y, 0) * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            inputVelocity = Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical")));

            float angle = Vector3.SignedAngle(transform.forward, inputDirection, Vector3.up);
            animator.SetFloat("direction", angle / 180);
            animator.SetFloat("velocity", inputVelocity);

            isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsTag("attack");
            float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            if (hitController.IsDead) {
                if (Input.GetButtonDown("Fire1"))
                    hitController.Revive();
                return;
            }
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Default") || (isAttacking && time > 0.5f)) {
                FindTarget();
                if (Input.GetButtonDown("Fire1") && !isRun) {

                    animator.SetBool("side", !animator.GetBool("side"));
                    animator.SetInteger("number", Random.Range(0, 3));
                    animator.SetTrigger("attack");
                    hitController.Attack();
                }


                /*if (Input.GetKeyDown(KeyCode.K)) {
                    animator.SetBool("side", !animator.GetBool("side"));
                    animator.SetInteger("number", 3);
                    animator.SetTrigger("attack");
                }*/
            }

            if (Input.GetButtonDown("Fire3")) {
                isRun = true;
                animator.SetBool("run", true);
                //Time.timeScale = 1;
            }
            if (Input.GetButtonUp("Fire3")) {
                animator.SetBool("run", false);
                isRun = false;
            }

            Vector3 pos = transform.TransformPoint(new Vector3(0, 0.6f, 1));
            if (target != null) pos = target.position;
            if (lookAtTimer > 0) {
                lookAtTimer -= Time.deltaTime;
                lookAtPos = Vector3.Lerp(lookAtPos, pos, (2 - lookAtTimer) / 2);
            } else {
                lookAtPos = pos;
            }
        }

        void FindTarget() {
            if (isRun) {
                findTargetTimer = 0;
                isFocused = false;
                ChangeTarget(null);
                return;
            }
            if (findTargetTimer > 0) {
                findTargetTimer -= Time.deltaTime;
            } else {
                if (targets.Length == 0)
                    targets = GameObject.FindGameObjectsWithTag("Enemy");
                float dist = float.MaxValue;
                int id = -1;

                for (int i = 0; i < targets.Length; i++) {
                    Vector3 dir = targets[i].transform.position - transform.position;
                    //float tempDot = inputVelocity == 0 ? 1 : Vector3.Dot(inputDirection, dir);
                    //tempDot > 0.75 && 
                    if (dir.magnitude < dist && dir.magnitude < 2) {
                        if (!targets[i].GetComponent<HitController>().IsDead) {
                            dist = dir.magnitude;
                            id = i;
                        }
                    }
                }
                isFocused = id != -1;
                if (isFocused)
                    ChangeTarget(targets[id].GetComponent<HitController>().head);
                else
                    ChangeTarget(null);

                findTargetTimer = 0.2f;
            }
        }

        void LateUpdate() {
            if (target == null && isAttacking) return;
            //if (inputVelocity == 0) return;
            Vector3 directionToTarget = transform.forward;

            if (target == null) {
                if (inputVelocity > 0)
                    directionToTarget = inputDirection;
            } else {
                directionToTarget = target.position - rb.position;
            }
            Quaternion rotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.Euler(0, rotation.eulerAngles.y, 0), Time.deltaTime * 10);
        }

        void ChangeTarget(Transform target) {
            this.target = target;
            lookAtTimer = 2;
        }

        void OnAnimatorIK() {
            if (lookAt) {
                animator.SetLookAtWeight(1, 0.5f);
                animator.SetLookAtPosition(lookAtPos);
            }
        }
    }
}