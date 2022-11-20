using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyManager : NetworkBehaviour
{
    private List<LobbyPlayerPanel> playerPanels;
    private ulong[] playerIDs;

    public GameObject playerScrollContent;
    public LobbyPlayerPanel playerPanelPrefab;

    public void Awake()
    {
        playerPanels = new List<LobbyPlayerPanel>();
        playerIDs = new ulong[0];
    }

    private void AddPlayerPanel(ulong clientID)
    {
        LobbyPlayerPanel newPanel = Instantiate(playerPanelPrefab);
        newPanel.transform.SetParent(playerScrollContent.transform, false);
        newPanel.SetName($"Player {clientID.ToString()}");
        //newPanel.SetColor(info.color);
        newPanel.SetReady(true);
        playerPanels.Add(newPanel);
    }

    private void RefreshPlayerPanels()
    {
        Debug.Log("Refresh called");
        foreach (LobbyPlayerPanel panel in playerPanels)
        {
            Destroy(panel.gameObject);
        }
        playerPanels.Clear();

        foreach (ulong cid in NetworkManager.Singleton.ConnectedClientsIds)
        {
            AddPlayerPanel(cid);
        }
    }

    private void RefreshPlayerPanels(ulong[] ids)
    {
        Debug.Log("Refresh called");
        foreach (LobbyPlayerPanel panel in playerPanels)
        {
            Destroy(panel.gameObject);
        }
        playerPanels.Clear();

        foreach (ulong cid in ids)
        {
            AddPlayerPanel(cid);
        }
    }

    public void BtnStartClicked()
    {
        if (IsHost)
            NetworkManager.SceneManager.LoadScene("ChrisTest", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    //Events
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        if (IsHost)
            RefreshPlayerPanels();
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsHost)
        {
            playerIDs = new ulong[NetworkManager.Singleton.ConnectedClientsIds.Count];
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
            {
                playerIDs[i] = NetworkManager.Singleton.ConnectedClientsIds[i];
            }

            RefreshPlayerPanelsClientRpc(playerIDs);
        }
    }

    //RPCS
    [ClientRpc]
    public void RefreshPlayerPanelsClientRpc(ulong[] cids)
    {
        Debug.Log("Received refresh rpc");
        RefreshPlayerPanels(cids);
    }

}
