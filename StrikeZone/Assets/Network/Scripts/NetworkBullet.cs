using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class NetworkBullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 25;

    private bool hasHit;
    private bool serverProjectile;

    public void Launch(Vector2 direction, Collider2D[] ownerColliders)
    {
        Launch(direction, ownerColliders, false);
    }

    public void Launch(
        Vector2 direction,
        Collider2D[] ownerColliders,
        bool isServerProjectile)
    {
        serverProjectile = isServerProjectile;

        Collider2D bulletCollider = GetComponent<Collider2D>();

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics2D.IgnoreCollision(
                    bulletCollider,
                    ownerCollider
                );
            }
        }

        GetComponent<Rigidbody2D>().linearVelocity =
            direction.normalized * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit)
            return;

        hasHit = true;

        if (serverProjectile &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsServer)
        {
            NetworkHealth health =
                collision.collider.GetComponentInParent<NetworkHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}