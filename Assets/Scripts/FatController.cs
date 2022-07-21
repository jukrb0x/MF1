using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private ConfigurableJoint hipJoint;
    [SerializeField] private Rigidbody hip;

    [SerializeField] private Animator targetAnimator;
    private bool isGrounded = false;

    private bool walk = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
#if !ENABLE_INPUT_SYSTEM
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertica`l");
#endif
        
        // jump
        if (Input.GetButtonDown("Jump") || isGrounded)
        {
            targetAnimator.SetBool("Jump", true);
            hip.AddForce(Vector3.up * this.speed, ForceMode.Impulse);
        }
        
        
        // ground check

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

            this.hipJoint.targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            this.hip.AddForce(direction * this.speed);

            this.walk = true;
        }  else {
            this.walk = false;
        }

        this.targetAnimator.SetBool("Walk", this.walk);
    }

    void GroundCheck()
    {
        isGrounded = false;
            
    }
}
