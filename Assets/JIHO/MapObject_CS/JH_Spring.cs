using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JH_Spring : MonoBehaviour
{
    [SerializeField] float Power;



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if(collision.gameObject.transform.position.y > transform.position.y);
            {
                collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * Power);

            }
        }
    }
}
