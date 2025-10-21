using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;   // 따라갈 Player
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float sensitivity = 3f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
        {
            // 시작할 때 플레이어 뒤쪽에 배치
            yaw = target.eulerAngles.y;
            pitch = 10f; // 약간 내려다보기
        }
    }

    private void OnDestroy()
    {
        // 씬이 바뀔 때 마우스 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (!target) return;

        // 마우스로만 카메라 회전
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -20f, 40f);

        // 회전 값 적용
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 플레이어 기준 위치 계산
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + Vector3.up * height + offset;

        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
