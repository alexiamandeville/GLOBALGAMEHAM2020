using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class ScroungeHotspot : MonoBehaviour
    {
        [SerializeField] protected float minRadius;
        [SerializeField] protected float maxRadius;

        [SerializeField] protected float scroungeSuccesRad;
        [SerializeField] protected float scroungeTime;

        [SerializeField] protected GameObject scroungeSuccessVFX;

        private float timer = 0.0f;
        private float rad;
        private bool bTriedThisFrame = false; // this is fucking horrible

        public bool IsConsumed
        {
            get
            {
                return timer >= scroungeTime;
            }
        }  

        void Start()
        {
            Reset();
        }

		public void Reset()
		{
            rad = Random.Range(minRadius, maxRadius);
            GetComponent<SphereCollider>().radius = rad;

            timer = 0;
		}

		private void LateUpdate()
		{
            if( !bTriedThisFrame && !IsConsumed )
            {
                timer = Mathf.Max(0, timer - Time.deltaTime);
            }
            bTriedThisFrame = false;
		}

		// returns scaled dist to succes (0-1)
		public float TryScrounge(Vector3 playerpos, PlayerInput playerInput)  //playerinput to ge a playercontroller to give it something later
        {

            if (timer >= scroungeTime)
            {
                return 0;
            }


            float dist = (transform.position - playerpos).magnitude;
            if( dist < scroungeSuccesRad )
            {
                timer += Time.deltaTime;
                bTriedThisFrame = true;

                if (timer >= scroungeTime)
                {
                    Debug.Log("SCROUNGE SUCCESS!!!");
                    Instantiate(scroungeSuccessVFX, playerpos, Quaternion.identity);

                    return 1.0f;
                }
            }


            float distratio = dist / rad;
            return distratio * distratio;
        }
    }
}