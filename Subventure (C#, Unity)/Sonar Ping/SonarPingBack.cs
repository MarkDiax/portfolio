using UnityEngine;

public class SonarPingBack : AbstractSonar
{
    private MeshRenderer _meshRenderer;
    private static SonarScript _mainSonarReference;

    [Tooltip("Optional objects for highlighting the object that's been sonarerered")]
    [SerializeField] private GameObject _highlightObject;
    private Light _pointLight; //the pointlight under _highlightObject;
    private float _highlightTimer = 0f;
    private int _highlightCount = 0; // counts how many time the object has been highlighted in one go.
    private bool _highlighting = false, _switcher = false;

    override protected void Start() {
        base.Start();

        _meshRenderer = GetComponent<MeshRenderer>();
        _mainSonarReference = FindObjectOfType<SonarScript>();
        _pointLight = _highlightObject.GetComponentInChildren<Light>();
        _highlightObject.SetActive(false);
        //make sure the sphere isn't visible at start
        EndSonar();
    }

    public override void StartSonar() {
        base.StartSonar();
        transform.localScale = new Vector3(_startSize, _startSize, _startSize);
        _meshRenderer.enabled = true;
        _highlightObject.SetActive(true);
        _highlighting = true;
        _highlightCount = 0;
    }

    public override void EndSonar() {
        base.EndSonar();
        transform.localScale = new Vector3(_startSize, _startSize, _startSize);     
    }

    protected override void Update() {

        UpdateHighlight();      //this updates a light that increases and decreases in intensity over time.

        if (!_isActive) {
            _meshRenderer.enabled = false;
            return;
        }

        if (_expandTimer <= 0f) {
            EndSonar();
            return;
        }
        _expandTimer -= Time.deltaTime;

        float mod = _expandValue * Time.deltaTime;
        Vector3 vMod = new Vector3(mod, mod, mod);
        transform.localScale += vMod;

        base.Update();
    }

    private void UpdateHighlight()
    {
        if (!_highlighting) return;

        float mod = 0;
        switch (_switcher)
        {
            case true:
                mod -= Time.deltaTime * 6;
                if (_pointLight.intensity <= 2)
                {
                    _switcher = false;
                    _highlightCount++;
                }
                if (_highlightCount >= 2)
                {
                    _highlightCount = 0;
                    _highlighting = false;
                    _highlightObject.SetActive(false);
                }
                break;
            case false:
                mod += Time.deltaTime * 6;
                if (_pointLight.intensity >= 25)
                {
                    _switcher = true;
                }
                break;
        }
        _pointLight.intensity += mod;
    }
}