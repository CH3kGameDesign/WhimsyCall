using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Whimsy.Creatures;

public class Navigation : MonoBehaviour
{
    public _PhysicsVar physicsVar;
    public _AIInfo aiInfo;
    [Space(10)]
    public Rigidbody objectMover;
    public Transform objectModel;

    private _Cooldowns cooldowns = new _Cooldowns();

    private Vector2Int gridPos;

    private Vector3 tarDir = Vector3.zero;
    private Vector3 objectMoveDirection = Vector3.zero;

    private Vector3 groundNormal = Vector3.up;
    

    // Start is called before the first frame update
    void Start()
    {
        aiInfo.detectionSphere.navController = this;
        float tempDetectRange = 1;
        foreach (var item in aiInfo.navItemInteractions)
        {
            if (item.detectionRange > tempDetectRange)
                tempDetectRange = item.detectionRange;
        }
        aiInfo.detectionSphere.GetComponent<SphereCollider>().radius = tempDetectRange;
    }

    public void triggerEnter(Collider other)
    {
        for (int i = 0; i < aiInfo.navItemInteractions.Count; i++)
        {
            if (other.tag == aiInfo.navItemInteractions[i].tag)
            {
                bool add = true;
                for (int j = 0; j < aiInfo.activeNavItems.Count; j++)
                {
                    if (aiInfo.activeNavItems[j].target == other.transform)
                    {
                        add = false;
                        break;
                    }
                }
                if (add && Vector3.Distance(other.transform.position, objectMover.position) < aiInfo.navItemInteractions[i].detectionRange)
                {
                    _NavItem temp = new _NavItem();
                    temp.target = other.transform;
                    temp.priority = aiInfo.navItemInteractions[i].priority;
                    temp.interaction = aiInfo.navItemInteractions[i].interaction;
                    temp.intendedDistance = aiInfo.navItemInteractions[i].intendedDistance;
                    temp.abandonRange = aiInfo.navItemInteractions[i].abandonRange;

                    aiInfo.activeNavItems.Add(temp);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        FakeGravity();

        PlayerModelRotation();

        Movement();
        UpdateCooldowns();

        UpdateGridPos();
    }

    #region DirectionHandle
    void Movement()
    {
        Vector3 tempDir = Vector3.zero;

        //AI HERE NEEEEEEEEEEEDS WORK
        //Currently just avoids and chases objects and doesn't avoid walls
        for (int i = aiInfo.activeNavItems.Count - 1; i >= 0; i--)
        {
            _NavItem item = aiInfo.activeNavItems[i];
            if (Vector3.Distance(item.target.position, objectMover.position) > aiInfo.activeNavItems[i].abandonRange)
            {
                aiInfo.activeNavItems.RemoveAt(i);
            }
            else
            {
                switch (item.interaction)
                {
                    case _Interaction.chase:
                        if (item.intendedDistance > Vector3.Distance(item.target.position, objectMover.position))
                        {
                            tempDir += Vector3.zero;
                        }
                        else
                            tempDir += (item.target.position - objectMover.position) * item.priority;
                        break;
                    case _Interaction.avoid:
                        tempDir -= (item.target.position - objectMover.position) * item.priority;
                        //if (item.intendedDistance > 0)
                          //  tempDir -= Vector3.Normalize(objectMover.position - item.target.position) * item.intendedDistance;
                        break;
                    default:
                        break;
                }
            }
        }

        tempDir.y = 0;
        tempDir = Vector3.ClampMagnitude(tempDir, 1);

        tarDir = Vector3.Lerp(Vector3.Normalize(tarDir), Vector3.Normalize(tempDir), Time.deltaTime * physicsVar.turningSpeed);


        //Option For Variable Speed
        
        tarDir *= tempDir.magnitude;

        ApplyMovement(tarDir);
    }
    #endregion
    void UpdateCooldowns()
    {
        if (cooldowns.dodgeCool > 0)
        {
            cooldowns.dodgeCool = Mathf.Clamp(cooldowns.dodgeCool - Time.deltaTime, 0, cooldowns.dodgeCool);
        }

        if (cooldowns.dodgeCool < physicsVar.dodgeCooldown - physicsVar.dodgeDuration)
            physicsVar.dodgeDirection = Vector3.zero;

        for (int i = 0; i < cooldowns.abilCool.Count; i++)
        {
            cooldowns.abilCool[i] = Mathf.Clamp(cooldowns.abilCool[i] - Time.deltaTime, 0, cooldowns.abilCool[i]);
        }

        if (cooldowns.invincibleTimer > 0)
            cooldowns.invincibleTimer -= Time.deltaTime;
    }
    
    #region Player Movement
    void FakeGravity()
    {
        //Transform To Ground
        RaycastHit hit;
        if (Physics.SphereCast(objectMover.transform.position, physicsVar.sphereCastRadius, Vector3.down, out hit, 10, physicsVar.groundLayers))
        {
            float temp = hit.point.y + physicsVar.playerHeight;

            temp = Mathf.MoveTowards(objectMover.transform.position.y, temp, Time.deltaTime * 9);

            objectMover.transform.position = new Vector3(objectMover.transform.position.x, temp, objectMover.transform.position.z);
        }

        //Slow Velocity
        objectMover.velocity = new Vector3(objectMover.velocity.x, objectMover.velocity.y * 0.5f, objectMover.velocity.z);
    }

    void ApplyMovement(Vector3 moveDir)
    {
        objectMoveDirection = moveDir;
        objectMoveDirection.y = 0;
        objectMoveDirection = AdjustMovementSlopes(objectMoveDirection);

        Vector3 tempVelocity = new Vector3(objectMover.velocity.x, 0, objectMover.velocity.z) * physicsVar.velocityEffect;

        Vector3 forcePush = objectMoveDirection;

        if (objectMover.velocity.magnitude > physicsVar.maxSpeed)
            forcePush -= tempVelocity;

        if (moveDir.magnitude == 0)
        {
            forcePush -= tempVelocity;
            forcePush *= physicsVar.brakingSpeed;
        }
        else
            forcePush *= physicsVar.speed;

        forcePush *= Time.deltaTime * 50;


        objectMover.AddForce(forcePush, ForceMode.Impulse);

        if (physicsVar.dodgeDirection.magnitude > 0)
        {
            objectMover.AddForce(physicsVar.dodgeDirection * physicsVar.dodgeForce * Time.deltaTime * 50, ForceMode.Impulse);
        }
    }

    void PlayerModelRotation()
    {
        bool facingTarget = false;
        foreach (var item in aiInfo.activeNavItems)
        {
            if (Vector3.Distance(item.target.position, objectMover.position) < item.intendedDistance + 1)
            {
                objectModel.rotation = Quaternion.RotateTowards(objectModel.rotation, Quaternion.LookRotation(item.target.position - objectMover.position, groundNormal), Time.deltaTime * 360);
                facingTarget = true;
            }
        }
        if (objectMoveDirection.magnitude > 0 && !facingTarget)
        {
            objectModel.rotation = Quaternion.RotateTowards(objectModel.rotation, Quaternion.LookRotation(objectMoveDirection), Time.deltaTime * 360);
        }
    }

    //Adjust MoveDir based on the ground below PlayerMover
    Vector3 AdjustMovementSlopes(Vector3 moveDir)
    {
        Vector3 temp = moveDir;
        RaycastHit hit;
        if (Physics.SphereCast(objectMover.transform.position, 0.4f, Vector3.down, out hit, 1, physicsVar.groundLayers))
        {
            groundNormal = hit.normal;
            temp = Vector3.ProjectOnPlane(temp, groundNormal);
            temp.y *= physicsVar.verticalMultiplier;
        }
        return temp;
    }

    void UpdateGridPos()
    {
        Vector2Int _gridPos = LevelGeneration.GetGridPos(objectMover.position);
        if (_gridPos != gridPos)
        {
            gridPos = _gridPos;
        }
    }
    #endregion

    #region Player Interation
    //rolls the player in the target direction, player has I-frames during dodge
    void Dodge(Vector2 dir)
    {
        if (cooldowns.dodgeCool <= 0)
        {
            physicsVar.dodgeDirection = objectMoveDirection;

            cooldowns.invincibleTimer = Mathf.Max(cooldowns.invincibleTimer, physicsVar.dodgeInvincibiliity);
            cooldowns.dodgeCool = physicsVar.dodgeCooldown;
        }
    }
    //Listeners are added to this void whenever the player is in their triggerArea
    public void Interaction()
    {

    }
    //Activates 1 of 3 abilities the player can have equipped at a time
    void ActivateAbility(int abilNum)
    {
        if (cooldowns.abilCool[abilNum] <= 0)
        {
            
        }
    }

    #endregion

   
    #region Classes

    [System.Serializable]
    public class _PhysicsVar
    {
        public float speed = 10f;
        public float brakingSpeed = 20f;
        public float turningSpeed = 1;

        public float maxSpeed = 10f;

        public float velocityEffect = 0.3f;
        public float verticalMultiplier = 5f;
        [Space(10)]
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
    public class _Cooldowns
    {
        public float invincibleTimer = 0;

        public float dodgeCool = 0;
        public List<float> abilCool = new List<float>(3);
    }
    [System.Serializable]
    public class _AIInfo
    {
        [HideInInspector]
        public Vector3 origin;

        public Vector2 maxDistanceRange = new Vector2(2,15);
        public Vector2 minDistanceRange = new Vector2(0,0);

        public List<_NavItemType> navItemInteractions = new List<_NavItemType>();

        public DetectionSphere detectionSphere;

        [HideInInspector]
        public List<_NavItem> activeNavItems = new List<_NavItem>();
    }

    [System.Serializable]
    public class _NavItemType
    {
        public string tag = "";
        public _Interaction interaction;
        public float priority = 1f;
        public float intendedDistance = 1f;
        [Space (10)]
        public float detectionRange = 10f;
        public float abandonRange = 15f;
    }

    public class _NavItem
    {
        public Transform target;
        public _Interaction interaction;
        public float priority = 1f;
        public float intendedDistance = 1f;
        public float abandonRange = 10f;
    }

    public enum _Interaction {chase,avoid};
    #endregion
}
