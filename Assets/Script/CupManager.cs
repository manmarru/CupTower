using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

public class CupManager : MonoBehaviour
{
    private const int MAXCUPINDEX = 27;
    private const int MAXUSABLEINDEX = 11;
    private const int GAP = 55;

    public Button[] Cups;
    public Button[] UsableCups;

    public SpriteAtlas Atlas;
    public string ChangeImageIndex = "2";
    
    private int Remain = MAXUSABLEINDEX + 1;
    private int ActiveCups = 7;
    private int SelectIndex = -1; // 하단 버튼 선택

    void Start()
    {
        //for debug
        Debug.Log(Remain);
        for (int i = 0; i < 7; ++i)
        {
            Cups[i].interactable = true;
        }

        Vector2 criterion = UsableCups[0].GetComponent<RectTransform>().anchoredPosition;
        for (int i = 0; i <= MAXUSABLEINDEX; ++i)
        {
            UsableCups[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(criterion.x + GAP * i, criterion.y);
        }
    }
    void Update() { }

    public void ChangeImage()
    {
        if (SelectIndex == -1)
            return;
        GameObject ClickedButton = EventSystem.current.currentSelectedGameObject;
        Image ThisImage = ClickedButton.GetComponent<Image>();
        ThisImage.sprite = Atlas.GetSprite(ChangeImageIndex);

        UseBol();

        int StackIndex = -1;
        Button Temp = ClickedButton.GetComponent<Button>();
        for (int i = 0; i < MAXCUPINDEX; ++i)
        {
            if (Temp == Cups[i])
            {
                StackIndex = i;
            }
        }
        if (StackIndex == -1)
            Debug.Log("StackIndex Error!");


        int Originalindex = StackIndex;
        int Floor = CheckFloor(ref StackIndex);
        //stackindex는 해당 층에서의 인덱스로 바뀜
        if (Floor == 7)
            return;
        if (StackIndex != 0) // 좌측체크
        {
            if (Cups[Originalindex - 1].GetComponent<Image>().sprite.name != "Blank")
            {
                int NextIndex = Originalindex + (6 - Floor);
                if (false == Cups[NextIndex].interactable)
                {
                    ++ActiveCups;
                    Cups[NextIndex].interactable = true;
                }
            }
        }
        if (StackIndex != 6 - Floor)
        {
            if (Cups[Originalindex + 1].GetComponent<Image>().sprite.name != "Blank")
            {
                int NextIndex = Originalindex + (6 - Floor) + 1;
                if (false == Cups[NextIndex].interactable)
                {
                    ++ActiveCups;
                    Cups[NextIndex].interactable = true;
                }
            }
            //우측체크
        }
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

        Vector2 criterion = UsableCups[0].GetComponent<RectTransform>().anchoredPosition;
        for (int i = SelectIndex + 1; i <= MAXUSABLEINDEX; ++i)
        {
            (UsableCups[i], UsableCups[i - 1]) = (UsableCups[i - 1], UsableCups[i]);
        }

        for (int i = SelectIndex; i <= MAXUSABLEINDEX; ++i)
        {
            UsableCups[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(criterion.x + GAP * i, criterion.y);
        }


        --Remain;
        --ActiveCups;
        SelectIndex = -1;
        UsableCups[Remain].interactable = false;
    }

    private int CheckFloor(ref int Index) // 인덱스로 층 체크하기
    {
        if (Index == MAXCUPINDEX)
        {
            Index = 0;
            return 6;
        }
        for (int i = 7; i >= 1; --i)
        {
            if (Index < i)
            {
                return 7 - i;
            }
            Index -= i;
        }

        Debug.Log("FloorCheck Error!");
        return -1;
    }
}
