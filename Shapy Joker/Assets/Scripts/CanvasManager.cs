using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    public static List<Sprite> visibleSprites;
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite playerTurnBG;
    [SerializeField] Sprite opponentTurnBG;
    [SerializeField] Image playerDeckImage;
    [SerializeField] Image opponentDeckImage;
    [SerializeField] Sprite[] deckLightImages;
    [SerializeField] Sprite[] deckDarkImages;
    [SerializeField] Image optionsScreen;
    [SerializeField] Button optionsScreenButton;
    [SerializeField] Image volumeImage;
    [SerializeField] Sprite volumeLight;
    [SerializeField] Sprite volumeDark;
    [SerializeField] Image timerImage;
    [SerializeField] Sprite timerLight;
    [SerializeField] Sprite timerDark;
    [SerializeField] TextMeshProUGUI playerTimerText;
    [SerializeField] TextMeshProUGUI opponentTimerText;
    [SerializeField] Image timeUpImage;
    private bool startedFlashing = false;

    private void Awake()
    {
        Blackboard.cm = this;
        visibleSprites = Resources.LoadAll("Cards", typeof(Sprite)).Cast<Sprite>().ToList();
    }

    public void SetAudioImage(float volume)
    {
        volumeImage.sprite = volume <= 0 ? volumeDark : volumeLight;
    }

    public void ChangeBackgroundImage(bool isPlayerTurn)
    {
        backgroundImage.sprite = isPlayerTurn ? playerTurnBG : opponentTurnBG;
        if (isPlayerTurn)
        {
            if (Blackboard.gm.playerScore / 10 > 1)
            {
                playerDeckImage.sprite = deckLightImages[Blackboard.gm.playerScore / 10 - 1];
            }
            if (Blackboard.gm.opponentScore / 10 > 1)
            {
                opponentDeckImage.sprite = deckDarkImages[Blackboard.gm.opponentScore / 10 - 1];
            }
        }
        else
        {
            if (Blackboard.gm.playerScore / 10 > 1)
            {
                playerDeckImage.sprite = deckDarkImages[Blackboard.gm.playerScore / 10 - 1];
            }
            if (Blackboard.gm.opponentScore / 10 > 1)
            {
                opponentDeckImage.sprite = deckLightImages[Blackboard.gm.opponentScore / 10 - 1];
            }
        }
    }

    public void SetOptionsScreenActive(bool value)
    {
        if (Blackboard.gm.isGameOver) return;
        optionsScreenButton.interactable = !value;
        Vector3 newPosition = value ? Vector3.zero : new Vector3(0, 9.24f, 0);
        iTween.MoveTo(optionsScreen.gameObject,  newPosition, 1.5f);
        GameManager.gamePaused = value;
    }

    public void ToggleTimerIcon(bool value)
    {
        timerImage.sprite = value ? timerLight : timerDark;
    }

    public void UpdateTimerText(float counter)
    {
        string timerText = (int)counter / 60 + ":" + (counter % 60 < 10 ? "0" + (counter % 60).ToString() : (counter % 60).ToString());
        TextMeshProUGUI activeTimer = Blackboard.gm.isPlayerTurn ? playerTimerText : opponentTimerText;
        if (counter <= 5)
        {
            if (!startedFlashing)
            {
                startedFlashing = true;
                iTween.FadeTo(activeTimer.gameObject, iTween.Hash("alpha", 0, "time", 0.25f, "looptype", iTween.LoopType.pingPong));
            }
            activeTimer.color = Color.red;
        }
        else activeTimer.color = Color.white;
        activeTimer.text = timerText;
    }

    public void ShowTimeUpMessage()
    {
        timeUpImage.gameObject.SetActive(true);
        iTween.FadeTo(timeUpImage.gameObject, iTween.Hash("alpha", 0, "time", 1f, "delay", 1f));
    }
}
