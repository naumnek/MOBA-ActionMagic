using System;
using System.Collections;
using UnityEngine;
using Platinum.Info;

public enum MoveState
{
    Idle,
    Walk,
    Sprint,
    Crouch
}
namespace Platinum.Controller
{
    [RequireComponent(typeof(CharacterController),typeof(Animator),typeof(AudioSource))]
    public class Movement : MonoBehaviour
    {
        [Header("General")]
        public bool DisableJump;

        public bool DisableSprint;
        public bool DisableCrouch;
        public bool VelocityMovementAnimation;
        
        #region BodyParemeters

        [Header("Physics")] public float returnForce = 2.0f;
        public float pushPower = 2.0f;

        [Header("Fall")] public float gravity = 20f;
        public float delayFallAnimation = 0.5f;
        public float KillHeight = -50f;

        [Header("Jump")] public float delayObstacleCheck = 0.2f;
        public float intervalJumps = 1.25f;
        public float jumpHeight = 3f;
        public float airControl = 6f;
        public float jumpDump = 0.5f;

        [Header("Movement")] public float stepDown = 0.1f;
        public float groundSpeed = 2f;
        public float lateralSpeedMultiplier = 1f;
        public float sprintSpeedMultiplier = 1.3f;
        public float crouchSpeedMultiplier = 0.8f;
        public float animationSpeedMultiplier = 1f;

        #endregion

        [Header("Audio")]
        public float audioFootstepSpeedMultiplier = 2f;
        public AudioClip FootstepSfx;
        public AudioClip JumpSfx;
        public AudioClip LandSfx;
        public AudioClip FallDamageSfx;

        public CharacterController CharacterController { get; private set; }
        public AudioSource AudioSource { get; private set; }
        public Animator Animator{ get; private set; }
        public Transform Body { get; private set; }
        public bool isJumping { get; private set; }
        
        private ControllerBase ControllerBase;

        private bool stoppedMove;
        
        protected bool waitJump;
        protected float currentGroundSpeed;
        private Vector3 rootMotion;
        private Vector3 velocity;
        private float m_FootstepDistanceCounter;
        private bool isHits;
        private Vector3 lastHitDirection;
        private bool obstacleCheck;
        protected MoveKeycode moveKeycode;

        protected void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            AudioSource = GetComponent<AudioSource>();
            Animator = GetComponent<Animator>();
            ControllerBase = GetComponentInParent<ControllerBase>();
            
            Animator.applyRootMotion = VelocityMovementAnimation;
            
            Body = transform;
            
            ControllerBase.onDie += OnDie;
            ControllerBase.onRespawn += OnRespawn;
        }

        public void SetMovementState(MoveKeycode keycode)
        {
            moveKeycode = stoppedMove ? new MoveKeycode() : keycode;
            if (stoppedMove) return;
                
            if (keycode.look.magnitude == 0f || !keycode.RotateInMove)
            {
                keycode.look = keycode.move;
                keycode.move = new Vector2(0, keycode.move.magnitude);
            }
            
            Jump(keycode.jump);
            Rotate(keycode.look);
            Move(keycode.move, keycode.MoveState);
        }

        protected void Rotate(Vector2 look)
        {
            if (look.magnitude == 0) return;
            
            float yawMove = Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg;
            Quaternion newRotation = Quaternion.Euler(0,yawMove,0);
            Body.rotation = newRotation;
        }
        
        protected void Move(Vector2 moveInput, MoveState moveState)
        {
            UpdateSpeed(moveInput, moveState);

            Animator.SetFloat("InputX", moveInput.x);
            Animator.SetFloat("InputY", moveInput.y);
        }
        
        protected virtual void Jump(bool jump)
        {
            if(jump || DisableJump || !waitJump || isJumping) return;
            
            waitJump = false;
            float jumpVelocity = Mathf.Sqrt(2 * gravity * jumpHeight);
            SetInAir(jumpVelocity);
            AudioSource.PlayOneShot(JumpSfx);
            Animator.SetBool("isJumping", true);
            StartCoroutine(WaitJump());
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!CharacterController.enabled) return;
            
            lastHitDirection = hit.moveDirection;
            Rigidbody body = hit.collider.attachedRigidbody;

            // no rigidbody
            if (body == null || body.isKinematic)
                if (isJumping && !isHits && obstacleCheck)
                {
                    isHits = true;
                    ReturnHitImpulse(lastHitDirection);
                }
            return;

            // We dont want to push objects below us
            if (hit.moveDirection.y < -0.3f)
                return;

            // Calculate push direction from move direction,
            // we only push objects to the sides never up and down
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.velocity = pushDir * pushPower;
        }

        private void OnAnimatorMove()
        {
            rootMotion += Animator.deltaPosition;
        }

        private void UpdateSpeed(Vector2 moveInput,MoveState moveState)
        {
            float requiredSpeed = groundSpeed;
            switch (moveState)
            {
                case MoveState.Sprint:
                    requiredSpeed *= DisableSprint ? 1 : sprintSpeedMultiplier;
                    break;
                case MoveState.Crouch:
                    requiredSpeed *= DisableCrouch ? 1 : crouchSpeedMultiplier;
                    break;
                default:
                    requiredSpeed = groundSpeed;
                    break;
            }

            currentGroundSpeed = moveInput.sqrMagnitude > 1 ? requiredSpeed * lateralSpeedMultiplier : requiredSpeed;

            Animator.speed = VelocityMovementAnimation ? currentGroundSpeed * animationSpeedMultiplier : Animator.speed;
            
            Animator.SetBool("isCrouching", moveState == MoveState.Crouch);
        }
        
        private void ReturnHitImpulse(Vector3 moveDirection)
        {
            Vector3 pushDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
            velocity = pushDirection * returnForce;
        }

        private void AudioFootstep()
        {
            if (!ControllerBase.isUser || isJumping || moveKeycode.move == Vector2.zero) return;

            AudioSource.volume = currentGroundSpeed / 5f;
            AudioSource.pitch = currentGroundSpeed / 5f;
            // keep track of distance traveled for footsteps sound
            float characterVelocity = currentGroundSpeed * audioFootstepSpeedMultiplier * Time.deltaTime;

            m_FootstepDistanceCounter += characterVelocity;

            if (m_FootstepDistanceCounter >= 1f)
            {
                m_FootstepDistanceCounter = 0f;
                AudioSource.PlayOneShot(FootstepSfx);
            }
        }

        protected void Update()
        {
            AudioFootstep();
        }
        
        private void FixedUpdate()
        {
            if (!CharacterController.enabled) return;

            if (isJumping) //IsAir state
            {
                UpdateInAir();
            }
            else //IsGrounded State
            {
                UpdateOnGround();
            }

            IsKillHeightPlayer();
        }

        private void IsKillHeightPlayer()
        {
            // check for Y kill
            if (Body.position.y < KillHeight)
            {
                ControllerBase.Health.Kill();
            }
        }

        private void UpdateInAir()
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
            Vector3 displacement = velocity * Time.fixedDeltaTime;
            if (!isHits) displacement += CalculateAir();
            CharacterController.Move(displacement);
            isJumping = !CharacterController.isGrounded;
            rootMotion = Vector3.zero;

            if (waitFall)
            {
                waitFall = false;
                StartCoroutine(FallAnimation());
            }
            if (!isJumping && Animator.GetBool("isJumping"))
            {
                obstacleCheck = false;
                isHits = false;
                Animator.SetBool("isJumping", isJumping);
                AudioSource.PlayOneShot(LandSfx);
            }
        }

        private bool waitFall = true;
        private IEnumerator FallAnimation()
        {
            yield return new WaitForSeconds(delayFallAnimation);
            Animator.SetBool("isJumping", isJumping);
            waitFall = true;
        }

        private void UpdateOnGround()
        {
            Vector3 stepForwardAmount = rootMotion * currentGroundSpeed;
            Vector3 stepDownAmount = Vector3.down * stepDown;
            Vector3 velocityMoveAmount = Vector3.up * stepDown;

            CharacterController.Move(stepForwardAmount + stepDownAmount);
            rootMotion = Vector3.zero;

            if (!CharacterController.isGrounded)
            {
                SetInAir(0);
            }
        }

        private IEnumerator WaitJump()
        {
            yield return new WaitForSeconds(delayObstacleCheck);
            obstacleCheck = true;
            yield return new WaitForSeconds(intervalJumps);
            waitJump = true;
        }

        private void SetInAir(float jumpVelocity)
        {
            isJumping = true;
            velocity = Animator.velocity * currentGroundSpeed * jumpDump;
            velocity.y = jumpVelocity;
            if (jumpVelocity == 0) ReturnHitImpulse(lastHitDirection);
        }

        private Vector3 CalculateAir()
        {
            return ((Body.forward * moveKeycode.move.y) + (Body.right * moveKeycode.move.x)) * (airControl / 100);
        }

        public void SetDirection(Vector2 direction)
        {
            Rotate(direction);
        }
        public void SetStoppedMove(float time)
        {
            if (stoppedMove) return;
            
            if (time > 0)
            {
                stoppedMove = true;
                Invoke(nameof(UnstoppedMove),time);
            }
        }

        private void UnstoppedMove() => stoppedMove = false;

        /*
        public void OrientTowards(Vector2 direction)
        {
            float yawMove = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Quaternion newRotation =  Quaternion.Euler(0,yawMove,0);
            UpdateRotation(newRotation);
        }
        */

        protected virtual void OnDie()
        {
            CharacterController.enabled = false;
            stoppedMove = true;
            currentGroundSpeed = groundSpeed;
            waitJump = true;
        }

        protected virtual void OnRespawn()
        {
            CharacterController.enabled = true;
            stoppedMove = false;
        }
    }
}
    