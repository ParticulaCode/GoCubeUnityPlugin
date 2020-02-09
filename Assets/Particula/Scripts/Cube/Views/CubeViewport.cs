using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Polymorph.Unity.MVVM;
using Particula.Cube;

namespace Particula.Common {

	public class CubeViewport : View, IImageRequester {

        new CubeImageViewModel viewModel { get { return base.viewModel as CubeImageViewModel; } }

        RawImage _image;
        RawImage image {
            get {
                if(_image == null) {
                    _image = GetComponent<RawImage>();
                }
                return _image;
            }
        }

        CanvasScaler _scaler;
        CanvasScaler scaler {
            get {
                if(_scaler == null) {
                    _scaler = GetComponentInParent<CanvasScaler>();
                }
                return _scaler;
            }
        }
        Canvas _canvas;
        Canvas canvas {
            get {
                if(_canvas == null) {
                    _canvas = GetComponentInParent<Canvas>();
                }
                return _canvas;
            }
        }

        Rect _myRect;
        Rect myRect {
            get {
                if(_myRect == null) {
                    _myRect = GetScreenCoordinates(image.rectTransform);
                }
                return _myRect;
            }
            set {
                _myRect = value;
            }
        }

        int resizeInFrames = -1;

        public float width {
            get { return myRect.width == 0 ? 1 : myRect.width; }
        }
        public float height {
            get { return myRect.height == 0 ? 1 : myRect.height; }
        }

        public override void ViewModelChanged(ViewModel m) {
            if(viewModel != null) {
                viewModel.RemoveRequester(this);
            }
            base.ViewModelChanged(m);
            if(viewModel != null) {
                viewModel.AddRequester(this);
            }
        }

        void Update() {
            if(resizeInFrames > -1) {
                --resizeInFrames;
                if(resizeInFrames == -1) {
                    DoResize();
                }
            }
        }

        void OnEnable() {
            OnRectTransformDimensionsChange();
        }

        private void OnRectTransformDimensionsChange() {
            myRect = GetScreenCoordinates(image.rectTransform);
            if(viewModel != null) {
                viewModel.RequesterChanged();
                RecalcSize();
            }
        }

        public Rect GetScreenCoordinates(RectTransform uiElement) {
            var worldCorners = new Vector3[4];
            uiElement.GetWorldCorners(worldCorners);
            worldCorners[0] = canvas.worldCamera.WorldToScreenPoint(worldCorners[0]);
            worldCorners[2] = canvas.worldCamera.WorldToScreenPoint(worldCorners[2]);
            var result = new Rect(
                          worldCorners[0].x,
                          worldCorners[0].y,
                          worldCorners[2].x - worldCorners[0].x,
                          worldCorners[2].y - worldCorners[0].y);
            return result;
        }

        public void TextureChagned(RenderTexture tex) {
            image.texture = tex;
            RecalcSize();
        }

        void RecalcSize() {
            resizeInFrames = 2;
        }

        void DoResize() {
            if(myRect.width > myRect.height) {
                var percent = myRect.height / myRect.width;
                var co = 1 / percent;
                var rect = image.uvRect;
                rect.x = (1 - co) / 2;
                rect.width = co;
                rect.y = 0;
                rect.height = 1;
                image.uvRect = rect;
            } else {
                var percent = myRect.width / myRect.height;
                var co = 1 / percent;
                var rect = image.uvRect;
                rect.y = (1 - co) / 2;
                rect.height = co;
                rect.x = 0;
                rect.width = 1;
                image.uvRect = rect;
            }
        }
    }
}
