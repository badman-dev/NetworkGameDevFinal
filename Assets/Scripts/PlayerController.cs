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
    public float blowbackScale = 4;

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

    private float Blowback(float currentVelocity) //adding gunshot blowback to velocity
    {
        Debug.Log("current: " + currentVelocity);
        float newVelocity = currentVelocity - blowbackScale;
        Debug.Log("new: " + newVelocity);
        return newVelocity;
    }

    [ServerRpc]
    private void RequestPositionForMovementServerRpc(Vector2 posChange, float rotChange) //updating network variables on server
    {
        if (!IsServer && !IsHost) return;

        PositionChange.Value = posChange;
        RotationChange.Value = rotChange;
    }

    [ServerRpc]
    private void RequestShootBulletServerRpc()
    {
        Rigidbody2D newBullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        newBullet.velocity = transform.right * bulletSpeed;
        newBullet.gameObject.GetComponent<NetworkObject>().Spawn();

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
                rot = Blowback(rot);
            }

            RequestPositionForMovementServerRpc(moveVect, rot);
        }

        if (!IsOwner || IsHost) //actually moving player
        {
            body.velocity = PositionChange.Value;
            body.rotation = RotationChange.Value;
        }
    }
}
