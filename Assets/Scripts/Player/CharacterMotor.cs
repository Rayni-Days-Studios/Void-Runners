using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMotor : Photon.MonoBehaviour
{
    // Does this script currently respond to input?
    public bool CanControl = true;

    public bool UseFixedUpdate = true;

    // For the next variables, [NonSerialized] tells Unity to not serialize the variable or show it in the inspector view.
    // Very handy for organization!

    // The current global direction we want the character to move in.
    [NonSerialized] internal Vector3 InputMoveDirection = Vector3.zero;

    // Is the jump button held down? We use this interface instead of checking
    // for the jump button directly so this script can also be used by AIs.
    [NonSerialized] internal bool InputJump = false;


    public class CharacterMotorMovement
    {
        //The maximum horizontal speed when moving
        public float MaxForwardSpeed = 8f;
        public float MaxSidewaySpeed = 8f;
        public float MaxBackwardsSpeed = 6f;

        // Curve for multiplying speed based on slope (negative = downwards)
        public AnimationCurve SlopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));

        // How fast does the character change speed? Higher is faster.
        public float MaxGroundAcceleration = 50.0f;
        public float MaxAirAcceleration = 20.0f;

        //The gravity for the character
        public float Gravity = 20.0f;
        public float MaxFallSpeed = 20.0f;

        // For the next variables, [NonSerialized] tells Unity to not serialize the variable or show it in the inspector view.
        // Very handy for organization!

        // The last collision flag returned from controller.Move
        [NonSerialized] internal CollisionFlags CollisionFlags;

        // We will keep track of the character's current velocity
        [NonSerialized] internal Vector3 Velocity;

        // This keeps track of out current velocity while we're not grounded
        [NonSerialized] internal Vector3 FrameVelocity;

        [NonSerialized] internal Vector3 HitPoint = Vector3.zero;

        [NonSerialized] internal Vector3 LastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
    }

    CharacterMotorMovement _movement = new CharacterMotorMovement();

    enum MovementTransferOnJump
    {
        None,           // The jump is not affected by velocity of floor at all.
        InitTransfer,   // Jump gets its initial velocity from the floor, the gradually comes to a stop.
        PermaTransfer,  // Jump gets its inital velocity from the floor, and keeps the velocity until landing.
        PermaLocked     // Jump is relative to the movement of the last touched floor and will move together with that floor.
    }

    // We will contain all jumping related variables in one helper class for clarity
    public class CharacterMotorJumping
    {
        // Can the character jump?
        public bool Enabled = true;

        // How high do we jump when pressing jump and letting go immediately
        public float BaseHeight = 1f;

        // We add extraHeight units (meters) on top when holding the button down longer while jumping
        public float ExtraHeight = 4.1f;

        // How much does the character jump out perpendicular to the sirface on walkable surfaces?
        // 0 means a fully vertical jump and 1  means fully perpendicular
        public float PerpAmount = 0.0f;

        // How much does the character jump out perpendicular to the surface on too steep surfaces?
        // 0 means a fully vertical jump and 1 means fully perpendicular
        public float SteepPerpAmount = 0.5f;

        // For the next variables, [NonSerialized] tells Unity to not serialize the variable or show it in the inspector view.
        // Very handy for organization!

        // Are we jumping? (Instantiated with jump button and not grounded yet)
        // To see if we are just in the air (initiated by jumping OR falling) see the grounded variable
        [NonSerialized] internal bool Jumping;

        public bool HoldingJumpButton;

        // The time we jump at (Used to determine how log to apply extra jump power after jumping.)
        [NonSerialized] internal float LastStartTime;

        [NonSerialized] internal float LastButtonDownTime = -100;

        [NonSerialized] internal Vector3 JumpDir = Vector3.up;
    }

    CharacterMotorJumping jumping = new CharacterMotorJumping();

    class CharacterMotorMovingPlatform
    {
        public bool enabled = true;

        public MovementTransferOnJump movementTransfer = MovementTransferOnJump.PermaTransfer;

        [NonSerialized] internal Transform HitPlatform;

        [NonSerialized] internal Transform ActivePlatform;

        [NonSerialized] internal Vector3 ActiveLocalPoint;

        [NonSerialized] internal Vector3 ActiveGlobalPoint;

        [NonSerialized] internal Quaternion ActiveLocalRotation;

        [NonSerialized] internal Quaternion ActiveGlobalRotation;

        [NonSerialized] internal Matrix4x4 LastMatrix;

        [NonSerialized] internal Vector3 PlatformVelocity;

        [NonSerialized] internal bool NewPlatform;
    }

    CharacterMotorMovingPlatform movingPlatform = new CharacterMotorMovingPlatform();

    public class CharacterMotorSliding
    {
        // Does character slide on too steep surfaces?
        public bool enabled = true;

        // How fast does the character slide on steep surfaces?
        public  float slidingSpeed = 15;

        // How much can the playyer control the sliding direction?
        // If the value is 0.5 the player can slideways with half the speed of the downwards sliding speed.
        public  float sidewaysControl = 1.0f;

        // How much can the player influence the sliding speed?
        // If the value is 0.5 the player can speed the slding up to 150 % or slow it down to 50%.
        public  float speedControl = 0.4f;
    }

    readonly CharacterMotorSliding _sliding = new CharacterMotorSliding();

    [NonSerialized] private bool _grounded = true;

    [NonSerialized] private Vector3 _groundNormal = Vector3.zero;

    Vector3 _lastGroundNormal = Vector3.zero;

    private Transform _tr;

    private CharacterController _controller;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _tr = transform;
    }

    private void UpdateFunction()
    {
        // We copy the actual velocity into a temporary variable that we can manipulate
        var velocity = _movement.Velocity;

        // Update velocity based on input
        velocity = ApplyInputVelocityChange(velocity);

        // Apply gravity and jumping force
        velocity = ApplyGravityAndJumping(velocity);

        // Moving platform support
        Vector3 moveDistance;
        if (MoveWithPlatform())
        {
            var newGlobalPoint = movingPlatform.ActivePlatform.TransformPoint(movingPlatform.ActiveLocalPoint);
            moveDistance = (newGlobalPoint - movingPlatform.ActiveGlobalPoint);
            if (moveDistance != Vector3.zero)
            {
                _controller.Move(moveDistance);
            }

            //  Support moving platfrom rotation as well
            var newGlobalRotation = movingPlatform.ActivePlatform.rotation*movingPlatform.ActiveLocalRotation;
            var rotationDiff = newGlobalRotation*Quaternion.Inverse(movingPlatform.ActiveGlobalRotation);

            var yRotation = rotationDiff.eulerAngles.y;
            if(yRotation != 0)
            {
                // Prevent rotation of the local up vector
                _tr.Rotate(0, yRotation, 0);
            }
        }

        // Save lastPosition for velocity calculation
        var lastPosition = _tr.position;

        // We always want the movement to be framerate independent. Multiplying by Time.deltaTime does this.
        var currentMovementOffset = velocity*Time.deltaTime;

        // Find out how much we need to puh towards the ground to avoid loosing grounding
        // when walking down a step or over a sharp change in slope.
        var pushDownOffset = Mathf.Max(_controller.stepOffset,
            new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);

        if (_grounded)
            currentMovementOffset -= pushDownOffset*Vector3.up;

        // Reset variables that will be set by coliision function
        movingPlatform.HitPlatform = null;
        _groundNormal = Vector3.zero;

        // Move our character!
        _movement.CollisionFlags = _controller.Move(currentMovementOffset);

        _movement.LastHitPoint = _movement.HitPoint;
        _lastGroundNormal = _groundNormal;

        if (movingPlatform.enabled && movingPlatform.ActivePlatform != movingPlatform.HitPlatform)
        {
            if (movingPlatform.HitPlatform != null)
            {
                movingPlatform.ActivePlatform = movingPlatform.HitPlatform;
                movingPlatform.LastMatrix = movingPlatform.HitPlatform.localToWorldMatrix;
                movingPlatform.NewPlatform = true;
            }
        }

        // Calculate the velocity based on tthe current and previous position
        // This means our velocity will only be the amoun the character actually moved as a result of collisions.
        var oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
        _movement.Velocity = (_tr.position - lastPosition)/Time.deltaTime;
        var newHVelocity = new Vector3(_movement.Velocity.x, 0, _movement.Velocity.z);

        // The CharacterController can be moved in unwanted directions when colliding with things.
        // We ant to prevent this from inluencing the recorded velocity.
        if (oldHVelocity == Vector3.zero)
        {
            _movement.Velocity = new Vector3(0, _movement.Velocity.y, 0);
        }
        else
        {
            var projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity)/oldHVelocity.sqrMagnitude;
            _movement.Velocity = oldHVelocity*Mathf.Clamp01(projectedNewVelocity) + _movement.Velocity.y*Vector3.up;
        }

        if (_movement.Velocity.y < velocity.y - 0.001)
        {
            if (_movement.Velocity.y < 0)
            {
                // Somthing is forcing tthe CharacterController down faster than it should.
                // Ignore this
                _movement.Velocity.y = velocity.y;
            }
            else
            {
                // The upwards movement of the character controller has been blocked.
                // This is trated like a ceiling - stop further jumping here.
                jumping.HoldingJumpButton = false;
            }
        }

        // We were grounded but just loosed grounding
        if (_grounded && !IsGroundedTest())
        {
            _grounded = false;

            // Apply inertia from platform
            if (movingPlatform.enabled &&
                (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                    movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
                )
            {
                _movement.FrameVelocity = movingPlatform.PlatformVelocity;
                _movement.Velocity += movingPlatform.PlatformVelocity;
            }

            SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
            // We pushed the character down to ensure it would stay on the groud if there was any.
            // But there wasn't so now we cancel the upwards offset to make the fall smoother.
            _tr.position += pushDownOffset*Vector3.up;
        }
            // We were not gorunded but just landed on something
        else if (!_grounded && IsGroundedTest())
        {
            _grounded = true;
            jumping.Jumping = false;
            SubtractNewPlatformVelocity();

            SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
        }

        // Moving platform support
        if (MoveWithPlatform())
        {
            // Use the center of the lower half sphere of the capsule as reference point.
            // This works best when the character is standing on moving titlting platforms.
            movingPlatform.ActiveGlobalPoint = _tr.position + Vector3.up * (float)(_controller.center.y - _controller.height*0.5 + _controller.radius);
            movingPlatform.ActiveLocalPoint = movingPlatform.ActivePlatform.InverseTransformPoint(movingPlatform.ActiveGlobalPoint);

            // Support moving platform rotation as well
            movingPlatform.ActiveGlobalRotation = transform.rotation;
            movingPlatform.ActiveLocalRotation = Quaternion.Inverse(movingPlatform.ActivePlatform.rotation)*
                                                    movingPlatform.ActiveGlobalRotation;
        }
    }

    void FixedUpdate()
    {
        if (movingPlatform.enabled)
        {
            if (movingPlatform.ActivePlatform != null)
            {
                if (!movingPlatform.NewPlatform)
                {
                    var lastVelocity = movingPlatform.PlatformVelocity;

                    movingPlatform.PlatformVelocity =
                        (movingPlatform.ActivePlatform.localToWorldMatrix.MultiplyPoint3x4(
                            movingPlatform.ActiveLocalPoint) -
                            movingPlatform.LastMatrix.MultiplyPoint3x4(movingPlatform.ActiveLocalPoint))/Time.deltaTime;
                }
                movingPlatform.LastMatrix = movingPlatform.ActivePlatform.localToWorldMatrix;
                movingPlatform.NewPlatform = false;
            }
            else { movingPlatform.PlatformVelocity = Vector3.zero; }
        }

        if (UseFixedUpdate)
            UpdateFunction();
    }

    void Update()
    {
        if(!UseFixedUpdate)
            UpdateFunction();
    }

    private Vector3 ApplyInputVelocityChange(Vector3 velocity)
    {
        if(!CanControl)
            InputMoveDirection = Vector3.zero;

        //Find desired velocity
        Vector3 desiredVelocity;
        if (_grounded && TooSteep())
        {
            // the direction we're sliding in
            desiredVelocity = new Vector3(_groundNormal.x, 0, _groundNormal.z).normalized;
            // Find the input movement direction projected onto the sliding direction
            var projectedMoveDir = Vector3.Project(InputMoveDirection, desiredVelocity);
            // Add the sliding direction, the speed control, and the sideways control vectors
            desiredVelocity = desiredVelocity + projectedMoveDir*_sliding.speedControl +
                                (InputMoveDirection - projectedMoveDir)*_sliding.sidewaysControl;
            // Multiplay with the slidding speed
            desiredVelocity *= _sliding.slidingSpeed;
        }
        else
        {
            desiredVelocity = GetDesiredHorizontalVelocity();
        }

        if (movingPlatform.enabled && movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
        {
            desiredVelocity += _movement.FrameVelocity;
            desiredVelocity.y = 0;
        }

        if (_grounded)
            desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, _groundNormal);
        else
        {
            velocity.y = 0;
        }

        // Enforce max velocity
        var maxVelocityChange = GetMaxAcceleration(_grounded)*Time.deltaTime;
        var velocityChangeVector = (desiredVelocity - velocity);
        if (velocityChangeVector.sqrMagnitude > maxVelocityChange*maxVelocityChange)
        {
            velocityChangeVector = velocityChangeVector.normalized*maxVelocityChange;
        }
        // If we're in the air and don't have control, don't apply any velocity change at all.
        // If we're on the grounnd and don''t have control we do apply it - it will corresponf to friction.
        if (_grounded || CanControl)
        {
            velocity += velocityChangeVector;
        }

        if (_grounded)
        {
            // When going uphill, the CharacterController wil automatically move up by the needed amount.
            // Not moving it upwards manully prevent risk of lifting off from the ground
            // When goind downhill, DO move down manually, as gravity is not enough on steep hills.
            velocity.y = Mathf.Min(velocity.y, 0);
        }

        return velocity;
    }

    private Vector3 ApplyGravityAndJumping(Vector3 velocity)
    {
        if (!InputJump || !CanControl)
        {
            jumping.HoldingJumpButton = false;
            jumping.LastButtonDownTime = -100;
        }

        if (InputJump && jumping.LastButtonDownTime < 0 && CanControl)
        {
            jumping.LastButtonDownTime = Time.time;
        }

        if (_grounded)
            velocity.y = Mathf.Min(0, velocity.y) - _movement.Gravity*Time.deltaTime;
        else
        {
            velocity.y = _movement.Velocity.y - _movement.Gravity*Time.deltaTime;

            // When jumping up we don't apply gravity for some time when the is holding the jump button.
            // This gives more control over jump height by pressing the button longer
            if (jumping.Jumping && jumping.HoldingJumpButton)
            {
                // Calculate the duration that the extra jump force should have effect.
                // If we're still less than that duration after the jumoing time, apply the force.
                if (Time.time <
                    jumping.LastStartTime + jumping.ExtraHeight/CalculateJumpVerticalSpeed(jumping.BaseHeight))
                {
                    velocity += jumping.JumpDir*_movement.Gravity*Time.deltaTime;
                }
            }

            // Make sure we don't fall any faster than maxFallSpeed. This gives our character a terminal velocity.
            velocity.y = Mathf.Max(velocity.y, -_movement.MaxFallSpeed);
        }

        if (_grounded)
        {
            // Jump only if the jump button was pressed down in the last 0.2 seconds.
            // We use this check instead of checking if it's pressed down right now
            // because players will often try to jump in the exact moment when hitting the ground after a jump
            // and if they hit the button a fraction of a second too soon and no new jump happens as a consequence,
            // it's confusing and it feels like the game is buggy.
            if (jumping.Enabled && CanControl && (Time.time - jumping.LastButtonDownTime < 0.2))
            {
                _grounded = false;
                jumping.Jumping = true;
                jumping.LastStartTime = Time.time;
                jumping.LastButtonDownTime = -100;

                // Calculate the jumpingg direction
                if (TooSteep())
                    jumping.JumpDir = Vector3.Slerp(Vector3.up, _groundNormal, jumping.SteepPerpAmount);
                else
                {
                    jumping.JumpDir = Vector3.Slerp(Vector3.up, _groundNormal, jumping.PerpAmount);
                }

                // Apply the jumping force to the velocity. Cancel any vertical velocity first.
                velocity.y = 0;
                velocity += jumping.JumpDir*CalculateJumpVerticalSpeed(jumping.BaseHeight);

                // Apply inertia from platform
                if (movingPlatform.enabled &&
                    (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                        movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
                    )
                {
                    _movement.FrameVelocity = movingPlatform.PlatformVelocity;
                    velocity += movingPlatform.PlatformVelocity;
                }

                SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                jumping.HoldingJumpButton = false;
            }
        }
        return velocity;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!(hit.normal.y > 0) || !(hit.normal.y > _groundNormal.y) || !(hit.moveDirection.y < 0)) return;

        if ((hit.point - _movement.LastHitPoint).sqrMagnitude > 0.001 || _lastGroundNormal == Vector3.zero)
            _groundNormal = hit.normal;
        else
        {
            _groundNormal = _lastGroundNormal;
        }

        movingPlatform.HitPlatform = hit.collider.transform;
        _movement.HitPoint = hit.point;
        _movement.FrameVelocity = Vector3.zero;
    }

    private IEnumerator<WaitForFixedUpdate> SubtractNewPlatformVelocity()
    {
        // When landing, subtract velocity of the new ground from the character's velocity
        // since movement in ground is relative to the movement of the ground.
        if (!movingPlatform.enabled ||
            (movingPlatform.movementTransfer != MovementTransferOnJump.InitTransfer &&
                movingPlatform.movementTransfer != MovementTransferOnJump.PermaTransfer)) yield break;

        // If we land on a new platform, we have to wait for two FixedUpdates
        // before we know the velocity of the platform under the character
        if (movingPlatform.NewPlatform)
        {
            var platform = movingPlatform.ActivePlatform;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            if (_grounded && platform == movingPlatform.ActivePlatform)
                yield return new WaitForFixedUpdate();
        }
        _movement.Velocity -= movingPlatform.PlatformVelocity;
    }

    private bool MoveWithPlatform()
    {
        return (movingPlatform.enabled &&
                (_grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked) &&
                movingPlatform.ActivePlatform != null);
    }

    private Vector3 GetDesiredHorizontalVelocity()
    {
        // Find desired velocity
        var desiredLocalDirection = _tr.InverseTransformDirection(InputMoveDirection);
        var maxSpeed = MaxSpeedInDirection(desiredLocalDirection);

        if (!_grounded) return _tr.TransformDirection(desiredLocalDirection*maxSpeed);

        // Modify max speed on slopes based on slope speed multiplayer curve
        var movementSlopeAngle = Mathf.Asin(_movement.Velocity.normalized.y)*Mathf.Rad2Deg;
        maxSpeed *= _movement.SlopeSpeedMultiplier.Evaluate(movementSlopeAngle);
        return _tr.TransformDirection(desiredLocalDirection*maxSpeed);
    }

    private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
    {
        var sideways = Vector3.Cross(Vector3.up, hVelocity);
        return Vector3.Cross(sideways, groundNormal).normalized*hVelocity.magnitude;
    }

    private bool IsGroundedTest()
    {
        return (_groundNormal.y > 0.01);
    }

    float GetMaxAcceleration(bool grounded)
    {
        // Maximum acceleration on ground and in air
        return grounded ? _movement.MaxGroundAcceleration : _movement.MaxAirAcceleration;
    }

    float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2*targetJumpHeight*_movement.Gravity);
    }

    bool IsJumping()
    {
        return jumping.Jumping;
    }

    bool IsSliding()
    {
        return (_grounded && _sliding.enabled && TooSteep());
    }

    bool IsTouchingCeiling()
    {
        return (_movement.CollisionFlags & CollisionFlags.CollidedAbove) != 0;
    }

    bool IsGrounded()
    {
        return _grounded;
    }

    bool TooSteep()
    {
        return (_groundNormal.y <= Mathf.Cos(_controller.slopeLimit*Mathf.Deg2Rad));
    }

    Vector3 GetDirection()
    {
        return InputMoveDirection;
    }

    void SetControllable(bool controllable)
    {
        CanControl = controllable;
    }

    // Project a direction onto elliptical quater seqments based on forward, sideways, and backwards speed.
    // The function returns the length of the resulting vector
    float MaxSpeedInDirection(Vector3 desiredMovementDirection)
    {
        if (desiredMovementDirection == Vector3.zero)
            return 0;

        var zAxisEllipseMultiplier = (desiredMovementDirection.z > 0
            ? _movement.MaxForwardSpeed
            : _movement.MaxBackwardsSpeed)/_movement.MaxSidewaySpeed;
        var temp =
            new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z/zAxisEllipseMultiplier).normalized;
        var length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * _movement.MaxSidewaySpeed;
        return length;
    }

    void SetVelocity(Vector3 velocity)
    {
        _grounded = false;
        _movement.Velocity = velocity;
        _movement.FrameVelocity = Vector3.zero;
        SendMessage("OnExternalVelocity");
    }
}