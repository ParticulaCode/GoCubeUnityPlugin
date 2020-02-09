using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymorph.Unity.MVVM;
using Polymorph.Unity.Core;
using System;

namespace Particula.Cube {

    public class CubeProvider : AdvancedBehaviour {

		public Action<CubeProvider, Rotation> onRotate;

		public virtual ICube cubeModel { get { return null; } }

    }
   
}
