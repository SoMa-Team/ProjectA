using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 코루틴 사용
using System.Collections.Generic;
using System.Text; // For ToString()

public class DoubleLinkedListNode<T>
{
    public T Value { get; set; }
    public DoubleLinkedListNode<T> Previous { get; set; }
    public DoubleLinkedListNode<T> Next { get; set; }
    public CardUI LinkedUI { get; set; } // 이 노드에 연결된 UI 객체 참조 (중요)

    public DoubleLinkedListNode(T value, CardUI ui)
    {
        Value = value;
        LinkedUI = ui;
    }
}

public class DoubleLinkedList<T>
{
    public DoubleLinkedListNode<T> Head { get; private set; }
    public DoubleLinkedListNode<T> Tail { get; private set; }
    public int Count { get; private set; }

    // 노드 추가 (맨 뒤)
    public DoubleLinkedListNode<T> AddLast(T value, CardUI ui)
    {
        var newNode = new DoubleLinkedListNode<T>(value, ui);
        if (Tail == null) // 리스트가 비어있을 때
        {
            Head = newNode;
            Tail = newNode;
        }
        else
        {
            Tail.Next = newNode;
            newNode.Previous = Tail;
            Tail = newNode;
        }
        Count++;
        return newNode;
    }

    // 특정 노드 뒤에 삽입
    public DoubleLinkedListNode<T> InsertAfter(DoubleLinkedListNode<T> node, T value, CardUI ui)
    {
        if (node == null) return AddLast(value, ui); // node가 null이면 맨 뒤에 추가

        var newNode = new DoubleLinkedListNode<T>(value, ui);
        newNode.Previous = node;
        newNode.Next = node.Next;

        if (node.Next != null)
        {
            node.Next.Previous = newNode;
        }
        else // node가 Tail이었을 경우
        {
            Tail = newNode;
        }
        node.Next = newNode;
        Count++;
        return newNode;
    }

    // 노드 제거
    public void Remove(DoubleLinkedListNode<T> nodeToRemove)
    {
        if (nodeToRemove == null) return;

        if (nodeToRemove.Previous != null)
        {
            nodeToRemove.Previous.Next = nodeToRemove.Next;
        }
        else // Head를 제거하는 경우
        {
            Head = nodeToRemove.Next;
        }

        if (nodeToRemove.Next != null)
        {
            nodeToRemove.Next.Previous = nodeToRemove.Previous;
        }
        else // Tail을 제거하는 경우
        {
            Tail = nodeToRemove.Previous;
        }

        // 연결된 UI 객체 파괴는 Manager에서 처리해야 함
        // if (nodeToRemove.LinkedUI != null) UnityEngine.Object.Destroy(nodeToRemove.LinkedUI.gameObject);

        nodeToRemove.Previous = null; // 연결 끊기
        nodeToRemove.Next = null;
        nodeToRemove.LinkedUI = null;
        Count--;
    }

    // 값으로 노드 찾기 (비효율적일 수 있음, 필요시 Dictionary 등으로 보완)
    public DoubleLinkedListNode<T> Find(T value)
    {
        var current = Head;
        while (current != null)
        {
            if (EqualityComparer<T>.Default.Equals(current.Value, value))
            {
                return current;
            }
            current = current.Next;
        }
        return null;
    }

     // UI 객체로 노드 찾기
    public DoubleLinkedListNode<T> FindByUI(CardUI ui)
    {
        var current = Head;
        while (current != null)
        {
            if (current.LinkedUI == ui)
            {
                return current;
            }
            current = current.Next;
        }
        return null;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        var current = Head;
        while(current != null)
        {
            sb.Append($"[{current.Value}] -> ");
            current = current.Next;
        }
        sb.Append("null");
        return sb.ToString();
    }
}

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; } // 싱글톤 접근

    [Header("Data")]
    private DoubleLinkedList<CardData> deckList = new DoubleLinkedList<CardData>();

    [Header("UI References")]
    [SerializeField] private GameObject cardUIPrefab; // 카드 UI 프리팹
    [SerializeField] private RectTransform deckContentParent; // 카드 UI들이 생성될 부모 (HorizontalLayoutGroup 보유)
    [SerializeField] private ScrollRect deckScrollRect; // 덱 스크롤뷰
    [SerializeField] private LayoutGroup deckLayoutGroup; // HorizontalLayoutGroup
    [SerializeField] private ContentSizeFitter deckContentSizeFitter;

    [Header("Settings")]
    [SerializeField] private float scrollThresholdWidth = 800f; // 스크롤 활성화 기준 너비

    // 드래그 앤 드롭 관련 임시 변수
    private CardUI currentlyDraggedCard = null; // 현재 드래그 중인 카드 (선택적)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 씬 전환 시 유지해야 한다면
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 외부 호출 함수 ---

    // Inventory에서 Deck으로 카드가 드롭되었을 때 호출됨
    public void AddCardFromInventory(CardUI cardUI, Vector2 dropPosition)
    {
        if (cardUI == null || cardUI.cardData == null) return;

        CardData cardData = cardUI.cardData;

        // 1. Inventory에서 제거 요청
        Inventory.Instance.RemoveCard(cardUI); // Inventory에서 UI 제거 및 데이터 제거
        
        // 1. 새 부모 설정 (worldPositionStays = false)
        cardUI.transform.SetParent(deckContentParent, false);
        cardUI.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        // 2. Deck List에 데이터 추가 (삽입 위치 결정)
        DoubleLinkedListNode<CardData> nodeToInsertAfter = FindInsertionNode(dropPosition);
        DoubleLinkedListNode<CardData> newNode;

        // 3. Deck UI 영역에 CardUI 배치 및 데이터 연결
        cardUI.transform.SetParent(deckContentParent, false); // Deck의 Content 영역으로 부모 변경
        cardUI.currentLocation = CardUI.CardLocation.Deck; // 위치 상태 변경

        if (nodeToInsertAfter == null) // 맨 앞에 삽입하거나 리스트가 비었을 경우 (맨 뒤에 추가 후 순서 조정)
        {
            newNode = deckList.AddLast(cardData, cardUI); // 일단 맨 뒤에 추가
            cardUI.transform.SetAsFirstSibling(); // UI상 맨 앞으로
            // 실제 리스트 순서도 맨 앞으로 조정 (AddFirst 구현 또는 Remove/AddFirst) - 복잡도 증가
            // 여기서는 UI 순서만 맞추고, 리스트는 맨 뒤 추가 후 필요시 정렬하는 방식 사용 가능
            // 더 정확하려면 InsertBefore(Head, ...) 또는 AddFirst 구현 필요
        }
        else
        {
            newNode = deckList.InsertAfter(nodeToInsertAfter, cardData, cardUI);
            // UI 순서 설정: 삽입 기준 노드 UI의 다음 순서로
            cardUI.transform.SetSiblingIndex(nodeToInsertAfter.LinkedUI.transform.GetSiblingIndex() + 1);
        }

        Debug.Log($"Added to Deck: {cardData.cardName}. Current Deck: {deckList}");

        // 4. 레이아웃 업데이트 및 스크롤 처리
        RequestLayoutUpdate();
    }

    // Deck에서 카드를 Inventory 또는 외부로 드래그 시작했을 때 (CardUI가 호출)
    public void NotifyDragStarted(CardUI card)
    {
        currentlyDraggedCard = card;
        // 필요시 추가 로직
    }

    // Deck에서 카드가 제거될 때 (Inventory로 이동 등)
    public void RemoveCardFromDeck(CardUI cardUI)
    {
        if (cardUI == null || cardUI.cardData == null) return;

        // 1. 연결 리스트에서 노드 찾기 및 제거
        DoubleLinkedListNode<CardData> nodeToRemove = deckList.FindByUI(cardUI);
        if (nodeToRemove != null)
        {
            CardData removedData = nodeToRemove.Value;
            deckList.Remove(nodeToRemove);
            Debug.Log($"Removed from Deck List: {removedData.cardName}. Current Deck: {deckList}");

            // 2. CardUI 오브젝트는 Inventory.AddCardFromDeck 등에서 관리하거나, 여기서 Destroy
            // 여기서는 Inventory로 이동하는 경우 Inventory가 CardUI를 인수한다고 가정
            // 만약 완전히 삭제하는 경우: Destroy(cardUI.gameObject);

            // 3. 레이아웃 업데이트
            RequestLayoutUpdate();
        }
        else
        {
            Debug.LogWarning($"Card UI {cardUI.name} not found in deck list.");
        }
    }

     // 덱 내에서 카드 순서 변경 시 호출됨
    public void ReorderCardInDeck(CardUI cardUI, Vector2 dropPosition)
    {
        if (cardUI == null || cardUI.cardData == null) return;

        // 1. 현재 노드 찾기
        DoubleLinkedListNode<CardData> currentNode = deckList.FindByUI(cardUI);
        if (currentNode == null) return;

        // 2. 드롭 위치 기반으로 새로운 이전 노드 찾기
        DoubleLinkedListNode<CardData> nodeToInsertAfter = FindInsertionNode(dropPosition, cardUI); // 자기 자신은 제외하고 찾기

        // 3. 위치 변경이 필요한 경우 리스트 재구성
        if ((nodeToInsertAfter == null && currentNode != deckList.Head) || // 맨 앞으로 이동 or
            (nodeToInsertAfter != null && nodeToInsertAfter != currentNode.Previous)) // 다른 위치로 이동
        {
            // a. 리스트에서 현재 노드 임시 제거 (데이터는 유지)
            CardData cardData = currentNode.Value;
            deckList.Remove(currentNode); // UI는 파괴하지 않음!

            // b. 새로운 위치에 다시 삽입
            DoubleLinkedListNode<CardData> newNode;
            if (nodeToInsertAfter == null) // 맨 앞으로 이동
            {
                 // AddFirst 구현 필요, 임시로 AddLast 후 순서조정
                 newNode = deckList.AddLast(cardData, cardUI);
                 cardUI.transform.SetAsFirstSibling();
                 // 리스트 순서 재조정 로직 추가 필요
            }
            else // 특정 노드 뒤에 삽입
            {
                newNode = deckList.InsertAfter(nodeToInsertAfter, cardData, cardUI);
                 // UI 순서 설정
                cardUI.transform.SetSiblingIndex(nodeToInsertAfter.LinkedUI.transform.GetSiblingIndex() + 1);
            }
            Debug.Log($"Reordered {cardData.cardName}. New Deck: {deckList}");
             RequestLayoutUpdate(); // 레이아웃 갱신
        }
        else
        {
            // 위치 변경 없음, UI만 부모에게 돌려줌
            cardUI.transform.SetParent(deckContentParent, false);
            // 원래 순서대로 sibling index 설정 (필요시)
            // cardUI.transform.SetSiblingIndex(FindNodeIndex(currentNode));
             Debug.Log($"No reorder needed for. Setting parent back.");
        }
    }

    // --- 내부 헬퍼 함수 ---

    // 드롭된 화면 좌표 기준으로 어느 노드 뒤에 삽입할지 결정
    private DoubleLinkedListNode<CardData> FindInsertionNode(Vector2 screenDropPosition, CardUI ignoreCard = null)
    {
        DoubleLinkedListNode<CardData> insertAfterNode = null;
        float minDistance = float.MaxValue; // 가장 가까운 카드 왼쪽/오른쪽 판단용 (옵션)

        // deckContentParent 내의 모든 자식 CardUI 순회
        for (int i = 0; i < deckContentParent.childCount; i++)
        {
            RectTransform childRect = deckContentParent.GetChild(i) as RectTransform;
            CardUI childCardUI = childRect?.GetComponent<CardUI>();

            if (childCardUI != null && childCardUI != ignoreCard && childCardUI.gameObject.activeSelf)
            {
                // 카드 UI의 월드 좌표 사각형 구하기
                Vector3[] corners = new Vector3[4];
                childRect.GetWorldCorners(corners);
                // corners[0]: BottomLeft, corners[1]: TopLeft, corners[2]: TopRight, corners[3]: BottomRight

                 // 드롭 위치가 카드 중심보다 왼쪽에 있는지 오른쪽에 있는지 판단
                float cardCenterX = (corners[0].x + corners[2].x) / 2f;

                // 드롭 위치가 현재 검사하는 카드(childCardUI)의 왼쪽에 떨어졌다면,
                // 이 카드의 '이전' 노드 뒤에 삽입되어야 함.
                // 드롭 위치가 카드 오른쪽에 떨어졌다면, 이 카드 뒤에 삽입되어야 함.

                // 간단화 버전: 드롭 X 좌표가 카드 X 좌표보다 작으면 그 이전 노드를 반환 대상으로 고려
                if (screenDropPosition.x < cardCenterX)
                {
                     // 현재 카드(childCardUI)의 이전 노드를 찾아 반환
                     DoubleLinkedListNode<CardData> prevNode = deckList.FindByUI(childCardUI)?.Previous;
                     return prevNode; // 첫 카드보다 왼쪽에 떨구면 null 반환됨 (맨 앞 삽입)
                }
                 // 드롭 X 좌표가 카드 오른쪽에 계속 있다면, insertAfterNode를 계속 업데이트
                insertAfterNode = deckList.FindByUI(childCardUI);
            }
        }
        // 모든 카드의 오른쪽보다 더 오른쪽에 떨어졌다면 마지막 카드가 insertAfterNode가 됨 (맨 뒤 삽입)
        return insertAfterNode;
    }

    // 레이아웃 업데이트 요청 및 스크롤 처리
    public void RequestLayoutUpdate()
    {
        // 레이아웃 그룹이 자식 요소들을 재배치하도록 강제
        // ContentSizeFitter가 크기를 조절하도록 한 프레임 기다려야 할 수 있음
        StartCoroutine(UpdateLayoutCoroutine());
    }

    private IEnumerator UpdateLayoutCoroutine()
    {
        // 레이아웃 그룹 비활성화 -> 활성화로 강제 업데이트 (더 확실한 방법)
        if (deckLayoutGroup != null)
        {
            deckLayoutGroup.enabled = false;
            yield return null; // 한 프레임 대기
            deckLayoutGroup.enabled = true;
        }

        // 또는 LayoutRebuilder 사용 (즉시 실행되지만 완벽하지 않을 수 있음)
        // LayoutRebuilder.ForceRebuildLayoutImmediate(deckContentParent);

        yield return new WaitForEndOfFrame(); // ContentSizeFitter가 적용될 시간 확보

        // 스크롤 필요 여부 체크 및 활성화/비활성화
        if (deckScrollRect != null && deckContentParent != null)
        {
            bool requiresScroll = deckContentParent.rect.width > scrollThresholdWidth;
            // ScrollRect 자체를 활성화/비활성화 하거나 스크롤바만 보이게/숨기게 처리
            deckScrollRect.enabled = requiresScroll;
             // 필요에 따라 Horizontal Scrollbar만 제어
             // deckScrollRect.horizontalScrollbar?.gameObject.SetActive(requiresScroll);
            Debug.Log($"Deck width: {deckContentParent.rect.width}, Scroll enabled: {requiresScroll}");
        }
    }


    // --- Scene Specific Logic ---
    void Start()
    {
        // 씬 시작 시 초기 덱 로드 (예시)
        // LoadInitialDeck();
        RequestLayoutUpdate(); // 초기 레이아웃 설정
    }

    // 전투씬에서는 UI를 비활성화 하는 로직 추가
    public void SetBattleMode(bool isBattle)
    {
        // 방법 1: 모든 CardUI 게임오브젝트 비활성화
        // foreach (Transform child in deckContentParent)
        // {
        //     child.gameObject.SetActive(!isBattle);
        // }

        // 방법 2: Deck 패널 자체를 비활성화 (더 간단)
        deckContentParent.gameObject.SetActive(!isBattle);
        if (deckScrollRect != null) deckScrollRect.gameObject.SetActive(!isBattle);

        // 전투 씬 로직은 CardData 리스트만 참조하여 별도 로직 수행
        if (isBattle)
        {
            Debug.Log("Entering Battle Mode. Deck UI hidden.");
            // BattleManager 등에 deckList 정보 전달
        }
        else
        {
             Debug.Log("Entering Deck Building Mode. Deck UI shown.");
             RequestLayoutUpdate(); // 다시 보일 때 레이아웃 업데이트
        }
    }

     // '더보기' 기능: 전체 카드 목록 그리드로 보여주기 (별도 패널 필요)
    public void ShowAllCardsInGrid(RectTransform gridParent)
    {
        // gridParent의 자식 모두 삭제 (기존 목록 초기화)
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // deckList 순회하며 그리드에 CardUI 생성
        var currentNode = deckList.Head;
        while(currentNode != null)
        {
             GameObject cardObj = Instantiate(cardUIPrefab, gridParent);
             CardUI cardUI = cardObj.GetComponent<CardUI>();
             if(cardUI != null)
             {
                 // 그리드용 카드는 드래그 불가능하게 하거나 다른 상호작용 설정 가능
                 cardUI.Setup(currentNode.Value, CardUI.CardLocation.Deck); // 위치 정보는 Deck으로 설정
                 // cardUI.GetComponent<CanvasGroup>().blocksRaycasts = false; // 예: 클릭만 가능하게
             }
             currentNode = currentNode.Next;
        }
         // 그리드 레이아웃 업데이트 필요시
         LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent);
    }

    public void CalculateStats()
    {
        // for loop in deckList 
        var currentNode = deckList.Head;
        while (currentNode != null)
        {
            // 카드 효과 적용
            CardData card = currentNode.Value as CardData;
            if (card != null)
            {
                // 카드 효과 적용 로직
                // 예: attackStats.attackDamage += card.attackDamageChange;
                StatManager.Instance.ApplyCardEffects(card);
                Debug.Log($"Applying effects of {card.cardName}");
            }
            currentNode = currentNode.Next;
        }
    }

    // 데이터로부터 새 카드 UI 생성 (내부 또는 외부 호출용)
    // 이전에 private이었다면 public 또는 internal로 변경하거나, 아래 AddInitialCardToDeck 내부에 로직 구현
    // 여기서는 이미 존재한다고 가정하고 호출합니다.
    private CardUI CreateCardUIInstance(CardData cardData, RectTransform parent, CardUI.CardLocation location)
    {
        if (cardUIPrefab == null || parent == null)
        {
            Debug.LogError("Card UI Prefab or Parent is not set!");
            return null;
        }
        GameObject cardInstance = Instantiate(cardUIPrefab, parent);
        cardInstance.name = $"Card_{cardData.cardName}";
        CardUI cardUIComponent = cardInstance.GetComponent<CardUI>();
        if (cardUIComponent == null)
        {
            Debug.LogError("CardUI component not found on the instantiated prefab!");
            Destroy(cardInstance);
            return null;
        }
        cardUIComponent.Setup(cardData, location);
        return cardUIComponent;
    }


    // 게임 시작 시 덱에 카드를 직접 추가하는 함수
    public void AddInitialCardToDeck(CardData cardData)
    {
        if (cardData == null)
        {
            Debug.LogError("Cannot add null CardData to deck.");
            return;
        }
        if (deckContentParent == null)
        {
             Debug.LogError("Deck Content Parent is not assigned in CardManager.");
            return;
        }

        // 1. 카드 UI 인스턴스 생성 및 설정
        // CreateCardUIInstance가 private이면 여기서 직접 Instantiate 로직 수행
        CardUI newCardUI = CreateCardUIInstance(cardData, deckContentParent, CardUI.CardLocation.Deck);

        if (newCardUI == null)
        {
            Debug.LogError($"Failed to create Card UI instance for {cardData.cardName}.");
            return; // UI 생성 실패 시 중단
        }

        // 2. 덱 데이터 리스트(DoubleLinkedList)에 추가
        // 여기서는 가장 뒤에 추가하는 것으로 가정 (AddLast 사용)
        DoubleLinkedListNode<CardData> newNode = deckList.AddLast(cardData, newCardUI);
        // AddLast는 UI를 부모의 마지막 자식으로 만듦. HorizontalLayoutGroup이 순서를 처리함.
        // 필요시 newCardUI.transform.SetAsLastSibling(); 호출 가능

        Debug.Log($"Successfully added initial card to Deck: {cardData.cardName}. Current Deck size: {deckList.Count}");
        // Debug.Log($"Deck List State: {deckList}"); // 리스트 상태 확인용 (ToString 구현 필요)

        // 3. 레이아웃 업데이트 요청
        RequestLayoutUpdate();
    }
}
