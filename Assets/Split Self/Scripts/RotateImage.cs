using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateImage : MonoBehaviour //控制小游戏关卡的背景，有退场过渡
{
    public float initialSpeed = 60f;
    public float deceleration = 30f;
    public RotationDirection rotationDirection = RotationDirection.Clockwise;
    public bool autoStart; //这里接关卡切换

    private float currentSpeed = 0f;
    private bool isStopping = false;

    public enum RotationDirection
    {
        Clockwise,
        CounterClockwise
    }

    // Start is called before the first frame update
    void Start()
    {
        if (autoStart)
            StartRotation();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(currentSpeed) > 0f)
        {
            float directionMultiplier = rotationDirection == RotationDirection.Clockwise ? -1f : 1f;
            transform.Rotate(0f, 0f, currentSpeed * Time.deltaTime * directionMultiplier);
            if (isStopping)
            {
                currentSpeed -= deceleration * Time.deltaTime;
                if (currentSpeed <= 0f)
                {
                    currentSpeed = 0f;
                    isStopping = false;
                }
            }
        }
    }

    public void StartRotation()
    {
        currentSpeed = initialSpeed;
        isStopping = false;
    }

    // 即将退场时减速并最终停下
    public void StopRotation()
    {
        isStopping = true;
    }
}
