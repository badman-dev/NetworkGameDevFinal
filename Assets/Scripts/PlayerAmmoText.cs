using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerAmmoText : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI ammoText;

    void Update()
    {
        ammoText.text = $"{player.Ammo.Value}";

        transform.eulerAngles = Vector3.zero;
    }
}
