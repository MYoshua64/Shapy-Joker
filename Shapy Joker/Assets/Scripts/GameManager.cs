using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] Hand playerHand;
    public Hand opponentHand;
    [SerializeField] Button submitButton;
    [SerializeField] TextMeshProUGUI playerDeckText;
    [SerializeField] TextMeshProUGUI opponentDeckText;
    public Hand activeHand { get; private set; }
    public bool isPlayerTurn { get; private set; } = true;
    public List<Vector2> lastPositions { get; private set; } = new List<Vector2>();
    int playerScore = 34, opponentScore = 34;

    private void Awake()
    {
        activeHand = isPlayerTurn ? playerHand : opponentHand;
        playerDeckText.text = opponentDeckText.text = "34";
    }

    public void SubmitSet()
    {
        lastPositions.Clear();
        foreach (CardView card in activeHand.transform.GetComponentsInChildren<CardView>())
        {
            lastPositions.Add(card.originalPos);
            activeHand.RemoveFromHand(card.attachedCard);
            Destroy(card.gameObject);
            if (isPlayerTurn)
            {
                playerScore--;
                playerDeckText.text = playerScore.ToString();
            }
            else
            {
                opponentScore--;
                opponentDeckText.text = opponentScore.ToString();
            }
        }
        FindObjectOfType<DeckBuilder>().DealNewCards(lastPositions.Count);
        isPlayerTurn = !isPlayerTurn;
        if (isPlayerTurn)
        {
            activeHand = playerHand;
        }
        else
        {
            activeHand = opponentHand;
            FindObjectOfType<Opponent>().StartTurn();
        }
    }

    public void SetSubmitButtonInteractable(bool value)
    {
        submitButton.interactable = value;
    }
}
