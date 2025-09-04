using UnityEngine;

public class SoundBox : MonoBehaviour
{
    public enum SOUND { SOUND_SELECT, SOUND_USE, SOUND_ERROR, SOUND_WIN, SOUND_LOSE, SOUND_END };
    public AudioSource[] Sounds;

    public void PlaySound(int Temp)
    {
        if (Temp >= (int)SOUND.SOUND_END)
        {
            Debug.Log("PlaySound Error");
            return;
        }

        Sounds[Temp].Play();
    }
}
