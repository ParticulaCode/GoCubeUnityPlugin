using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Particula.Cube {

	public class SpinAxis : MonoBehaviour {
	
	public Axis x;
        public Axis y;
        public Axis z;

        public Axis currentAxis;

        public bool SpinIsAllowed(FaceView face) {
            if(currentAxis == null) {
                return true;
            } else if(currentAxis.Check(face)) {
                return true;
            } else {
                if(currentAxis.locked) {
                    currentAxis.Expedite();
                    return false;
                } else {
                    return true;
                }
            }
        }

        public bool RotateAxis(FaceView face, PieceView[] edges, float angle) {
            var retVal = SpinIsAllowed(face);

            if(retVal) {
				Debug.Log("retVal of spin is not null");
				currentAxis = GetAxis(face);
                face.Spin(edges, angle);
            }
			else
			{
				Debug.Log("retVal of spin is null");
			}
            return retVal;
        }

        public Axis GetAxis(FaceView face) {
            if(x.Check(face)) {
                return x; 
            } else if(y.Check(face)) {
                return y;
            } else if(z.Check(face)) {
                return z;
            } else {
                Debug.LogError("Could not find face in SpinAxes");
                return null;
            }
        }
	}
}
