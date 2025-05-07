using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // TextMeshPro 사용하는 경우

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Data")]
    public CardData cardData { get; private set; } // 참조할 카드 데이터

    [Header("UI References")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI nameText; // TMP 사용 시
    [SerializeField] private TextMeshProUGUI descriptionText; // TMP 사용 시
    // [SerializeField] private Text costText; // 레거시 Text 사용 시

    private CanvasGroup canvasGroup;
    private Transform originalParent; // 드래그 시작 시 원래 부모
    private Vector3 originalPosition; // 드래그 시작 시 원래 위치 (선택적)
    private int originalSiblingIndex; // 원래 순서 (삽입 위치 계산용)

    // 드래그가 시작된 위치 (Deck or Inventory)
    public enum CardLocation { Deck, Inventory, None }
    public CardLocation currentLocation;

    // 참조 설정 (CardManager 또는 Inventory에서 호출)
    public void Setup(CardData data, CardLocation location)
    {
        cardData = data;
        currentLocation = location;

        if (cardData != null)
        {
            if (cardImage != null) cardImage.sprite = cardData.cardSprite;
            if (nameText != null) nameText.text = cardData.cardName;
            if (descriptionText != null) descriptionText.text = cardData.description;
            // 이름, 설명 등 UI 요소 업데이트
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // --- 드래그 앤 드롭 인터페이스 구현 ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardData == null) return; // 데이터 없으면 드래그 불가

        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        // 최상위 캔버스로 이동시켜 다른 UI 위에 보이게 함
        transform.SetParent(GetComponentInParent<Canvas>().transform, true);
        transform.SetAsLastSibling(); // 가장 위에 보이도록

        canvasGroup.blocksRaycasts = false; // 드래그 중에는 자신 아래의 UI가 이벤트를 받도록
        Debug.Log($"Drag Started: {cardData.cardName} from {currentLocation}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (cardData == null) return;
        // 스크린 좌표를 캔버스 로컬 좌표로 변환하여 따라다니게 함
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera, // 또는 GetComponentInParent<Canvas>().worldCamera
            out Vector2 localPos
        );
        transform.localPosition = localPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (cardData == null) return;

        canvasGroup.blocksRaycasts = true; // 레이캐스트 다시 활성화

        // 드롭된 위치에 DropZone이 있는지 확인
        GameObject dropTarget = eventData.pointerEnter;
        CardDropZone dropZone = (dropTarget != null) ? dropTarget.GetComponentInParent<CardDropZone>() : null;

        if (dropZone != null && dropZone.AcceptsCard(this))
        {
            // 유효한 DropZone에 드롭됨 -> DropZone이 처리하도록 맡김
            // DropZone의 OnDrop에서 부모 설정 및 데이터 이동 로직 호출
            Debug.Log($"Dropped {cardData.cardName} onto {dropZone.zoneType}");
        }
        else
        {
            // 유효하지 않은 곳에 드롭 -> 원래 위치로 복귀
            Debug.Log($"Invalid drop for {cardData.cardName}. Returning to original parent.");
            transform.SetParent(originalParent, false);
            transform.localPosition = originalPosition; // 로컬 포지션으로 복귀
            transform.SetSiblingIndex(originalSiblingIndex); // 원래 순서로 복귀
        }
        // CardManager나 Inventory에 레이아웃 갱신 요청 (필요시)
        // CardManager.Instance?.RequestLayoutUpdate();
        // Inventory.Instance?.RequestLayoutUpdate();
    }
}
