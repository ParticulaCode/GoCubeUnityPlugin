using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymorph.Unity.MVVM;

namespace Particula.Cube {

    public interface IImageRequester {
        float width { get; }
        float height { get; }
        void TextureChagned(RenderTexture tex);
    }

	public class CubeImageViewModel : ViewModel {

        CubeImageRenderer model;
        RenderTexture currentTexture;
        List<IImageRequester> requesters;
        RenderTextureDescriptor descriptor;

        public CubeImageViewModel(CubeImageRenderer model) {
            this.model = model;
            requesters = new List<IImageRequester>();
            descriptor = model.texPrototype.descriptor;
            descriptor.width = 1;
            descriptor.height = 1;
            currentTexture = RenderTexture.GetTemporary(descriptor);
            model.cam.targetTexture = currentTexture;
        }

        public void AddRequester(IImageRequester requester) {
            requesters.Add(requester);
            var size = GetLowest(requester);
            if(size > descriptor.height) {
                CreateNewTexture(size);
            } else {
                requester.TextureChagned(currentTexture);
            }
        }

        public void RemoveRequester(IImageRequester requester) {
            requesters.Remove(requester);
            RequesterChanged();
        }

        public void RequesterChanged() {
            int highest = 1;
            for(int i = 0; i < requesters.Count; ++i) {
                var size = GetLowest(requesters[i]);
                if(size > highest) {
                    highest = size;
                }
            }
            if(highest != descriptor.height) {
                CreateNewTexture(highest);
            }
        }

        void CreateNewTexture(int size) {
            descriptor.width = size;
            descriptor.height = size;
            currentTexture.Release();
            currentTexture = RenderTexture.GetTemporary(descriptor);
            model.cam.targetTexture = currentTexture;
            Debug.Log("Camera got new texture " + currentTexture.name);
            for(int i = 0; i < requesters.Count; ++i) {
                requesters[i].TextureChagned(currentTexture);
            }
        }

        int GetLowest(IImageRequester requester) {
            return Mathf.CeilToInt(requester.height > requester.width ? requester.width : requester.height);
        }

        public override string ToString() {
            return model.name;
        }
    }
}
