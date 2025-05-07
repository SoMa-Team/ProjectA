// Assets/Scripts/Editor/EditorPlayModeInitializer.cs
using UnityEngine;
using System.Collections.Generic; // List 사용 확인을 위해 추가 (InitialCardSettings에서 이미 사용)

public static class EditorPlayModeInitializer
{
    private const string SettingsAssetName = "DefaultStartCards";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeCardsOnPlay()
    {
        Debug.Log("Initializing multiple cards automatically on Play Mode start...");

        // 1. 설정 로드 (이전과 동일)
        var settings = Resources.Load<InitialCardSettings>(SettingsAssetName);
        if (settings == null)
        {
            Debug.LogError($"Failed to load InitialCardSettings asset named '{SettingsAssetName}'. Make sure it exists in a 'Resources' folder.");
            return;
        }

        // 2. CardManager 및 Inventory 인스턴스 찾기 (이전과 동일)
        CardManager cardManager = CardManager.Instance;
        Inventory inventory = Inventory.Instance;

        if (cardManager == null) Debug.LogError("CardManager instance not found!");
        if (inventory == null) Debug.LogError("Inventory instance not found!");


        // --- 수정된 부분 시작 ---

        // 3. 인벤토리에 카드 목록 추가 (foreach 루프 사용)
        if (inventory != null && settings.startingInventoryCards != null)
        {
            Debug.Log($"Auto-adding {settings.startingInventoryCards.Count} cards to Inventory.");
            foreach (CardData cardData in settings.startingInventoryCards)
            {
                if (cardData != null) // 리스트 요소가 null이 아닌지 확인
                {
                    inventory.AddCardData(cardData);
                }
                else
                {
                    Debug.LogWarning("Null CardData found in startingInventoryCards list. Skipping.");
                }
            }
             Debug.Log("Finished adding cards to Inventory.");
        }
        else if (inventory != null)
        {
             Debug.LogWarning("Starting Inventory Cards list is null or not assigned in InitialCardSettings.");
        }


        // 4. 덱에 카드 목록 추가 (foreach 루프 사용)
        if (cardManager != null && settings.startingDeckCards != null)
        {
            Debug.Log($"Auto-adding {settings.startingDeckCards.Count} cards to Deck.");
            foreach (CardData cardData in settings.startingDeckCards)
            {
                if (cardData != null) // 리스트 요소가 null이 아닌지 확인
                {
                    cardManager.AddInitialCardToDeck(cardData);
                }
                else
                {
                    Debug.LogWarning("Null CardData found in startingDeckCards list. Skipping.");
                }
            }
             Debug.Log("Finished adding cards to Deck.");
        }
         else if (cardManager != null)
        {
             Debug.LogWarning("Starting Deck Cards list is null or not assigned in InitialCardSettings.");
        }

        // --- 수정된 부분 끝 ---
    }
}