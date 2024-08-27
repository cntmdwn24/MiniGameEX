using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Game2 : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject PausePanel;
    public GameObject StartPanel;
    public GameObject ColorButtonsPanel;
    public GameObject GameOverPanel;

    [Header("Score UI")]
    public TMP_Text scoreText;
    public TMP_Text maxCorrectText;

    [Header("Game Elements")]
    public GameObject objectToDestroy;
    public Image Heart1;
    public Image Heart2;
    public Image Heart3;
    public Image marbleImage;

    [Header("Color Buttons")]
    public Button[] colorButtons;

    [Header("Game Settings")]
    public Color[] colors = new Color[9];

    private List<int> sequence = new List<int>();
    private int score = 0;
    private int hearts = 3;
    private bool isPaused = true;
    private bool isPlayerTurn = false;
    private int currentStep = 0;

    private int maxCorrectStreak = 0;
    private int currentCorrectStreak = 0;
    private int currentTurnCorrectCount = 0;

    private GameManager gameManager;

    private void Awake()
    {
        // "Massege" 태그가 붙은 모든 오브젝트를 찾아서 배열에 저장
        GameObject[] massegeObjects = GameObject.FindGameObjectsWithTag("Massege");

        // 배열에 있는 모든 오브젝트를 삭제
        foreach (GameObject obj in massegeObjects)
        {
            Destroy(obj);
        }
    }

    private void Start()
    {
        UpdateHearts();
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("GameManager instance is not found.");
        }
        UpdateScoreUI();
        UpdateMaxCorrectUI();
    }

    #region Game Flow
    public void StartGame()
    {
        Debug.Log("StartGame called");
        StartPanel.SetActive(false);
        ResumeGame();
        StartCoroutine(GameTurn());
    }

    public void Pause()
    {
        Debug.Log("Pause called");
        PausePanel.SetActive(true);
        PauseGame();
    }

    public void ResumeGame()
    {
        Debug.Log("ResumeGame called");
        PausePanel.SetActive(false);
        UnpauseGame();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void UnpauseGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void LeaveGame()
    {
        Debug.Log("LeaveGame called");
        ActivateCanvasObjects();
        DestroyObjectToDestroy();
        UnpauseGame();
        if (gameManager != null)
        {
            gameManager.EndMiniGame(score);
        }
    }

    private void ActivateCanvasObjects()
    {
        Canvas[] allCanvasObjects = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvasObject in allCanvasObjects)
        {
            canvasObject.gameObject.SetActive(true);
        }
    }

    private void DestroyObjectToDestroy()
    {
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
            Debug.Log("Object to destroy destroyed");
        }
    }
    #endregion

    #region Game Logic
    private IEnumerator GameTurn()
    {
        Debug.Log("GameTurn started");
        isPlayerTurn = false;
        currentStep = 0;
        currentTurnCorrectCount = 0;

        sequence.Add(Random.Range(0, colors.Length));

        for (int i = 0; i < sequence.Count; i++)
        {
            marbleImage.color = colors[sequence[i]];
            yield return new WaitForSeconds(1f);
            marbleImage.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }

        isPlayerTurn = true;
        ColorButtonsPanel.SetActive(true);
        Debug.Log("Player's turn started");
    }

    public void OnColorButtonClick(int colorIndex)
    {
        if (!isPlayerTurn) return;

        if (colorIndex == sequence[currentStep])
        {
            currentStep++;
            currentTurnCorrectCount++;

            if (currentStep >= sequence.Count)
            {
                score += 10 * sequence.Count;
                UpdateScoreUI();

                if (currentTurnCorrectCount > maxCorrectStreak)
                {
                    maxCorrectStreak = currentTurnCorrectCount;
                    UpdateMaxCorrectUI();
                }

                StartCoroutine(GameTurn());
                ColorButtonsPanel.SetActive(false);
            }
        }
        else
        {
            Debug.Log($"Incorrect color selected: {colorIndex}");
            DecreaseHearts();
            ColorButtonsPanel.SetActive(false);
            if (hearts <= 0)
            {
                GameOver();
                GameOverPanel.SetActive(true);
            }
            else
            {
                StartCoroutine(GameTurn());
            }
        }
    }
    #endregion

    #region Score Management
    public void IncreaseScore(int amount)
    {
        if (amount > 0)
        {
            score += amount;
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    private void DecreaseHearts()
    {
        if (hearts > 0)
        {
            hearts--;
            UpdateHearts();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over Called");
        if (gameManager != null)
        {
            gameManager.EndMiniGame(score);
        }
        GameOverPanel.SetActive(true);
    }
    #endregion

    #region Heart Management
    private void UpdateHearts()
    {
        Heart1.gameObject.SetActive(hearts > 0);
        Heart2.gameObject.SetActive(hearts > 1);
        Heart3.gameObject.SetActive(hearts > 2);
    }
    #endregion

    #region Max Correct Management
    private void UpdateMaxCorrectUI()
    {
        if (maxCorrectText != null)
        {
            maxCorrectText.text = "최대로 맞춘 개수: " + maxCorrectStreak.ToString();
        }
    }
    #endregion
}