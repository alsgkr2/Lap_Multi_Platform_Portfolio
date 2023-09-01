using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool isBig;  //  체크시 크기 3배, 돈 5배 
    public string index="";

    private void Awake()
    {
        if (isBig)
        {
            transform.localScale = 2 * transform.localScale;
        }
    }
    private void Update()
    {
        transform.Rotate(0, 100 * Time.deltaTime, 0);
        
    }
}
