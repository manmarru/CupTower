using UnityEngine;

public class StackCup : MonoBehaviour
{
    private string m_SpriteName = "Blank";

    public string GetName()
    {
        if (m_SpriteName[0] == 'B')
            return "Blank";
        return m_SpriteName[0].ToString();
    }
    public void SetName(string Temp) { m_SpriteName = Temp; }
}