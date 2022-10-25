using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PredictionManagerServer : NetworkBehaviour
{
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f;
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;
    private Queue<InputPayload> inputQueue;

    void Start()
    {
        if (!IsHost)
        {
            this.enabled = false;
        }
    }

}
