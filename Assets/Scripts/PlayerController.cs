using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<Vector2> PositionChange = new NetworkVariable<Vector2>();
    public NetworkVariable<float> RotationChange = new NetworkVariable<float>();

    public NetworkVariable<int> Health = new NetworkVariable<int>(1);

    public NetworkVariable<int> Ammo = new NetworkVariable<int>(10);

    private GameManager gm;

    public GameObject cursorPrefab; //aim, curso
    public Rigidbody2D bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 20f;

    private Rigidbody2D body;

    public float movementSpeed = .4f;
    public float blowbackScale = 100;

    public int maxAmmo = 10;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost || IsClient)
        {
            //spawn aim cursor locally
            Instantiate(cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }

        if (IsServer)
        {
            AddAmmo(maxAmmo);
        }

        gm = FindObjectOfType<GameManager>();
    }

    private void Damage(int dmg)
    {
        Health.Value -= dmg;
        HealthCheck();
    }

    private void HealthCheck()
    {
        if (Health.Value <= 0)
        {
            gm.CheckEndGameServerRpc();

            DieClientRpc();
        }
    }

    public void AddAmmo(int ammo)
    {
        if (Ammo.Value + ammo >= maxAmmo)
        {
            Ammo.Value = maxAmmo;
        }
        else
        {
            Ammo.Value += ammo;
        }
    }

    private Vector2 CalcPos() //calculating wasd movement vector
    {
        Vector2 moveVect = new Vector2(Input.GetAxis("Horizontal") * movementSpeed, Input.GetAxis("Vertical") * movementSpeed);
        //Vector2 moveVect = new Vector2(Input.GetAxisRaw("Horizontal") * movementSpeed, Input.GetAxisRaw("Vertical") * movementSpeed);
        moveVect = Vector2.ClampMagnitude(moveVect, movementSpeed);

        return moveVect;
    }

    private float CalcRot() //calculating mouse rotate float
    {
        //manually blocks mouse input when not focused window
        if (!Application.isFocused)
            return body.rotation;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rotVect = mousePos - transform.position;

        float rot = Mathf.Atan2(rotVect.y, rotVect.x) * Mathf.Rad2Deg;

        return rot;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsHost || !IsServer) { return; }

        GameObject source = collision.gameObject;

        //check if other player's bullet OR other kind of damage source
        if (source.CompareTag("Bullet")) {
            List<ulong> omitList = source.GetComponent<DamageSource>().omitList;

            bool inOmitList = false;
            foreach(ulong playerId in omitList)
            {
                if (gameObject.GetComponent<NetworkObject>().OwnerClientId == playerId)
                {
                    inOmitList = true;
                }
            }

            if (!inOmitList)
            {
                Damage(source.GetComponent<DamageSource>().damage);
            }
        }
        else if (source.GetComponent<DamageSource>())
        {
            Damage(source.GetComponent<DamageSource>().damage);
        }
    }

    [ServerRpc]
    private void RequestBlowbackServerRpc() //gunfire blowback for movement
    {
        Vector2 velocityForce = -transform.right * blowbackScale;
        body.AddForce(velocityForce);
    }

    [ServerRpc]
    private void RequestPositionForMovementServerRpc(Vector2 posChange, float rotChange) //updating network variables on server
    {
        if (!IsServer && !IsHost) return;

        PositionChange.Value = posChange;
        RotationChange.Value = rotChange;
    }

    [ServerRpc]
    private void ShootBulletServerRpc(ServerRpcParams rpcParams = default)
    {
        Rigidbody2D newBullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        newBullet.velocity = transform.right * bulletSpeed;

        newBullet.gameObject.GetComponent<DamageSource>().omitList.Add(gameObject.GetComponent<NetworkObject>().OwnerClientId);

        Ammo.Value -= 1;

        Destroy(newBullet.gameObject, 3);

        ShootBulletClientRpc(shootPoint.position, shootPoint.rotation, transform.right);
    }

    [ClientRpc]
    private void ShootBulletClientRpc(Vector3 shootPos, Quaternion shootRot, Vector3 right)
    {
        if (IsServer) { return; }

        Rigidbody2D newBullet = Instantiate(bulletPrefab, shootPos, shootRot);
        newBullet.velocity = transform.right * bulletSpeed;

        newBullet.gameObject.GetComponent<DamageSource>().omitList.Add(gameObject.GetComponent<NetworkObject>().OwnerClientId);

        Destroy(newBullet.gameObject, 3);
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer r in renderers)
        {
            r.enabled = false;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach(Collider c in colliders)
        {
            c.enabled = false;
        }

        Canvas[] canvases = GetComponentsInChildren<Canvas>();
        foreach(Canvas c in canvases)
        {
            c.enabled = false;
        }
    }

    private void Update()
    {
        if (IsOwner && Health.Value > 0)
        {
            Vector2 moveVect = CalcPos();
            float rot = CalcRot();

            if (Input.GetButtonDown("Fire1") && Ammo.Value > 0)
            {
                ShootBulletServerRpc();
                RequestBlowbackServerRpc();
            }

            RequestPositionForMovementServerRpc(moveVect, rot);

            body.rotation = rot; //rotating for self
        }

        if (!IsOwner) //moving the player with the server informed information on non-owners
        {
            body.rotation = RotationChange.Value;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || IsHost) //actually moving player
        {
            //body.velocity = PositionChange.Value; //this doesn't work with the bullet addforce but was consistent in speed
            body.AddForce(PositionChange.Value); //somewhat exponential speed increase wasd
        }
    }
}
