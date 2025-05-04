using System;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    private List<Card> cardPrefabs = new List<Card>();

    private void Awake()
    {
        // get all prefabs in the scene
        cardPrefabs.AddRange(FindObjectsByType<Card>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID));

        // log for debugging
        if (cardPrefabs.Count == 0)
        {
            Debug.LogError("No card prefabs found in the scene.");
        }
    }

    // call method to add card
    public void AddCard(Card newCard)
    {
        cardPrefabs.Add(newCard);
    }

    //  call method to remove card
    public void RemoveCard(Card cardToRemove)
    {
        if (cardPrefabs.Contains(cardToRemove))
        {
            cardPrefabs.Remove(cardToRemove);
        }
    }

    // call method to get card -> activates when button is clicked
    public void CalculateStats()
    {
        // sort the cardPrefabs list by Id
        cardPrefabs.Sort((card1, card2) => card1.Id.CompareTo(card2.Id));

        // apply card effects to StatManager
        foreach (var card in cardPrefabs)
        {
            StatManager.Instance.ApplyCardEffects(card);
        }
    }
}