using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("-----------[ Core ]")]
    public int score;
    public int maxLevel = 2;
    public bool isGameover;
    public bool isReset;

    [Header("-----------[ Object Pooling ]")]
    public GameObject donglePrefab;
    public GameObject effectPrefab;
    public Transform dongleGroup;
    public Transform effectGroup;
    public List<Dongle> donglePool;
    public List<ParticleSystem> effectPool;

    [Range(1,30)]
    public int poolSize;
    public int poolCursor;
    public Dongle currentDongle;
    public Dongle nextDongle;

    [Header("-----------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClips;
    public enum SFX { LevelUp, Next, Attach, Button, Over };
    int sfxCursor;

    [Header("-----------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI subScoreText;
    public NextDongleImage nextDongleImage;

    [Header("-----------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }

        if (!PlayerPrefs.HasKey("BestScore"))
            PlayerPrefs.SetInt("BestScore", 0);

        bestScoreText.text = PlayerPrefs.GetInt("BestScore").ToString();
    }
    public void GameStart()
    {
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        bestScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        nextDongleImage.gameObject.SetActive(true);

        bgmPlayer.Play();
        SFXPlay(SFX.Button);
        Invoke("NextDongle", 1.5f);
    }
    void LateUpdate()
    {
        scoreText.text = "점수 : " + score;
    }
    Dongle MakeDongle()
    {
        GameObject effectObject = Instantiate(effectPrefab, effectGroup);
        effectObject.name = "Effect" + effectPool.Count;
        ParticleSystem effect = effectObject.GetComponent<ParticleSystem>();
        effectPool.Add(effect);

        GameObject dongleObject = Instantiate(donglePrefab, dongleGroup);
        dongleObject.name = "Dongle" + donglePool.Count;
        Dongle dongle = dongleObject.GetComponent<Dongle>();
        dongle.manager = this;
        dongle.effect = effect;
        donglePool.Add(dongle);

        return dongle;
    }
    Dongle GetDongle()
    {
        for(int i = 0;i < donglePool.Count;i++)
        {
            poolCursor = (poolCursor + 1) % donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf)
            {
                return donglePool[poolCursor];
            }
        }
        return MakeDongle();
    }
    void NextDongle()
    {
        if (isGameover)
            return;

        if (nextDongle == null)
        {
            currentDongle = GetDongle();
            currentDongle.level = GetRandomLevel();
        }
        else
        { 
            currentDongle = nextDongle;
        }

        currentDongle.gameObject.SetActive(true);

        nextDongle = GetDongle();
        nextDongle.level = GetRandomLevel();
        nextDongle.gameObject.SetActive(false);
        nextDongleImage.SetImage(nextDongle.level);

        SFXPlay(SFX.Next);

        StartCoroutine("WaitNext");
    }
    public int GetRandomLevel()
    {
        int level = 0;
        int LimitLevel = 0;
        if (maxLevel <= 4)
            LimitLevel = maxLevel;
        else
            LimitLevel = 5;

        level = UnityEngine.Random.Range(0, LimitLevel);
        return level;
    }
    IEnumerator WaitNext()
    {
        while (currentDongle != null) 
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        NextDongle();
    }
    public void TouchDown()
    {
        if (currentDongle == null)
            return;

        currentDongle.Drag();
    }
    public void TouchUp()
    {
        if (currentDongle == null)
            return;

        currentDongle.Drop();
        currentDongle = null;
    }
    public void GameOver()
    {
        if (isGameover)
            return;

        isGameover = true;

        StartCoroutine("GameoverRoutine");
    }
    private int CompareByYDescending(Dongle a, Dongle b)//내림차순
    {
        return b.transform.position.y.CompareTo(a.transform.position.y);
    }
    IEnumerator GameoverRoutine()
    {
        Dongle[] dongles = FindObjectsOfType<Dongle>();
        Array.Sort(dongles, CompareByYDescending);

        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false;
        }
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        int bestScore = Mathf.Max(score, PlayerPrefs.GetInt("BestScore"));
        PlayerPrefs.SetInt("BestScore", bestScore);

        subScoreText.text = scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SFXPlay(SFX.Over);
    }
    public void Reset()
    {
        if (isReset)
            return;

        isReset = true;
        SFXPlay(SFX.Button);
        StartCoroutine("ResetCoroutine");
    }
    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(0);
    }
    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }
    public void SFXPlay(SFX type)
    {
        switch(type)
        {
            case SFX.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClips[UnityEngine.Random.Range(0, 3)]; 
                break;
            case SFX.Next:
                sfxPlayer[sfxCursor].clip = sfxClips[3];
                break;
            case SFX.Attach:
                sfxPlayer[sfxCursor].clip = sfxClips[4];
                break;
            case SFX.Button:
                sfxPlayer[sfxCursor].clip = sfxClips[5];
                break;
            case SFX.Over:
                sfxPlayer[sfxCursor].clip = sfxClips[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
        
    }
}
