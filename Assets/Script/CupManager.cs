using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CupManager : MonoBehaviour
{
    private const int MAXCUPINDEX = 27;
    private const int MAXUSABLEINDEX = 11;
    public Button[] Cups;
    public Button[] UsableCups;

    public SpriteAtlas Atlas;
    int Remain = MAXUSABLEINDEX + 1;
    public string ChangeImageIndex = "2";
    private int SelectIndex = -1; // 하단 버튼 선택

    void Start()
    {
        //for debug
        Debug.Log(Remain);
        foreach (Button button in Cups)
        {
            button.interactable = true;
        }
    }
    void Update() { }

    public void ChangeImage()
    {
        if (SelectIndex == -1)
            return;
        Debug.Log("function called : ChangeImage");
        GameObject ClickedButton = EventSystem.current.currentSelectedGameObject;
        Image ThisImage = ClickedButton.GetComponent<Image>();
        ThisImage.sprite = Atlas.GetSprite(ChangeImageIndex);

        UseBol();
    }

    public void SelectBol()
    {
        //선택한 버튼의 인덱스를 저장하기
        GameObject selectButton = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < Remain; ++i)
        {
            if (UsableCups[i].gameObject == selectButton)
            {
                SelectIndex = i;
                ChangeImageIndex = selectButton.GetComponent<Image>().sprite.name;
                break;
            }
        }
        Debug.Log("Bol Selected : " + ChangeImageIndex);
    }

    public void UseBol()
    {
        //선택하고 있었던 버튼을 맨 뒤로 보내고 디폴트 이미지로 전환
        if (Remain <= 0)
        {
            Debug.Log("UseBol Failed : No Remaining!");
            return;
        }

        UsableCups[SelectIndex].GetComponent<Image>().sprite = Atlas.GetSprite("Blank");
        for (int i = SelectIndex + 1; i <= MAXUSABLEINDEX; ++i)
        {
            string Temp = UsableCups[i].GetComponent<Image>().sprite.name;
            string Src = UsableCups[i].GetComponent<Image>().sprite.name;
            (UsableCups[i], UsableCups[i - 1]) = (UsableCups[i - 1], UsableCups[i]);
        }

        --Remain;
        SelectIndex = -1;
        Debug.Log($"Remain bol : {Remain}");
        UsableCups[Remain].interactable = false;
    }

    private int CheckFloor(int Index) // 인덱스로 층 체크하기
    {
        if (Index == 27)
            return 6;
        for (int i = 7; i < 1; --i)
        {
            Index -= i;
            if (Index < 0)
                return 7 - i;
        }

        Debug.Log("FloorCheck Error!");

        return -1;
    }
}
