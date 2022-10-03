using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<Vector2> PositionChange = new NetworkVariable<Vector2>();
    public NetworkVariable<float> RotationChange = new NetworkVariable<float>();

    public GameObject cursorPrefab; //aim, cursor

    private Rigidbody2D body;

    public float movementSpeed = .4f;

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
        moveVect = Vector2.ClampMagnitude(moveVect, movementSpeed);

        return moveVect;
    }

    private float CalcRot() //calculating mouse rotate float
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rotVect = mousePos - transform.position;

        float rot = Mathf.Atan2(rotVect.y, rotVect.x) * Mathf.Rad2Deg;

        return rot;
    }

    [ServerRpc]
    private void RequestPositionForMovementServerRpc(Vector2 posChange, float rotChange) //updating network variables on server
    {
        if (!IsServer && !IsHost) return;

        PositionChange.Value = posChange;
        RotationChange.Value = rotChange;
    }

    private void Update()
    {
        Vector2 moveVect = CalcPos();
        float rot = CalcRot();

        RequestPositionForMovementServerRpc(moveVect, rot);

        if (!IsOwner || IsHost) //actually moving player
        {
            body.velocity = moveVect;
            body.rotation = rot;
        }
    }
}
