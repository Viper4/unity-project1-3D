using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    GameManager gameManager;
    GameObject sceneLoader;

    StatSystem stats;

    CharacterController controller;

    //Player inputs
    public Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    public float gravity = -12;
    public float pushPower = 3;

    [Range(0, 1)] public float airControlPercent;

    public float speedSmoothTime = 0.1f;
    float speedSmoothVelocity;
    float currentSpeed;
    float velocityY;

    //Rotation
    public float mouseSensitivity;
    public Vector2 pitchMinMax = new Vector2(-60, 90);
    public float rotationSmoothTime = 0.1f;

    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;
    float yaw;
    float pitch;

    public bool seenTutorial { get; set; } = false;

    bool startRegenStamina = false;
    bool regenStamina = false;
    public bool disableInput = false;
    public bool disableMovement { get; set; }  = true;
    bool enablingMovement = false;

    //Start is called before the first frame update
    void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        if(sceneLoader == null)
        {
            sceneLoader = GameObject.Find("SceneLoader");
        }
        stats = GetComponent<StatSystem>();
        controller = GetComponent<CharacterController>();
        stats.health = stats.maxHealth;

        keys.Add("Forward", KeyCode.W);
        keys.Add("Left", KeyCode.A);
        keys.Add("Backward", KeyCode.S);
        keys.Add("Right", KeyCode.D);
        keys.Add("Jump", KeyCode.Space);
        keys.Add("Run", KeyCode.LeftShift);
        keys.Add("Crouch", KeyCode.LeftControl);
        keys.Add("Primary", KeyCode.Mouse0);
        keys.Add("Secondary", KeyCode.Mouse1);
        keys.Add("Interact", KeyCode.E);
        keys.Add("Drop", KeyCode.Q);
        keys.Add("Inventory", KeyCode.Tab);
        keys.Add("Quick Save", KeyCode.F5);
        keys.Add("Quick Load", KeyCode.F9);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(GetInputAxis("Horizontal"), GetInputAxis("Vertical"));
        Vector2 inputDir = input.normalized;

        if (Input.GetKeyDown(keys["Quick Save"]))
        {
            sceneLoader.GetComponent<SaveData>().QuickSave();
            StartCoroutine(gameManager.importantTransforms["UI"].GetComponent<UISystem>().TextPrompt("Quick Saved", 0.5f));
        }
        else if (Input.GetKeyDown(keys["Quick Load"]))
        {
            disableMovement = true;
            sceneLoader.GetComponent<SaveData>().QuickLoad();
            StartCoroutine(gameManager.importantTransforms["UI"].GetComponent<UISystem>().TextPrompt("Quick Loaded", 0.5f));
        }
        else if (Input.GetKeyDown(keys["Inventory"]))
        {
            gameManager.importantTransforms["UI"].GetComponent<UISystem>().ToggleInventory();
        }

        if (!disableInput)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);

            gameManager.importantTransforms["Main Camera"].rotation = Quaternion.Euler(currentRotation.x, transform.eulerAngles.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, currentRotation.y, transform.eulerAngles.z);

            if (Input.GetKey(keys["Jump"]))
            {
                Jump();
            }
            else if (Input.GetKey(keys["Primary"]))
            {
                gameManager.importantTransforms["Item Slot"].GetComponent<ItemSystem>().Use(0);
            }
            else if (Input.GetKey(keys["Secondary"]))
            {
                gameManager.importantTransforms["Item Slot"].GetComponent<ItemSystem>().Use(1);
            }
            else if (Input.GetKeyDown(keys["Interact"]))
            {
                Transform bestTarget = null;
                float closestDistance = Mathf.Infinity;
                foreach (Transform potentialTarget in GameObject.Find("Interactables").transform)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, potentialTarget.position);
                    if (potentialTarget.GetComponent<InteractableObject>().playerInRange && distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        bestTarget = potentialTarget;
                    }
                }
                if (bestTarget != null)
                {
                    bestTarget.GetComponent<InteractableObject>().Interact();
                }
            }
            else if (Input.GetKey(keys["Drop"]))
            {
                gameManager.importantTransforms["Item Slot"].GetComponent<ItemSystem>().Drop(false);
            }
        }

        KeyCode[] runningKeys = { keys["Run"], keys["Forward"] };
        if (!disableMovement)
        {
            Move(inputDir, GetKeys(runningKeys), Input.GetKey(keys["Crouch"]));
        }

        if(regenStamina)
        {
            stats.stamina = currentSpeed < 0.5 ? stats.stamina + 0.1f : stats.stamina + 0.05f;

            if(stats.stamina >= stats.maxStamina)
            {
                regenStamina = false;
            }
        }
        if(!enablingMovement && disableMovement != disableInput)
        {
            StartCoroutine(EnableMovement());
        }
    }

    //Waiting a small amount of time before enabling movement to stop Move method from teleporting player back to default position
    IEnumerator EnableMovement()
    {
        enablingMovement = true;
        yield return new WaitForSeconds(0.1f);
        disableMovement = false;
        enablingMovement = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        Vector3 force;

        // no rigidbody
        if (body == null || body.isKinematic) { return; }

        // We use gravity to push things down, we use
        // our velocity and push power to push things other directions
        if (hit.moveDirection.y < -0.3f)
        {
            force = new Vector3(0, -0.5f, 0) * -gravity;
        }
        else
        {
            force = hit.controller.velocity * pushPower;
        }

        // Apply the push
        body.AddForceAtPosition(force, hit.point);
    }

    private float GetInputAxis(string axis)
    {
        float horizontal = 0;
        float vertical = 0;

        if (!disableInput)
        {
            switch (axis)
            {
                case "Horizontal":
                    if (Input.GetKey(keys["Right"]))
                    {
                        horizontal += 1;
                    }
                    if (Input.GetKey(keys["Left"]))
                    {
                        horizontal -= 1;
                    }
                    return horizontal;
                case "Vertical":
                    if (Input.GetKey(keys["Forward"]))
                    {
                        vertical += 1;
                    }
                    if (Input.GetKey(keys["Backward"]))
                    {
                        vertical -= 1;
                    }
                    return vertical;
            }
        }
                
        return 0;
    }

    private void Move(Vector2 inputDir, bool running, bool crouching)
    {
        if (crouching)
        {
            if (controller.height > stats.jumpHeight)
            {
                controller.height = stats.jumpHeight;
            }
        }
        else
        {
            if (controller.height < stats.jumpHeight * 2)
            {
                controller.height += 0.035f;
            }
        }

        stats.stamina = running && currentSpeed > 0.5 ? stats.stamina - 0.025f : stats.stamina;
        if (running)
        {
            regenStamina = false;
        }

        float targetSpeed;

        if (stats.stamina > 0)
        {
            targetSpeed = running ? stats.runSpeed : stats.walkSpeed * inputDir.magnitude;
        }
        else
        {
            targetSpeed = stats.walkSpeed / 2 * inputDir.magnitude;
        }
        targetSpeed = crouching ? targetSpeed / 2 : targetSpeed;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed * stats.speedMultiplier, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        velocityY += Time.deltaTime * gravity;
        Vector3 velocity = transform.right * inputDir[0] * currentSpeed + transform.forward * inputDir[1] * currentSpeed + Vector3.up * velocityY;

        controller.Move(velocity * Time.deltaTime);

        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;
        
        if (stats.stamina < stats.maxStamina && !running && !regenStamina && !startRegenStamina)
        {
            StartCoroutine(StartRegenStamina());
        }

        if (controller.isGrounded)
        {
            velocityY = 0;
        }
    }

    private void Jump()
    {
        if(controller.isGrounded && stats.stamina >= 4)
        {
            regenStamina = false;
            stats.stamina -= 4;
            float jumpVelocity = Mathf.Sqrt(-2 * gravity * stats.jumpHeight);
            velocityY = jumpVelocity;
        }
    }

    IEnumerator StartRegenStamina()
    {
        startRegenStamina = true;
        float waitTime;

        if(stats.stamina > 0)
        {
            waitTime = currentSpeed < 0.5 ? 1 : 2.5f;
        }
        else
        {
            waitTime = 3;
        }
        
        yield return new WaitForSeconds(waitTime);
        regenStamina = true;
        startRegenStamina = false;
    }

    float GetModifiedSmoothTime(float smoothTime)
    {
        if (controller.isGrounded)
        {
            return smoothTime;
        }

        if (airControlPercent == 0)
        {
            return float.MaxValue;
        }
        return smoothTime / airControlPercent;
    }

    bool GetKeys(KeyCode[] keys)
    {
        if (!disableInput)
        {
            int successCount = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKey(keys[i]))
                {
                    successCount++;
                }
            }
            if (successCount == keys.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
