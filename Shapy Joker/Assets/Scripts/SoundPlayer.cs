using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType
{
    CardTake,
    CardPlace,
    CardWhooshThree,
    CardJingleFour,
    CardSpecialFive,
    CardFlyAway,
    Win,
    Lose,
    WrongSet,
    UIClick,
    Opening
}

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] float volume = 0.4f;
    [SerializeField] List<AudioClip> cardTakeSFX;
    [SerializeField] List<AudioClip> cardPlaceSFX;
    [SerializeField] AudioClip cardsWhooshThree;
    [SerializeField] AudioClip cardsJingleFour;
    [SerializeField] AudioClip submitFiveSpecial;
    [SerializeField] AudioClip cardFlyAway;
    [SerializeField] AudioClip winSound;
    [SerializeField] AudioClip loseSound;
    [SerializeField] AudioClip wrongSetSFX;
    [SerializeField] AudioClip uiClickSFX;
    [SerializeField] AudioClip openingClip;

    AudioSource mySource;

    private void Awake()
    {
        if (ReferenceEquals(Blackboard.sfxPlayer, null))
        {
            DontDestroyOnLoad(gameObject);
            Blackboard.sfxPlayer = this;
            mySource = GetComponent<AudioSource>();
            PlaySFX(SFXType.Opening);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTimerSFXOn(bool value)
    {
        if (value)
        {
            mySource.Play();
        }
        else
        {
            mySource.Stop();
        }
    }

    public void PlayUIPress()
    {
        PlaySFX(SFXType.UIClick);
    }

    public void PlaySFX(SFXType type)
    {
        AudioClip playedClip = null;
        switch (type)
        {
            case SFXType.CardTake:
                playedClip = cardTakeSFX[Random.Range(0, cardTakeSFX.Count)];
                break;
            case SFXType.CardPlace:
                playedClip = cardPlaceSFX[Random.Range(0, cardPlaceSFX.Count)];
                break;
            case SFXType.CardWhooshThree:
                playedClip = cardsWhooshThree;
                break;
            case SFXType.CardJingleFour:
                playedClip = cardsJingleFour;
                break;
            case SFXType.CardSpecialFive:
                playedClip = submitFiveSpecial;
                break;
            case SFXType.CardFlyAway:
                playedClip = cardFlyAway;
                break;
            case SFXType.Win:
                playedClip = winSound;
                break;
            case SFXType.Lose:
                playedClip = loseSound;
                break;
            case SFXType.WrongSet:
                playedClip = wrongSetSFX;
                break;
            case SFXType.UIClick:
                playedClip = uiClickSFX;
                break;
            case SFXType.Opening:
                playedClip = openingClip;
                break;
        }
        AudioSource.PlayClipAtPoint(playedClip, Camera.main.transform.position, volume);
    }

    public void SetPlayerPause(bool value)
    {
        if (value) mySource.UnPause();
        else mySource.Pause();
    }
}
