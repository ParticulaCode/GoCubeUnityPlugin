using UnityEngine;
using System;
using Polymorph.Unity.MVVM;
using Particula.Common;

namespace Particula.Cube {

    public class EdgeViewModel : PieceViewModel {
        public EdgeViewModel(IEdge edge) : base(edge) { }
    }
}