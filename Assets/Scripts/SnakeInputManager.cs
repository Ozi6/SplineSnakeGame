using UnityEngine;

public class SnakeInputController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float strafeSpeed = 1f;

    [Header("Control Keys")]
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;

    private SnakeController snakeController;
    private Transform snakeHead;

    void Start()
    {
        snakeController = SnakeController.Instance;
        if (snakeController == null)
            return;

        snakeHead = snakeController.head.transform;
    }

    void Update()
    {
        if (snakeController == null || snakeHead == null) return;
        HandleMovementInput();
    }

    void HandleMovementInput()
    {
        if (Input.GetKey(moveLeftKey))
            snakeHead.Translate(-strafeSpeed * Time.deltaTime, 0, 0);
        else if (Input.GetKey(moveRightKey))
            snakeHead.Translate(strafeSpeed * Time.deltaTime, 0, 0);
    }
}