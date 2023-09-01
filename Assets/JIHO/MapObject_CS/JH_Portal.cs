using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JH_Portal : MonoBehaviour
{
    [SerializeField] GameObject exit;
   [SerializeField]  bool isEnter;
   [SerializeField] GameObject WarpTarget;
    [SerializeField] bool isTokenportal;
    private void Update()
    {
        Warp();
    }
    private void OnTriggerEnter(Collider other)
    {
        isEnter = true;
        WarpTarget = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        isEnter = false;
        WarpTarget = null;
    }
    private void Warp()
    {
        if(isTokenportal==false)
        {
            if (Input.GetKeyDown(KeyCode.X) && isEnter == true)
            {
                WarpTarget.transform.position = exit.transform.position;
                WarpTarget.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
        else
        {
            //µ· Ã¼Å© ÈÄ
            if (Input.GetKeyDown(KeyCode.X) && isEnter == true)
            {
                WarpTarget.transform.position = exit.transform.position;
                WarpTarget.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
       
    }
}
