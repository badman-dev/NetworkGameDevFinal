using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AmmoSpawn : NetworkBehaviour
{
    public GameObject ammoPickupPrefab;
    public float countDownTime = 10;

    private GameObject ammoPickup;

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
        ammoPickup = Instantiate(ammoPickupPrefab, Vector3.zero, Quaternion.identity);
        ammoPickup.GetComponent<NetworkObject>().Spawn();
        ammoPickup.GetComponent<AmmoPickup>().SetSpawnRelation(gameObject.GetComponent<AmmoSpawn>());
    }

    public IEnumerator SpawnCountdown()
    {
        //despawning old pickup since it's been "picked up"
        ammoPickup.GetComponent<NetworkObject>().Despawn();
        Destroy(ammoPickup);

        Debug.Log("starting countdown");
        yield return new WaitForSeconds(countDownTime);

        Debug.Log("finished countdown");

        SpawnAmmoServerRpc();

        //while (countDownTimer > 0)
        //{
        //    yield return new WaitForSeconds(1);
        //}
    }
}
