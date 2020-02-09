using System.Text;
using System.Collections;
using System.Collections.Generic;
using Polymorph.Unity.MVVM;
using Polymorph.Serialization;

namespace Particula.Cube {

	public class PieceView : View {

        public CubeView cube;
        public EdgeColor[] colors;

        new PieceViewModel viewModel { get { return base.viewModel as PieceViewModel; } }


        bool valid = false;

        public override void ViewModelChanged(ViewModel m) {
            base.ViewModelChanged(m);
            if(!valid) {
                valid = true;
                colors = GetComponentsInChildren<EdgeColor>();
                if(cube == null) {
                    cube = GetComponentInParent<CubeView>();
                }
            }
            ReplaceIds();
            ConnectColors();
        }

        public virtual string GetState() {
            var builder = new StringBuilder();
            builder.Append("{");
            builder.Append("\"transform\": ");
            builder.Append(JsonSerializer.Serialize(transform));
            builder.Append(", \"colors\": [");
            for(int i = 0; i < colors.Length; ++i) {
                if(i > 0) {
                    builder.Append(", ");
                }
                builder.Append("{ \"id\": ");
                builder.Append(JsonSerializer.Serialize(colors[i].id));
                builder.Append("}");
            }
            builder.Append("]");
            builder.Append("}");
            return builder.ToString();
        }

        public bool Is(byte color) {
            for(int i = 0; i < colors.Length; ++i) {
                if(colors[i].id == color) {
                    return true;
                }
            }
            return false;
        }

        public bool IsFacing(byte color) {
            if(viewModel != null) {
                return viewModel.piece.IsFacing(color);
            } else {
                return false;
            }
        }

        public virtual void SetState(string state) {

			JsonSerializer.Deserialize<PieceView>(state, this);
            for(int i = 0; i < colors.Length; ++i) {
                colors[i].edgeRenderer.material = cube.colorMaterials[colors[i].id];
            }
        }


        void ReplaceIds() {
            foreach(var color in colors) {
                color.id = viewModel.GetColorForDirection((byte) color.id);
            }
        }

        void ConnectColors() {
            foreach(var color in colors) {
                color.SetMaterial(cube.colorMaterials[color.id]);
            }
        }

        public override string ToString() {
            var retVal = new StringBuilder();
            retVal.Append("Piece(");
            for(int i = 0; i < colors.Length; ++i) {
                retVal.Append(colors[i].id);
            }
            retVal.Append(")");
            return retVal.ToString();
        }
    }
}
