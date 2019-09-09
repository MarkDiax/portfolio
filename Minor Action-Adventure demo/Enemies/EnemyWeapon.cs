using UnityEngine;
using System.Collections.Generic;

public class EnemyWeapon : MonoBehaviour
{
	[SerializeField] float _pushForce, _pushDuration;

	int _attackDamage;
	bool _processingCollisions;
	BaseEnemy _parentEnemy;

	List<Collider> _activeColliders = new List<Collider>();
	List<Collider> _hasHitDuringAttack = new List<Collider>();

	private void Start() {
		_parentEnemy = GetComponentInParent<BaseEnemy>();
		if (_parentEnemy == null)
			Debug.LogError(gameObject + " is not attached to an Enemy!");
	}

	private void OnTriggerEnter(Collider Other) {
		if (!_activeColliders.Contains(Other))
			_activeColliders.Add(Other);
	}

	private void OnTriggerStay(Collider Other) {
		if (!_activeColliders.Contains(Other))
			_activeColliders.Add(Other);
	}

	private void OnTriggerExit(Collider Other) {
		if (_activeColliders.Contains(Other)) {
			_activeColliders.Remove(Other);
		}
	}

	private void Update() {
		if (_activeColliders.Count == 0)
			_processingCollisions = false;

		if (_processingCollisions) {
			ProcessCollisions();
		}
	}

	private void ProcessCollisions() {
		for (int i = 0; i < _activeColliders.Count; i++) {
			Player player = _activeColliders[i].GetComponent<Player>();

			if (player != null && !_hasHitDuringAttack.Contains(_activeColliders[i])) {
				OnPlayerHit(_attackDamage, player);

				_hasHitDuringAttack.Add(_activeColliders[i]);
				_activeColliders.Remove(_activeColliders[i]);
				continue;
			}
		}
	}

	private void OnPlayerHit(int Damage, Player Player) {
		print("EnemyWeapon::OnPlayerHit() \t Damage sent: " + Damage);
		Player.Controller.TakeDamage(Damage, (Player.transform.position - _parentEnemy.transform.position).normalized, _pushForce, _pushDuration);
	}

	public void Attack(int Damage) {
		_hasHitDuringAttack.Clear();
		_activeColliders.Clear();

		_attackDamage = Damage;
		_processingCollisions = true;
	}
}