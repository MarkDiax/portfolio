using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Weapon: MonoBehaviour
{
    [SerializeField] int _damage;

    protected bool processingCollisions;
    List<Collider> _activeColliders = new List<Collider>();
    List<Collider> _hasHitDuringAttack = new List<Collider>();

    protected virtual void Start() { }

    protected virtual void OnTriggerEnter(Collider Other) {
        if (!_activeColliders.Contains(Other))
            _activeColliders.Add(Other);
    }
  
    protected virtual void OnTriggerStay(Collider Other) {
        if (!_activeColliders.Contains(Other))
            _activeColliders.Add(Other);
    }

    protected virtual void OnTriggerExit(Collider Other) {
        if (_activeColliders.Contains(Other)) {
            _activeColliders.Remove(Other);
        }
    }

    private void Update() {
        if (processingCollisions && _activeColliders.Count > 0) {
            ProcessCollisions();
        }
    }

    protected virtual void ProcessCollisions() {
        for (int i = 0; i < _activeColliders.Count; i++) {
            BaseEnemy enemy = _activeColliders[i].GetComponent<BaseEnemy>();

            if (enemy != null && !_hasHitDuringAttack.Contains(_activeColliders[i])) {
                OnEnemyHit(_damage, enemy);
                _hasHitDuringAttack.Add(_activeColliders[i]);
                _activeColliders.Remove(_activeColliders[i]);
                continue;
            }

            InteractableLock interactableLock = _activeColliders[i].GetComponent<InteractableLock>();

            if (interactableLock != null)
                interactableLock.Unlock();
        }
    }

    protected void ClearStoredHits() {
        _activeColliders.Clear();
        _hasHitDuringAttack.Clear();
    }

    protected abstract void OnEnemyHit(int Damage, BaseEnemy Enemy);

    public abstract void Attack();
	public abstract void EndAttack();
}