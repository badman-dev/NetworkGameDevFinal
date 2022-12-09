using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DamageSource : MonoBehaviour
{
    public int damage = 1;
    public List<ulong> omitList = new List<ulong>();
    public bool destroyAfterCollision = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyAfterCollision)
        {
            if (!collision.gameObject.GetComponent<NetworkObject>())
            {
                Physics2D.IgnoreCollision(collision.collider, gameObject.GetComponent<Collider2D>());
                Destroy(gameObject);
            }
            else
            {
                foreach(ulong playerId in omitList)
                {
                    if (collision.gameObject.GetComponent<NetworkObject>().OwnerClientId != playerId)
                    {
                        Physics2D.IgnoreCollision(collision.collider, gameObject.GetComponent<Collider2D>());
                        Destroy(gameObject);
                        break;
                    }
                }
            }
        }
    }
}
