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
        private bool canAttack = false;
        private bool started = false;

        IEnumerator StartPhase3CoolDown()
        {
            currentMoveType = SelectMoveType();
            ShowMoveHint();
            yield return new WaitForSeconds(cooldownTime);
            canAttack = true;
            HideMoveHint();
        }

        IEnumerator StartPhase1CoolDown()
        {
            currentMoveType = SelectMoveType(true);
            ShowMoveHint();
            yield return new WaitForSeconds(startingCooldown);
            canAttack = true;
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

        private bool dead = false;
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

        private void DashSlash()
        {
            canAttack = false;
            Debug.Log("Dashing slash");
            StartCoroutine(StartPhase3CoolDown());
        }

        private void JumpSlam()
        {
            canAttack = false;
            Debug.Log("Jumping slam");
            StartCoroutine(StartPhase3CoolDown());
        }

        private void Laser()
        {
            canAttack = false;
            Debug.Log("Laser");
            StartCoroutine(StartPhase3CoolDown());
        }

        private void Laser2()
        {
            canAttack = false;
            Debug.Log("Laser2");
            StartCoroutine(StartPhase3CoolDown());
        }

        private void Roll()
        {
            canAttack = false;
            Debug.Log("Roll");
            StartCoroutine(StartPhase3CoolDown());
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

            if (!canAttack) return;

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
