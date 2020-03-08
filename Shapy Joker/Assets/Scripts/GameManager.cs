using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] RectTransform playerWinScreen;
    [SerializeField] RectTransform playerLoseScreen;
    [SerializeField] DeckBuilder deck;
    [SerializeField] Hand playerHand;
    [SerializeField] Button submitButton;
    [SerializeField] TextMeshProUGUI playerDeckText;
    [SerializeField] TextMeshProUGUI opponentDeckText;
    public Hand opponentHand;
    public Hand activeHand { get; private set; }
    public static bool isPlayerTurn { get; private set; } = true;
    public static bool isGameOver = false;
    public int playerScore { get; private set; } = 34;
    public int opponentScore { get; private set; } = 34;
    public static bool gamePaused;

    //This will contain the positions of the cards before they are submitted
    public List<Vector3> lastPositions { get; private set; } = new List<Vector3>();

    private void Awake()
    {
        Blackboard.gm = this;
        Blackboard.cm.ChangeBackgroundImage(isPlayerTurn);
        activeHand = isPlayerTurn ? playerHand : opponentHand;
        if (!isPlayerTurn) Invoke("ActivateOpponent", 1f);
        playerDeckText.text = opponentDeckText.text = "34";
    }

    public void SetVolume(Slider volumeSlider)
    {
        AudioListener.volume = volumeSlider.value;
        Blackboard.cm.SetAudioImage(volumeSlider.value);
    }

    public void SubmitSet()
    {
        if (gamePaused) return;
        foreach (RectTransform slot in activeHand.cardSlots)
        {
            CardVisual card = slot.GetComponentInChildren<CardVisual>();
            if (!card) continue;
            card.SetSubmitted();
        }
        StartCoroutine(MoveCardsOutOfScreen());
    }

    IEnumerator MoveCardsOutOfScreen()
    {
        lastPositions.Clear();
        foreach (RectTransform slot in activeHand.cardSlots)
        {
            CardVisual card = slot.GetComponentInChildren<CardVisual>();
            if (!card) continue;
            lastPositions.Add(card.originalPos);
            activeHand.RemoveFromHand(card, true);
            Blackboard.tableCardsParent.RemoveFromFormation(card);
            iTween.MoveTo(card.gameObject, new Vector3(12, 0, 0), 2f);
            Destroy(card.gameObject, 1f);
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
            Blackboard.cm.ChangeBackgroundImage(isPlayerTurn);
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
        Blackboard.opponent.StartTurn();
    }

    void HandleGameOver()
    {

    }
}
