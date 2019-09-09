using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : CharacterAnimator
{
    private Player _player;
    private Camera _camera;

    private bool _onRope, _drawingBow, _aimingBow;
    private bool[] _useRootMotion = new bool[2];

    [Header("Bones")]
    [SerializeField]
    private Transform _spine;

    protected override void Awake() {
        base.Awake();

        _player = Player.Instance;
        _camera = Camera.main;
    }

    private void Start() {
        AddListeners();
    }

    private void AddListeners() {
        EventManager.InputEvent.OnCameraZoom.AddListener(OnZoom);
        EventManager.InputEvent.OnBowDraw.AddListener(OnDrawBow);
        EventManager.PlayerEvent.OnMove.AddListener((Dir) => {
            SetFloat("Move_Combat_X", Dir.x);
            SetFloat("Move_Combat_Y", Dir.y);
        });

    }

    public void OnRopeClimb() {
        if (EventManager.RopeEvent.OnRopeClimb != null)
            EventManager.RopeEvent.OnRopeClimb.Invoke();
    }

    public void OnRopeHold() {
        if (EventManager.RopeEvent.OnRopeHold != null)
            EventManager.RopeEvent.OnRopeHold.Invoke();
    }

    private void OnZoom(bool Zooming) {
        SetBool("AimBow", Zooming);
        _aimingBow = Zooming;
    }

    private void OnDrawBow(bool Drawing) {
        SetBool("DrawBow", Drawing);
        _drawingBow = Drawing;
    }

    private Quaternion _oldSpineRot;

    private void LateUpdate() {
        Quaternion targetSpineRot = _spine.rotation;

        if (_drawingBow)
            targetSpineRot = Quaternion.Euler(_spine.eulerAngles.x, _spine.eulerAngles.y, _camera.transform.eulerAngles.x);

        if (!_aimingBow)
            _oldSpineRot = _spine.rotation;

        _spine.rotation = Quaternion.Lerp(_oldSpineRot, targetSpineRot, 10 * Time.deltaTime);
        _oldSpineRot = _spine.rotation;
    }

    private void OnAnimatorMove() {
        if (_useRootMotion[0])
            transform.position += GetDeltaPosition;
        if (_useRootMotion[1])
            transform.rotation *= GetDeltaRotation;
    }

    public void SetRootMotion(bool UseRootPos, bool UseRootRot) {
        _useRootMotion[0] = UseRootPos;
        _useRootMotion[1] = UseRootRot;
    }

    public Vector3 GetDeltaPosition {
        get { return Animator.deltaPosition; }
    }

    public Quaternion GetDeltaRotation {
        get { return Animator.deltaRotation; }
    }

    public Transform UpperSpine { get { return _spine; } }
}