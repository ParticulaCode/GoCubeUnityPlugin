using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymorph.Unity.MVVM;

namespace Particula.Cube {

	public class PieceViewModel : ViewModel {

        public IPiece piece;

        public PieceViewModel(IPiece model) {
            piece = model;
        }
        public int GetColorForDirection(byte direction) {
            return piece.GetColor(direction);
        }
        public bool Check(byte id1, byte id2) {
            return piece.Is(id1) && piece.Is(id2);
        }
        public bool Check(byte id1, byte id2, byte id3) {
            return piece.Is(id1) && piece.Is(id2) && piece.Is(id3);
        }
        public override string ToString() {
            return piece.ToString();
        }
        public string GetCurrent() {
            return piece.ReadableFacing();
        }
    }
}
