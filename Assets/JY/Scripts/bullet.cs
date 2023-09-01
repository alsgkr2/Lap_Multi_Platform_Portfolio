using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    private Vector2 V;
    public Vector2 v
    {
        set { V = value; }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Translate(V * 9.0f*Time.deltaTime);
    }
    private IEnumerator OnCollisionEnter(Collision collision)
    {
        transform.position = Vector3.one * 1000;
        if (collision.collider.CompareTag("Player")) 
        {
            if (collision.gameObject.TryGetComponent(out JY_Player i))
                yield return StartCoroutine(i.Hit());
            else if (collision.gameObject.TryGetComponent(out ClientCube j))
                yield return StartCoroutine(j.Hit());
            else if (collision.gameObject.TryGetComponent(out Cube k))
                yield return StartCoroutine(k.Hit());
        }
        Destroy(gameObject);
        
        Debug.Log("Ãæµ¹");
    }
}
