using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class P : MonoBehaviour
{

    Text text;
    NetworkManager_Server s;
    NetworkManager_Client c;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        s = GameObject.Find("NetworkManager_Server").GetComponent<NetworkManager_Server>();
        c = GameObject.Find("NetworkManager_Client").GetComponent<NetworkManager_Client>();
        text.text = "";

        if (s.serverReady)
        {
            for (int i = 0; i < s.finishTimes.Count; i++)
            {
                if (s.finishTimes[i] == null)
                {
                    continue;
                }
                text.text += (i + 1).ToString() + "등 :" + s.finishTimes[i].Item1.ToString() + ", 시간 :" + s.finishTimes[i].Item2.ToString() + "\n";
            }
        }
        else
        {
            for (int i = 0; i < c.finishTimes.Count; i++)
            {
                if (c.finishTimes[i] == null)
                {
                    continue;
                }
                text.text += (i + 1).ToString() + "등 :" + c.finishTimes[i].Item1.ToString() + ", 시간 :" + c.finishTimes[i].Item2.ToString() + "\n";
            }
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        //text.text = s.finishTime.ToString();
        
        

    }
}
