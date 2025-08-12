using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int CurScene = 0;
    void Start()
    {

    }

    void Update()
    {

    }

    public void SceneMove(int Scene)
    {
        if (CurScene == Scene)
            return;
        CurScene = Scene;
        SceneManager.LoadScene(Scene);
    }
}