using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Dweiss {

	public class AlpahFadeInOut : MonoBehaviour {

        public float fadeOutTime = .5f;
        public float fadeInTime = .5f;

        public UnityEngine.CanvasRenderer rndr;

        private void Awake()
        {
            if(rndr == null) rndr = GetComponent<CanvasRenderer>();
        }

        private Color GetColor()
        {
            return rndr.GetColor();
            //var clr = new Color(material.color.r, material.color.g, material.color.b);
        }
        private void SetColor(Color clr)
        {
            rndr.SetColor(clr);
        }



        private void OnEnable()
        {
            StartCoroutine(Fade());
        }
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator Fade() {

            while (true)
            {
                yield return FadePhase(fadeInTime, 0, 1);
                yield return FadePhase(fadeOutTime, 1, 0);
            }
			
		}

        private IEnumerator FadePhase(float length, float startAlpha, float endAlpha)
        {
            var startTime = Time.time;
            var endTime = Time.time + length;
            var clr = GetColor();
            while (Time.time < endTime)
            {
                var percent = (Time.time - startTime) / length;
                clr.a = percent * (endAlpha - startAlpha) + startAlpha;
                SetColor(clr);
                yield return 0;
            }
        }
	}
}
