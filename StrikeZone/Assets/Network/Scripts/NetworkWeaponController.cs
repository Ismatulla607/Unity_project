using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class NetworkWeaponController : NetworkBehaviour
{
    [Header("Shooting")]
    [SerializeField] private NetworkBullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shotsPerSecond = 8f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int reserveAmmo = 90;
    [SerializeField] private float reloadDuration = 2f;

    private NetworkVariable<int> currentAmmo = new NetworkVariable<int>();
    private NetworkVariable<int> remainingAmmo = new NetworkVariable<int>();

    private float nextShotTime;
    private bool isReloading;
    private float reloadFinishTime;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentAmmo.Value = magazineSize;
            remainingAmmo.Value = reserveAmmo;
        }
    }

    private void Update()
    {
        if (!IsSpawned)
            return;

        if (IsOwner)
        {
            Keyboard keyboard = Keyboard.current;

            if (Mouse.current != null &&
                Mouse.current.leftButton.isPressed &&
                firePoint != null)
            {
                ShootRpc(firePoint.right);
            }

            if (keyboard != null &&
                keyboard.rKey.wasPressedThisFrame)
            {
                ReloadRpc();
            }
        }

        if (IsServer && isReloading &&
            Time.time >= reloadFinishTime)
        {
            FinishReload();
        }
    }

    [Rpc(SendTo.Server)]
    private void ShootRpc(Vector2 direction)
    {
        if (isReloading)
            return;

        if (currentAmmo.Value <= 0)
        {
            StartReload();
            return;
        }

        if (Time.time < nextShotTime)
            return;

        if (bulletPrefab == null || firePoint == null)
            return;

        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.Normalize();

        nextShotTime = Time.time +
                       1f / Mathf.Max(shotsPerSecond, 0.1f);

        Collider2D[] ownerColliders =
            GetComponentsInChildren<Collider2D>();

        float angle = Mathf.Atan2(direction.y, direction.x)
                      * Mathf.Rad2Deg;

        NetworkBullet bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.Euler(0f, 0f, angle)
        );

        bullet.Launch(direction, ownerColliders, true);

        currentAmmo.Value--;

        ShowBulletRpc(firePoint.position, direction);

        if (currentAmmo.Value == 0)
            StartReload();
    }

    [Rpc(SendTo.NotServer)]
    private void ShowBulletRpc(Vector3 position, Vector2 direction)
    {
        if (bulletPrefab == null)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x)
                      * Mathf.Rad2Deg;

        NetworkBullet bullet = Instantiate(
            bulletPrefab,
            position,
            Quaternion.Euler(0f, 0f, angle)
        );

        Collider2D[] ownerColliders =
            GetComponentsInChildren<Collider2D>();

        bullet.Launch(direction, ownerColliders, false);
    }

    [Rpc(SendTo.Server)]
    private void ReloadRpc()
    {
        StartReload();
    }

    private void StartReload()
    {
        if (isReloading ||
            currentAmmo.Value >= magazineSize ||
            remainingAmmo.Value <= 0)
            return;

        isReloading = true;
        reloadFinishTime = Time.time + reloadDuration;
    }

    private void FinishReload()
    {
        int needed = magazineSize - currentAmmo.Value;
        int amount = Mathf.Min(needed, remainingAmmo.Value);

        currentAmmo.Value += amount;
        remainingAmmo.Value -= amount;

        isReloading = false;
    }

    private void OnGUI()
    {
        if (!IsSpawned || !IsOwner)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 28;
        style.normal.textColor = Color.white;

        string text = isReloading
            ? "Reloading..."
            : $"Ammo: {currentAmmo.Value} / {remainingAmmo.Value}";

        GUI.Label(new Rect(20, 20, 400, 50), text, style);
    }
}