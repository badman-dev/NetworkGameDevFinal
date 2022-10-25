using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//https://github.com/Ajackster/ClientPredictionTutorial/tree/master/Assets/Scripts
public struct InputPayload
{
    public int tick;
    public Vector3 inputVector;
}

public struct StatePayload
{
    public int tick;
    public Vector3 position;
}

public class PlayerController : NetworkBehaviour
{
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f;
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] clientStateBuffer;
    private InputPayload[] clientInputBuffer;
    private StatePayload clientLatestServerState;
    private StatePayload clientLastProcessedState;

    private StatePayload[] serverStateBuffer;
    private Queue<InputPayload> serverInputQueue;

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

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        clientStateBuffer = new StatePayload[BUFFER_SIZE];
        clientInputBuffer = new InputPayload[BUFFER_SIZE];

        serverStateBuffer = new StatePayload[BUFFER_SIZE];
        serverInputQueue = new Queue<InputPayload>();
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
                RequestBlowbackServerRpc();
            }

            RequestPositionForMovementServerRpc(moveVect, rot);
        }

        if (!IsOwner || IsHost) //actually rotating player
        {
            body.rotation = RotationChange.Value;
        }

        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || IsHost) //actually moving player
        {
            //body.velocity = PositionChange.Value; //this doesn't work with the bullet addforce but was consistent in speed
            body.AddForce(PositionChange.Value); //somewhat exponential speed increase wasd
        }
        else if (IsOwner)
        {

        }
    }

    private void HandleTick()
    {

    }
}
