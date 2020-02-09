using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dweiss {

	public class RotateAngleSkip : MonoBehaviour {
        public float angle;
        public float timeBetweenAngleSkip;
        public Vector3 rotateAxis = Vector3.forward;

        private Transform t;

        private void Awake()
        {
            t = transform;
        }

        private void OnEnable()
        {
            StartCoroutine(Rotate());
        }
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator Rotate() {
            while (true)
            {
                t.Rotate(rotateAxis, angle);
                yield return new WaitForSeconds(timeBetweenAngleSkip);
            }	
		}
		
	}
}
