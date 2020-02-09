using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymorph.Unity.MVVM;

namespace Particula.Cube {

    public class Operation {

        public enum OpType { Rotation, StateReset }

        public OpType type;
        public FaceViewModel face;
        public PieceViewModel[] edges;
        public float angle;

        public Operation() {
            edges = new PieceViewModel[8];
        }

        public override string ToString() {
            var builder = new System.Text.StringBuilder();
            builder.Append("Operation(type:");
            builder.Append(type);
            builder.Append(", face:");
            builder.Append(face != null ? face.id.ToString() : "null");
            builder.Append(", angle:");
            builder.Append(angle);
            builder.Append(", edges:[");
            for(int i = 0; i < edges.Length; ++i) {
                builder.Append("\n");
                builder.Append(edges[i]);
            }
            builder.Append("]");
            return builder.ToString();
        }
    }

    public class CubeViewModel : ViewModel, Particula.Common.ICube {

        static Quaternion _globalOffset = Quaternion.identity;
        public static Quaternion globalOffset { get { return _globalOffset; } protected set { _globalOffset = value; } }
        public Quaternion fixedOffset = Quaternion.Euler(90, 90, 0);

        public class OperationQueue {

            CubeViewModel model;
            Operation floatingOp;
            Queue<Operation> operations;
            Stack<Operation> pool;
            bool resetInProgress = false;

            public OperationQueue(Stack<Operation> pool, CubeViewModel model) {
                this.pool = pool;
                this.model = model;
                operations = new Queue<Operation>();
            }
            public void AddSpinOperation(FaceViewModel face) {

                if(resetInProgress) { return; }
                var op = GetLooseOperation();

                op.type = Operation.OpType.Rotation;
                op.face = face;
                model.GetFaceEdges(face, ref op.edges);
                op.angle = face.angle;

                operations.Enqueue(op);
            }
            public void AddStateReset() {
                resetInProgress = true;
                Clear();
                var op = GetLooseOperation();
                op.type = Operation.OpType.StateReset;
                operations.Enqueue(op);
            }
            public Operation GetNextOperation() {
                if(floatingOp != null) {
					pool.Push(floatingOp);
                    floatingOp = null;
                }
                if(operations.Count > 0) {
					floatingOp = operations.Dequeue();
                    if(resetInProgress && (operations.Count == 0)) {
						resetInProgress = false;
                    }
					return floatingOp;
                }
				else {
					return null;
                }
            }
            Operation GetLooseOperation() {
                if(pool.Count == 0) {
                    return new Operation();
                } else {
                    return pool.Pop();
                }
            }

            void Clear() {
                while(operations.Count > 0) {
                    var op = operations.Dequeue();
                    pool.Push(op);
                }
            }
        }

        protected ICube model;

        public event Action onBatteryUpdate;
        public event Action<Rotation> onRotation;

        public FaceViewModel[] faces;

        protected Quaternion localRotation;
        public Quaternion rotation { get { return (useLocalRotaion ? localRotation : TransformQuat(model.orientation)) * fixedOffset; } }

        protected bool useLocalRotaion = false;

        Dictionary<object, OperationQueue> queues;

        Stack<Operation> pool;
        Dictionary<IPiece, PieceViewModel> crossRef;

        public PieceViewModel this[IPiece piece] {
            get { return crossRef[piece]; }
        }

        public CubeViewModel(ICube model) {
            queues = new Dictionary<object, OperationQueue>();
            pool = new Stack<Operation>();
            this.model = model;

            // Register to the events of getting the cube full state
            model.afterFullState += AddStateReset;

            // Register to the events of the rotations of the real cube
            model.afterRotation += CubeRotation;
            RebuildReferences();
        }

        void RebuildReferences() {

            DisposeOfChildren();

            crossRef = new Dictionary<IPiece, PieceViewModel>();
            faces = new FaceViewModel[model.faces.Length];

            for(int i = 0; i < model.pieces.Length; ++i) {
                var piece = model.pieces[i];
                if(piece is IFace) {
                    var face = piece as IFace;
                    var vm = new FaceViewModel(this, face);
                    vm.onAngleChanged += AddSpinOperation;
                    faces[face.id] = vm;
                    crossRef.Add(face, vm);
                } else if(piece is IEdge) {
                    var edge = piece as IEdge;
                    var vm = new EdgeViewModel(edge);
                    crossRef.Add(edge, vm);
                } else {
                    var corner = piece as ICorner;
                    var vm = new CornerViewModel(corner);
                    crossRef.Add(corner, vm);
                }
            }
        }

        public override void Dispose() {
            base.Dispose();
            DisposeOfChildren();
        }

        void DisposeOfChildren() {
            if(crossRef != null) {
                foreach(var key in crossRef.Keys) {
                    if(key is IFace) {
                        (crossRef[key] as FaceViewModel).onAngleChanged -= AddSpinOperation;
                    }
                    crossRef[key].Dispose();
                }
            }
            model.afterRotation -= CubeRotation;
        }

        public void CreateQueue(object owner) {
            queues.Add(owner, new OperationQueue(pool, this));
        }

        public void DestroyQueue(object owner) {
            queues.Remove(owner);
        }

        public Operation GetNextOperation(object owner) {

			if (queues.ContainsKey(owner))
			{
				return queues[owner].GetNextOperation();
			}
			else
			{
				return null;
			}
			 
        }

        void CubeRotation(Rotation rotation) {
            if(onRotation != null) {
                onRotation(rotation);
            }
        }

        void AddSpinOperation(FaceViewModel face) {
            foreach(var key in queues.Keys) {
                queues[key].AddSpinOperation(face);
            }
        }

        void AddStateReset() {
            RebuildReferences();
            foreach(var key in queues.Keys) {
                queues[key].AddStateReset();
            }
        }

        public PieceViewModel FindPiece(byte c1, byte? c2 = null, byte? c3 = null) {
            if(c2.HasValue) {
                if(c3.HasValue) {
                    foreach(var key in crossRef.Keys) {
                        if(key is ICorner) {
                            if(key.Is(c1) && key.Is(c2.Value) && key.Is(c3.Value)) {
                                return crossRef[key];
                            }
                        }
                    }
                } else {
                    foreach(var key in crossRef.Keys) {
                        if(key is IEdge) {
                            if(key.Is(c1) && key.Is(c2.Value)) {
                                return crossRef[key];
                            }
                        }
                    }
                }
            } else {
                foreach(var key in crossRef.Keys) {
                    if(key is IFace) {
                        if(key.Is(c1)) {
                            return crossRef[key];
                        }
                    }
                }
            }
            return null;
        }

        public PieceViewModel FindPieceFacing(byte c1, byte? c2 = null, byte? c3 = null) {
            if(c2.HasValue) {
                if(c3.HasValue) {
                    foreach(var key in crossRef.Keys) {
                        if(key is ICorner) {
                            if(key.IsFacing(c1) && key.IsFacing(c2.Value) && key.IsFacing(c3.Value)) {
                                return crossRef[key];
                            }
                        }
                    }
                } else {
                    foreach(var key in crossRef.Keys) {
                        if(key is IEdge) {
                            if(key.IsFacing(c1) && key.IsFacing(c2.Value)) {
                                return crossRef[key];
                            }
                        }
                    }
                }
            } else {
                foreach(var key in crossRef.Keys) {
                    if(key is IFace) {
                        if(key.IsFacing(c1)) {
                            return crossRef[key];
                        }
                    }
                }
            }
            return null;
        }

        public PieceViewModel GetPiece(IPiece piece) {
            return crossRef[piece];
        }

        public FaceViewModel GetFace(IFace face) {
            return crossRef[face] as FaceViewModel;
        }

        public FaceViewModel GetFace(int id) {
            foreach(var face in faces) {
                if(face.Check(id)) {
                    return face;
                }
            }
            return null;
        }

        public void GetFaceEdges(FaceViewModel face, ref PieceViewModel[] edges) {
            for(int i = 0; i < 8; ++i) {
                var piece = GetPiece(face[i]);
                edges[i] = piece;
            }
        }

        Quaternion TransformQuat(Quat q) {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
	}
}
