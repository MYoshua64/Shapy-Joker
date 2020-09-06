using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public bool allowCardPickUp { get; private set; } = true;
    public bool hotseatMode;
    [SerializeField] RectTransform playerWinScreen;
    [SerializeField] RectTransform playerLoseScreen;
    [SerializeField] Hand player1Hand;
    [Tooltip("For hotseat mode only!!")] [SerializeField] Hand player2Hand;
    [SerializeField] Button submitButtonDefault;
    [SerializeField] Button submitButtonCorrect;
    [SerializeField] TextMeshProUGUI playerDeckText;
    [SerializeField] TextMeshProUGUI opponentDeckText;
    [SerializeField] Sprite buttonDefault;
    [SerializeField] Sprite buttonWrong;
    [SerializeField] Toggle[] timerToggles;
    [SerializeField] LayerMask cards;
    public Hand opponentHand;
    public Hand activeHand { get; private set; }
    public bool isMainPlayerTurn { get; private set; } = true;
    public bool isGameOver { get; set; } = false;
    public int playerScore { get; private set; } = 34;
    public int opponentScore { get; private set; } = 34;
    public static bool gamePaused;
    public bool timerOn { get; private set; } = false;
    List<CardVisual> cardsOnScreen = new List<CardVisual>();
    bool playerSubmitted = false;
    static bool soundOn = true;
    static float prevVolume;

    /// <summary>
    /// Used to prevent the same time toggle to be pressed twice
    /// </summary>
    Toggle currentToggle;

    /// <summary>
    /// For card animation ONLY! Used to control animation timing between functions
    /// </summary>
    int cardCount = 0;

    /// <summary>
    /// A list that contains the cards position on the board before they are submitted
    /// </summary>
    public List<Vector3> lastPositions { get; private set; } = new List<Vector3>();

    #region Monobehaviour Callbacks

    private void Awake()
    {
        Blackboard.gm = this;
        Blackboard.cm.ChangeBackgroundImage(isMainPlayerTurn);
        Blackboard.deckBuilder.BuildDeck();
        activeHand = isMainPlayerTurn ? player1Hand : player2Hand;
        if (!isMainPlayerTurn && !hotseatMode) Invoke("ActivateOpponent", 1f);
        else if (isMainPlayerTurn) StartCoroutine(CheckForSets());
        playerDeckText.text = opponentDeckText.text = "34";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPeriod) && isMainPlayerTurn)
            HandleTurnEnd();
    }

    #endregion

    #region Sound

    public void ToggleVolume()
    {
        soundOn = !soundOn;
        if (!soundOn)
        {
            prevVolume = AudioListener.volume;
            SetVolume(0);
        }
        else
        {
            SetVolume(prevVolume);
            Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
        }
    }

    public void SetVolume(float value)
    {
        if (value > 0)
        {
            prevVolume = value;
        }
        AudioListener.volume = value;
        Blackboard.cm.SetAudioImage(value);
    }

    private static void PlaySumbitSFX(Hand submittingHand)
    {
        SFXType submitSFX = SFXType.CardWhooshThree;
        if (submittingHand.cardsInHand.Count == 4)
        {
            submitSFX = SFXType.CardJingleFour;
        }
        else if (submittingHand.cardsInHand.Count == 5)
        {
            submitSFX = SFXType.CardSpecialFive;
        }
        Blackboard.sfxPlayer.PlaySFX(submitSFX);
    }

    #endregion

    #region Submitting And Ending Turn

    public void SubmitSetWrong(Hand submittingHand)
    {
        if (gamePaused || submittingHand.submitted || activeHand != submittingHand) return;
        Blackboard.sfxPlayer.PlaySFX(SFXType.WrongSet);
        CancelInvoke("ResetSubmitButtonSprite");
        submittingHand.submitButtonWrong.GetComponent<Image>().sprite = buttonWrong;
        Invoke("ResetSubmitButtonSprite", 2f);
        ReportBadSetSubmission();
    }

    public void SubmitSet(Hand submittingHand)
    {
        if (gamePaused || submittingHand.submitted || activeHand != submittingHand) return;
        submittingHand.submitted = true;
        allowCardPickUp = false;
        PlaySumbitSFX(submittingHand);
        SetSubmitButtonsTrue(submittingHand.submitButton, submittingHand.submitButtonWrong, false);
        if (timerOn) Blackboard.timer.Stop();
        foreach (CardData card in activeHand.cardsInHand)
        {
            CardVisual _Card = card.cardView;
            if (!_Card) continue;
            _Card.SetSubmitted();
        }
        StartCoroutine(MoveCardsOutOfScreen());
    }

    IEnumerator MoveCardsOutOfScreen()
    {
        lastPositions.Clear();
        List<CardData> repCards = new List<CardData>();
        repCards.AddRange(activeHand.cardsInHand);
        for (int i = 0; i < repCards.Count; i++)
        {
            CardVisual _Card = repCards[i].cardView;
            if (!_Card) continue;
            lastPositions.Add(_Card.originalPos);
            activeHand.RemoveFromHand(_Card, true);
            Blackboard.tableCardsParent.RemoveFromFormation(_Card);
        }
        while (gamePaused)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        for (int j = 0; j < repCards.Count; j++)
        {
            CardVisual _Card = repCards[j].cardView;
            float zAngle = repCards.Count % 2 == 1 ? 15 * (repCards.Count / 2) - 15 * j : 22.5f - 15 * j;
            float yPos = repCards.Count % 2 == 1 ? -2f * Mathf.Pow(j - repCards.Count / 2, 2) + 0.3f: -3f * (0.5f * Mathf.Pow(j, 2) - 1.5f * j);
            Vector3 position = Vector2.zero + Vector2.up * yPos;
            position.x += Screen.height / Screen.width * 12f * (j - repCards.Count / 2 + (repCards.Count % 2 == 1 ? 0 : 0.5f));
            Vector3 cardPosition = Camera.main.ViewportToWorldPoint(position);
            iTween.MoveTo(_Card.gameObject, iTween.Hash("position", cardPosition, "time", 0.7f, "islocal", true, "easetype", iTween.EaseType.easeOutBounce));
            iTween.ScaleTo(_Card.gameObject, iTween.Hash("scale", 2 * Vector3.one, "time", 0.7f));
            iTween.RotateTo(_Card.gameObject, iTween.Hash("rotation", zAngle * Vector3.forward, "time", 0.7f));
            _Card.transform.SetAsLastSibling();
        }
        while (gamePaused)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < repCards.Count; i++)
        {
            CardVisual _Card = repCards[repCards.Count - 1 - i].cardView;
            iTween.MoveTo(_Card.gameObject, iTween.Hash("position", _Card.transform.position + 10 * Vector3.right, "time", 1.5f));
            Blackboard.sfxPlayer.PlaySFX(SFXType.CardFlyAway);
            Blackboard.deckBuilder.RemoveCardFromDeck(_Card.attachedCard);
            Destroy(_Card.gameObject, 1.5f);
            activeHand.cardSlots[i].transform.SetParent(Blackboard.cm.transform);
            yield return new WaitForSeconds(0.2f);
        }
        while (gamePaused)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        Blackboard.deckBuilder.DealNewCards(lastPositions.Count);
    }

    public void HandleTurnEnd(bool timeUp = false)
    {
        isMainPlayerTurn = !isMainPlayerTurn;
        if (timerOn) Blackboard.cm.ToggleNameActive(isMainPlayerTurn);
        if (timeUp)
        {
            Blackboard.cm.ShowTimeUpMessage();
            ReturnCards();
        }
        if (!isGameOver)
        {
            if (isMainPlayerTurn)
            {
                activeHand = player1Hand;
                if (timeUp)
                {
                    Invoke("StartSearchForSets", 3f);
                }
                else
                {
                    StartSearchForSets();
                }
                player1Hand.submitted = false;
            }
            else
            {
                if (hotseatMode)
                {
                    activeHand = player2Hand;
                    if (timeUp)
                    {
                        Invoke("StartSearchForSets", 3f);
                    }
                    else
                    {
                        StartSearchForSets();
                    }
                    player2Hand.submitted = false;
                }
                else
                {
                    activeHand = opponentHand;
                    opponentHand.submitted = false;
                    float timeToActivateOpponent = timeUp ? 3f : 1f;
                    Invoke("ActivateOpponent", timeToActivateOpponent);
                }
            }
            Blackboard.cm.ChangeBackgroundImage(isMainPlayerTurn);
            if (timerOn)
            {
                Blackboard.timer.ResetTimer();
                Blackboard.cm.UpdateTimerText();
            }
        }
        else
        {
            HandleGameOver();
        }
    }

    private void ReturnCards()
    {
        int cardsCount = activeHand.cardsInHand.Count;
        for (int iter = 0; iter < cardsCount; iter++)
        {
            CardVisual _Card = activeHand.cardsInHand[0].cardView;
            if (!_Card) continue;
            activeHand.RemoveFromHand(_Card, true);
        }
    }

    public void LowerScore()
    {
        if (isMainPlayerTurn)
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

    public void ResetSubmitButtonSprite()
    {
        Button buttonToReset = isMainPlayerTurn ? player1Hand.submitButtonWrong : player2Hand.submitButtonWrong;
        buttonToReset.GetComponent<Image>().sprite = buttonDefault;
    }

    public void SetSubmitButtonsTrue(Button defaultButton, Button wrongButton, bool value)
    {
        if (wrongButton && defaultButton)
        {
            wrongButton.gameObject.SetActive(!value);
            defaultButton.gameObject.SetActive(value);
        }
    }

    public void ReportBadSetSubmission()
    {
        Blackboard.cm.ShowBadSetMessage();
    }

    void ActivateOpponent()
    {
        if (timerOn)
        {
            Blackboard.timer.Run();
        }
        Blackboard.opponent.StartTurn();
    }

    void HandleGameOver()
    {
        if (playerScore <= 0)
        {
            Blackboard.sfxPlayer.PlaySFX(SFXType.Win);
            Blackboard.cm.ShowWinScreen();
        }
        else if (opponentScore <= 0)
        {
            Blackboard.cm.ShowLoseScreen();

            if (!hotseatMode)
            {
                Blackboard.sfxPlayer.PlaySFX(SFXType.Lose);
                return;
            }
            Blackboard.sfxPlayer.PlaySFX(SFXType.Win);
        }
    }

    #endregion
    

    #region Timer Toggling
    
    public void SetTimerOnOff()
    {
        Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
        timerOn = !timerOn;
        foreach (Toggle toggle in timerToggles)
        {
            toggle.gameObject.SetActive(timerOn);
        }
        if (timerOn)
        {
            SetTimerValue(20);
            Blackboard.timer.ResetTimer();
            Blackboard.cm.ToggleNameActive(isMainPlayerTurn);
            timerToggles[0].isOn = true;
        }
        else
        {
            Blackboard.cm.ResetNameActive();
        }
        Blackboard.cm.ToggleTimerIcon(timerOn);
        if (!timerOn) Blackboard.timer.Stop();
    }

    public void SetTimerValue(float time)
    {
        if (!timerOn) return;
        Blackboard.timer.SetTurnTime(time);
        Blackboard.timer.ResetTimer();
        Blackboard.timer.Run();
    }

    public void SetCurrentToggle(Toggle toggle)
    {
        if (!timerOn || !toggle.isOn) return;
        toggle.interactable = false;
        Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
        if (currentToggle)
        {
            currentToggle.isOn = toggle == currentToggle;
        }
        currentToggle = toggle;
        for (int i = 0; i < timerToggles.Length; i++)
        {
            if (timerToggles[i] == currentToggle) continue;
            timerToggles[i].isOn = false;
            timerToggles[i].interactable = true;
        }
        float time = float.Parse(currentToggle.name);
        SetTimerValue(time);
    }

    #endregion

    #region Combination Checking

    void StartSearchForSets()
    {
        StartCoroutine(CheckForSets());
    }

    IEnumerator CheckForSets()
    {
        activeHand.initCheck = true;
        cardsOnScreen = Blackboard.tableCardsParent.GetCardsOnTable();
        bool isThereValidSet = false;
        int cardIndex = 0, firstIndex = 0;
        do
        {
            activeHand.ClearHand();
            List<CardVisual> potentialCards = new List<CardVisual>();
            if (cardIndex >= cardsOnScreen.Count)
            {
                yield return StartCoroutine(Reshuffle());
                cardIndex = 0;
            }
            CardVisual inspectedCard = cardsOnScreen[cardIndex];
            if (inspectedCard.attachedCard.jokerCard)
            {
                cardIndex++;
                continue;
            }
            cardIndex++;
            potentialCards = FindMatchesIn(inspectedCard.attachedCard, cardsOnScreen);
            activeHand.AddToHand(inspectedCard);
            firstIndex = 0;
            do
            {
                if (firstIndex >= potentialCards.Count) break;
                if (player1Hand.cardsInHand.Count > 1)
                {
                    activeHand.RemoveFromHand(inspectedCard, default, true);
                }
                inspectedCard = potentialCards[firstIndex];
                if (inspectedCard.attachedCard.jokerCard)
                {
                    firstIndex++;
                    continue;
                }
                activeHand.AddToHand(inspectedCard);
                string[] neededCard = CalculateNeededCard();
                SearchForCardWithID(neededCard, out isThereValidSet);
                firstIndex++;
            } while (!isThereValidSet);
        } while (!isThereValidSet);
        activeHand.ClearHand();
        activeHand.initCheck = false;
        Debug.Log("Done checking, combination found!");
        allowCardPickUp = true;
        if (timerOn)
            Blackboard.timer.Run();
    }

    public string[] CalculateNeededCard(GroupType groupType = GroupType.None)
    {
        string[] cardParams = new string[3];
        switch (groupType)
        {
            case GroupType.None:
                cardParams[0] = CalculateNeededColor(activeHand.cardsInHand[0].color != activeHand.cardsInHand[1].color);
                cardParams[1] = CalculateNeededShape(activeHand.cardsInHand[0].shape != activeHand.cardsInHand[1].shape);
                cardParams[2] = CalculateNeededNumber(activeHand.cardsInHand[0].number != activeHand.cardsInHand[1].number);
                break;
            case GroupType.NumberColor:
                cardParams[0] = CalculateNeededColor();
                cardParams[1] = CalculateNeededShape(true);
                cardParams[2] = CalculateNeededNumber();
                break;
            case GroupType.ShapeNumber:
                cardParams[0] = CalculateNeededColor(true);
                cardParams[1] = CalculateNeededShape();
                cardParams[2] = CalculateNeededNumber();
                break;
            case GroupType.ShapeColorCons:
                cardParams[0] = CalculateNeededColor();
                cardParams[1] = CalculateNeededShape();
                cardParams[2] = CalculateNeededNumber(true);
                break;
            case GroupType.ColorCons:
                cardParams[0] = CalculateNeededColor();
                cardParams[1] = CalculateNeededShape(true);
                cardParams[2] = CalculateNeededNumber(true);
                break;
            case GroupType.ShapeCons:
                cardParams[0] = CalculateNeededColor(true);
                cardParams[1] = CalculateNeededShape();
                cardParams[2] = CalculateNeededNumber(true);
                break;
        }
        return cardParams;
    }

    string CalculateNeededShape(bool different = false)
    {
        string shapeStr = "";
        if (!different)
        {
            shapeStr = ConvertToString(activeHand.cardsInHand[0].shape);
        }
        else
        {
            int index = 0;
            bool match = true;
            bool uniqueShapeFound = true;
            CardShape _shape = CardShape.Circle;
            for (; index < 4; index++)
            {
                uniqueShapeFound = true;
                _shape = (CardShape)index;
                for (int i = 0; i < activeHand.cardsInHand.Count; i++)
                {
                    match = activeHand.cardsInHand[i].shape == _shape;
                    uniqueShapeFound = uniqueShapeFound && !match;
                }
                if (uniqueShapeFound) shapeStr += ConvertToString(_shape);
            }
        }
        return shapeStr;
    }

    string CalculateNeededColor(bool different = false)
    {
        string colorStr = "";
        if (!different)
        {
            colorStr = ConvertToString(activeHand.cardsInHand[0].color);
        }
        else
        {
            int index = 0;
            bool match = true;
            bool uniqueColorFound = true;
            CardColor _color = CardColor.Blue;
            for (; index < 4; index++)
            {
                uniqueColorFound = true;
                _color = (CardColor)index;
                for (int i = 0; i < activeHand.cardsInHand.Count; i++)
                {
                    match = activeHand.cardsInHand[i].color == _color;
                    uniqueColorFound = uniqueColorFound && !match;
                }
                if (uniqueColorFound)
                {
                    colorStr += ConvertToString(_color);
                }
            }
        }
        return colorStr;
    }

    string CalculateNeededNumber(bool different = false)
    {
        string numStr = "";
        if (!different)
        {
            numStr = activeHand.cardsInHand[0].number.ToString();
        }
        else
        {
            int cardIndex = 0;
            if (FindSmallestNumber(out cardIndex) > 1)
            {
                numStr += (activeHand.cardsInHand[cardIndex].number - 1).ToString();
            }
            if (FindGreatestNumber(out cardIndex) < 5)
            {
                numStr += (activeHand.cardsInHand[cardIndex].number + 1).ToString();
            }
        }
        return numStr;
    }

    void SearchForCardWithID(string[] neededCard, out bool isMySetValid)
    {
        isMySetValid = false;
        string neededJoker = neededCard[0] + "J";
        for (int i = 0; i < cardsOnScreen.Count; i++)
        {
            if (ThisCardIs(cardsOnScreen[i], neededCard) || (!activeHand.ContainsJoker() && ThisCardIs(cardsOnScreen[i], neededJoker)))
            {
                activeHand.AddToHand(cardsOnScreen[i]);
                isMySetValid = activeHand.isCombinationValid();
                if (!isMySetValid) activeHand.RemoveFromHand(cardsOnScreen[i], default, true);
                return;
            }
        }
    }

    private bool ThisCardIs(CardVisual cardVisual, string[] neededCard)
    {
        string cardId = cardVisual.attachedCard.id;
        return neededCard[0].Contains(cardId[0].ToString()) && neededCard[1].Contains(cardId[1].ToString()) && neededCard[2].Contains(cardId[2].ToString());
    }

    private bool ThisCardIs(CardVisual cardVisual, string neededCard)
    {
        string cardId = cardVisual.attachedCard.id;
        return neededCard.Contains(cardId[0].ToString()) && neededCard[neededCard.Length - 1] == cardId[1];
    }

    public List<CardVisual> FindMatchesIn(CardData inspectedCard, ICollection collection)
    {
        List<CardVisual> potentialCards = new List<CardVisual>();
        foreach (CardVisual comparedCard in collection)
        {
            if (comparedCard.attachedCard == inspectedCard) continue;
            int matches = 0;
            //Whenever there is a match in an attribute, the counter goes up by 1
            matches += Compare(comparedCard.attachedCard.color, inspectedCard.color) +
                Compare(comparedCard.attachedCard.shape, inspectedCard.shape);
            if ((matches >= 1 && Compare(comparedCard.attachedCard.number, inspectedCard.number) == 1) || IsThereJoker(comparedCard.attachedCard, inspectedCard)) potentialCards.Add(comparedCard);
        }
        return potentialCards;
    }

    int Compare(int a, int b)
    {
        return Mathf.Abs(a - b) <= 1 ? 1 : 0;
    }

    int Compare(object a, object b)
    {
        return a.Equals(b) ? 1 : 0;
    }

    bool IsThereJoker(CardData a, CardData b)
    {
        return a.jokerCard || b.jokerCard;
    }

    string ConvertToString(CardShape shape)
    {
        string str = "";
        switch (shape)
        {
            case CardShape.Joker:
                str = "J";
                break;
            case CardShape.Star:
                str = "S";
                break;
            case CardShape.Rectangle:
                str = "R";
                break;
            case CardShape.Triangle:
                str = "T";
                break;
            case CardShape.Circle:
                str = "C";
                break;
        }
        return str;
    }

    string ConvertToString(CardColor color)
    {
        string str = "";
        switch (color)
        {
            case CardColor.Yellow:
                str = "Y";
                break;
            case CardColor.Blue:
                str = "B";
                break;
            case CardColor.Red:
                str = "R";
                break;
            case CardColor.Green:
                str = "G";
                break;
        }
        return str;
    }

    int FindSmallestNumber(out int index)
    {
        int number = 6;
        index = 0;
        for (int i = 0; i < player1Hand.cardsInHand.Count; i++)
        {
            CardData comparedCard = player1Hand.cardsInHand[i];
            if (comparedCard.number < number)
            {
                number = comparedCard.number;
                index = i;
            }
        }
        return number;
    }

    int FindGreatestNumber(out int index)
    {
        int number = 0;
        index = 0;
        for (int i = 0; i < player1Hand.cardsInHand.Count; i++)
        {
            CardData comparedCard = player1Hand.cardsInHand[i];
            if (comparedCard.number > number)
            {
                number = comparedCard.number;
                index = i;
            }
        }
        return number;
    }

    #endregion

    #region Card Shuffle Animation

    public void StartReshuffleAnimation()
    {
        Blackboard.cm.ShowReshuffling();
    }
    
    public void StartReshuffle()
    {
        StartCoroutine(Reshuffle());
    }

    IEnumerator Reshuffle()
    {
        allowCardPickUp = false;
        cardCount = 0;
        Blackboard.deckBuilder.ReshuffleDeck();
        List<CardVisual> cardsOnScreen = Blackboard.tableCardsParent.cardsOnTable;
        for (int i = 0; i < cardsOnScreen.Count; i++)
        {
            iTween.RotateBy(cardsOnScreen[i].gameObject, iTween.Hash("y", 0.25, "time", 0.25f, "easetype", iTween.EaseType.linear, 
                "oncompletetarget", gameObject, "oncomplete", "ShowCardBack", "oncompleteparams", cardsOnScreen[i]));
            yield return null;
        }
        while (cardCount < cardsOnScreen.Count)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        cardCount = 0;
        for (int i = 0; i < cardsOnScreen.Count; i++)
        {
            iTween.RotateBy(cardsOnScreen[i].gameObject, iTween.Hash("y", 0.25, "time", 0.25f, "easetype", iTween.EaseType.linear,
                "oncompletetarget", gameObject, "oncomplete", "EndAnimation", "oncompleteparams", cardsOnScreen[i]));
            yield return null;
        }
        while (cardCount < cardsOnScreen.Count)
        {
            yield return null;
        }
        allowCardPickUp = true;
        Debug.Log("Done");
    }

    public void ShowCardBack(CardVisual _Card)
    {
        _Card.SetSprite(Blackboard.cm.backgroundSettings.cardBack);
        iTween.RotateBy(_Card.gameObject, iTween.Hash("y", -0.25, "time", 0.25f, "easetype", iTween.EaseType.linear,
            "oncompletetarget", gameObject, "oncomplete", "IncrementCardCount"));
    }

    public void IncrementCardCount()
    {
        cardCount++;
    }

    public void EndAnimation(CardVisual _Card)
    {
        Blackboard.deckBuilder.AttachCardToView(_Card);
        iTween.RotateBy(_Card.gameObject, iTween.Hash("y", -0.25, "time", 0.25f, "easetype", iTween.EaseType.linear,
            "oncompletetarget", gameObject, "oncomplete", "IncrementCardCount"));
    }

    #endregion
}
