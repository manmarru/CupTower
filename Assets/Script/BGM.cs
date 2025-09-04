using UnityEngine;


public class BGM : MonoBehaviour
{
    private static BGM Instance;
    private const int MAINSCENEINDEX = 3;
    public AudioSource CurrBGM;

    void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
            CurrBGM.Play();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance.CurrBGM.Stop();
            Instance.CurrBGM.clip = this.CurrBGM.clip;
            Instance.CurrBGM.Play();
            Destroy(gameObject);
        }
    }
}
