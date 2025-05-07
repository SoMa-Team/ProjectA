using UnityEngine;
using UnityEngine.EventSystems;

public class CardDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum ZoneType { DeckZone, InventoryZone }
    public ZoneType zoneType; // 이 드롭존이 덱인지 인벤토리인지 구분

    // 드롭존 하이라이트 등 시각적 피드백 (선택적)
    // [SerializeField] private Image highlightImage;

    void Start()
    {
        // if (highlightImage != null) highlightImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 드래그 중인 카드가 들어왔을 때 시각적 피드백 (선택적)
        if (eventData.pointerDrag != null)
        {
            CardUI draggedCard = eventData.pointerDrag.GetComponent<CardUI>();
            if (draggedCard != null && AcceptsCard(draggedCard))
            {
                // if (highlightImage != null) highlightImage.enabled = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 드래그 중인 카드가 나갔을 때 시각적 피드백 해제 (선택적)
        // if (highlightImage != null) highlightImage.enabled = false;
    }


    public void OnDrop(PointerEventData eventData)
    {
        // if (highlightImage != null) highlightImage.enabled = false; // 드롭 시 하이라이트 해제

        CardUI draggedCardUI = eventData.pointerDrag.GetComponent<CardUI>();
        if (draggedCardUI != null && AcceptsCard(draggedCardUI))
        {
            Debug.Log($"{draggedCardUI.cardData.cardName} dropped on {zoneType}");

            // 1. 어느 영역에서 왔는지 확인 (Deck -> Inventory, Inventory -> Deck)
            CardUI.CardLocation sourceLocation = draggedCardUI.currentLocation;

            // 2. 목적지에 따른 처리
            if (zoneType == ZoneType.DeckZone && sourceLocation == CardUI.CardLocation.Inventory)
            {
                // Inventory -> Deck 이동
                // CardManager에 카드 추가 요청 (삽입 위치 계산 필요)
                CardManager.Instance.AddCardFromInventory(draggedCardUI, eventData.position);
            }
            else if (zoneType == ZoneType.InventoryZone && sourceLocation == CardUI.CardLocation.Deck)
            {
                // Deck -> Inventory 이동
                // Inventory에 카드 추가 요청, CardManager에서 제거 요청
                Inventory.Instance.AddCardFromDeck(draggedCardUI);
            }
            else
            {
                // 같은 영역 내 이동 (예: 덱 내 순서 변경) - CardManager에서 처리
                if (zoneType == ZoneType.DeckZone)
                {
                   CardManager.Instance.ReorderCardInDeck(draggedCardUI, eventData.position);
                }
                // 인벤토리 내 이동은 보통 Grid Layout이라 자동 처리되거나 불필요
            }
        }
    }

    // 이 드롭존이 특정 카드를 받을 수 있는지 확인하는 로직 (선택적)
    public bool AcceptsCard(CardUI card)
    {
        if (card == null) return false;
        // 예: 덱에는 특정 타입 카드만 받거나, 꽉 찼는지 등 조건 추가 가능
        return true;
    }
}
