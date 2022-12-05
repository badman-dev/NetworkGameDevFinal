using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public GameObject playerPrefab;
    public GameObject spawnPoints;

    public TextMeshProUGUI endText;
    public int endCountdownTime = 6;

    private int spawnIndex = 0;
    private List<Transform> availableSpawnPositions = new List<Transform>();

    public void Awake()
    {
        RefreshSpawnPoints();
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            SpawnPlayers();
        }
    }

    private void RefreshSpawnPoints()
    {
        Transform[] allpoints = spawnPoints.GetComponentsInChildren<Transform>();
        availableSpawnPositions.Clear();
        foreach (Transform trans in allpoints)
        {
            if (trans != spawnPoints.transform)
            {
                availableSpawnPositions.Add(trans);
            }
        }
    }

    public Vector3 GetNextSpawnLocation()
    {
        var newPosition = availableSpawnPositions[spawnIndex].position;
        newPosition.y = 1.5f;
        spawnIndex += 1;

        if (spawnIndex > availableSpawnPositions.Count - 1)
        {
            spawnIndex = 0;
        }
        return newPosition;
    }

    private void SpawnPlayers()
    {
        foreach (ulong cid in NetworkManager.Singleton.ConnectedClientsIds) 
        {
            GameObject playerInst = Instantiate(playerPrefab, GetNextSpawnLocation(), Quaternion.identity);
            playerInst.GetComponent<NetworkObject>().SpawnAsPlayerObject(cid);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckEndGameServerRpc() //called from a player whenever their health changes. checks if only one living player and then starts end game process if true
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        int playersAlive = 0;
        foreach (PlayerController player in players)
        {
            if (player.Health.Value > 0)
            {
                playersAlive++;
                if (playersAlive > 1)
                {
                    return;
                }
            }
        }

        foreach (PlayerController player in players)
        {
            bool didWin = false;
            if (player.Health.Value > 0)
            {
                didWin = true;
            }

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { player.gameObject.GetComponent<NetworkObject>().OwnerClientId }
                }
            };

            StartEndGameClientRpc(didWin, clientRpcParams);
        }
    }

    [ClientRpc]
    private void StartEndGameClientRpc(bool didWin, ClientRpcParams clientRpcParams = default) //handles text updating and starts the countdown coroutine
    {
        endText.enabled = true;
        if (didWin)
        {
            endText.text = "You win!";
        }
        else
        {
            endText.text = "You loose!";
        }

        //this is currently run everywhere but the endgame only actually happens on host. this would allow a visible countdown for everyone if we add that
        StartCoroutine(EndCountdown());
    }

    [ServerRpc]
    private void EndGameServerRpc() //moves back to lobby scene at the end of the EndCountdown coroutine
    {
        NetworkObject[] netObjects = FindObjectsOfType<NetworkObject>();
        foreach (NetworkObject netObject in netObjects)
        {
            Destroy(netObject.gameObject);
        }

        NetworkManager.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public IEnumerator EndCountdown() //creates a countdown time in which the end game text is displayed for a bit before returning to lobby
    {
        //handling countdown
        int currentTime = endCountdownTime;

        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime -= 1;
        }

        if (IsHost)
        {
            EndGameServerRpc();
        }
    }
}
