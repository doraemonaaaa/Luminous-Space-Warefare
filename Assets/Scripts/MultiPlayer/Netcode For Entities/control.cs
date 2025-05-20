using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 移动速度

    void Update()
    {
        // 获取输入
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D 或 ←/→
        float verticalInput = Input.GetAxis("Vertical");     // W/S 或 ↑/↓

        // 计算移动方向
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // 移动物体
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}