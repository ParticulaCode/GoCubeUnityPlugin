using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Particula.Common;
using System;
using Polymorph.Unity.MVVM;

namespace Particula.Cube {

	public class FaceViewModel : PieceViewModel, Particula.Common.IFace {

        IFace face;

        float absoluteOrientation;
        public int id { get { return face.id; } }
        public float angle { get; private set; }
        public event Action<FaceViewModel> onAngleChanged;
        public event Action<FaceViewModel> onResetAngle;

        public IPiece this[int i] {
            get { return face.GetEdge(i); }
        }

        public FaceViewModel(CubeViewModel cube, IFace face) : base(face) {
            this.face = face;
            absoluteOrientation = face.absoluteOrientation;
            angle = face.orientation;
            face.onOrientationChanged += OrientationChanged;
        }

        public override void Dispose() {
            base.Dispose();
            face.onOrientationChanged -= OrientationChanged;
        }

        void OrientationChanged(IFace face, float newOrientation) {
            if(absoluteOrientation != face.absoluteOrientation) {
                absoluteOrientation = face.absoluteOrientation;
                angle = absoluteOrientation;
            } else {
                angle = face.orientation;
            }
            if(onAngleChanged != null) {
                onAngleChanged(this);
            }
        }

        public bool Check(int id) {
            return face.id == id;
        }

        public override string ToString() {
            return "Face(" + face.id + ")";
        }

        public string PrintFace() {
            return face.ToString();
        }

        public override bool Equals(object obj) {
            if(obj is IFace) {
                return (obj as IFace) == face;
            } else {
                return base.Equals(obj);
            }
        }
	}
}
