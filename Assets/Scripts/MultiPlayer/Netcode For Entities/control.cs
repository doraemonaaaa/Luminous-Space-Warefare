using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // �ƶ��ٶ�

    void Update()
    {
        // ��ȡ����
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D �� ��/��
        float verticalInput = Input.GetAxis("Vertical");     // W/S �� ��/��

        // �����ƶ�����
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // �ƶ�����
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}