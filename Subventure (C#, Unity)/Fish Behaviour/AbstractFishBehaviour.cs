using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbstractFishBehaviour : MonoBehaviour
{
	protected float movementSpeed, rotationSpeed, groupingDistance, evadeDistance;
	protected Vector3 newGoal;
	private int groupingChannel;

	protected FishTracker fishTracker;
	protected List<AbstractFishBehaviour> fishObjects;
	protected Renderer[] meshRenderers;

	protected bool isTurningBack = false;
	protected Vector3 direction = Vector3.zero;

	private bool _fadingIn = false, _trackingCurrentFish = false, _hasFadedOut = false, _initiated = false;

	private float _fadeTimer = 1f;
	private int _spawnRadius;

	public void SetInitialValues(float pMovementSpeed, float pRotationSpeed, float pGroupingDistance, float pEvadeDistance, int pGroupingChannel, int pSpawnRadius) {
		movementSpeed = pMovementSpeed;
		rotationSpeed = pRotationSpeed;
		groupingDistance = pGroupingDistance;
		evadeDistance = pEvadeDistance;
		groupingChannel = pGroupingChannel;
		_spawnRadius = pSpawnRadius;
		_initiated = true;
	}

	public virtual void Start() {
		meshRenderers = GetComponentsInChildren<Renderer>();
		fishTracker = GetComponentInParent<FishTracker>();
		//get all fish in the scene so we can track if they're nearby or not (in extended classes)
		fishObjects = fishTracker.AllActiveFish;
	}

	public virtual void Update() {
		if (!_initiated) return;

		UpdateFishFade();

		transform.Translate(0, 0, movementSpeed * Time.deltaTime);
	}


	public void StartFade() {
		_fadingIn = false;
		_trackingCurrentFish = true;
	}

	/// <summary>
	/// Fades the fish out when the fish is out of bounds.
	/// Fades it back in when it has been transported back.
	/// </summary>
	private void UpdateFishFade() {

		if (_trackingCurrentFish) {
			switch (_fadingIn) {

				case true:
				if (_fadeTimer >= 1) {
					_fadeTimer = 1f;
					_fadingIn = false;
					_hasFadedOut = false;
					_trackingCurrentFish = false;
					break;
				}
				_fadeTimer += Time.deltaTime / 5;
				break;
				case false:
				if (_fadeTimer <= 0f) {
					_fadeTimer = 0f;
					_fadingIn = true;
					_hasFadedOut = true;
					break;
				}
				_fadeTimer -= Time.deltaTime / 5;
				break;
			}
			for (int i = 0; i < meshRenderers.Length; i++) {
				//Go through all renderers and fade the alpha in/out.
				Renderer renderer = meshRenderers[i];
				for (int j = 0; j < renderer.materials.Length; j++) {
					Color color = renderer.materials[j].color;
					color.a = _fadeTimer;
					renderer.materials[j].color = color;
				}
			}
		}
	}

	public virtual void OnTriggerEnter(Collider other) {

		if (!isTurningBack)
			//grab a place behind the fish to turn towards.
			newGoal = transform.position - other.transform.position;

		isTurningBack = true;
	}

	public virtual void OnTriggerExit(Collider other) {
		isTurningBack = false;
	}

	public bool HasFadedOut {
		get { return _hasFadedOut; }
	}

	public int SpawnRadius {
		get { return _spawnRadius; }
		set { _spawnRadius = value; }
	}
	public int GroupingChannel {
		get { return groupingChannel; }
	}
}
