using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymorph.Unity.MVVM;

namespace Particula.Cube {

	public class CubeImageRenderer : MonoBehaviour {

        public Camera cam;
        public CubeView cube;
        public Vector2 sensitivity;
        public RenderTexture texPrototype;
		public Transform rotationMaster;

        string path;
        CubeImageViewModel vm;

        private void Awake() {
            if(texPrototype != null) {
                path = DataContext.GetPath(transform);
                if(!string.IsNullOrEmpty(path)) {
                    vm = new CubeImageViewModel(this);
                    ViewModelRegistry.DeclareProvider(path, vm);
                }
            }
        }

        void Update() {
			if (rotationMaster != null) {
				transform.rotation = rotationMaster.rotation;
			}
        }

        private void OnDestroy() {
            if(vm != null) {
                ViewModelRegistry.ClearProvider(path);
            }
        }

        public void TurnOn() {
            gameObject.SetActive(true);
        }

        public void TurnOff() {
            gameObject.SetActive(false);
        }
    }
}
