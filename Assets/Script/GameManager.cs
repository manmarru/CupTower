using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static int CurScene = 0;
    void Start()
    {

    }

    void Update()
    {

    }

    public void SceneMove(int Scene)
    {
        if (CurScene == Scene)
        {
            Debug.Log("Same Scene!");
            return;
        }
            
        CurScene = Scene;
        Debug.Log($"Move To {Scene}");
        SceneManager.LoadScene(Scene);
    }
    public void NextScene()
    {
        ++CurScene;
        Debug.Log($"Move To {CurScene}");
        SceneManager.LoadScene(CurScene);
    }
}