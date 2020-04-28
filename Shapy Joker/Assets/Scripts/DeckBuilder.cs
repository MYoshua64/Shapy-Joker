using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    [SerializeField] RectTransform playerDeck;
    [SerializeField] RectTransform opponentDeck;
    [SerializeField] CardVisual cardViewPF;
    List<string> preDeck = new List<string>();
    List<CardData> deck = new List<CardData>();
    List<CardVisual> cardsOnScreen;
    int currentCardIndex = 0;

    private void Start()
    {
        FillDeck();
        ShuffleDeck();
        AttachCardsToView();
    }

    void FillDeck()
    {
        for (int index = 0; index < 88; index++)
        {
            if (index < 80)
            {
                int number = index % 5 + 1;
                int shape = index / 5 % 4;
                int color = index / 20 % 4;
                DetermineStringID(color, shape, number);
            }
            else
            {
                int color = (index - 80) / 2 % 4;
                DetermineStringID(color);
            }
            CreateCard(preDeck[index], index);
        }
        
    }

    void DetermineStringID(int color, int shape, int number)
    {
        string id = "";
        switch (color)
        {
            case 0:
                id += "B";
                break;
            case 1:
                id += "G";
                break;
            case 2:
                id += "R";
                break;
            case 3:
                id += "Y";
                break;
        }
        switch (shape)
        {
            case 0:
                id += "C";
                break;
            case 1:
                id += "R";
                break;
            case 2:
                id += "S";
                break;
            case 3:
                id += "T";
                break;
        }
        id += number.ToString();
        preDeck.Add(id);
    }

    void DetermineStringID(int color)
    {
        string id = "";
        switch (color)
        {
            case 0:
                id += "B";
                break;
            case 1:
                id += "G";
                break;
            case 2:
                id += "R";
                break;
            case 3:
                id += "Y";
                break;
        }
        id += "J";
        preDeck.Add(id);
    }

    void CreateCard(string cardID, int index)
    {
        CardData newCard = new CardData(cardID);
        newCard.SetIndex(index);
        deck.Add(newCard);
    }

    void ShuffleDeck()
    {
        List<CardData> tempDeck = new List<CardData>();
        tempDeck.AddRange(deck);
        deck.Clear();
        while (tempDeck.Count > 0)
        {
            int index = Random.Range(0, tempDeck.Count);
            deck.Add(tempDeck[index]);
            tempDeck.RemoveAt(index);
        }
        currentCardIndex = 0;
    }

    void AttachCardsToView()
    {
        cardsOnScreen = Blackboard.tableCardsParent.cardsOnTable;
        for (; currentCardIndex < cardsOnScreen.Count; currentCardIndex++)
        {
            cardsOnScreen[currentCardIndex].AttachCardData(deck[currentCardIndex]);
            cardsOnScreen[currentCardIndex].SetOriginalPosition(cardsOnScreen[currentCardIndex].transform.localPosition);
        }
    }

    void AttachCardToView(CardVisual cardView)
    {
        cardView.AttachCardData(deck[currentCardIndex]);
        currentCardIndex++;
    }

    public void DealNewCards(int numberToDeal)
    {
        StartCoroutine(StartDealing(numberToDeal));
    }

    IEnumerator StartDealing(int numberToDeal)
    {
        RectTransform activeDeck = Blackboard.gm.isPlayerTurn ? playerDeck : opponentDeck;
        while (numberToDeal > 0 && !Blackboard.gm.isGameOver)
        {
            CardVisual newCard = Instantiate(cardViewPF, activeDeck.position, Quaternion.identity, Blackboard.tableCardsParent.transform);
            Blackboard.tableCardsParent.AddToFormation(newCard);
            newCard.SetOriginalPosition(Blackboard.gm.lastPositions[0]);
            iTween.MoveTo(newCard.gameObject, iTween.Hash("position", Blackboard.gm.lastPositions[0], "time", 0.75f,
                "oncompletetarget", Blackboard.sfxPlayer.gameObject, "oncomplete", "PlaySFX", "oncompleteparameters", false));
            Blackboard.gm.lastPositions.RemoveAt(0);
            AttachCardToView(newCard);
            numberToDeal--;
            Blackboard.gm.LowerScore();
            yield return new WaitForSeconds(0.3f);
        }
        Blackboard.gm.HandleTurnEnd();
    }
}
