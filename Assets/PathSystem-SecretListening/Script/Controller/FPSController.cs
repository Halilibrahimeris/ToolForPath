using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public float walkSpeed = 5.0f;
    public float gravity = 9.8f;
    public float mouseSensitivity = 2.0f;
    public Transform playerCamera;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float verticalSpeed = 0;
    private float cameraPitch = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

        transform.Rotate(Vector3.up * mouseX);

        float moveX = Input.GetAxis("Horizontal") * walkSpeed;
        float moveZ = Input.GetAxis("Vertical") * walkSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        if (characterController.isGrounded)
        {
            verticalSpeed = 0;
        }
        else
        {
            verticalSpeed -= gravity * Time.deltaTime;
        }

        moveDirection = new Vector3(move.x, verticalSpeed, move.z);

        characterController.Move(moveDirection * Time.deltaTime);
    }
}
