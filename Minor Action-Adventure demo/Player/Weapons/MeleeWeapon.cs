using UnityEngine;
using System.Collections;

public class MeleeWeapon : Weapon
{
	public override void Attack() {
		ClearStoredHits();
		processingCollisions = true;
	}

	public override void EndAttack() {
		ClearStoredHits();
		processingCollisions = false;
	}

	protected override void OnEnemyHit(int Damage, BaseEnemy Enemy) {
		if (Enemy != null && !Enemy.IsDead) {
			Enemy.TakeDamage(Damage);
			GameManager.Instance.TriggerSlowmotion();
		}
	}
}