using UnityEngine;

public class StackCup : MonoBehaviour
{
    private string m_SpriteName = "5"; // 5 = blank

    public string GetName()
    {
        return m_SpriteName[0].ToString();
    }
    public void SetName(string Temp) { m_SpriteName = Temp; }
}