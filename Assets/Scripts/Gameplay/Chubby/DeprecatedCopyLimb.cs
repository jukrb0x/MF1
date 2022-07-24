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
        SyncRotation();
    }

    /// <summary>
    ///   set physical limb rotation to match animated limb rotation.
    ///   the animated rotation should be cached in the <c>Start()</c>
    /// </summary>
    private void SyncRotation()
    {
        // todo: the position can be synced as well
        Quaternion localRotation = animatedLimb.transform.localRotation;
        _configurableJoint.targetRotation = Quaternion.Inverse(localRotation) * _lastAnimatedRotation;
    }
}