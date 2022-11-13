using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class AmmoSpawn : NetworkBehaviour
{
    public GameObject ammoPickupPrefab;
    public int countdownTime = 6;

    private GameObject ammoPickup;
    public TextMeshPro countdownText;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(0.375f, 0.375f, 0.375f));
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnAmmoServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnAmmoServerRpc()
    {
        ammoPickup = Instantiate(ammoPickupPrefab, transform.position, Quaternion.identity);
        ammoPickup.GetComponent<NetworkObject>().Spawn();
        ammoPickup.GetComponent<AmmoPickup>().SetSpawnRelation(gameObject.GetComponent<AmmoSpawn>());
    }

    [ClientRpc]
    private void UpdateCountdownDisplayClientRpc(int current)
    {
        countdownText.text = current.ToString();
    }

    [ClientRpc]
    private void ToggleCountdownDisplayClientRpc(bool enabled)
    {
        countdownText.enabled = enabled;
    }

    public void StartCountdown()
    {
        StartCoroutine(SpawnCountdown());
    }

    public IEnumerator SpawnCountdown()
    {
        //despawning old pickup since it's been "picked up"
        ammoPickup.GetComponent<NetworkObject>().Despawn();
        Destroy(ammoPickup);

        //handling countdown
        int currentTime = countdownTime;
        ToggleCountdownDisplayClientRpc(true);
        UpdateCountdownDisplayClientRpc(currentTime);
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime -= 1;
            UpdateCountdownDisplayClientRpc(currentTime);
        }
        ToggleCountdownDisplayClientRpc(false);

        //spawning ammo after countdown
        SpawnAmmoServerRpc();
    }
}
