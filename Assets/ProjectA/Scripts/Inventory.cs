using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections; // 코루틴 사용

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Data")]
    private List<CardData> inventoryCards = new List<CardData>(); // 인벤 카드 데이터 리스트

    [Header("UI References")]
    [SerializeField] private GameObject cardUIPrefab;
    [SerializeField] private RectTransform inventoryContentParent; // 카드 UI 생성 부모 (GridLayoutGroup 보유)
    [SerializeField] private LayoutGroup inventoryLayoutGroup; // GridLayoutGroup

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 초기 인벤토리 로드 (예시: CardTest 오브젝트들)
        // LoadInitialInventory();
        RequestLayoutUpdate();
    }

    // Deck에서 카드가 드롭되었을 때 호출 (CardDropZone -> 여기)
    public void AddCardFromDeck(CardUI cardUI)
    {
        if (cardUI == null || cardUI.cardData == null) return;

        CardData cardData = cardUI.cardData;

        // 1. CardManager에 카드 제거 요청
        CardManager.Instance.RemoveCardFromDeck(cardUI); // Deck에서 데이터/UI 제거

        // 2. Inventory List에 데이터 추가
        inventoryCards.Add(cardData);
        
        // 3. 새 부모 설정 (worldPositionStays = false)
        cardUI.transform.SetParent(inventoryContentParent, false);
        cardUI.transform.localScale = cardUI.transform.localScale;

        // 3. Inventory UI 영역으로 CardUI 이동 및 설정
        cardUI.transform.SetParent(inventoryContentParent, false);
        cardUI.currentLocation = CardUI.CardLocation.Inventory; // 위치 상태 변경
        cardUI.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        Debug.Log($"Added to Inventory: {cardData.cardName}");

        // 4. 레이아웃 업데이트
        RequestLayoutUpdate();
    }

    // Inventory에서 카드가 제거될 때 (Deck으로 이동 등)
    public void RemoveCard(CardUI cardUI)
    {
        if (cardUI == null || cardUI.cardData == null) return;

        CardData cardData = cardUI.cardData;

        // 1. 데이터 리스트에서 제거
        bool removed = inventoryCards.Remove(cardData);

        if (removed)
        {
            Debug.Log($"Removed from Inventory List: {cardData.cardName}");
            // 2. CardUI 오브젝트 파괴 (Deck으로 이동하는 경우 DeckManager가 관리)
            // 만약 Deck으로 이동하는 경우, 여기서는 Destroy하면 안 됨.
            // CardManager.AddCardFromInventory에서 부모만 변경됨.
            // 만약 인벤토리에서 버리는 기능이라면 여기서 Destroy.
             // Destroy(cardUI.gameObject); // <- Deck으로 이동 시 주석 처리!

            // 3. 레이아웃 업데이트 (자동으로 될 수도 있음)
            RequestLayoutUpdate();
        }
    }

     // 외부에서 인벤토리에 카드 추가 (예: 상점에서 구매)
    public void AddCardData(CardData cardData)
    {
        if (cardData == null) return;

        inventoryCards.Add(cardData);

        // UI 생성
        GameObject cardObj = Instantiate(cardUIPrefab, inventoryContentParent);
        CardUI cardUI = cardObj.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.Setup(cardData, CardUI.CardLocation.Inventory);
        }
         Debug.Log($"Added to Inventory directly: {cardData.cardName}");
         RequestLayoutUpdate();
    }

    // 레이아웃 업데이트 요청
    public void RequestLayoutUpdate()
    {
        StartCoroutine(UpdateLayoutCoroutine());
    }

     private IEnumerator UpdateLayoutCoroutine()
    {
        if (inventoryLayoutGroup != null)
        {
            inventoryLayoutGroup.enabled = false;
            yield return null;
            inventoryLayoutGroup.enabled = true;
        }
        // LayoutRebuilder.ForceRebuildLayoutImmediate(inventoryContentParent);
        // yield return new WaitForEndOfFrame(); // 필요시 대기
    }
}
