using InputSystem;
using UnityEngine;

namespace Chubby
{
    public class DeprecatedChubbyController : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private ConfigurableJoint hipJoint;
        [SerializeField] private Rigidbody hip;
        [SerializeField] private Animator targetAnimator;
        private PlayerInputs _playerInputs;
        private bool isGrounded;

        private bool walk;

        // Start is called before the first frame update
        private void Start()
        {
            _playerInputs = GetComponent<PlayerInputs>();
        }

        // Update is called once per frame
        private void Update()
        {
#if !ENABLE_INPUT_SYSTEM
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
#else
            var horizontal = _playerInputs.move.x;
            var vertical = _playerInputs.move.y;
            var direction = new Vector3(horizontal, 0f, vertical).normalized;
#endif
            // jump
            if (_playerInputs.jump || isGrounded)
            {
                if (targetAnimator) targetAnimator.SetBool("Jump", true);
                hip.AddForce(Vector3.up * speed, ForceMode.Impulse);
            }

            if (_playerInputs.move.magnitude >= 0.1f) // validation, 0f is not accurate
            {
                var targetAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

                hipJoint.targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

                hip.AddForce(direction * speed);

                walk = true;
            }
            else
            {
                walk = false;
            }

            if (targetAnimator) targetAnimator.SetBool("Walk", walk);
        }

        private void GroundCheck()
        {
            // todo
            isGrounded = false;
        }
    }
}