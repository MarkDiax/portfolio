using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour
{
    [SerializeField]
    private RopeBehaviour _rope;

    [SerializeField]
    private int _damage;
    [SerializeField]
    private float _forceOnImpact;

    private void OnCollisionEnter(Collision collision) {
        BaseEnemy enemy = collision.collider.GetComponent<BaseEnemy>();
        if (enemy != null && !enemy.IsDead) {
            enemy.TakeDamage(_damage);
			GameManager.Instance.TriggerSlowmotion();
            Destroy(gameObject);
        }

        InteractableLock interactableLock = collision.collider.GetComponent<InteractableLock>();
        if (interactableLock != null)
            interactableLock.Unlock();

        Interactable interactable = collision.collider.GetComponent<Interactable>();
        if (interactable != null) {
            //type checking
            if (interactable.GetType() == typeof(Lever)) {
                PullLever(interactable);
            }
            else if (interactable.GetType() == typeof(RopeConnector)) { // destroy on rope hit
                Destroy(GetComponent<Rigidbody>());
                Destroy(GetComponent<Collider>());
                interactable.Interact(gameObject);

            }
            else
                interactable.Interact(gameObject);
        }
    }

    private void PullLever(Interactable Object) {
        Lever lever = Object as Lever;

        if (!lever.IsInOriginalPosition)
            lever.Interact(gameObject);
    }
}