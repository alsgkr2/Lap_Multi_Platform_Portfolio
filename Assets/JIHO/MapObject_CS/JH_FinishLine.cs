using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JH_FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
          //레이스 종료
        }
    }
}
