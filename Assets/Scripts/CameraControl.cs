using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform _target;
    public float smoothSpeed;

    private float _fixedX;
    private Vector3 _offsetYZ;

    // Start is called before the first frame update
    void Start()
    {
        _fixedX = transform.position.x;
		if (_target != null)
        	_offsetYZ = transform.position - new Vector3(_fixedX, _target.position.y, _target.position.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (_target == null) 
            return;

        Vector3 desiredPosition = new Vector3(_fixedX, _target.position.y + _offsetYZ.y, _target.position.z + _offsetYZ.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
