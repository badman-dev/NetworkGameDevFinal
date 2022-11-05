using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<Vector2> PositionChange = new NetworkVariable<Vector2>();
    public NetworkVariable<float> RotationChange = new NetworkVariable<float>();

    public GameObject cursorPrefab; //aim, curso
    public Rigidbody2D bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 20f;

    private Rigidbody2D body;

    public float movementSpeed = .4f;
    public float blowbackScale = 100;

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
    }

    private void Die()
    {
        Destroy(gameObject);
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

        GameObject bullet = collision.gameObject;

        if (bullet.CompareTag("Bullet") && bullet.GetComponent<NetworkObject>().OwnerClientId != gameObject.GetComponent<NetworkObject>().OwnerClientId) {
            Die();
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
    private void RequestShootBulletServerRpc(ServerRpcParams rpcParams = default)
    {
        Rigidbody2D newBullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        newBullet.velocity = transform.right * bulletSpeed;
        newBullet.gameObject.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);

        Destroy(newBullet.gameObject, 3);
    }

    private void Update()
    {
        if (IsOwner)
        {
            Vector2 moveVect = CalcPos();
            float rot = CalcRot();

            if (Input.GetButtonDown("Fire1"))
            {
                RequestShootBulletServerRpc();
                RequestBlowbackServerRpc();
            }

            RequestPositionForMovementServerRpc(moveVect, rot);
        }

        if (!IsOwner || IsHost) //actually moving player
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
