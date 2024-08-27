using UnityEngine;

public class BackgroundLooper : MonoBehaviour
{
    public float backgroundSpeed = 0.1f; // 배경 이동 속도
    public float resetPositionX = -20f; // 배경이 리셋되는 X 위치
    public float startPositionX = 20f; // 배경이 시작되는 X 위치

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // 초기 위치 저장
    }

    void Update()
    {
        // 배경을 오른쪽에서 왼쪽으로 이동
        transform.Translate(Vector3.left * backgroundSpeed * Time.deltaTime);

        // 배경이 특정 위치에 도달하면 초기 위치로 되돌림
        if (transform.position.x <= resetPositionX)
        {
            transform.position = new Vector3(startPositionX, transform.position.y, transform.position.z);
        }
    }
}