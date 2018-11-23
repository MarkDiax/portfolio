using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// wrapper class for the data that a fish group holds
/// </summary>
[System.Serializable]
public class FishData
{
	public enum BehaviourType
	{
		DefaultFlockingBehaviour, OnSeafloorBehaviour
	}

	public GameObject fishPrefab;                   //the model and animations
	public BehaviourType fishBehaviour;             //the behaviour the fish has
	public int groupingChannel = 0;                 //the group in which the fish swim

	public int fishAmount = 10, spawnRadius = 5;
	[Range(0, 10)]
	public float movementSpeed;
	public float rotationSpeed, groupingDistance, evadeDistance;
}

/// <summary>
/// Class that instantiate & keeps track of all fish in the scene
/// </summary>
public class FishTracker : MonoBehaviour
{
	[SerializeField]
	private Transform _seaBed;

	[Header("Fish Properties:")]
	[SerializeField]
	private FishData[] _fishData;
	private List<AbstractFishBehaviour> _allFish = new List<AbstractFishBehaviour>();   //all fish under this object

	private Transform[] _wayPoints;
	private Vector3 _goalPosition;

	private Dictionary<int, Vector3> _currentWaypoints = new Dictionary<int, Vector3>(); //all waypoints paired with their group channels

	private Collider _fishBounds;                           //Boundary for the fish

	private bool _isInstantiated = false, _spawnedFish = false;

	void Awake() {
		Debug.Assert(_seaBed != null, "FishTracker: Mising value on _seaBed!\n" +
			"Check your Inspector settings.");

		_fishBounds = GetComponent<Collider>();

		//waypoints are all child transforms of this object
		_wayPoints = transform.GetComponentsInChildren<Transform>().Where(val => val != transform).ToArray();

		_isInstantiated = true;
	}

	void Update() {
		TrackFish();

		if (Random.Range(0, 2000) < _wayPoints.Length) {

			//update the waypoints for all fish channels
			UpdateWaypointPositions();
		}
	}
	/// <summary>
	/// Assigns a waypoint set in the scene to each grouping channel.
	/// </summary>
	private void UpdateWaypointPositions() {
		_currentWaypoints.Clear();
		for (int i = 0; i < _fishData.Length; i++) {
			if (!_currentWaypoints.ContainsKey(_fishData[i].groupingChannel)) {
				_currentWaypoints.Add(_fishData[i].groupingChannel, GetRandomWaypoint());
			}
		}
	}

	private Vector3 GetRandomWaypoint() {
		return _wayPoints[Random.Range(0, _wayPoints.Length)].position;
	}

	public Vector3 GetWaypointFromChannel(int pChannel) {
		Vector3 waypoint;
		_currentWaypoints.TryGetValue(pChannel, out waypoint);
		return waypoint;
	}

	/// <summary>
	/// Track if the fish are still within the boundary
	/// If not, fade them out slowly and teleport them back
	/// </summary>
	private void TrackFish() {
		for (int i = 0; i < _allFish.Count; i++) {
			if (!_fishBounds.bounds.Contains(_allFish[i].transform.position)) {
				if (_allFish[i].HasFadedOut) {
					Vector3 spawn = RandomVector(_allFish[i].SpawnRadius);
					_allFish[i].transform.position = _seaBed.position + spawn;
					continue;
				}
				_allFish[i].StartFade();
			}
		}
	}
	/// <summary>
	/// Instantiates all fish defined in the inspector.
	/// </summary>
	/// <param name="pCollection"></param>
	private void InstantiateFish(FishData[] pCollection) {
		for (int i = 0; i < pCollection.Length; i++) {
			FishData fish = pCollection[i];

			//look which type these fish belong to
			switch (fish.fishBehaviour) {
				case FishData.BehaviourType.DefaultFlockingBehaviour:
				//then instantiate the given amount
				for (int j = 0; j < fish.fishAmount; j++) {
					GameObject obj;
					Vector3 rnd = RandomVector(fish.spawnRadius);
					Vector3 position = _seaBed.position + rnd + new Vector3(0, 5f, 0);

					//instantiate the object with the given prefab
					obj = Instantiate(fish.fishPrefab, position, Quaternion.identity);
					//add the specified behaviour and needed values
					obj.AddComponent<DefaultFishBehaviour>().SetInitialValues(fish.movementSpeed, fish.rotationSpeed, fish.groupingDistance, fish.evadeDistance, fish.groupingChannel, fish.spawnRadius);
					obj.transform.parent = transform;
					//and start tracking the fish
					_allFish.Add(obj.GetComponent<AbstractFishBehaviour>());
				}
				break;
				case FishData.BehaviourType.OnSeafloorBehaviour:
				//then instantiate the given amount
				for (int j = 0; j < fish.fishAmount; j++) {
					GameObject obj;
					Vector3 rnd = RandomVector(fish.spawnRadius);
					//these fish are spawned on the seafloor itself
					Vector3 offset = new Vector3(rnd.x, 0.02f, rnd.y);
					Vector3 position = _seaBed.position + offset;

					//instantiate the object with the given prefab
					obj = Instantiate(fish.fishPrefab, position, Quaternion.identity);
					//add the specified behaviour and needed values
					obj.AddComponent<FloorFishBehaviour>().SetInitialValues(fish.movementSpeed, fish.rotationSpeed, fish.groupingDistance, fish.evadeDistance, fish.groupingChannel, fish.spawnRadius);
					obj.transform.parent = transform;
					//and start tracking the fish
					_allFish.Add(obj.GetComponent<AbstractFishBehaviour>());
				}
				break;
			}
		}
		//after all fish have spawned in, update the waypoints according to their group channels.
		UpdateWaypointPositions();
	}

	private Vector3 RandomVector(int pSize) {
		//returns a random vector based on the size of the given radius
		return new Vector3(Random.Range(-pSize, pSize),
						   Random.Range(-pSize, pSize),
						   Random.Range(-pSize, pSize));
	}

	public List<AbstractFishBehaviour> AllActiveFish {
		//returns all active fish under this object
		get { return _allFish; }
	}

	public Vector3 GetCentre {
		//returns the centre of the fish group
		get { return transform.position; }
	}

	public void EnableFishTracker() {
		if (!_spawnedFish && _isInstantiated) {
			InstantiateFish(_fishData);
			_spawnedFish = true;
		}
	}

	public void DisableFishTracker() {
		for (int i = 0; i < _allFish.Count; i++) {
			Destroy(_allFish[i].gameObject);
		}
		_allFish.Clear();
		_spawnedFish = false;
	}
}