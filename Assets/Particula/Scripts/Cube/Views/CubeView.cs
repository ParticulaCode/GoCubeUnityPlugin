using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Particula.Common;
using Polymorph.Unity.MVVM;
using Polymorph.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Particula.Cube {

    public class CubeView : View {

        [System.Serializable]
        public class ObjectState {
            public PieceView obj;
            public string state;
            public ObjectState(PieceView obj, string state) {
                this.obj = obj;
                this.state = state;
            }
            public void RevertToState() {
                obj.SetState(state);
            }
        }

        [System.Serializable]
        class SpinOperation {

            public bool consumed = true;
            public FaceView face;
            public PieceView[] edges;
            public float angle;

            public SpinOperation() {
                edges = new PieceView[8];
            }
        }

        new CubeViewModel viewModel { get { return base.viewModel as CubeViewModel; } }

        public bool debug = false;
        public bool followLogicRotation = true;
        public float spinFactor = 0.13f;
        public Vector3 noIMURotation;

        private bool isFixed = false;
        public Transform offset;
        public SpinAxis spinAxes;
        public PieceView[] edges;
        public FaceView[] faces;
        public Material[] colorMaterials;

        PieceView[] _combined;
        public PieceView[] combined {
            get {
                if(_combined == null) {
                    var length = edges.Length + faces.Length;
                    _combined = new PieceView[length];
                    var index = 0;
                    for(int i = 0; i < faces.Length; ++i) {
                        _combined[index] = faces[i];
                        ++index;
                    }
                    for(int i = 0; i < edges.Length; ++i) {
                        _combined[index] = edges[i];
                        ++index;
                    }
                }
                return _combined;
            }
        }


        [SerializeField]
        List<ObjectState> initialState;

		Dictionary<PieceViewModel, PieceView> crossRef;

        public bool lockOperations; // used to lock rotations when in hint mode
        SpinOperation currentOperation;

        [SerializeField]
        bool destroyed = false;

        internal bool rotating {
            get {
                foreach(var faceView in faces) {
                    if(faceView.rotating)
                        return true;
                }
                return false;
            }
        }

        private void Awake() {
            if(offset == null) {
                var offsetGo = new GameObject("Offset");
                offset = offsetGo.transform;
                offset.SetParent(transform);
                offset.localPosition = Vector3.zero;
                offset.SetParent(transform.parent);
                transform.SetParent(offset);
            }
        }

		//Operation op;
        void Update() {
            if(viewModel == null) { return; }
            if(followLogicRotation) {
                if(offset != null) {
                    offset.localRotation = Quaternion.Slerp(offset.localRotation, CubeViewModel.globalOffset, spinFactor * Time.deltaTime);
                }
                if(!isFixed) {
                    transform.localRotation = Quaternion.Slerp(transform.localRotation, viewModel.rotation, spinFactor * Time.deltaTime);
                }
            }

            if(lockOperations)
                return;
            if(currentOperation.consumed) {
                var op = viewModel.GetNextOperation(this);
                if(op != null) {
                    if(debug) {
                        Debug.Log("New Operation: " + op);
                    }
                    switch(op.type) {
                        case Operation.OpType.Rotation:
                            currentOperation.consumed = false;
                            currentOperation.face = crossRef[op.face] as FaceView;
                            currentOperation.angle = op.angle;
                            for(int i = 0; i < op.edges.Length; ++i) {
                                currentOperation.edges[i] = crossRef[op.edges[i]] as PieceView;
                            }
                            break;
                        case Operation.OpType.StateReset:
                            RebuildCube();
                            break;
                    }
                    Update();
                }
            } else {
               if(spinAxes.RotateAxis(currentOperation.face, currentOperation.edges, currentOperation.angle)) {
                   currentOperation.consumed = true;
               }
            }
        }

        private void OnDestroy() {
            destroyed = true;
            if(viewModel != null) {
                viewModel.DestroyQueue(this);
                base.viewModel = null;
            }
        }

        public override void ViewModelChanged(ViewModel m) {

			if (destroyed) { return; }
            if(viewModel != null) {
				viewModel.DestroyQueue(this);
            }
            base.ViewModelChanged(m);
            if(viewModel != null) {
				currentOperation = new SpinOperation();
                RebuildCube();
                viewModel.CreateQueue(this);
            }
        }

        void RebuildCube() {
			ResetState();

            crossRef = new Dictionary<PieceViewModel, PieceView>();

            foreach(var face in faces) {
                var vm = viewModel.GetFace(face.id);
                face.ViewModelChanged(vm);
                crossRef.Add(vm, face);
            }
            foreach(var edge in edges) {
                PieceViewModel vm;
                if(edge is CornerView) {
                    var corner = edge as CornerView;
                    vm = viewModel.FindPieceFacing((byte) corner.colors[0].id, (byte) corner.colors[1].id, (byte) corner.colors[2].id);
                    corner.ViewModelChanged(vm);
                } else {
                    vm = viewModel.FindPieceFacing((byte) edge.colors[0].id, (byte) edge.colors[1].id);
                    edge.ViewModelChanged(vm);
                }
                crossRef.Add(vm, edge);
            }
        }

        [ContextMenu("--Debug-- Reset State")]
        public void ResetState() {
			foreach (var state in initialState) {
                state.RevertToState();
            }
        }

    }
}
