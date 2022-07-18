using UnityEngine;

namespace FightDemo {
    public class EnemyController : CharController {

        public Transform targetHead;
        public Transform targetBody;
        public bool lookAt;

        public GameObject[] targets;

        private Vector3 moveTargetPosition;
        private Vector3 inputDirection;
        private float inputVelocity;
        private Rigidbody rb;
        private Animator animator;
        private HitController hitController;
        private EnemyCollectiveController collectiveController;

        private bool isRun;
        public bool isAttackState;

        public bool IsDead {
            get {
                if (hitController == null)
                    hitController = GetComponent<HitController>();
                return hitController.IsDead;
            }
        }

        // Use this for initialization
        void Start() {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            hitController = GetComponent<HitController>();
            //Time.timeScale = 0.3f;
        }

        void FixedUpdate() {
        }

        public void SetMoveTarget(Vector3 pos, bool attack) {
            moveTargetPosition = pos;
        }

        public void SetEnemyCollectiveController(EnemyCollectiveController collectiveController) {
            this.collectiveController = collectiveController;
        }

        public override void Die() {
            isAttackState = false;
            collectiveController.NewEnemiesCircle();
        }

        // Update is called once per frame
        void Update() {
            if (hitController.IsDead) return;
            //inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            //inputVelocity = Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical")));

            if (targetHead != null) {
                inputDirection = moveTargetPosition - rb.position;
            } else {
                inputDirection = Vector3.zero;
            }

            float vel = Mathf.Clamp(inputDirection.magnitude * 2, -1, 1);
            inputVelocity = Mathf.Abs(vel);

            float angle = Vector3.SignedAngle(transform.forward, (inputDirection * vel).normalized, Vector3.up);
            animator.SetFloat("direction", angle / 180);
            animator.SetFloat("velocity", inputVelocity);

            isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsTag("attack");
            float time = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            float dist = Vector3.Distance(targetBody.position, rb.position);
            if (dist <= 0.6f && !isRun) {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Default")) {
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

            if (dist > 6 && !isRun) {
                isRun = true;
                animator.SetBool("run", true);
                //Time.timeScale = 1;
            }
            if (dist <= 6 && isRun) {
                animator.SetBool("run", false);
                isRun = false;
            }

        }

        void LateUpdate() {
            if (targetBody == null && isAttacking) return;
            Vector3 directionToTarget = targetBody != null ? targetBody.position - rb.position : transform.forward;
            Quaternion rotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.Euler(0, rotation.eulerAngles.y, 0), Time.deltaTime * 10);
        }

        void OnAnimatorIK() {
            if (lookAt) {
                animator.SetLookAtWeight(1, 0.5f);
                animator.SetLookAtPosition(targetHead.position);
            }
        }
    }
}