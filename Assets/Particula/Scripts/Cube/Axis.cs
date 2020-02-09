using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Particula.Cube {

	public class Axis : MonoBehaviour {

        public FaceView positive;
        public FaceView negative;

        public bool locked { get { return positive.locked || negative.locked; } }

        public bool Check(FaceView face) {
            return (positive == face) || (negative == face);
        }
        public void Expedite() {
            positive.Expedite();
            negative.Expedite();
        }
        public override string ToString() {
            return positive + " " + negative;
        }
	}
}
