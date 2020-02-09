using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymorph.Unity.Core;

namespace Particula.Cube {

	public class EdgeColor : AdvancedBehaviour {

		public int id;
		public MeshRenderer edgeRenderer;

        public bool debug = false;

        Color currentColor;

        Color normal;
        Color dark;

		public void SetMaterial(Material material){
			edgeRenderer.material = material;
            normal = material.color;
            dark = Color.Lerp(normal, Color.black, 0.85f);
            currentColor = normal;
		}

        public void ChangeColor(bool darken = false, float time = 0) {
            StartCoroutine(ChangeColorCo(darken ? dark : normal, time));
        }

        IEnumerator ChangeColorCo(Color goal, float time) {

            yield return new AquireDriveThroughSemaphore();

            var start = edgeRenderer.material.color;
            var totalTime = time;

            while(time > 0) {
                yield return null;
                time -= Time.deltaTime;
                edgeRenderer.material.color = Color.Lerp(start, goal, 1 - (time / totalTime));
            }
            edgeRenderer.material.color = goal;
        }
	}
}
