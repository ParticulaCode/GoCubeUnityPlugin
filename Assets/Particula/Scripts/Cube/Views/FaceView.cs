using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Polymorph.Unity.MVVM;
using Polymorph.Unity.Core;
using Polymorph.Serialization;
using System.Text;
using System;

namespace Particula.Cube {

	public class FaceView : PieceView {

		new FaceViewModel viewModel { get { return base.viewModel as FaceViewModel; } }

		public CubeView cubeView;
		public Transform childrenContainer;
		public Vector3 rotationAxis;
		public int id;
		public float factor = 0.17f;
		public float maxSpeed = 500f;
		public bool locked {
			get { return rotating || !(Mathf.Approximately((currentAngle % 90), 0)); }
		}

		[Header("Debug")]
		public float currentSpeed;
		public float currentAngle;
		public bool dbgLocked;

		public bool rotating = false;
		public bool expedite = false;

		ICoroutine currentSpin = null;

		EdgeColor edgeColor;

		private void Awake() {
			edgeColor = GetComponentInChildren<EdgeColor>();
		}

		void Update() {
			currentAngle = Quaternion.Angle(transform.localRotation, Quaternion.identity);
			dbgLocked = locked;
		}

		void OnDestroy() {
			if (viewModel != null) {
				viewModel.onResetAngle -= ResetAngle;
			}
		}

		public override string GetState() {
			var builder = new StringBuilder();
			builder.Append("{");
			builder.Append("\"transform\": ");
			builder.Append(JsonSerializer.Serialize(transform));
			builder.Append("}");
			return builder.ToString();
		}

		public override void SetState(string state) {			
			if (currentSpin != null) {
				StopCoroutine(currentSpin);
            }
			
			JsonSerializer.Deserialize<FaceView>(state, this);			
		}

		public void SetSpeed(float speed) {
			maxSpeed = speed;
		}

		public override void ViewModelChanged(ViewModel m) {
			base.ViewModelChanged(m);
            ResetAngle();
		}

		public void Spin(PieceView[] edges, float angle) {
			if (currentSpin != null) {
				StopCoroutine(currentSpin);
			}
			currentSpin = StartCoroutine(SpinCo(edges, angle));
		}

		public void Expedite() {
			if (currentSpin != null) {
				expedite = true;
			}
		}

		void ResetAngle(FaceViewModel face) {
			transform.localRotation = Quaternion.AngleAxis(face.angle, rotationAxis);
		}

        public void ResetAngle() {
            if(viewModel != null) {
                transform.localRotation = Quaternion.AngleAxis(viewModel.angle, rotationAxis);
            }
        }

		Vector3 RoundVector(Vector3 v) {
			return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
		}

		IEnumerator SpinCo(PieceView[] edges, float angle) {
            expedite = false;
			rotating = true;

			foreach (var edge in edges) {
				var et = edge.transform;
				et.parent = childrenContainer;
				et.localScale = Vector3.one;
				et.localRotation = Quaternion.Euler(RoundVector(et.localRotation.eulerAngles));
			}

			var goal = Quaternion.AngleAxis(angle, rotationAxis);
			var deltaAngle = 500f;
			var step2Angle = 5;

			while (deltaAngle > step2Angle) {
				if (expedite) {
					break;
				} else {
					deltaAngle = Quaternion.Angle(transform.localRotation, goal);
					transform.localRotation = Quaternion.RotateTowards(transform.localRotation, goal, maxSpeed * Time.deltaTime);
				}
				yield return null;
			}

			while (deltaAngle > 1) {
				if (expedite) {
					break;
				} else {
					deltaAngle = Quaternion.Angle(transform.localRotation, goal);
					transform.localRotation = Quaternion.Slerp(transform.localRotation, goal, (maxSpeed / 10) * Time.deltaTime);
				}
				yield return null;
			}

			transform.localRotation = goal;

			while (childrenContainer.childCount > 0) {
				childrenContainer.GetChild(0).SetParent(transform.parent);
			}

			rotating = false;
			currentSpin = null;
		}

		public override string ToString() {
			return viewModel.ToString();
		}
	}
}
