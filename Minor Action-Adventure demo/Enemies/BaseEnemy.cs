using UnityEngine;
using System.Collections;
using UnityEngine.AI;


/*
 * This class was made in case i had enough time to develop a second enemy type.
 * 
 */
public class BaseEnemy : MonoBehaviour
{
	[SerializeField]
	private bool _Debugging, _Static;

	[SerializeField]
	private int _health;

	[Header("Traversal")]
	[SerializeField]
	protected float idleTime;
	[SerializeField]
	protected Transform[] waypoints;

	[Header("Player Detection")]
	[SerializeField]
	[Tooltip("The range in which the enemy is able to detect the player")]
	protected float playerSearchRange = 10;
	[SerializeField]
	[Tooltip("The range in which the enemy instantly detects the player, regardless if the enemy is seeing the player")]
	private float _instantDetectionRange = 4;
	[SerializeField]
	[Tooltip("Angle in which the enemy is able to detect the player")]
	private float _fovCone = 120;

	protected Player player;
	protected Animator animator;
	protected float velocity;
	protected float deltaTime;
	protected float playerDistance;
	protected float moveDelay;
	protected bool isDead;

	private bool _isPlayerDead;

	public enum EnemyState
	{
		Idle, Patrol, MoveToAttack, Attack, Tired, Scared, Hide, Taunt, Jump, Climb, Dead,
	}
	protected EnemyState currentState;

	protected virtual void Awake() {
		AIManager.Instance.Enemies.Add(this);
	}

	protected virtual void OnDestroy() {
		AIManager.Instance.Enemies.Remove(this);
	}
	protected virtual void Start() {
		player = Player.Instance;
		animator = GetComponent<Animator>();

		EventManager.PlayerEvent.OnDeath.AddListener(OnPlayerDeath);
	}

	private void OnPlayerDeath() {
		_isPlayerDead = true;
		animator.Rebind();
	}

	protected virtual void Update() {
		if (_Static) {
			Animate();
			return;
		}

		deltaTime = Time.deltaTime;
		Vector3 oldPos = transform.position;

		if (moveDelay > 0)
			moveDelay -= deltaTime;
		else {
			switch (currentState) {
				case EnemyState.Idle:
				Idle(idleTime);
				break;
				case EnemyState.MoveToAttack:
				MoveToAttack();
				break;
				case EnemyState.Patrol:
				Patrol();
				break;
				case EnemyState.Attack:
				Attack();
				break;
				case EnemyState.Tired:
				Tired();
				break;
				case EnemyState.Dead:
				DeadState();
				break;
				//etc
			}
		}

		velocity = Vector3.Distance(transform.position, oldPos);

		Animate();
	}

	protected virtual void Idle(float TimeInSeconds) { }
	protected virtual void MoveToAttack() { }
	protected virtual void Patrol() { }
	protected virtual void Attack() { }
	protected virtual void Tired() { }
	protected virtual void DeadState() { }
	protected virtual void Animate() { }

	public void SwitchState(EnemyState NewState) {
		currentState = NewState;

		if (_Debugging)
			print(name + ": switching to AI state " + currentState);
	}

	protected virtual bool DetectPlayer() {
		if (_isPlayerDead)
			return false;

		Vector3 dir = player.transform.position - transform.position;
		float angle = Vector3.Angle(transform.forward, dir.normalized);
		playerDistance = Vector3.Distance(transform.position, player.transform.position);

		if (angle <= (_fovCone / 2)) {
			if (Vector3.Distance(transform.position, player.transform.position) <= playerSearchRange) {
				return true;
			}
		}

		if (playerDistance < _instantDetectionRange)
			return true;

		return false;
	}

	public virtual void TakeDamage(int pDamage) {
		_health -= pDamage;

		if (_health <= 0)
			Die();
	}

	public virtual void Die() {
		isDead = true;
		SwitchState(EnemyState.Dead);

		if (EventManager.AIEvent.OnEnemyDeath != null)
			EventManager.AIEvent.OnEnemyDeath.Invoke(gameObject);
	}

	public bool IsDead {
		get { return isDead; }
	}
}