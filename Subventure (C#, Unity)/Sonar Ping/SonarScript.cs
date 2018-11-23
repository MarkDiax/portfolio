using System;
using UnityEngine;
using UnityEngine.Assertions;

public class SonarScript : AbstractSonar
{
    [Tooltip("The amount of seconds before the sonar gets activated.")]
    [SerializeField] private float _appearTimer = 15;
    private float _backupAppearTime;
    [SerializeField]
    private MeshRenderer _meshRenderer;
    private bool _OnImageTarget = false;

    override protected void Start() {
        _meshRenderer = GetComponent<MeshRenderer>();
        _backupAppearTime = _appearTimer;

        //make sure the sphere isn't visible at start
        EndSonar();

        base.Start();
    }

    public override void StartSonar() {
        base.StartSonar();
        gameObject.transform.localScale = new Vector3(_startSize, _startSize, _startSize);
        _meshRenderer.enabled = true;
        _appearTimer = _backupAppearTime;

    }

    public override void EndSonar() {
        base.EndSonar();
        gameObject.transform.localScale = new Vector3(_startSize, _startSize, _startSize);
        _meshRenderer.enabled = false;
        _appearTimer = _backupAppearTime;   //restart the countdown till next ping
    }


    protected override void Update() {

        if (_OnImageTarget == false) return;    //no nothing if the sub / sonar is not on any image target.

        if(!_isActive) {
            _appearTimer -= Time.deltaTime;
            if (_appearTimer <= 0f)
            {
                StartSonar();
                return;
            }
            return;
        }

        if(_expandTimer <= 0f) {
                EndSonar();
                return;
        }

        _expandTimer -= Time.deltaTime;

        float mod = _expandValue * Time.deltaTime;
        Vector3 vMod = new Vector3(mod, mod, mod);
        transform.localScale += vMod;

        base.Update();
    }

    private void OnTriggerEnter(Collider other) {
        if(!_isActive) {  //to prevent triggering sonar waves when they shouldn't
            return;
        }

        SonarPingBack ping = other.gameObject.GetComponent<SonarPingBack>();
        if(ping != null) {
            ping.StartSonar();
        }
    }

    public void ActivateSonar(bool pActivate = true)
    {
        _OnImageTarget = pActivate;
        if (pActivate == false)
        {
            if (this != null)
            EndSonar();
        }
    }
}