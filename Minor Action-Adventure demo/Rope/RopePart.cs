using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RopePart : MonoBehaviour
{
    private Rigidbody _body;
    private CharacterJoint _joint;
    private CapsuleCollider _collider;

    [HideInInspector]
    public Vector3 defaultPos;

    [HideInInspector]
    public Transform playerHolder;

    private void Awake() {
        _joint = GetComponent<CharacterJoint>();
        _collider = GetComponent<CapsuleCollider>();
        _body = GetComponent<Rigidbody>();

        playerHolder = transform.GetChild(0);
    }

    public float Radius {
        get { return _collider.radius; }
    }

    private void Update() {

        if (CharacterJoint == null)
            return;

        //if pull force is 70% on its way to breaking
        if ((_joint.currentForce.magnitude / _joint.breakForce * 1000) > 70) {
            //play sound as a warning
        }
    }

    public void OnJointBreak(float breakForce) {
        Rope.Respawn();
    }

    public CharacterJoint CharacterJoint {
        get {
            if (_joint == null)
                _joint = GetComponent<CharacterJoint>();
            return _joint;
        }
    }

    public Rigidbody Rigidbody {
        get {
            if (_body == null)
                _body = GetComponent<Rigidbody>();
            return _body;
        }
    }

    public RopeBehaviour Rope { get; set; }

    public int GetRopeIndex() {
        for (int i = 0; i < Rope.ropeSegments.Count; i++) {
            if (this == Rope.ropeSegments[i])
                return i;
        }

        return 0;
    }

    public void IsTrigger(bool Trigger) {
        _collider.isTrigger = Trigger;
    }
}