using UnityEngine;

public class Player : MonoBehaviour
{
    bool isJump = false;
    int jumpCount = 0;
    float jumpPower = 7f;
    public new Rigidbody2D rigidbody2D;
    public new Collider2D collider2D;
    public Collider2D groundChecker;

    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    private bool hasCollided = false; // 충돌 여부를 체크하기 위한 플래그

    void Update()
    {
        GroundCheck();
    }

    public void Jump()
    {
        if (jumpCount < 1)
        {
            rigidbody2D.velocity = Vector2.up * jumpPower;
            jumpCount++;
        }
    }

    public void GroundCheck()
    {
        isJump = Physics2D.IsTouchingLayers(groundChecker, groundLayer);
        
        if (isJump)
        {
            jumpCount = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasCollided && ((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Debug.Log("장애물과 충돌했습니다: " + other.gameObject.name);
            hasCollided = true; // 충돌 처리 완료 플래그 설정
            Game1 game1 = FindObjectOfType<Game1>(); // Game1 스크립트 참조 가져오기
            if (game1 != null)
            {
                game1.DecreaseHearts(); // Game1의 DecreaseHearts 함수 호출
            }
        }
    }

    // 충돌이 끝났을 때 호출되는 메서드
    void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            hasCollided = false; // 충돌 상태 초기화
        }
    }
}