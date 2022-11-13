using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MainUI : NetworkBehaviour
{
    public Button btnHost;
    public Button btnClient;
    public TMPro.TMP_Text txtStatus;

    private string startText = "";
    private int count = 0;
    private float timer = 0f;
    private bool isLoading = false;

    public void Start()
    {
        btnHost.onClick.AddListener(OnHostClicked);
        btnClient.onClick.AddListener(OnClientClicked);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }



    private void OnHostClicked()
    {
        btnClient.gameObject.SetActive(false);
        btnHost.gameObject.SetActive(false);
        startText = "Starting Server";
        isLoading = true;
        StartHost();
    }

    private void OnClientClicked()
    {
        btnClient.gameObject.SetActive(false);
        btnHost.gameObject.SetActive(false);
        startText = "Searching for game";
        isLoading = true;
        NetworkManager.Singleton.StartClient();
    }

    private void Update()
    {
        if (isLoading)
        {
            timer += Time.deltaTime;
            if (timer >= .25f)
            {
                timer = 0f;
                if (count >= 3)
                {
                    txtStatus.text = startText;
                    count = 0;
                }
                else
                {
                    txtStatus.text = txtStatus.text + ".";
                    count++;
                }
            }
        }
    }
}
