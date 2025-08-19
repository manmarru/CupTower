using UnityEngine;

public class StackCup : MonoBehaviour
{
    private string m_SpriteName = "Blank";

    public string GetName() { return m_SpriteName; }
    public void SetName(string Temp) { m_SpriteName = Temp; }
}