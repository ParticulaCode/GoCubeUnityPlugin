using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymorph.Unity.MVVM;

namespace Particula.Cube {

	public class CubeObjectView : View {

        public virtual string GetState() {
            return null;
        }

        public virtual void SetState(string state) {

        }
	}
}
