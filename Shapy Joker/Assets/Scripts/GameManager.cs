using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] RectTransform playerWinScreen;
    [SerializeField] RectTransform playerLoseScreen;
    Opponent opponent;
    [SerializeField] DeckBuilder deck;
    [SerializeField] Hand playerHand;
    public Hand opponentHand;
    [SerializeField] Button submitButton;
    [SerializeField] TextMeshProUGUI playerDeckText;
    [SerializeField] TextMeshProUGUI opponentDeckText;
    public Hand activeHand { get; private set; }
    public static bool isPlayerTurn { get; private set; } = true;
    public static bool isGameOver = false;
    public int playerScore { get; private set; } = 34;
    public int opponentScore { get; private set; } = 34;
    public static bool gamePaused;
    CanvasManager canvas;

    //This will contain the positions of the cards before they are submitted
    public List<Vector3> lastPositions { get; private set; } = new List<Vector3>();

    private void Awake()
    {
        canvas = FindObjectOfType<CanvasManager>();
        canvas.ChangeBackgroundImage(isPlayerTurn);
        opponent = FindObjectOfType<Opponent>();
        activeHand = isPlayerTurn ? playerHand : opponentHand;
        if (!isPlayerTurn) Invoke("ActivateOpponent", 1f);
        playerDeckText.text = opponentDeckText.text = "34";
    }

    public void SubmitSet()
    {
        if (gamePaused) return;
        StartCoroutine(MoveCardsOutOfScreen());
    }

    IEnumerator MoveCardsOutOfScreen()
    {
        lastPositions.Clear();
        foreach (RectTransform slot in activeHand.cardSlots)
        {
            CardVisual card = slot.GetComponentInChildren<CardVisual>();
            if (!card) break;
            lastPositions.Add(card.originalPos);
            activeHand.RemoveFromHand(card);
            CardVisual.tableCardsParent.RemoveFromFormation(card);
            iTween.MoveTo(card.gameObject, new Vector3(-8, 0, 0), 2f);
            Destroy(card.gameObject, 2f);
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.3f);
        deck.DealNewCards(lastPositions.Count);
    }

    public void HandleTurnEnd()
    {
        if (!isGameOver)
        {
            isPlayerTurn = !isPlayerTurn;
            if (isPlayerTurn)
            {
                activeHand = playerHand;
            }
            else
            {
                activeHand = opponentHand;
                Invoke("ActivateOpponent", 1f);
            }
            canvas.ChangeBackgroundImage(isPlayerTurn);
        }
        else
        {
            if (playerScore <= 0)
            {
                playerWinScreen.gameObject.SetActive(true);
            }
            else if (opponentScore <= 0)
            {
                playerLoseScreen.gameObject.SetActive(true);
            }
        }
    }

    public void LowerScore()
    {
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
        isGameOver = playerScore <= 0 || opponentScore <= 0;
    }

    public void SetSubmitButtonInteractable(bool value)
    {
        submitButton.interactable = value;
    }

    void ActivateOpponent()
    {
        opponent.StartTurn();
    }

    void HandleGameOver()
    {

    }
}
