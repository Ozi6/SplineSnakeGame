using UnityEngine;

public class SnakeInputController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float strafeSpeed = 1f;

    [Header("Control Keys")]
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;

    private SnakeSplineController snakeController;
    private Transform snakeHead;
    private bool wasStrafingLeft = false;
    private bool wasStrafingRight = false;
    private float currentStrafeDirection = 0f;

    void Start()
    {
        snakeController = SnakeSplineController.Instance;
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
        bool isStrafingLeft = Input.GetKey(moveLeftKey);
        bool isStrafingRight = Input.GetKey(moveRightKey);

        if (isStrafingLeft && !isStrafingRight)
        {
            snakeHead.Translate(-strafeSpeed * Time.deltaTime, 0, 0);
            currentStrafeDirection = -1f;
        }
        else if (isStrafingRight && !isStrafingLeft)
        {
            snakeHead.Translate(strafeSpeed * Time.deltaTime, 0, 0);
            currentStrafeDirection = 1f;
        }
        else
            currentStrafeDirection = 0f;

        if (wasStrafingLeft && !isStrafingLeft)
            snakeController.OnStrafeStop(-1f);
        else if (wasStrafingRight && !isStrafingRight)
            snakeController.OnStrafeStop(1f);
        wasStrafingLeft = isStrafingLeft;
        wasStrafingRight = isStrafingRight;
    }

    void OnGUI()
    {
        if (currentStrafeDirection != 0f)
        {
            string direction = currentStrafeDirection > 0 ? "Right" : "Left";
            GUI.Label(new Rect(10, 10, 200, 20), $"Strafing: {direction}");
        }
    }
}