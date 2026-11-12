using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 25;

    private bool hasHit;

    public void Launch(Vector2 direction, Collider2D[] ownerColliders)
    {
        Collider2D bulletCollider = GetComponent<Collider2D>();

        foreach (Collider2D ownerCollider in ownerColliders)
        {
            Physics2D.IgnoreCollision(bulletCollider, ownerCollider);
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

        Health health = collision.collider.GetComponentInParent<Health>();

        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }
}