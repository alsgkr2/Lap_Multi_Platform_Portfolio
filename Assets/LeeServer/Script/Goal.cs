using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    private bool isServer;

    public NetworkManager_Server server;
    public NetworkManager_Client client;
    [SerializeField]
    GameManager gm;
    [SerializeField]
    Text text;
    [SerializeField]
    int players=4;
    float timer = 300f;
    public bool isGameStart = false;
    private void Awake()
    {
        text.enabled = false;
        gm = GetComponent<GameManager>();
        
    }
    private void Start()
    {
        server = GameObject.Find("NetworkManager_Server").GetComponent<NetworkManager_Server>(); 
        client = GameObject.Find("NetworkManager_Client").GetComponent<NetworkManager_Client>();
        isServer = server.serverReady;
    }
    public int PlayersToArray()
    {
        return GameObject.FindGameObjectsWithTag("Player").Length;
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag != "Player") return;
        if (collider.TryGetComponent(out JY_Player i))
        {
            if (isServer)
            {
                server.GameEnd();
            }
            else
            {
                client.GameEnd();
            }
            i.Goal();
        }
        if (timer >= 5.5f)
        {
            timer = 5.5f;
            text.enabled = true;
            players = GameObject.FindGameObjectsWithTag("Player").Length;
        }
        players--;
        //countDownStart();
        
    }

    float delay = 1;
    private void Update()
    {
        if (players <= 0&&timer>0) timer = 0;
        text.text = Math.Round(timer).ToString();
        Debug.Log(isGameStart);
        if (isGameStart) timer -= Time.deltaTime;
        text.text = ((int)timer).ToString();
        if (timer > 0) return;
        text.text = "GameOver";     //  카운트다운이 종료되면 게임 종료
        if (isServer)
        {
            server.Rank();
        }
        if (isServer)
        {
            server.StopAllCoroutines();
        }
        else
        {
            client.StopAllCoroutines();
        }
        if (delay > 0)
        {
            delay -= Time.deltaTime;
        }
        else
        SceneManager.LoadScene("END");
        
        //gm.GameEnd();
    }
    /// <summary>
    /// 1등이 진입했을 때만, 모든 사용자의 컴퓨터에서 호출
    /// </summary>
    public void countDownStart()
    {
    }
}
