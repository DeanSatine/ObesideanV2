using System.Collections;
using UnityEngine;

namespace Boss_Enemy
{
    public class BossMech : MonoBehaviour
    {
        #region MoveType Defininitions and Functions
        
        enum MoveType
        {
            DashSlash = 0,
            JumpSlam = 1,
            Laser = 2,
            Laser2 = 3,
            Roll = 4
        }

        private MoveType currentMoveType;
        private MoveType previousMoveType;

        MoveType SelectMoveType(bool firstTime = false)
        {
            int randNum = Random.Range(0, 4);
            MoveType randMoveType =  (MoveType)randNum;

            if (!firstTime && randMoveType == previousMoveType)
            {
                randMoveType = SelectMoveType();
            }
            
            return randMoveType;
        }
        
        #endregion

        #region Cooldown Defininitions and Functions

        [SerializeField]
        private float startingCooldown = 10;
        [SerializeField]
        private float cooldownTime = 10;
        private bool canAttack;
        private bool started;
        private bool moveJustStarted;

        IEnumerator StartPhase3CoolDown()
        {
            currentMoveType = SelectMoveType();
            ShowMoveHint();
            yield return new WaitForSeconds(cooldownTime);
            canAttack = true;
            moveJustStarted = true;
            HideMoveHint();
        }

        IEnumerator StartPhase1CoolDown()
        {
            currentMoveType = SelectMoveType(true);
            ShowMoveHint();
            yield return new WaitForSeconds(startingCooldown);
            canAttack = true;
            moveJustStarted = true;
            HideMoveHint();
        }

        private void ShowMoveHint()
        {
            // todo: shows move hints in-game via holographic meshes
        }
        
        private void HideMoveHint()
        {
            // todo: shows move hints in-game via holographic meshes
        }

        #endregion

        #region Health and Death Defininitions

        private bool dead;
        [SerializeField]
        private float maxHealth = 100;
        private float health;

        private void DeathEffects()
        {
            dead = true;
            Debug.Log("Boss Mech Dead");
        }

        #endregion

        #region Moves Logic

        [SerializeField]
        private Transform playerTransform;
        
        private Vector3 capturedPlayerPosition;
        [SerializeField]
        private float dashSpeed = 60;

        private void DashSlash()
        {
            if (moveJustStarted)
            {
                Debug.Log("Dashing slash started");
                capturedPlayerPosition = playerTransform.position;
                capturedPlayerPosition.y = transform.position.y;
                moveJustStarted = false;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, capturedPlayerPosition, dashSpeed * Time.deltaTime);
            
            if (transform.position == capturedPlayerPosition)
            {
                canAttack = false;
                StartCoroutine(StartPhase3CoolDown());
                Debug.Log("Dashing slash ended!");
            }
        }

        [SerializeField]
        private float jumpHeight = 10;
        [SerializeField]
        private float jumpSpeed = 40;

        private int jumpPhase = 1; // 1 means it still needs to jump, 2 means it has to leap at the player
        private Vector3 jumpDestination; 

        private void JumpSlam()
        {
            if (moveJustStarted)
            {
                Debug.Log("Jump slam started");
                capturedPlayerPosition = playerTransform.position;
                capturedPlayerPosition.y = transform.position.y;
                jumpDestination = new Vector3(transform.position.x, transform.position.y + jumpHeight, transform.position.z);
                jumpPhase = 1;
                moveJustStarted = false;
            }

            if (jumpPhase == 1)
            {
                transform.position = Vector3.MoveTowards(transform.position, jumpDestination, jumpSpeed * Time.deltaTime);
                
                if (Mathf.Approximately(transform.position.y, jumpDestination.y))
                {
                    jumpDestination = capturedPlayerPosition;
                    jumpPhase = 2;
                }
            } 
            else if (jumpPhase == 2)
            {
                transform.position = Vector3.MoveTowards(transform.position, jumpDestination, dashSpeed * Time.deltaTime);
                
                if (transform.position == capturedPlayerPosition)
                {
                    canAttack = false;
                    Debug.Log("Jumping slam ended");
                    StartCoroutine(StartPhase3CoolDown());
                }
            }
        }
        
        [SerializeField]
        private float laserSpeed = 20f;

        private Quaternion destinationRotation;

        private void Laser() // 90 degree laser
        {
            if (moveJustStarted)
            {
                Debug.Log("Laser started");
                destinationRotation = Quaternion.Euler(
                    0,
                    Quaternion.LookRotation(transform.position - playerTransform.position).eulerAngles.y - 45,
                    0
                    );
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 45, 0);
                moveJustStarted = false;
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, destinationRotation, Time.deltaTime * laserSpeed);
            
            if (Mathf.RoundToInt(transform.rotation.eulerAngles.y) == Mathf.RoundToInt(destinationRotation.eulerAngles.y))
            {
                canAttack = false;
                Debug.Log("Laser ended");
                StartCoroutine(StartPhase3CoolDown());
            }
        }

        private void Laser2() // 170 degree laser (not 180 because equations don't work well in half of 360 cases)
        {
            if (moveJustStarted)
            {
                Debug.Log("Laser2 started");
                destinationRotation = Quaternion.Euler(
                    0,
                    Quaternion.LookRotation(transform.position - playerTransform.position).eulerAngles.y - 85,
                    0
                );
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 85, 0);
                moveJustStarted = false;
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, destinationRotation, Time.deltaTime * laserSpeed);
            
            if (Mathf.RoundToInt(transform.rotation.eulerAngles.y) == Mathf.RoundToInt(destinationRotation.eulerAngles.y))
            {
                canAttack = false;
                Debug.Log("Laser2 ended");
                StartCoroutine(StartPhase3CoolDown());
            }
        }

        private void Roll()
        {
            JumpSlam();
        }

        #endregion
        
        #region Start and Update

        private void Start()
        {
            health = maxHealth;
            canAttack = false;
            StartCoroutine(StartPhase1CoolDown());
        }

        private void Update()
        {
            if (dead) return; 
            if (health <= 0)
            {
                health = 0;
                DeathEffects();
                return;
            }

            if (!canAttack)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - playerTransform.position);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                return;
            }

            switch (currentMoveType)
            {
                case MoveType.DashSlash:
                    DashSlash();
                    break;
                case MoveType.JumpSlam:
                    JumpSlam();
                    break;
                case MoveType.Laser:
                    Laser();
                    break;
                case MoveType.Laser2:
                    Laser2();
                    break;
                case MoveType.Roll:
                    Roll();
                    break;
                default:
                    Debug.Log($"{currentMoveType} Move not recognized!");
                    break;
            }
            
            previousMoveType = currentMoveType;
        }
        
        #endregion

        #region Public Functions

        public void Damage(float damage)
        {
            health -= damage;
        }

        public bool IsDead()
        {
            return dead;
        }

        public float GetHealth()
        {
            return health;
        }

        public float GetHealthPercent()
        {
            // gives a number between 0 - 1
            return health / maxHealth;
        }

        #endregion
    }
}
