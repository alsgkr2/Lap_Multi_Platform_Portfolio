using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    string[] scenes;
    public void StartGame()
    {
        SceneManager.LoadScene(scenes[1]);
    }
    public void GameEnd()
    {
        SceneManager.LoadScene(scenes[2]);
    }
}
