using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyManager : NetworkBehaviour
{
    private List<LobbyPlayerPanel> playerPanels;

    public GameObject playerScrollContent;
    public TMPro.TMP_Text txtPlayerNumber;
    public Button btnStart;
    public Button btnReady;
    public LobbyPlayerPanel playerPanelPrefab;

    public void Awake()
    {
        playerPanels = new List<LobbyPlayerPanel>();
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

    //Events
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        RefreshPlayerPanels();
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        RefreshPlayerPanels();
    }

}
