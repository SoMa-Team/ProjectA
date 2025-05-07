// Assets/Scripts/Data/InitialCardSettings.cs (새 스크립트 파일 생성)

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InitialCardSettings", menuName = "Card Game/Initial Card Settings")]
public class InitialCardSettings : ScriptableObject
{
    // 단일 CardData 대신 List<CardData> 사용
    public List<CardData> startingDeckCards = new List<CardData>();
    public List<CardData> startingInventoryCards = new List<CardData>();
}