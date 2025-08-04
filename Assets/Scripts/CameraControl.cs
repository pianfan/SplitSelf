using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("角色引用")]
    public Transform playerOutside; // 表角色
    public Transform playerInside;  // 里角色

    [Header("跟随设置")]
    public float smoothSpeed;
    public Transform currentTarget;
    private float _fixedX;
    private Vector3 _offsetYZ;

    void Start()
    {
        _fixedX = transform.position.x;
        UpdateOffset(); // 初始化偏移
    }

    void LateUpdate()
    {
        if (currentTarget == null) return;
        Vector3 desiredPosition = new Vector3(
            _fixedX, 
            currentTarget.position.y + _offsetYZ.y, 
            currentTarget.position.z + _offsetYZ.z
        );
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    // 公开更新偏移量的方法（供外部切换目标后调用）
    public void UpdateOffset()
    {
        if (currentTarget != null)
        {
            _offsetYZ = transform.position - new Vector3(_fixedX, currentTarget.position.y, currentTarget.position.z);
        }
    }
}
