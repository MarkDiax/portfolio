using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using Cinemachine;

public class PlayerController : BaseController
{
	#region Movement Control Fields
	[Header("Default Movement Settings")]
	[SerializeField]
	float _runSpeed = 6;
	float _targetSpeed;

	[SerializeField] float _turnSmoothTime = 0.2f;
	float _turnSmoothVelocity;

	[SerializeField] float _tiltSmoothTime = 0.1f;
	float _tiltSmoothVelocity;

	[SerializeField] float _speedSmoothTime = 0.1f;
	float _speedSmoothVelocity;
	float _currentSpeed;

	[Space]
	[SerializeField]
	Transform[] _groundCastPoints;
	[SerializeField] float _jumpHeight;
	[SerializeField] float _gravityMod;
	[SerializeField] [Range(0, 1)] float _airControl;
	float _jumpForce;
	bool _isGrounded, _isJumping, _isRunning;

	Vector2 _inputDir;
	Vector3 _moveDir, _jumpDir;
	float _moveDelay;
	#endregion

	#region Combat Fields
	[SerializeField] [Header("Combat")] float _health;

	[SerializeField] [Header("Combat Movement Settings")] float _combatMoveSpeed;
	[SerializeField] float _meleeMoveSpeed;
	[SerializeField] float _walkZoomSpeed, _walkDrawSpeed;
	[SerializeField] float _dodgeStrength;
	bool _isDodgeing, _isAttacking;

	[SerializeField] [Header("Combat Weapons")] Weapon _swordObject;
	[SerializeField] Weapon _bowObject;
	[SerializeField] Transform _bowTransformParent, _swordTransformParent;
	[SerializeField] Transform _bowTransformSheathed, _swordTransformSheathed;
	bool _hasSwordEquipped, _hasBowEquipped;

	[SerializeField] [Header("Other Arrow Properties")] Arrow _arrowPrefab;
	[SerializeField] Transform _arrowSpawnPoint;
	[SerializeField] float _arrowForceAddOnDraw, _arrowForceMax, _arrowDistanceMax;
	float _currentArrowForce;

	bool _drawingBow;
	bool _inCombat;
	Vector2 _animSpeed;
	Quaternion _dodgeRotation = Quaternion.identity;
	BaseEnemy _targetedEnemy;
	int _randomParam;

	#endregion

	#region Animation Events
	// Animator events don't support boolean parameters, so i'm using ints. 1 = true, 0 = false. 
	void A_OnEquipSword(int Equipped) {
		_hasSwordEquipped = (Equipped == 1);

		if (_hasSwordEquipped) {
			_swordObject.transform.parent = _swordTransformParent;
			_swordObject.transform.localPosition = Vector3.zero;
			_swordObject.transform.localRotation = Quaternion.identity;
		}
		else
			_swordObject.transform.parent = _swordTransformSheathed;

		player.Animator.SetBool("HasSwordEquipped", _hasSwordEquipped);

		if (EventManager.PlayerEvent.OnEquipSword != null)
			EventManager.PlayerEvent.OnEquipSword.Invoke(_hasSwordEquipped);
	}

	// Animator events don't support boolean parameters, so i'm using ints. 1 = true, 0 = false. 
	void A_OnEquipBow(int Equipped) {
		_hasBowEquipped = (Equipped == 1);

		if (_hasBowEquipped) {
			_bowObject.transform.parent = _bowTransformParent;
			_bowObject.transform.localPosition = Vector3.zero;
		}
		else
			_bowObject.transform.parent = _bowTransformSheathed;

		_bowObject.transform.localRotation = Quaternion.identity;
		player.Animator.SetBool("HasBowEquipped", _hasBowEquipped);

		if (EventManager.PlayerEvent.OnEquipBow != null)
			EventManager.PlayerEvent.OnEquipBow.Invoke(_hasBowEquipped);
	}

	// The moment the animation looks like it is about to hit the enemy.
	void A_OnMeleeImpact() {
		_swordObject.Attack();
	}

	// Moment when the melee animation ends and the player exits combat.
	void A_OnMeleeEnd() {
		if (_targetedEnemy == null) {
			_inCombat = false;
		}
		_isAttacking = false;
		_swordObject.EndAttack();
	}

	//Moment when the player starts moving during the dodge animations
	void A_OnDodgeStart() {
		_isDodgeing = true;
		player.Animator.SetRootMotion(false, false);
	}

	void A_OnDodgeEnd() {
		_isDodgeing = false;
		player.Animator.SetRootMotion(false, false);
	}

	// The moment the player starts jumping in the animation.
	void A_OnJump() {
		_isJumping = true;
		_isGrounded = false;
		_jumpForce = Mathf.Sqrt(-2 * gravity * _jumpHeight);
	}

	void A_OnLanding() {
		_isJumping = false;
		_isGrounded = true;
	}

	// Used for suspending movement at Idle Jump or when landing from fall loop.
	void A_OnSuspendMovement(float Delay) {
		_moveDelay = Delay;
	}
	#endregion

	#region Animation Properties
	const string AP_RND = "RND";
	const string AP_Jump = "Jump";
	const string AP_MoveSpeed = "Speed";
	const string AP_IsGrounded = "Grounded";
	const string AP_GroundDistance = "GroundDistance";
	//Combat:
	const string AP_Dodge = "Dodge";
	const string AP_InCombat = "InCombat";
	const string AP_MoveCombat_X = "Move_Combat_X";
	const string AP_MoveCombat_Y = "Move_Combat_Y";
	const string AP_EquipBow = "EquipBow";
	const string AP_EquipSword = "EquipSword";
	const string AP_FireArrow = "FireArrow";
	const string AP_MeleeLight = "MeleeLight";
	const string AP_Impact = "Impact";
	const string AP_Die = "Die";
	//
	#endregion

	protected override void Awake() {
		base.Awake();

		usePhysics = true;
		gravity = Physics.gravity.y * _gravityMod;

		StartCoroutine(RandomGenerator(2));
	}

	public override void Resume() {
		AddListeners();
		player.Animator.SetRootMotion(false, false);
		A_OnEquipSword(0);
		A_OnEquipBow(0);
	}

	public override void Suspend() {
		RemoveListeners();
		player.Animator.SetRootMotion(false, false);
		A_OnEquipSword(0);
		A_OnEquipBow(0);
	}

	private void AddListeners() {
		EventManager.InputEvent.OnBowDraw.AddListener(OnBowDraw);
		EventManager.InputEvent.OnCameraZoom.AddListener(OnCameraZoom);
	}
	private void RemoveListeners() {
		EventManager.InputEvent.OnBowDraw.RemoveListener(OnBowDraw);
		EventManager.InputEvent.OnCameraZoom.RemoveListener(OnCameraZoom);
	}

	protected override void UpdateInput() {
		if (_isGrounded) {
            if (Input.GetMouseButton(1))
            {
                Debug.Log("yaaa");
            }
			if (InputManager.GetKey(InputKey.Aim) && _hasBowEquipped) {
				if (EventManager.InputEvent.OnCameraZoom != null)
					EventManager.InputEvent.OnCameraZoom.Invoke(true);

				if (InputManager.GetKey(InputKey.Shoot)) {
					if (EventManager.InputEvent.OnBowDraw != null)
						EventManager.InputEvent.OnBowDraw.Invoke(true);
				}
				if (InputManager.GetKeyUp(InputKey.Shoot)) {
					FireArrow();

					if (EventManager.InputEvent.OnBowDraw != null)
						EventManager.InputEvent.OnBowDraw.Invoke(false);
				}
			}
			else if (InputManager.GetKeyDown(InputKey.Melee) && _hasSwordEquipped)
				MeleeAttack();

			if (InputManager.GetKeyUp(InputKey.Aim)) {
				if (EventManager.InputEvent.OnCameraZoom != null)
					EventManager.InputEvent.OnCameraZoom.Invoke(false);

				if (EventManager.InputEvent.OnBowDraw != null)
					EventManager.InputEvent.OnBowDraw.Invoke(false);
			}

			if (InputManager.GetKeyDown(InputKey.Target))
				TargetEnemy();

			if (InputManager.GetKeyDown(InputKey.Interact1)) {
				Collider[] objects = Physics.OverlapSphere(player.transform.position, 2f, 1 << (int)Layers.Interactable);

				for (int i = 0; i < objects.Length; i++) {
					Interactable interactable = objects[i].GetComponent<Interactable>();
					if (interactable != null) {
						interactable.Interact(gameObject);
						break;
					}
				}
			}

			if (InputManager.GetKeyDown(InputKey.Interact2))
				InteractWithRope();

			_isRunning = InputManager.GetKey(InputKey.Run);
		}

		Vector2 keyboardInput = new Vector2(InputManager.GetAxis(InputKey.MoveHorizontal), InputManager.GetAxis(InputKey.MoveVertical));
		_inputDir = keyboardInput.normalized;

		//FOR TESTING ONLY
		if (Input.GetKeyDown(KeyCode.Alpha2))
			player.Animator.SetTrigger(AP_EquipSword);
		if (Input.GetKeyDown(KeyCode.Alpha1))
			player.Animator.SetTrigger(AP_EquipBow);
		if (Input.GetKeyDown(KeyCode.L))
			TakeDamage(100);

		//


	}

	private void InteractWithRope() {
		Collider[] ropeParts = Physics.OverlapSphere(transform.position, 2.5f, 1 << (int)Layers.Rope);
		print(ropeParts.Length);

		if (ropeParts.Length > 0) {
			Transform closestCollider = GetClosestCollider(ropeParts, 5f);
			RopePart closestNode = closestCollider.GetComponent<RopePart>();

			if (closestNode == null) {
				Debug.LogWarning("RopePart: " + closestCollider + " does not have behaviour 'RopePart'!");
				return;
			}

			EventManager.PlayerEvent.OnGrabRope.Invoke(closestNode);
		}
	}

	public Transform GetClosestCollider(Collider[] Colliders, float MaxRange) {
		Transform closestTransform = null;
		float range = MaxRange;

		for (int i = 0; i < Colliders.Length; i++) {

			if (Colliders[i].CompareTag("Player"))
				continue;
			print(Colliders[i].name);
			float distance = Vector3.Distance(Colliders[i].transform.position, transform.position);

			if (distance < range) {
				range = distance;
				closestTransform = Colliders[i].transform;
			}
		}

		return closestTransform;
	}

	private void TargetEnemy() {
		if (_targetedEnemy != null) {
			EventManager.AIEvent.OnEnemyDeath.RemoveListener(OnTargetDeath);
			_targetedEnemy = null;
			_inCombat = false;
		}
		else {
			_targetedEnemy = GetNextTarget();

			if (_targetedEnemy != null) {
				_inCombat = true;
				EventManager.AIEvent.OnEnemyDeath.AddListener(OnTargetDeath);
			}
		}
	}

	private void OnTargetDeath(GameObject Target) {
		if (_targetedEnemy != null && Target == _targetedEnemy.gameObject)
			_targetedEnemy = null;
	}

	private BaseEnemy GetNextTarget() {
		List<BaseEnemy> visibleEnemies = new List<BaseEnemy>();

		for (int i = 0; i < AIManager.Instance.Enemies.Count; i++) {
			List<BaseEnemy> enemies = AIManager.Instance.Enemies;

			Vector3 screenPoint = mainCamera.WorldToViewportPoint(enemies[i].transform.position);
			if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
				visibleEnemies.Add(enemies[i]);
		}

		BaseEnemy closestEnemy = null;
		float closestDistance = Mathf.Infinity;

		for (int i = 0; i < visibleEnemies.Count; i++) {
			if (visibleEnemies[i].IsDead)
				continue;

			float newDistance = Vector3.Distance(visibleEnemies[i].transform.position, player.transform.position);

			if (newDistance < closestDistance) {
				closestEnemy = visibleEnemies[i];
				closestDistance = newDistance;
			}
		}

		return closestEnemy;
	}

	private void Move_Default() {
		if (InputManager.GetKeyDown(InputKey.Jump) && _isGrounded) {
			player.Animator.SetTrigger(AP_Jump);
		}

		if (_moveDelay > 0f)
			_currentSpeed = 0f;
		else {
			_targetSpeed = _runSpeed * _inputDir.magnitude;
			_currentSpeed = Mathf.SmoothDamp(_currentSpeed, _targetSpeed, ref _speedSmoothVelocity, GetModifiedSmoothTime(_speedSmoothTime));
		}

		if (_isGrounded)
			_moveDir.y = 0f;

		if (_jumpForce > 0f) {
			_moveDir.y = _jumpForce;
			_jumpForce = 0f;
		}

		_moveDir = player.transform.forward * _currentSpeed + Vector3.up * _moveDir.y;
	}

	private void Move_Combat() {
		if (InputManager.GetKeyDown(InputKey.Jump) && _isGrounded)
			player.Animator.SetTrigger(AP_Dodge);

		//if player is very close to the enemy, make sure he can't get any closer
		if (_targetedEnemy != null) {
			float targetDistance = Vector3.Distance(transform.position, _targetedEnemy.transform.position);

			if (targetDistance < 1.5f)
				_inputDir.y = Mathf.Clamp(_inputDir.y, -1, 0);
		}

		if (_moveDelay > 0f)
			_currentSpeed = 0f;
		else {
			float moveSpeed = _isAttacking ? _meleeMoveSpeed : _combatMoveSpeed;
			_targetSpeed = moveSpeed * _inputDir.magnitude;
			_currentSpeed = Mathf.SmoothDamp(_currentSpeed, _targetSpeed, ref _speedSmoothVelocity, _speedSmoothTime);
		}

		_moveDir.y = _isGrounded ? 0f : _moveDir.y;

		if (_isDodgeing) {
			_moveDir = Quaternion.Euler(0, player.transform.eulerAngles.y, 0) * Vector3.forward * _dodgeStrength + (_moveDir.y * Vector3.up);
		}
		else {
			_moveDir = new Vector3(_inputDir.x, 0, _inputDir.y) * _currentSpeed + Vector3.up * _moveDir.y;
			_moveDir = Quaternion.Euler(0, player.transform.eulerAngles.y, 0) * _moveDir;
			_animSpeed = Vector2.Lerp(_animSpeed, _inputDir, 5 * Time.deltaTime);
		}
	}

	public override void Step() {
		if (!_isJumping)
			_isGrounded = Grounded(); //ground check before the main loop for accurate input

		if (!_isGrounded && _jumpDir == Vector3.zero)
			_jumpDir = _moveDir;
		else if (_isGrounded)
			_jumpDir = Vector3.zero;

		base.Step();
	}

	protected override void Move() {
		_moveDir.y += gravity * Time.deltaTime;

		if (!_inCombat)
			Move_Default();
		else
			Move_Combat();


		if (_jumpDir != Vector3.zero) {
			_jumpDir.y = _moveDir.y;
			_moveDir = _jumpDir;
		}

		controller.Move(_moveDir * Time.deltaTime);

		_currentSpeed = new Vector2(_moveDir.x, _moveDir.z).magnitude;
	}


	private void Rotate_Default() {
		float previousY = player.transform.rotation.eulerAngles.y;

		if (_inputDir != Vector2.zero) {
			//direction
			float targetRotation = Mathf.Atan2(_inputDir.x, _inputDir.y) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
			Vector3 moveVector = Vector3.up * Mathf.SmoothDampAngle(player.transform.eulerAngles.y, targetRotation, ref _turnSmoothVelocity, GetModifiedSmoothTime(_turnSmoothTime));

			/**
            //z-tilt
            float zOffset = moveVector.y - previousY;
            Vector3 tiltVector = Vector3.forward * Mathf.SmoothDampAngle(player.transform.eulerAngles.z, -zOffset * 1.2f, ref _tiltSmoothVelocity, GetModifiedSmoothTime(_tiltSmoothTime));
          /**/

			//stop rotating when player is falling or is playing land anim
			if (_moveDelay > 0 || DistanceToGround() > 2f)
				moveVector = player.transform.eulerAngles;

			player.transform.eulerAngles = moveVector; //+ tiltVector;
		}
		else {
			float zAxis = Mathf.LerpAngle(player.transform.eulerAngles.z, 0, Time.deltaTime * 50f);
			player.transform.eulerAngles -= Vector3.forward * zAxis;
		}
	}

	private void Rotate_Combat() {
		Quaternion targetRotation;

		if (_targetedEnemy != null && !_drawingBow) {
			if (_isDodgeing) {
				if (_dodgeRotation == Quaternion.identity) {
					float targetYAngle = Mathf.Atan2(_inputDir.x, _inputDir.y) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
					_dodgeRotation = Quaternion.Euler(Vector3.up * targetYAngle);
					print("PlayerController:Dodge (Rotate)");
				}

				targetRotation = _dodgeRotation;
			}
			else {
				_dodgeRotation = Quaternion.identity;
				Quaternion lookRotation = Quaternion.LookRotation(_targetedEnemy.transform.position - player.transform.position);
				targetRotation = Quaternion.Euler(player.transform.eulerAngles.x, lookRotation.eulerAngles.y, player.transform.eulerAngles.z);
			}
		}
		else
			targetRotation = Quaternion.Euler(player.transform.eulerAngles.x, mainCamera.transform.eulerAngles.y, player.transform.eulerAngles.z);

		float rotateSpeed = 5 * _moveDir.magnitude;
		if (_drawingBow)
			rotateSpeed = 12 + 2 * rotateSpeed;

		player.transform.rotation = Quaternion.Lerp(player.transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
	}

	protected override void Rotate() {
		if (_isGrounded) {
			if (!_inCombat)
				Rotate_Default();
			else
				Rotate_Combat();
		}
	}

	private void MeleeAttack() {
		_inCombat = true;
		_isAttacking = true;
		player.Animator.SetTrigger(AP_MeleeLight);
	}

	private void OnBowDraw(bool Drawing) {
		_drawingBow = Drawing;
		_targetSpeed = _walkDrawSpeed * _inputDir.magnitude;

		if (!_drawingBow) {
			_currentArrowForce = 0;
			return;
		}

		_currentArrowForce = Mathf.Clamp(_currentArrowForce + (_arrowForceAddOnDraw * Time.deltaTime), 0, _arrowForceMax);
	}

	private void FireArrow() {
		player.Animator.SetTrigger(AP_FireArrow);

		GameObject arrow = Instantiate(_arrowPrefab).gameObject;
		arrow.transform.position = _arrowSpawnPoint.position;

		Vector3 reticlePos = UIManager.Instance.Crosshair.GetComponent<Image>().rectTransform.position;
		Vector3 reticleWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(reticlePos.x, reticlePos.y, _arrowDistanceMax));
		Vector3 dir = (reticleWorldPos - _arrowSpawnPoint.position).normalized;

		arrow.GetComponent<Rigidbody>().AddForce(dir * _currentArrowForce, ForceMode.Impulse);
		arrow.transform.rotation = mainCamera.transform.rotation;
		_currentArrowForce = 0;
	}

	private void OnCameraZoom(bool Zooming) {
		if (!Zooming) {
			if (_targetedEnemy == null)
				_inCombat = false;

			UIManager.Instance.EnableCrosshair(false);
		}
		else {
			_inCombat = true;
			_targetSpeed = _walkZoomSpeed * _inputDir.magnitude;
			player.Animator.SetRootMotion(false, false);

			if (player.Animator.Animator.GetCurrentAnimatorStateInfo(0).IsTag("AT_Combat_Move"))
				UIManager.Instance.EnableCrosshair(true);
		}
	}

	protected override void Animate() {
		float animationSpeed = (_currentSpeed / _runSpeed) * _inputDir.magnitude;
		player.Animator.SetFloat(AP_MoveSpeed, animationSpeed, _speedSmoothTime, Time.deltaTime);
		player.Animator.SetFloat(AP_MoveCombat_X, _animSpeed.x);
		player.Animator.SetFloat(AP_MoveCombat_Y, _animSpeed.y);

		player.Animator.SetBool(AP_IsGrounded, _isGrounded);
		player.Animator.SetFloat(AP_GroundDistance, DistanceToGround());
		player.Animator.SetBool(AP_InCombat, _inCombat);
		player.Animator.SetInt(AP_RND, _randomParam);

		if (_moveDelay > 0)
			_moveDelay -= Time.deltaTime;
	}

	private void LateUpdate() {
		if (!_hasSwordEquipped) {
			_swordObject.transform.localPosition = Vector3.Lerp(_swordObject.transform.localPosition, Vector3.zero, 10 * Time.deltaTime);
			_swordObject.transform.localRotation = Quaternion.Lerp(_swordObject.transform.localRotation, Quaternion.identity, 10 * Time.deltaTime);
		}

		if (!_hasBowEquipped)
			_bowObject.transform.localPosition = Vector3.Lerp(_bowObject.transform.localPosition, Vector3.zero, 10 * Time.deltaTime);
	}

	public void Stagger() {
		if (_isDodgeing)
			A_OnDodgeEnd();

		_currentSpeed = 0f;
	}

	public void TakeDamage(float Damage) {
		player.Animator.SetTrigger(AP_Impact);
		Stagger();

		_health -= Damage;
		if (_health <= 0)
			Die();

		if (EventManager.PlayerEvent.OnHealthChanged != null)
			EventManager.PlayerEvent.OnHealthChanged.Invoke(_health);
	}

	public void TakeDamage(float Damage, Vector3 HitDirection, float HitForce = 0.4f, float ForceDuration = 0.4f) {
		TakeDamage(Damage);

		StartCoroutine(Internal_AddForce(HitDirection, HitForce, ForceDuration));
	}

	private IEnumerator Internal_AddForce(Vector3 HitDirection, float Force, float ForceDuration) {
		float timer = ForceDuration;

		while (timer > 0) {
			timer -= Time.deltaTime;
			transform.position += HitDirection * Force * timer;

			yield return new WaitForEndOfFrame();
		}
	}

	private void Die() {
		if (EventManager.PlayerEvent.OnDeath != null)
			EventManager.PlayerEvent.OnDeath.Invoke();

		isDead = true;
		player.Animator.SetTrigger(AP_Die);
		print("PlayerController::Die()");
	}

	private float DistanceToGround() {
		RaycastHit hitInfo;

		if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, Mathf.Infinity))
			return hitInfo.distance;

		return Mathf.Infinity;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit) {
		//add force to colliding rigidbodies
		AddPhysicsForceOnHit(hit.collider.attachedRigidbody, hit);
	}

	private void AddPhysicsForceOnHit(Rigidbody rigidbody, ControllerColliderHit hit) {
		if (!usePhysics || rigidbody == null || rigidbody.isKinematic)
			return;

		if (hit.moveDirection.y < -0.3F)
			return;

		Vector3 pushDir = new Vector3(hit.moveDirection.x, hit.moveDirection.y, hit.moveDirection.z);
		rigidbody.AddForce(pushDir * 50, ForceMode.Force);
	}

	private float GetModifiedSmoothTime(float smoothTime) {
		if (_isGrounded)
			return smoothTime;

		if (_airControl == 0)
			return float.MaxValue;

		return smoothTime / _airControl;
	}

	private bool Grounded() {
		if (controller.isGrounded)
			return true;

		RaycastHit hit;

		for (int i = 0; i < _groundCastPoints.Length; i++) {
			if (Physics.Raycast(_groundCastPoints[i].position, Vector3.down, out hit, 0.1f)) {
				controller.Move(Vector3.down * hit.distance);
				return true;
			}
		}

		return false;
	}

	private IEnumerator RandomGenerator(int Frequency) {
		System.Random rnd = new System.Random();

		while (true) {
			yield return new WaitForSeconds(Frequency);
			_randomParam = rnd.Next(0, 10);
			yield return new WaitForEndOfFrame();
		}
	}

	public float GetHealth {
		get { return _health; }
	}
}