using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AmmoPickup : NetworkBehaviour
{
    public int ammoValue = 10;

    private AmmoSpawn spawn;

    public void SetSpawnRelation(AmmoSpawn spawn)
    {
        this.spawn = spawn;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) { return; }

        if (collision.gameObject.GetComponent<PlayerController>())
        {
            collision.gameObject.GetComponent<PlayerController>().AddAmmo(ammoValue);
        }

        spawn.StartCountdown();
    }
}
