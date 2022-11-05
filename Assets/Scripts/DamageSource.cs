using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DamageSource : MonoBehaviour
{
    public int damage = 1;
    public bool destroyAfterCollision = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyAfterCollision && (!collision.gameObject.GetComponent<NetworkObject>() || collision.gameObject.GetComponent<NetworkObject>().OwnerClientId != gameObject.GetComponent<NetworkObject>().OwnerClientId))
        {
            Physics2D.IgnoreCollision(collision.collider, gameObject.GetComponent<Collider2D>());
            Destroy(gameObject);
        }
    }
}
