using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeBehaviour : MonoBehaviour
{
    [SerializeField] RopePart _ropePartPrefab;
    [SerializeField] GameObject _ropeHanger;

    [SerializeField] float _segments;
    [SerializeField] float _distanceBetweenSegments;
    [SerializeField] bool _showColliders;

    [SerializeField] Transform LastChild;

    LineRenderer _line;
    Coroutine _lineRoutine;

    [HideInInspector]
    public List<RopePart> ropeSegments;

    private void Start() {
        ropeSegments = new List<RopePart>();

        if (!CheckForChildren()) {
            CreateRope();
            CheckForChildren();
        }

        SetupJoints();
        SetupRendering();
    }

    private bool CheckForChildren() {
        if (transform.childCount > 1) {
            for (int i = 0; i < transform.childCount; i++) {
                RopePart node = transform.GetChild(i).GetComponent<RopePart>();

                if (node != null) {
                    ropeSegments.Add(node);
                }
            }

            return true;
        }

        return false;
    }

    public void CreateRope() {
        for (int i = 0; i < _segments; i++) {
            RopePart node = Instantiate(_ropePartPrefab.gameObject, transform).GetComponent<RopePart>();

            node.name = _ropePartPrefab.name + i;
            node.tag = _ropeHanger.tag;
            node.gameObject.layer = _ropeHanger.layer;

            node.transform.position = transform.GetChild(i).position + (Vector3.down * _distanceBetweenSegments);
            node.defaultPos = node.transform.position;
        }
    }

    private void SetupJoints() {
        for (int i = 0; i < ropeSegments.Count; i++) {
            ropeSegments[i].GetComponent<Renderer>().enabled = _showColliders;
            ropeSegments[i].Rope = this;

            GameObject connectedObject = i > 0 ? ropeSegments[i - 1].gameObject : _ropeHanger;
            ropeSegments[i].CharacterJoint.connectedBody = connectedObject.GetComponent<Rigidbody>();
        }
        LastChild = ropeSegments[ropeSegments.Count - 1].transform;

    }

    private void SetupRendering() {
        _line = GetComponent<LineRenderer>();

        _line.positionCount = ropeSegments.Count;
        _lineRoutine = StartCoroutine(UpdateLine());
    }

    private IEnumerator UpdateLine() {
        while (true) {
            for (int i = 0; i < _line.positionCount; i++) {
                if (ropeSegments[i] == null)
                    yield break;

                _line.SetPosition(i, ropeSegments[i].transform.position);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    public void Update() {
        if (LastChild == null)
            return;

        float dist = Vector3.Distance(LastChild.position, transform.position);
    }

    public void Respawn() {
        StopCoroutine(_lineRoutine);

        if (EventManager.RopeEvent.OnRopeBreak != null)
            EventManager.RopeEvent.OnRopeBreak.Invoke(this);

        StartCoroutine(RespawnRope());
    }

    private IEnumerator RespawnRope() {
        for (int i = 0; i < ropeSegments.Count; i++) {
            Destroy(ropeSegments[i].gameObject);
        }
        ropeSegments.Clear();


        yield return new WaitUntil(() => transform.childCount == 1);
        print("Respawning Rope: " + name);

        CreateRope();
        CheckForChildren();
        SetupJoints();
        SetupRendering();
    }


    public RopePart GetClosestNode(Transform FromTransform) {
        float range = float.MaxValue;
        RopePart closestNode = null;

        for (int i = 0; i < ropeSegments.Count; i++) {
            float distance = Vector3.Distance(ropeSegments[i].transform.position, FromTransform.position);

            if (distance < range) {
                range = distance;
                closestNode = ropeSegments[i];
            }
        }

        return closestNode;
    }
}