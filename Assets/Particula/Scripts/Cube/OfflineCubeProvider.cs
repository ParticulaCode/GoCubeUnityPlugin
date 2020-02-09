using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Particula.Cube {

    public class OfflineCubeProvider : CubeProvider {
        public virtual IOfflineCube offlineModel { get { return null; } }
    }
}
