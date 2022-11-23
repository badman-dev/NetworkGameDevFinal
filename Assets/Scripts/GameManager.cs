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

    private List<PlayerController> playerList = new List<PlayerController>();

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

            //playerList.Add(playerInst.GetComponent<PlayerController>());
            //AddPlayerListClientRpc(playerInst.GetComponent<PlayerController>());
        }
    }

    //[ClientRpc]
    //private void AddPlayerListClientRpc(PlayerController player)
    //{
    //    if (IsServer) { return; }

    //    playerList.Add(player);
    //}

    [ClientRpc]
    public void CheckEndGameClientRpc()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        //int aliveCount = 0;
        //foreach (PlayerController player in players)
        //{
        //    if (player.Health.Value > 0)
        //    {
        //        aliveCount++;
                
        //        if (aliveCount > 1) { return; }
        //    }
        //}

        endText.enabled = true;

        Debug.Log("My ID: " + NetworkManager.Singleton.LocalClientId);
        //int myHealth = GetPlayerHealthServerRpc(players, NetworkManager.Singleton.LocalClientId);

        //if (myHealth > 0)
        //{
        //    endText.text = "You win!";
        //}
        //else
        //{
        //    endText.text = "You loose!";
        //}
        foreach (PlayerController player in players)
        {
            //Debug.Log(player.gameObject.name);

            if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId ==
                NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log(player.gameObject.GetComponent<NetworkObject>().OwnerClientId);
                Debug.Log("Health: " + player.Health.Value);
                if (player.Health.Value > 0)
                {
                    endText.text = "You win!";
                }
                else
                {
                    endText.text = "You loose!";
                }
                break;
            }
        }

        StartCoroutine(EndCountdown());
    }

    //[ServerRpc]
    //private int GetPlayerHealthServerRpc(PlayerController[] players, ulong clientId)
    //{
    //    foreach(PlayerController player in players)
    //    {
    //        if (player.gameObject.GetComponent<NetworkObject>().OwnerClientId == clientId)
    //        {
    //            Debug.Log("This player's health is " + player.Health.Value);
    //            return player.Health.Value;
    //        }
    //    }

    //    return -1;
    //}

    [ServerRpc]
    private void EndGameServerRpc()
    {
        NetworkManager.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public IEnumerator EndCountdown()
    {
        //handling countdown
        int currentTime = endCountdownTime;

        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime -= 1;
        }

        EndGameServerRpc();
    }
}
