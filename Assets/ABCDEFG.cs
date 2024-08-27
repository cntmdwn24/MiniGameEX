using UnityEngine;

public class ABCDEFG : MonoBehaviour
{
    public new Collider2D collider;

    public LayerMask targetLayerMask;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null && ((1 << collision.collider.gameObject.layer) & targetLayerMask) != 0)
        {
            Debug.Log("장애물과 충돌했습니다!");
            Destroy(gameObject);
        }
    }
    
}