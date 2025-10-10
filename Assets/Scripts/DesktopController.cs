using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DesktopController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public bool requireRightClick = true;

    private CharacterController controller;
    private float verticalVelocity;
    private float xRot = 0f;
    private Transform playerBody;

    void Start()
    {
        controller = GetComponentInParent<CharacterController>();
        playerBody = controller.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // move relative to the player's facing direction
        Vector3 move = playerBody.forward * moveZ + playerBody.right * moveX;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // gravity
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        bool canLook = !requireRightClick || Input.GetMouseButton(1);
        if (!canLook)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // vertical look (pitch) - camera only
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);
        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        // horizontal look (yaw) - body only
        // Use global Y axis to avoid tilt accumulation
        Vector3 bodyEuler = playerBody.eulerAngles;
        bodyEuler.y += mouseX;
        playerBody.eulerAngles = bodyEuler;

        // keep camera centered on body
        transform.localPosition = new Vector3(0f, 1.6f, 0f);
    }
}
