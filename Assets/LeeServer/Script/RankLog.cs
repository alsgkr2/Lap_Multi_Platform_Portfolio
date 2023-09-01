using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Common_Data;


public class RankLog : MonoBehaviour
{
    public Text textObject;
    // Start is called before the first frame update
    void Start()
    {
        textObject = this.GetComponent<Text>();
        textObject.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RankLogPrint(stSendRanking stSend)
    {
        ushort[] str = stSend.ClientNames;

        float[] clientTime = stSend.time;

        foreach (ushort i in stSend.ClientNames)
        {
            textObject.text += (i + 1).ToString() + "µî :" + str[i].ToString() + " ½Ã°£ :" + clientTime[i].ToString() + "\n";
        }

       

    }
}
