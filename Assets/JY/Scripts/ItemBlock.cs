using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBlock : MonoBehaviour
{
    [SerializeField]
    private Item item;
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "Player") return;
        collision.gameObject.GetComponent<JY_Player>().GetItem(item);
        
    }
}
