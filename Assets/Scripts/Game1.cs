using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Game1 : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject PausePanel;
    public GameObject StartPanel;
    public GameObject GameOverPanel;
    public GameObject JumpButton;

    [Header("Score UI")]
    public TMP_Text scoreText;
    public GameObject objectToDestroy;

    [Header("Game Elements")]
    public GameObject obstaclePrefab;

    public Image Heart1;
    public Image Heart2;
    public Image Heart3;

    private int score = 0;
    public int hearts = 3;
    private bool isPaused = false;
    private bool gameStarted = false; // 게임이 시작되었는지 확인하는 플래그
    private GameManager gameManager;

    private float scoreIncreaseInterval = 1f; // 점수 증가 간격
    private int scoreIncreaseAmount = 1; // 점수 증가량
    private Coroutine scoreIncreaseCoroutine;
    private Coroutine obstacleSpawnCoroutine;

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
        PauseGame();
        UpdateHearts();
        UpdateScoreUI();
        StartPanel.SetActive(true); // 시작 패널 표시
        GameOverPanel.SetActive(false); // 게임 오버 패널 숨기기
        PausePanel.SetActive(false); // 일시정지 패널 숨기기

        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("GameManager instance is not found.");
        }
    }

    public void StartGame()
    {
        score = 0;
        hearts = 3;
        UpdateScoreUI();
        UpdateHearts();
        StartPanel.SetActive(false);
        GameOverPanel.SetActive(false);
        ResumeGame();
        gameStarted = true;
        JumpButton.SetActive(true);

        // 점수 증가 코루틴 시작
        if (scoreIncreaseCoroutine != null)
        {
            StopCoroutine(scoreIncreaseCoroutine);
        }
        scoreIncreaseCoroutine = StartCoroutine(IncrementScoreOverTime());

        // 장애물 스폰 코루틴 시작
        if (obstacleSpawnCoroutine != null)
        {
            StopCoroutine(obstacleSpawnCoroutine);
        }
        obstacleSpawnCoroutine = StartCoroutine(SpawnObstacles());
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        PauseGame();
    }

    public void ResumeGame()
    {
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
        // 게임 오버 패널을 활성화하고 게임 매니저에 점수를 전달
        GameOver();
        GameOverPanel.SetActive(true);
        ActivateCanvasObjects();
        DestroyObjectToDestroy();
        if (gameManager != null)
        {
            gameManager.EndMiniGame(score);
        }

        // 일시정지 해제
        UnpauseGame();
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

    private IEnumerator SpawnObstacles()
    {
        while (gameStarted)
        {
            // 랜덤 타이밍 설정
            float delay = Random.Range(1f, 2f);
            yield return new WaitForSeconds(delay);

            // 장애물 스폰 위치 및 회전 각도 설정
            Vector2 spawnPosition = new Vector2(4f, -1.5f);
            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)); // 랜덤 회전

            GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, spawnRotation);

            Rigidbody2D obstacleRb = obstacle.GetComponent<Rigidbody2D>();

            if (obstacleRb != null)
            {
                // 랜덤한 던지는 힘 설정
                float speed = Random.Range(6f, 8f); // 속도 범위
                float angle = Random.Range(-30f, 30f); // 각도 범위

                Vector2 direction = new Vector2(-1f, Mathf.Tan(Mathf.Deg2Rad * angle)); // 방향 벡터
                direction.Normalize(); // 벡터 정규화
                Vector2 force = direction * speed; // 최종 힘 계산

                obstacleRb.velocity = force;

                float torque = Random.Range(-10f, 10f);
                obstacleRb.AddTorque(torque);
            }

            // 장애물이 삭제될 시간 설정
            Destroy(obstacle, 10f);
        }
    }

    public void IncreaseScore(int amount)
    {
        if (amount > 0)
        {
            score += amount;
            UpdateScoreUI();
        }
    }

    private IEnumerator IncrementScoreOverTime()
    {
        while (gameStarted)
        {
            yield return new WaitForSeconds(scoreIncreaseInterval);
            IncreaseScore(scoreIncreaseAmount);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void DecreaseHearts()
    {
        if (hearts > 0)
        {
            hearts--;
            UpdateHearts();
            if (hearts <= 0)
            {
                GameOver();
                GameOverPanel.SetActive(true);
            }
        }
    }

    private void UpdateHearts()
    {
        Heart1.gameObject.SetActive(hearts > 0);
        Heart2.gameObject.SetActive(hearts > 1);
        Heart3.gameObject.SetActive(hearts > 2);
    }

    private void GameOver()
    {
        PauseGame();
        // 점수 증가 코루틴 중지
        if (scoreIncreaseCoroutine != null)
        {
            StopCoroutine(scoreIncreaseCoroutine);
        }

        // 장애물 스폰 코루틴 중지
        if (obstacleSpawnCoroutine != null)
        {
            StopCoroutine(obstacleSpawnCoroutine);
        }

        // 게임 오버 패널 활성화
        GameOverPanel.SetActive(true);
        Debug.Log("Game Over. Final Score: " + score);
        gameStarted = false; // 게임 종료 시 게임 시작 플래그를 false로 설정
    }
}