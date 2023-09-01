using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    public bool isServer;
    public bool IsServer { set { isServer = value; } } //�Ƹ� �Ⱦ���

    NetworkManager_Client client;
    NetworkManager_Server server;
    GameObject i;

    public bool isScenechange = false; //�Ƹ� ����
    // Start is called before the first frame update

    void Awake()
    {
        i = GameObject.Find("ServerFuntion");
        client = i.GetComponentInChildren<NetworkManager_Client>();
        server = i.GetComponentInChildren<NetworkManager_Server>();
    }

    // Update is called once per frame
    void Update()
    {
        if (true)
        {
           
        }
        if (isServer)
        {
            //GameStart1();



        }
        else if(!isServer)
        {
            //StartCoroutine(GameScene());
            //Debug.Log(isScenechange);
            //server.GameStart();
            //SceneManager.LoadScene("GAME");

        }

       


    }
    public void GameStart1()
    {
        StartCoroutine(GameScene());
        
    }

    public IEnumerator GameScene()
    {
        SceneManager.LoadScene("GAME");
        //Debug.Log("fu");
        yield return new WaitForSeconds(0.05f);
        server.GameStart();
    }

}
