using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Whimsy.Creatures;
using static PlayerHolder;

public class PlayerHolder : MonoBehaviour
{
    public _KeyboardInput keyboardInput;
    public _TapInput tapInput;
    [Space (10)]
    public _PhysicsVar physicsVar;
    public Rigidbody playerMover;
    public Transform playerModel;
    [Space(10)]
    public _CameraInfo cameraInfo;

    private _Cooldowns cooldowns = new _Cooldowns();

    private float clickTimer = 0;

    private Vector2Int gridPos;

    ClickInformation curClick;
    ClickInformation initialClick;

    private Vector2 V2_moveInput;

    //Time since last movement
    private float idleTimer = 0;
    private Vector3 playerMoveDirection = Vector3.zero;
    
    private LensDistortion settings;

    [HideInInspector]
    public List<CreatureObject> equippedCreatures = new List<CreatureObject>();
    

    

    // Start is called before the first frame update
    void Start()
    {
        PostProcessVolume temp;
        if (cameraInfo.cameraMain.TryGetComponent<PostProcessVolume>(out temp))
            settings = temp.sharedProfile.GetSetting<LensDistortion>();
        else
            settings = null;
    }

    // Update is called once per frame
    void Update()
    {
        FakeGravity();

        CameraTracking();
        PlayerModelRotation();

        GetInput();
        UpdateCooldowns();

        UpdateGridPos();
    }

    #region InputHandlers
    void GetInput()
    {
       
        V2_moveInput = Vector2.zero;

        #region TapInputs
        if (Input.GetMouseButton(0))
        {
            curClick = GetClickInfo();
            if (initialClick == null)
                initialClick = curClick;

            clickTimer += Time.deltaTime;
            if (clickTimer >= tapInput.clickWaiter)
                SendHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (clickTimer < tapInput.clickWaiter)
                SendClick();
            clickTimer = 0;
            initialClick = null;
        }
        #endregion

        #region KeyboardInputs
        if (Input.GetKey(keyboardInput.forward))
            V2_moveInput += new Vector2(0, 1);
        if (Input.GetKey(keyboardInput.backward))
            V2_moveInput += new Vector2(0, -1);
        if (Input.GetKey(keyboardInput.left))
            V2_moveInput += new Vector2(-1, 0);
        if (Input.GetKey(keyboardInput.right))
            V2_moveInput += new Vector2(1, 0);

        V2_moveInput = Vector2.ClampMagnitude(V2_moveInput, 1);

        if (Input.GetKeyDown(keyboardInput.rotateLeft))
            CameraRotation(true);
        if (Input.GetKeyDown(keyboardInput.rotateRight))
            CameraRotation(false);

        if (V2_moveInput.magnitude > 0)
        {
            if (Input.GetKeyDown(keyboardInput.dodge))
                Dodge(V2_moveInput);
            idleTimer = 0;
        }
        else
        {
            if (Input.GetKeyDown(keyboardInput.dodge))
                Interaction();
            idleTimer += Time.deltaTime;
        }
    

        if (Input.GetKeyDown(keyboardInput.abil1))
            ActivateAbility(0);
        if (Input.GetKeyDown(keyboardInput.abil2))
            ActivateAbility(1);
        if (Input.GetKeyDown(keyboardInput.abil3))
            ActivateAbility(2);
        #endregion

        ApplyMovement(V2_moveInput);
    }

    void UpdateCooldowns()
    {
        if (cooldowns.dodgeCool > 0)
            cooldowns.dodgeCool = Mathf.Clamp(cooldowns.dodgeCool - Time.deltaTime, 0, cooldowns.dodgeCool);

        if (cooldowns.dodgeCool < physicsVar.dodgeCooldown - physicsVar.dodgeDuration)
            physicsVar.dodgeDirection = Vector3.zero;

        for (int i = 0; i < cooldowns.abilCool.Count; i++)
        {
            cooldowns.abilCool[i] = Mathf.Clamp(cooldowns.abilCool[i] - Time.deltaTime, 0, cooldowns.abilCool[i]);
        }

        if (cooldowns.invincibleTimer > 0)
            cooldowns.invincibleTimer -= Time.deltaTime;
    }

    #region Mouse/Tap Input
    ClickInformation GetClickInfo()
    {
        ClickInformation temp = new ClickInformation();
        temp.screenPos = Input.mousePosition;

        Ray ray = cameraInfo.cameraMain.ScreenPointToRay(temp.screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000))
        {
            temp.distance = hit.distance;
            temp.hitPosition = hit.point;
            temp.hitObject = hit.transform;
        }
        else
        {
            temp.distance = -1;
            temp.hitObject = null;
            temp.hitPosition = Vector3.zero;
        }

        return temp;
    }

    void SendClick()
    {
        if (Vector3.Distance( curClick.screenPos, initialClick.screenPos) > tapInput.cameraSwipeDistance)
        {
            if (curClick.screenPos.x < initialClick.screenPos.x)
                CameraRotation(true);
            else
                CameraRotation(false);
        }
    }

    void SendHold()
    {
        V2_moveInput += new Vector2(curClick.screenPos.x - initialClick.screenPos.x, curClick.screenPos.y - initialClick.screenPos.y);
        V2_moveInput = Vector2.ClampMagnitude(V2_moveInput, 1);
    }

    public Ray RaycastWithDistortion(Vector2 screenPosition)
    {
        if (settings != null)
        {
            if (settings.active)
            {
                return cameraInfo.cameraMain.ViewportPointToRay(DistortAndNormalizeScreenPosition(screenPosition));
            }
            else
            {
                return cameraInfo.cameraMain.ScreenPointToRay(screenPosition);
            }
        }
        else
            return cameraInfo.cameraMain.ScreenPointToRay(screenPosition);
    }

    public Vector2 DistortAndNormalizeScreenPosition(Vector2 screenPosition)
    {
        return DistortUV(cameraInfo.cameraMain.ScreenToViewportPoint(screenPosition));
    }

    public Vector3 DistortAndNormalizeScreenPosition(Vector3 screenPosition)
    {
        Vector2 temp = new Vector2(screenPosition.x, screenPosition.y);
        temp = DistortUV(cameraInfo.cameraMain.ScreenToViewportPoint(temp));
        return new Vector3(temp.x, temp.y, screenPosition.z);
    }

    Vector2 DistortUV(Vector2 uv)
    {
        Vector2 half = new Vector2(0.5f, 0.5f);
        float amount = 1.6f * Mathf.Max(Mathf.Abs(settings.intensity.value), 1);
        float theta = Mathf.Deg2Rad * Mathf.Min(160f, amount);
        float sigma = 2 * Mathf.Tan(theta * 0.5f);
        Vector4 distortionAmount = new Vector4(settings.intensity.value >= 0f ? theta : 1f / theta, sigma, 1f / settings.scale.value, settings.intensity.value);
        uv = ((uv - half) * distortionAmount.z) + half;
        Vector2 distortionCentre = new Vector2(settings.centerX.value, settings.centerY.value);
        Vector2 distortionScale = new Vector2(Mathf.Max(settings.intensityX.value, 1e-4f), Mathf.Max(settings.intensityY.value, 1e-4f));
        Vector2 ruv = distortionScale * (uv - half - distortionCentre);
        float ru = ruv.magnitude;
        if (distortionAmount.w > 0)
        {
            ru = Mathf.Tan(ru * distortionAmount.x) / (ru * distortionAmount.y);
        }
        else
        {
            ru = distortionAmount.x * Mathf.Atan(ru * distortionAmount.y) / ru;
        }
        uv = uv + (ruv * (ru - 1f));
        return uv;
    }
    #endregion

    #endregion

    #region Player Movement
    void FakeGravity ()
    {
        //Transform To Ground
        RaycastHit hit;
        if (Physics.SphereCast(playerMover.transform.position, physicsVar.sphereCastRadius, Vector3.down, out hit, 10, physicsVar.groundLayers))
        {
            float temp = hit.point.y + physicsVar.playerHeight;

            temp = Mathf.MoveTowards(playerMover.transform.position.y, temp, Time.deltaTime * 9);

            playerMover.transform.position = new Vector3(playerMover.transform.position.x, temp, playerMover.transform.position.z);
        }

        //Slow Velocity
        playerMover.velocity = new Vector3(playerMover.velocity.x, playerMover.velocity.y * 0.5f, playerMover.velocity.z);
    }

    void ApplyMovement(Vector2 moveDir)
    {
        playerMoveDirection = 
            moveDir.x * cameraInfo.cameraHook.right +
            moveDir.y * cameraInfo.cameraHook.forward;

        playerMoveDirection = AdjustMovementSlopes(playerMoveDirection);

        Vector3 tempVelocity = new Vector3(playerMover.velocity.x,0, playerMover.velocity.z) * physicsVar.velocityEffect;

        Vector3 forcePush = playerMoveDirection;

        if (playerMover.velocity.magnitude > physicsVar.maxSpeed)
            forcePush -= tempVelocity;

        if (V2_moveInput.magnitude == 0)
        {
            forcePush -= tempVelocity;
            forcePush *= physicsVar.brakingSpeed;
        }
        else
            forcePush *= physicsVar.speed;

        forcePush *= Time.deltaTime * 50;
        

        playerMover.AddForce(forcePush, ForceMode.Impulse);

        if (physicsVar.dodgeDirection.magnitude > 0)
        {
            Debug.Log("Dodging?");
            playerMover.AddForce(physicsVar.dodgeDirection * physicsVar.dodgeForce * Time.deltaTime * 50, ForceMode.Impulse);
        }
    }

    void PlayerModelRotation()
    {
        if (playerMoveDirection.magnitude > 0)
        {
            //playerModel.forward = playerMoveDirection;
            playerModel.rotation = Quaternion.RotateTowards(playerModel.rotation, Quaternion.LookRotation(playerMoveDirection),Time.deltaTime * 360);
        }
    }

    //Adjust MoveDir based on the ground below PlayerMover
    Vector3 AdjustMovementSlopes(Vector3 moveDir)
    {
        Vector3 temp = moveDir;
        RaycastHit hit;
        if (Physics.SphereCast(playerMover.transform.position,0.4f,Vector3.down,out hit,1,physicsVar.groundLayers))
        {
            Vector3 normal = hit.normal;
            temp = Vector3.ProjectOnPlane(temp, normal);
            temp.y *= physicsVar.verticalMultiplier;
        }
        return temp;
    }

    void UpdateGridPos ()
    {
        Vector2Int _gridPos = LevelGeneration.GetGridPos(playerMover.position);
        if (_gridPos != gridPos)
        {
            gridPos = _gridPos;
            if (cameraInfo.viewRangeActive)
                LevelGeneration._ViewFinder(gridPos, cameraInfo.viewRange);
        }
    }
    #endregion

    #region Player Interation
    //rolls the player in the target direction, player has I-frames during dodge
    void Dodge (Vector2 dir)
    {
        if (cooldowns.dodgeCool <= 0)
        {
            physicsVar.dodgeDirection = playerMoveDirection;

            cooldowns.invincibleTimer = Mathf.Max(cooldowns.invincibleTimer, physicsVar.dodgeInvincibiliity);
            cooldowns.dodgeCool = physicsVar.dodgeCooldown;
        }
    }
    //Listeners are added to this void whenever the player is in their triggerArea
    void Interaction()
    {

    }
    //Activates 1 of 3 abilities the player can have equipped at a time
    void ActivateAbility (int abilNum)
    {
        if (cooldowns.abilCool[abilNum] <= 0)
        {
            if (equippedCreatures.Count > abilNum)
            {

            }
        }
    }

    #endregion

    #region Camera Movement
    void CameraTracking()
    {
        #region Position
        Vector3 tarPos = cameraInfo.cameraHook.position;
        tarPos.x = Mathf.MoveTowards(tarPos.x, cameraInfo.cameraHolder.position.x, cameraInfo.cameraTrackDeadzone.x);
        tarPos.z = Mathf.MoveTowards(tarPos.z, cameraInfo.cameraHolder.position.z, cameraInfo.cameraTrackDeadzone.y);

        cameraInfo.cameraHolder.position = Vector3.Lerp(
            cameraInfo.cameraHolder.position,
            tarPos,
            (1 / cameraInfo.moveSpeed) * Time.deltaTime);

        if (idleTimer > cameraInfo.idleTimeLimit)
            cameraInfo.cameraHolder.position = Vector3.Lerp(
            cameraInfo.cameraHolder.position,
            cameraInfo.cameraHook.position,
            (0.25f / cameraInfo.moveSpeed) * Time.deltaTime);
        #endregion

        #region Rotation
        cameraInfo.cameraHook.localEulerAngles = new Vector3(0, cameraInfo.tarRot, 0);

        cameraInfo.cameraHolder.rotation = Quaternion.Lerp(
            cameraInfo.cameraHolder.rotation,
            cameraInfo.cameraHook.rotation,
            1 / cameraInfo.rotSpeed);
        #endregion
    }
    void CameraRotation(bool left)
    {
        if (left)
            cameraInfo.tarRot -= 90;
        else
            cameraInfo.tarRot += 90;
    }
    #endregion

    #region Classes
    class ClickInformation
    {
        public Transform hitObject;
        public Vector3 hitPosition;
        public Vector3 screenPos;
        public float distance;
    }
    [System.Serializable]
    public class _KeyboardInput
    {
        public KeyCode forward = KeyCode.W;
        public KeyCode backward = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;
        [Space(10)]
        public KeyCode rotateLeft = KeyCode.Q;
        public KeyCode rotateRight = KeyCode.E;
        [Space(10)]
        public KeyCode dodge = KeyCode.DownArrow;
        public KeyCode abil1 = KeyCode.LeftArrow;
        public KeyCode abil2 = KeyCode.UpArrow;
        public KeyCode abil3 = KeyCode.RightArrow;
    }
    [System.Serializable]
    public class _TapInput
    {
        [Tooltip("How long before a tap is considered a hold")]
        public float clickWaiter = 0.15f;

        [Tooltip("How far to swipe before its a camera swipe")]
        public float cameraSwipeDistance = 0.5f;
    }

    [System.Serializable]
    public class _PhysicsVar
    {
        public float speed = 10f;
        public float brakingSpeed = 20f;

        public float maxSpeed = 10f;

        public float velocityEffect = 0.3f;
        public float verticalMultiplier = 5f;
        [Space (10)]
        public float dodgeCooldown = 1f;
        public float dodgeForce = 15f;
        public float dodgeDuration = 0.2f;
        public float dodgeInvincibiliity = 0.2f;
        [HideInInspector]
        public Vector3 dodgeDirection = Vector3.zero;
        [Space(10)]
        public LayerMask groundLayers = new LayerMask();
        public float sphereCastRadius = 0.25f;
        public float playerHeight = 0.5f;
    }

    [System.Serializable]
    public class _CameraInfo
    {
        public Transform cameraHook;
        public Transform cameraHolder;
        public Camera cameraMain;

        public float moveSpeed = 3;
        public float rotSpeed = 3;

        [HideInInspector]
        public float tarRot = 0;
        [Space(10)]
        public bool viewRangeActive = true;
        public int viewRange = 10;

        public Vector2 cameraTrackDeadzone = new Vector2(3, 1.5f);
        public float idleTimeLimit = 1f;
    }

    public class _Cooldowns
    {
        public float invincibleTimer = 0;

        public float dodgeCool = 0;
        public List<float> abilCool = new List<float>(3);
    }
    #endregion
}
