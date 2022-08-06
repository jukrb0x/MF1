using System;
using UnityEngine;

namespace Gameplay.Ragdoll.Core
{
    /// <summary>
    ///  the ragdoll will be killed when in KillY
    /// </summary>
    public class RagdollLife : RagdollCore
    {
        private GameObject  _stabilizer;
        public  Transform   animatedTorso;
        // public  GameObject  gameManagerObj;
        public  float       killY = 10f; // the value which animated body below will be killed
        public  bool        isWasted;
        // private GameManager _gameManager;

        protected override void Start()
        {
            base.Start();
            // ref validation
            if (animatedTorso == null)
                animatedTorso = ragdoll.ragdollBody.animatedTorso;

            TryGetStabilizer();
            // fixme: head shaking after restart game
            // workaround: reset the animation
            ragdoll.gameObject.GetComponent<RagdollAnimation>().enabled = false;
            ragdoll.gameObject.GetComponent<RagdollAnimation>().enabled = true;


        }
        private void OnValidate()
        {
            if (ragdoll)
            {
                if (animatedTorso == null)
                    animatedTorso = ragdoll.ragdollBody.animatedTorso;
            }
#if playmode
            if(isWasted) KillRagdoll();
#endif
        }
        private void TryGetStabilizer()
        {
            _stabilizer = ragdoll.ragdollPhysics.stabilizer;
        }

        public void KillRagdoll()
        {
            if (isWasted) return;
            _stabilizer.SetActive(false);
            isWasted = true;
            GameManager.Instance.GameOver();
        }

        // Game Manager Input Press R will reload the scene
        public void Revitalize()
        {
            if (!isWasted) return;
            _stabilizer.SetActive(true);
            isWasted = false;
            GameManager.Instance.RestartGame();
        }

        private void Update()
        {
            if (_stabilizer == null) TryGetStabilizer(); // i know this is bad, just workaround...
            if (!isWasted && animatedTorso.position.y < -killY)
            {
                KillRagdoll();
            }
        }


        
        
        
        
        
        
        
    }
}
