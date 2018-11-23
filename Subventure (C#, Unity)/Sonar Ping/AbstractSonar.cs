using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractSonar : MonoBehaviour {
    [SerializeField]
    protected bool _isActive = false;
    [SerializeField]
    protected float _expandTimer = 3f;
    protected float _startSize = 1f, _expandValue = 40f, _timerBackup;

    protected static Camera _mainCamera;

    protected virtual void Start() {
        _mainCamera = Camera.main;
        _timerBackup = _expandTimer;
    }

    protected virtual void Update() {
        Vector3 targetPos = transform.position + _mainCamera.transform.rotation * Vector3.right;
        Vector3 targetO = _mainCamera.transform.rotation * Vector3.forward;
        transform.LookAt(targetPos, targetO);
    }

    public virtual void StartSonar() {
        _expandTimer = _timerBackup;
        _isActive = true;
    }

    public virtual void EndSonar() {
        gameObject.transform.localScale = new Vector3(_startSize, _startSize, _startSize);
        _isActive = false;
    }
}
