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

    //private string startText = "";

    public void Start()
    {
        btnHost.onClick.AddListener(OnHostClicked);
        btnClient.onClick.AddListener(OnClientClicked);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        //NetworkManager.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }



    private void OnHostClicked()
    {
        btnClient.gameObject.SetActive(false);
        btnHost.gameObject.SetActive(false);
        txtStatus.text = "Starting Server...";
        StartHost();
    }

    private void OnClientClicked()
    {
        btnClient.gameObject.SetActive(false);
        btnHost.gameObject.SetActive(false);
        txtStatus.text = "Searching for game...";
        NetworkManager.Singleton.StartClient();
    }

    //private void Update()
    //{
    //    float timer = 0f;
    //    int count = 0;
    //    timer += Time.deltaTime;
    //    if (timer >= .25f)
    //    {
    //        timer = 0f;
    //        if (count >= 2)
    //        {
    //            txtStatus.text = startText;
    //            count = 0;
    //        }
    //        else
    //        {
    //            txtStatus.text += ".";
    //            count++;
    //        }
    //    }
    //}
}
