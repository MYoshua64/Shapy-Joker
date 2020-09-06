using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class CanvasManager : MonoBehaviour
{
    [System.Serializable]
    public class BGSettings
    {
        public Image backgroundImage;
        public Sprite playerTurnBG;
        public Sprite opponentTurnBG;
        public Sprite cardBack;
        public Image playerDeckImage;
        public Image opponentDeckImage;
        public Color deckColorDark;
        public Sprite[] deckImages;
        public TextMeshProUGUI playerDeckCount;
        public TextMeshProUGUI opponentDeckCount;
        
        public NotificationImage player1BadSetMessage;
        public NotificationImage player2BadSetMessage;

        public Image screenFaderImage;

        public Image reshufflingImage;
    }

    [System.Serializable]
    public class InstructionsSettings
    {
        public GameObject instructionPanel;
        public Sprite[] instructionsPages;
        public Image instructionImage;
        public Button nextArrowButton;
        public Button previousArrowButton;
        private BaseEventData e;

        public int currentPage { get; private set; } = 1;

        public void NextPage()
        {
            Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
            currentPage++;
            instructionImage.sprite = instructionsPages[currentPage - 1];
            nextArrowButton.interactable = currentPage < instructionsPages.Length;
            nextArrowButton.OnDeselect(e);
            previousArrowButton.interactable = true;
        }

        public void PreviousPage()
        {
            Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
            currentPage--;
            instructionImage.sprite = instructionsPages[currentPage - 1];
            previousArrowButton.interactable = currentPage > 1;
            previousArrowButton.OnDeselect(e);
            nextArrowButton.interactable = true;
        }
    }

    [System.Serializable]
    public class OptionsScreen
    {
        public Image optionsScreen;
        public Button optionsScreenButton;
        public Image volumeImage;
        public Sprite volumeLight;
        public Sprite volumeDark;
        [HideInInspector]
        public List<Image> optionImages;
        public Slider volumeSlider;
        [HideInInspector]
        public bool active;
    }

    [System.Serializable]
    public class TimerSettings
    {
        public Image timerImage;
        public Sprite timerLight;
        public Sprite timerDark;
        public Image playerName;
        public Image opponentName;
        public TextMeshProUGUI playerTimerText;
        public TextMeshProUGUI opponentTimerText;
        public Color normalTimerColor;
        public Color normalOutlineTimerColor;
        public Color redTintTimer;
        public Color redTintOutlineTimer;
        public Image timeUpImage;
        public GameObject timeUpPanel;
    }

    [System.Serializable]
    public class EndScreens
    {
        public RectTransform playerWinScreen;
        public Image winningPlayerName;
        public Sprite player1;
        public Sprite player2;
        public RectTransform playerLoseScreen;
    }

    public BGSettings backgroundSettings;
    public OptionsScreen optionsScreenSettings;
    public TimerSettings timerSettings;
    public InstructionsSettings instructionsSettings;
    public EndScreens endScreens;
    public static List<Sprite> visibleSprites;
    NotificationImage badSetMessage;
    TextMeshProUGUI activeTimer;
    SpriteRenderer nameToShow;
    private bool startedFlashing = false;

    private void Awake()
    {
        Blackboard.cm = this;
        visibleSprites = Resources.LoadAll("Cards", typeof(Sprite)).Cast<Sprite>().ToList();
    }

    private void Start()
    {
        Image[] buttons = optionsScreenSettings.optionsScreen.GetComponentsInChildren<Image>();
        optionsScreenSettings.optionImages = buttons.ToList();
        optionsScreenSettings.optionImages.RemoveAt(0);
        optionsScreenSettings.optionImages.RemoveAt(0);
        optionsScreenSettings.volumeSlider.value = AudioListener.volume;
    }

    public void SetAudioImage(float volume)
    {
        optionsScreenSettings.volumeImage.sprite = volume <= 0 ?optionsScreenSettings.volumeDark : optionsScreenSettings.volumeLight;
        optionsScreenSettings.volumeSlider.value = AudioListener.volume;
    }

    public void ChangeBackgroundImage(bool isPlayerTurn)
    {
        backgroundSettings.backgroundImage.sprite = isPlayerTurn ? backgroundSettings.playerTurnBG : backgroundSettings.opponentTurnBG;
        if (isPlayerTurn)
        {
            backgroundSettings.playerDeckImage.color = Color.white;
            backgroundSettings.opponentDeckImage.color = backgroundSettings.deckColorDark;
            if (Blackboard.gm.playerScore / 10 > 1)
            {
                backgroundSettings.playerDeckImage.sprite = backgroundSettings.deckImages[Blackboard.gm.playerScore / 10 - 1];
            }
            if (Blackboard.gm.opponentScore / 10 > 1)
            {
                backgroundSettings.opponentDeckImage.sprite = backgroundSettings.deckImages[Blackboard.gm.opponentScore / 10 - 1];
            }
        }
        else
        {
            backgroundSettings.playerDeckImage.color = backgroundSettings.deckColorDark;
            backgroundSettings.opponentDeckImage.color = Color.white;
            if (Blackboard.gm.playerScore / 10 > 1)
            {
                backgroundSettings.playerDeckImage.sprite = backgroundSettings.deckImages[Blackboard.gm.playerScore / 10 - 1];
            }
            if (Blackboard.gm.opponentScore / 10 > 1)
            {
                backgroundSettings.opponentDeckImage.sprite = backgroundSettings.deckImages[Blackboard.gm.opponentScore / 10 - 1];
            }
        }
    }

    public void ToggleOptionsMenu()
    {
        if (Blackboard.gm.isGameOver) return;
        optionsScreenSettings.active = !optionsScreenSettings.active;
        Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
        Blackboard.sfxPlayer.SetPlayerPause(!optionsScreenSettings.active);
        //On open options, give about 1.5seconds before buttons are interactable, on cole set interactable to false immediately
        if (optionsScreenSettings.active)
        {
            Invoke("EnableOptionsMenuButtons", 0.5f);
        }
        else
        {
            for (int i = 0; i < optionsScreenSettings.optionImages.Count; i++)
            {
                optionsScreenSettings.optionImages[i].raycastTarget = false;
            }
        }
        Vector3 position = optionsScreenSettings.active ? Vector3.zero : 980f * Vector3.up;
        iTween.MoveTo(optionsScreenSettings.optionsScreen.gameObject, iTween.Hash("position", position, "time", 1.5f, "islocal", true));
        GameManager.gamePaused = optionsScreenSettings.active;
    }

    void EnableOptionsMenuButtons()
    {
        optionsScreenSettings.optionImages[0].raycastTarget = true;
        for (int i = 0; i < optionsScreenSettings.optionImages.Count; i++)
        {
            optionsScreenSettings.optionImages[i].raycastTarget = true;
        }
    }

    public void ToggleTimerIcon(bool value)
    {
        timerSettings.timerImage.sprite = value ? timerSettings.timerLight : timerSettings.timerDark;
        if (value)
        {
            activeTimer = Blackboard.gm.isMainPlayerTurn ? timerSettings.playerTimerText : timerSettings.opponentTimerText;
        }
    }

    public void UpdateTimerText()
    {
        float counter = Mathf.Ceil(Blackboard.timer.timeCountDown);
        string timerText = (int)counter / 60 + ":" + (counter % 60 < 10 ? "0" + (counter % 60).ToString() : (counter % 60).ToString());
        activeTimer = Blackboard.gm.isMainPlayerTurn ? timerSettings.playerTimerText : timerSettings.opponentTimerText;
        if (counter <= 5)
        {
            if (!startedFlashing)
            {
                Blackboard.sfxPlayer.SetTimerSFXOn(true);
                activeTimer.faceColor = timerSettings.redTintTimer;
                activeTimer.outlineColor = timerSettings.redTintOutlineTimer;
                startedFlashing = true;
                activeTimer.GetComponent<TimerText>().StartBlinking();
            }
        }
        else
        {
            startedFlashing = false;
            activeTimer.faceColor = timerSettings.normalTimerColor;
            activeTimer.outlineColor = timerSettings.normalOutlineTimerColor;
        }
        activeTimer.text = timerText;
    }

    public void StopTimerTextBlinking()
    {
        //TODO Find why this is null
        activeTimer.GetComponent<TimerText>().StopBlinking();
        Blackboard.sfxPlayer.SetTimerSFXOn(false);
    }

    public void ToggleNameActive(bool isPlayerTurn)
    {
        timerSettings.playerName.gameObject.SetActive(!isPlayerTurn);
        timerSettings.opponentName.gameObject.SetActive(isPlayerTurn);
        if (isPlayerTurn) timerSettings.opponentTimerText.text = "";
        else timerSettings.playerTimerText.text = "";

    }

    public void ResetNameActive()
    {
        timerSettings.playerName.gameObject.SetActive(true);
        timerSettings.playerTimerText.text = "";
        timerSettings.opponentName.gameObject.SetActive(true);
        timerSettings.opponentTimerText.text = "";
    }

    public void ShowTimeUpMessage()
    {
        activeTimer.GetComponent<TimerText>().StopBlinking();
        timerSettings.timeUpImage.gameObject.SetActive(true);
        Invoke("HideTimeUpMessage", 168f / 60f);
    }

    void TurnOffTimeUpMessages()
    {
        timerSettings.timeUpPanel.SetActive(false);
    }

    void HideTimeUpMessage()
    {
        timerSettings.timeUpImage.gameObject.SetActive(false);
    }

    public void ShowBadSetMessage()
    {
        badSetMessage = Blackboard.gm.isMainPlayerTurn ? backgroundSettings.player1BadSetMessage : backgroundSettings.player2BadSetMessage;
        badSetMessage.Show();
    }

    public void SetInstructionScreenActive(bool value)
    {
        Blackboard.sfxPlayer.PlaySFX(SFXType.UIClick);
        instructionsSettings.instructionPanel.SetActive(value);
    }

    public void NextPage()
    {
        instructionsSettings.NextPage();
    }

    public void PreviousPage()
    {
        instructionsSettings.PreviousPage();
    }

    public void ShowReshuffling()
    {
        backgroundSettings.reshufflingImage.gameObject.SetActive(true);
        Invoke("DisableShufflingImage", 140f / 60f);
    }

    void DisableShufflingImage()
    {
        backgroundSettings.reshufflingImage.gameObject.SetActive(false);
        Blackboard.gm.StartReshuffle();
    }

    public void ShowWinScreen()
    {
        if (Blackboard.gm.hotseatMode)
        {
            endScreens.winningPlayerName.sprite = endScreens.player1;
        }
        endScreens.playerWinScreen.gameObject.SetActive(true);
    }

    public void ShowLoseScreen()
    {
        if (Blackboard.gm.hotseatMode)
        {
            endScreens.winningPlayerName.sprite = endScreens.player2;
            endScreens.playerWinScreen.gameObject.SetActive(true);
            return;
        }
        backgroundSettings.screenFaderImage.gameObject.SetActive(true);
        endScreens.playerLoseScreen.gameObject.SetActive(true);
    }
}
