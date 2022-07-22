using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class DeprecatedCopyLimb : MonoBehaviour
{
    [FormerlySerializedAs("targetLimb")] [SerializeField]
    private Transform animatedLimb;

    private ConfigurableJoint _configurableJoint;

    private Quaternion _lastAnimatedRotation;

    void Start()
    {
        _configurableJoint = gameObject.GetComponent<ConfigurableJoint>();
        _lastAnimatedRotation = animatedLimb.transform.localRotation;
    }

    private void FixedUpdate()
    {
        SyncPhysics();
    }

    private void SyncPhysics()
    {
        Quaternion localRotation = animatedLimb.transform.localRotation;
        _configurableJoint.targetRotation = Quaternion.Inverse(localRotation) * _lastAnimatedRotation;
    }
}