
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JH_Lava : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
       if(other.transform.CompareTag("Player"))
        {
            //플레이어 주금
        }
    }

}
