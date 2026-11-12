using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(100)]
public class WeaponController : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shotsPerSecond = 8f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int reserveAmmo = 90;
    [SerializeField] private float reloadDuration = 2f;

    [Header("Mobile Input")]
    [SerializeField] private VirtualJoystick aimJoystick;
    [SerializeField, Range(0.1f, 1f)] private float fireThreshold = 0.5f;
    private Collider2D[] ownerColliders;
    private int currentAmmo;
    private float nextShotTime;
    private bool isReloading;
    private float reloadFinishTime;

    private void Awake()
    {
        Rigidbody2D owner = GetComponentInParent<Rigidbody2D>();
        ownerColliders = owner.GetComponentsInChildren<Collider2D>();

        currentAmmo = magazineSize;
    }

    private void LateUpdate()
    {
        if (isReloading)
        {
            if (Time.time >= reloadFinishTime)
                FinishReload();

            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            StartReload();
            return;
        }

       bool pointerOverUI = EventSystem.current != null
                     && EventSystem.current.IsPointerOverGameObject();

        bool mouseFire = Mouse.current != null
                 && Mouse.current.leftButton.isPressed
                 && !pointerOverUI;

        bool spaceFire = keyboard != null
                         && keyboard.spaceKey.isPressed;

        bool joystickFire = aimJoystick != null
                    && aimJoystick.Value.magnitude >= fireThreshold;

        if ((mouseFire || spaceFire || joystickFire)
             && Time.time >= nextShotTime)
        {
         Shoot();
}
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        if (currentAmmo <= 0)
            return;

        nextShotTime = Time.time
                       + 1f / Mathf.Max(shotsPerSecond, 0.1f);

        Bullet bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        bullet.Launch(firePoint.right, ownerColliders);
        currentAmmo--;
    }

    private void StartReload()
    {
        if (currentAmmo >= magazineSize || reserveAmmo <= 0)
            return;

        isReloading = true;
        reloadFinishTime = Time.time + reloadDuration;
    }

    private void FinishReload()
    {
        int neededAmmo = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;
    }

    private void OnDisable()
    {
        isReloading = false;
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 28;
        style.normal.textColor = Color.white;

        string text = isReloading
            ? "Reloading..."
            : $"Ammo: {currentAmmo} / {reserveAmmo}";

        GUI.Label(new Rect(20, 20, 400, 50), text, style);
    }
    public void RequestReload(){
    if (!isReloading)
        StartReload();
}
}