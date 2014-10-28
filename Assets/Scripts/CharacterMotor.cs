﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
    [AddComponentMenu("Character/Character Motor")]

    // Does this script currently respond to input?
    private bool _canControl = true;

    private bool _useFixedUpdate = true;

    // For the next variables, [NonSerialized] tells Unity to not serialize the variable or show it in the inspector view.
    // Very handy for organization!

    // The current global direction we want the character to move in.
    [NonSerialized] internal Vector3 _inputMoveDirection = Vector3.zero;

    // Is the jump button held down? We use this interface instead of checking
    // for the jump button directly so this script can also be used by AIs.
    [NonSerialized] internal bool _inputJump = false;


    class ChracterMotorMovement
    {
        //The maximum horizontal speed when moving
        private float maxForwardSpeedd = 10.0f;
        private float maxSidedwaysSpeed = 10.0f;
        private float maxBackwardsSpeed = 10.0f;

        // Curve for multiplying speed based on slope (negative = downwards)
        AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));

        // How fast does the character change speed? Higher is faster.
        private float maxGroundAcceleration = 30.0f;
        private float maxAirAcceleration = 20.0f;

        //The gravity for the character
        private float gravity = 10.0f;
        private float maxFallSpeed = 20.0f;

        // For the next variables, [NonSerialized] tells Unity to not serialize the variable or show it in the inspector view.
        // Very handy for organization!

        // The last collision flag returned from controller.Move
        [NonSerialized]
        private CollisionFlags collisionFlags;

        // We will keep track of the character's current velocity
        [NonSerialized]
        private Vector3 velocity;

        // This keeps track of out current velocity while we're not grounded
        [NonSerialized]
        private Vector3 frameVelocity;

        [NonSerialized]
        private Vector3 hitPoint = Vector3.zero;

        [NonSerialized]
        Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
    }

    CharacterMotorMovement movement = new CharacterMotorMovement();

    enum MovementTransferOnJump
    {
        None,           // The jump is not affected by velocity of floor at all.
        InitTransfer,   // Jump gets its initial velocity from the floor, the gradually comes to a stop.
        PermaTransfer,  // Jump gets its inital velocity from the floor, and keeps the velocity until landing.
        PermaLocked     // Jump is relative to the movement of the last touched floor and will move together with that floor.
    }

    // We will contain all jumping related variables in one helper class for clarity
    class CharacterMotorJumping
    {
        // Can the chracter jump?
        internal bool enabled = true;

        // How high do we jump when pressing jump and letting go immediately
        internal float baseHeight = 1.0f;

        // We add extraHeight units (meters) on top when holding the button down longer while jumping
        internal float extraHeight = 4.1f;

        // How much does the character jump out perpendicular to the sirface on walkable surfaces?
        // 0 means a fully vertical jump and 1  means fully perpendicular
        internal float perpAmount = 0.0f;

        // How much does the character jump out perpendicular to the surface on too steep surfaces?
        // 0 means a fully vertical jump and 1 means fully perpendicular
        internal float steepPerpAmount = 0.5f;

        // For the next variables, [NonSerialized] tells Unity to not serialize the variable or show it in the inspector view.
        // Very handy for organization!

        // Are we jumping? (Instantiated with jump button and not grounded yet)
        // To see if we are just in the air (initiated by jumping OR falling) see the grounded variable
        [NonSerialized] internal bool Jumping;

        internal bool HoldingJumpButton;

        // The time we jump at (Used to determine how log to apply extra jump power after jumping.)
        [NonSerialized] internal float LastStartTime;

        [NonSerialized] internal float LastButtonDownTime = -100;

        [NonSerialized] internal Vector3 JumpDir = Vector3.up;
    }

    CharacterMotorJumping jumping = new CharacterMotorJumping();

    class CharacterMotorMovingPlatform
    {
        internal bool enabled = true;

        internal MovementTransferOnJump movementTransfer = MovementTransferOnJump.PermaTransfer;

        [NonSerialized] internal Transform HitPlatform;

        [NonSerialized] internal Transform ActivePlatform;

        [NonSerialized] internal Vector3 activeLocalPoint;

        [NonSerialized] internal Vector3 ActiveGlobalPoint;

        [NonSerialized] internal Quaternion ActiveLocalRotation;

        [NonSerialized] internal Quaternion ActiveGlobalRotation;

        [NonSerialized] internal Matrix4x4 LastMatrix;

        [NonSerialized] internal Vector3 PlatformVelocity;

        [NonSerialized] internal bool NewPlatform;
    }

    CharacterMotorMovingPlatform movingPlatform = new CharacterMotorMovingPlatform();

    class CharacterMotorSliding
    {
        // Does character slide on too steep surfaces?
        internal bool enabled = true;

        // How fast does the character slide on steep surfaces?
        internal float slidingSpeed = 15;

        // How much can the playyer control the sliding direction?
        // If the value is 0.5 the player can slideways with half the speed of the downwards sliding speed.
        internal float sidewaysControl = 1.0f;

        // How much can the player influence the sliding speed?
        // If the value is 0.5 the player can speed the slding up to 150 % or slow it down to 50%.
        internal float speedControl = 0.4f;
    }

    CharacterMotorSliding sliding = new CharacterMotorSliding();

    [NonSerialized] private bool _grounded = true;

    [NonSerialized] private Vector3 _groundNormal = Vector3.zero;

    Vector3 _lastGroundNormal = Vector3.zero;

    private Transform _tr;

    private CharacterController _controller;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _tr = transform;
    }

    private void UpdateFunction()
    {
        // We copy the actual velocity into a temporary variable that we can manipulate
        var velocity = movement.velocity;

        // Update velocity based on input
        velocity = ApplyInputVelocityChange(velocity);

        // Apply gravity and jumping force
        velocity = ApplyGravityAndJumping(velocity);

        // Moving platform support
        Vector3 moveDistance;
        if (MoveWithPlatform())
        {
            var newGlobalPoint = movingPlatform.ActivePlatform.TransformPoint(movingPlatform.activeLocalPoint);
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
        movement.collisionFlags = _controller.Move(currentMovementOffset);

        movement.lastHitPoint = movement.hitPoint;
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
        movement.velocity = (_tr.position - lastPosition)/Time.deltaTime;
        var newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);

        // The CharacterController can be moved in unwanted directions when colliding with things.
        // We ant to prevent this from inluencing the recorded velocity.
        if (oldHVelocity == Vector3.zero)
        {
            movement.velocity = new Vector3(0, movement.velocity.y, 0);
        }
        else
        {
            var projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity)/oldHVelocity.sqrMagnitude;
            movement.velocity = oldHVelocity*Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y*Vector3.up;
        }

        if (movement.velocity.y < velocity.y - 0.001)
        {
            if (movement.velocity.y < 0)
            {
                // Somthing is forcing tthe CharacterController down faster than it should.
                // Ignore this
                movement.velocity.y = velocity.y;
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
                movement.frameVelocity = movingPlatform.PlatformVelocity;
                movement.velocity += movingPlatform.PlatformVelocity;
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
            movingPlatform.activeLocalPoint = movingPlatform.ActivePlatform.InverseTransformPoint(movingPlatform.ActiveGlobalPoint);

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
                            movingPlatform.activeLocalPoint) -
                         movingPlatform.LastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint))/Time.deltaTime;
                }
                movingPlatform.LastMatrix = movingPlatform.ActivePlatform.localToWorldMatrix;
                movingPlatform.NewPlatform = false;
            }
            else { movingPlatform.PlatformVelocity = Vector3.zero; }
        }

        if (_useFixedUpdate)
            UpdateFunction();
    }

    void Update()
    {
        if(!_useFixedUpdate)
            UpdateFunction();
    }

    private Vector3 ApplyInputVelocityChange(Vector3 velocity)
    {
        if(!_canControl)
            _inputMoveDirection = Vector3.zero;

        //Find desired velocity
        Vector3 desiredVelocity;
        if (_grounded && TooSteep())
        {
            // the direction we're sliding in
            desiredVelocity = new Vector3(_groundNormal.x, 0, _groundNormal.z).normalized;
            // Find the input movement direction projected onto the sliding direction
            var projectedMoveDir = Vector3.Project(_inputMoveDirection, desiredVelocity);
            // Add the sliding direction, the speed control, and the sideways control vectors
            desiredVelocity = desiredVelocity + projectedMoveDir*sliding.speedControl +
                              (_inputMoveDirection - projectedMoveDir)*sliding.sidewaysControl;
            // Multiplay with the slidding speed
            desiredVelocity *= sliding.slidingSpeed;
        }
        else
        {
            desiredVelocity = GetDesiredHorizontalVelocity();
        }

        if (movingPlatform.enabled && movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
        {
            desiredVelocity += movement.frameVelocity;
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
        if (_grounded || _canControl)
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
        if (!_inputJump || !_canControl)
        {
            jumping.HoldingJumpButton = false;
            jumping.LastButtonDownTime = -100;
        }

        if (_inputJump && jumping.LastButtonDownTime < 0 && _canControl)
        {
            jumping.LastButtonDownTime = Time.time;
        }

        if (_grounded)
            velocity.y = Mathf.Min(0, velocity.y) - movement.gravity*Time.deltaTime;
        else
        {
            velocity.y = movement.velocity.y - movement.gravity*Time.deltaTime;

            // When jumping up we don't apply gravity for some time when the is holding the jump button.
            // This gives more control over jump height by pressing the button longer
            if (jumping.Jumping && jumping.HoldingJumpButton)
            {
                // Calculate the duration that the extra jump force should have effect.
                // If we're still less than that duration after the jumoing time, apply the force.
                if (Time.time <
                    jumping.LastStartTime + jumping.extraHeight/CalculateJumpVerticalSpeed(jumping.baseHeight))
                {
                    velocity += jumping.JumpDir*movement.gravity*Time.deltaTime;
                }
            }

            // Make sure we don't fall any faster than maxFallSpeed. This gives our character a terminal velocity.
            velocity.y = Mathf.Max(velocity.y, -movement.maxFallSpeed);
        }

        if (_grounded)
        {
            // Jump only if the jump button was pressed down in the last 0.2 seconds.
            // We use this check instead of checking if it's pressed down right now
            // because players will often try to jump in the exact moment when hitting the ground after a jump
            // and if they hit the button a fraction of a second too soon and no new jump happens as a consequence,
            // it's confusing and it feels like the game is buggy.
            if (jumping.enabled && _canControl && (Time.time - jumping.LastButtonDownTime < 0.2))
            {
                _grounded = false;
                jumping.Jumping = true;
                jumping.LastStartTime = Time.time;
                jumping.LastButtonDownTime = -100;

                // Calculate the jumpingg direction
                if (TooSteep())
                    jumping.JumpDir = Vector3.Slerp(Vector3.up, _groundNormal, jumping.steepPerpAmount);
                else
                {
                    jumping.JumpDir = Vector3.Slerp(Vector3.up, _groundNormal, jumping.perpAmount);
                }

                // Apply the jumping force to the velocity. Cancel any vertical velocity first.
                velocity.y = 0;
                velocity += jumping.JumpDir*CalculateJumpVerticalSpeed(jumping.baseHeight);

                // Apply inertia from platform
                if (movingPlatform.enabled &&
                    (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                     movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
                    )
                {
                    movement.frameVelocity = movingPlatform.PlatformVelocity;
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

        if ((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001 || _lastGroundNormal == Vector3.zero)
            _groundNormal = hit.normal;
        else
        {
            _groundNormal = _lastGroundNormal;
        }

        movingPlatform.HitPlatform = hit.collider.transform;
        movement.hitPoint = hit.point;
        movement.frameVelocity = Vector3.zero;
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
        movement.velocity -= movingPlatform.PlatformVelocity;
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
        var desiredLocalDirection = _tr.InverseTransformDirection(_inputMoveDirection);
        var maxSpeed = MaxSpeedInDirection(desiredLocalDirection);

        if (!_grounded) return _tr.TransformDirection(desiredLocalDirection*maxSpeed);

        // Modify max speed on slopes based on slope speed multiplayer curve
        var movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y)*Mathf.Rad2Deg;
        maxSpeed *= movement.slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
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
        return grounded ? movement.maxGroundAcceleration : movement.maxAirAcceleration;
    }

    float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards speed 
	    // for the character to reach at the apex.
        return Mathf.Sqrt(2*targetJumpHeight*movement.gravity);
    }

    bool IsJumping()
    {
        return jumping.Jumping;
    }

    bool IsSliding()
    {
        return (_grounded && sliding.enabled && TooSteep());
    }

    bool IsTouchingCeiling()
    {
        return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;
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
        return _inputMoveDirection;
    }

    void SetControllable(bool controllable)
    {
        _canControl = controllable;
    }

    // Project a direction onto elliptical quater seqments based on forward, sideways, and backwards speed.
    // The function returns the length of the resulting vector
    float MaxSpeedInDirection(Vector3 desiredMovementDirection)
    {
        if (desiredMovementDirection == Vector3.zero)
            return 0;

        var zAxisEllipseMultiplier = (desiredMovementDirection.z > 0
            ? movement.maxForwardSpeed
            : movement.maxBackwardsSpeed)/movement.maxSidewaysSpeed;
        var temp =
            new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z/zAxisEllipseMultiplier).normalized;
        var length = new Vector3(temp.x, 0, temp.z*zAxisEllipseMultiplier).magnitude*movement.maxSidewaysSpeed;
        return length;
    }

    void SetVelocity(Vector3 velocity)
    {
        _grounded = false;
        movement.velocity = velocity;
        movement.frameVelocity = Vector3.zero;
        SendMessage("OnExternalVelocity");
    }
}
