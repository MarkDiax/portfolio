using UnityEngine;
using System.Collections;
using EnemyData;
using Cinemachine;

public class StandardEnemy : BaseEnemy
{
	[SerializeField] PatrolData _patrolData;
	[SerializeField] AttackData _attackData;

	float _currentSpeed;
	int _patrolIndex;
	bool _attacking;
	bool _isAlerted;

	Coroutine _idleRoutine;
	Coroutine _leapRoutine;
	Coroutine _lookAtRoutine;
	float _currentStateTimer; // used by the current state

	EnemyWeapon _sword;

	#region Animation Events

	void A_OnAttackImpact(int Damage) {
		if (Damage == 0)
			Damage = _attackData.fallbackAttackDamage;

		_sword.Attack(Damage);
	}

	void A_OnAttackStartLeap(float Duration) {
		LeapTowards(player.transform, Duration);
	}

	void A_OnAttackEnd() {
		SwitchState(EnemyState.Tired);
		_attacking = false;
		animator.ResetTrigger(AP_AttackMelee);
	}

	void A_OnSuspendMovement(float Time) {
		moveDelay += Time;
	}

	#endregion

	#region Animation Properties
	const string AP_AttackMelee = "AttackMelee";
	const string AP_MoveDirection = "MoveDir";
	const string AP_PlayerDistance = "PlayerDistance";
	const string AP_RND = "RND"; //random generator between values 0 and 10
	const string AP_Impact = "Impact";
	const string AP_Death = "Die";
	const string AP_IsDead = "IsDead";
	#endregion

	protected override void Start() {
		base.Start();

		_sword = GetComponentInChildren<EnemyWeapon>();

		SwitchState(EnemyState.Idle);
		StartCoroutine(RandomGenerator());
	}

	private IEnumerator RandomGenerator() {
		System.Random rnd = new System.Random();

		while (true) {
			animator.SetInteger(AP_RND, rnd.Next(0, 10));
			yield return new WaitForSeconds(2f);
		}
	}

	protected override void Idle(float TimeInSeconds) {
		base.Idle(TimeInSeconds);

		if (DetectPlayer())
			SwitchState(EnemyState.MoveToAttack);

		if (_idleRoutine == null)
			_idleRoutine = StartCoroutine(IdleTimer(TimeInSeconds));
	}

	protected override void Patrol() {
		base.Patrol();

		if (DetectPlayer()) {
			SwitchState(EnemyState.MoveToAttack);
			return;
		}

		if (waypoints.Length == 0) {
			SwitchState(EnemyState.Idle);
			return;
		}

		float targetDistance = Vector3.Distance(transform.position, waypoints[_patrolIndex].position);

		if (targetDistance < _patrolData.waypointPrecision) {
			_currentSpeed -= _patrolData.deceleration * deltaTime;

			if (_currentSpeed < 0.15f) {
				_currentSpeed = 0f;
				SwitchState(EnemyState.Idle);
				_patrolIndex = MathX.Int.GetRandomIndex(_patrolIndex, waypoints.Length);
			}

			return;
		}

		if (!MathX.Float.NearlyEqual(_currentSpeed, _patrolData.movementSpeed, 0.01f)) {

			if (_currentSpeed < _patrolData.movementSpeed)
				_currentSpeed += _patrolData.acceleration * deltaTime;
			else
				_currentSpeed -= _patrolData.deceleration * deltaTime;
		}

		if (_currentSpeed > _patrolData.movementSpeed / 2)
			RotateTowards(waypoints[_patrolIndex].position, _patrolData.rotationSpeed);
	}


	protected override void MoveToAttack() {
		base.MoveToAttack();
		DetectPlayer();

		if (playerDistance < _attackData.minimumAttackDistance) {
			SwitchState(EnemyState.Attack);
			return;
		}

		if (playerDistance > playerSearchRange && !_isAlerted)
			SwitchState(EnemyState.Patrol);

		if (_currentSpeed < _attackData.movementSpeed)
			_currentSpeed += _attackData.acceleration * deltaTime;

		if (_currentSpeed > _attackData.movementSpeed / 4)
			RotateTowards(player.transform.position, _attackData.rotationSpeed);
	}

	private void RotateTowards(Vector3 TargetPos, float Speed) {
		Vector3 direction = TargetPos - transform.position;
		direction.y = 0f;

		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Speed * deltaTime);
	}

	private IEnumerator LookAt(Quaternion TargetRotation, float MaximumTime, float RotationSpeed) {
		while (MaximumTime > 0) {
			float deltaTime = Time.deltaTime;
			MaximumTime -= deltaTime;

			transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, RotationSpeed * deltaTime);

			yield return new WaitForEndOfFrame();
		}

		transform.rotation = TargetRotation;
		_lookAtRoutine = null;
	}

	protected override void Attack() {
		base.Attack();

		_currentSpeed = playerDistance <= _attackData.maximumAttackDistance ? 0f : 0.5f;

		if (!_attacking && DetectPlayer()) {
			_attacking = true;
			animator.SetTrigger(AP_AttackMelee);
			_isAlerted = false;

			Vector3 lookDirection = player.transform.position - transform.position;
			lookDirection.y = 0f;

			if (_lookAtRoutine != null)
				StopCoroutine(_lookAtRoutine);

			_lookAtRoutine = StartCoroutine(LookAt(Quaternion.LookRotation(lookDirection), 0.2f, 5f));
		}
		else if (!DetectPlayer())
			SwitchState(EnemyState.Patrol);
		else {
			_attacking = false;

			if (playerDistance > _attackData.minimumAttackDistance) {
				if (DetectPlayer())
					SwitchState(EnemyState.MoveToAttack);
				else
					SwitchState(EnemyState.Patrol);
			}
		}
	}

	protected override void Tired() {
		base.Tired();

		if (_currentStateTimer >= _attackData.timeBetweenAttacks) {
			_currentStateTimer = 0f;
			SwitchState(EnemyState.MoveToAttack);
		}

		_currentSpeed = -0.5f;
		RotateTowards(player.transform.position, 2f);
		_currentStateTimer += deltaTime;
	}

	protected override void Animate() {
		base.Animate();

		animator.SetFloat(AP_MoveDirection, _currentSpeed);
		animator.SetFloat(AP_PlayerDistance, Vector3.Distance(transform.position, player.transform.position));
		animator.SetBool(AP_IsDead, IsDead);
	}

	private void LeapTowards(Transform Target, float LeapDuration) {
		if (_leapRoutine != null)
			StopCoroutine(_leapRoutine);

		_leapRoutine = StartCoroutine(Internal_Leap(Target, LeapDuration));
	}
	private IEnumerator IdleTimer(float TimeInSeconds) {
		while (TimeInSeconds >= 0) {
			TimeInSeconds -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		SwitchState(EnemyState.Patrol);
		_idleRoutine = null;
	}

	private IEnumerator Internal_Leap(Transform Target, float LeapDuration) {
		while (LeapDuration > 0) {
			float deltaTime = Time.deltaTime;

			if (playerDistance < 1.5f)
				yield break;

			if (playerDistance <= _attackData.minimumAttackDistance) {
				Vector3 lookDirection = Target.position - transform.position;
				lookDirection.y = 0f;

				transform.position = Vector3.Slerp(transform.position, Target.position, 2.5f * deltaTime);
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 2f * deltaTime);
			}

			LeapDuration -= Time.deltaTime;

			yield return new WaitForEndOfFrame();
		}

		_leapRoutine = null;
	}

	public override void TakeDamage(int pDamage) {
		base.TakeDamage(pDamage);

		if (!isDead) {
			_isAlerted = true;
			animator.SetTrigger(AP_Impact);
			SwitchState(EnemyState.Tired);
		}
	}

	public override void Die() {
		base.Die();

		animator.SetTrigger(AP_Death);
	}
}