using InputSystem;
using UnityEngine;

namespace Chubby
{
    public class DeprecatedChubbyController : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float jumpForce = 60f;
        [SerializeField] private ConfigurableJoint hipJoint;
        [SerializeField] private Rigidbody hip;
        [SerializeField] private Animator targetAnimator;
        private InputEvents _inputEvents;
        private bool isGrounded;

        private bool walk;

        // Start is called before the first frame update
        private void Start()
        {
            _inputEvents = GetComponent<InputEvents>();
        }

        // Update is called once per frame
        private void Update()
        {
            var horizontal = _inputEvents.move.x;
            var vertical = _inputEvents.move.y;
            var direction = new Vector3(horizontal, 0f, vertical).normalized;
            
            // jump
            if (_inputEvents.jump /* && isGrounded */)
            {
                if (targetAnimator) targetAnimator.SetBool("Jump", true);
                hip.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _inputEvents.jump = false;
            }

            // move
            if (_inputEvents.move.magnitude >= 0.1f) // validation, 0f is not accurate
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