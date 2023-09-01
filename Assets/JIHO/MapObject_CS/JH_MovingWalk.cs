using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JH_MovingWalk : MonoBehaviour
{
    [SerializeField] List<GameObject> enterPlayer=new List<GameObject>();
    [SerializeField] Transform checkPos;
    bool isEnter;
    [SerializeField] Vector3 dir;
    [SerializeField] float speed;

    private void Start()
    {
       
    }
    private void FixedUpdate()
    {
        Accel();
    }

    void Accel()
    {
        if(enterPlayer!=null)
        {
            foreach(GameObject player in enterPlayer)
            {
                player.transform.Translate(dir * speed * Time.deltaTime);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
           if(!enterPlayer.Contains(collision.gameObject))
            {
                enterPlayer.Add(collision.gameObject);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (enterPlayer.Contains(collision.gameObject))
            {
                enterPlayer.Remove(collision.gameObject);
            }
        }
    }

}
