using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BLE;

namespace Particula.Bluetooth {

    public class BLEWrapper : MonoBehaviour, IBluetooth {

        [Serializable]
        public class StringEvent : UnityEvent<string> { }

        public enum DebugLevel {
            Verbose,
            Simple,
            None
        }

        public DebugLevel dbgLevel;
        public StringEvent onStateChanged;

        IBleBridge bridge;
        List<Peripheral> peripherals = new List<Peripheral>();

        private void Awake() {
            CreateBle();
        }

        void OnApplicationPause(bool pause) {
            UpdatePause(pause);
        }

        void OnApplicationFocus(bool focus) {
            UpdatePause(focus == false);
        }

        void OnDestroy() {
            bridge.Cleanup();
        }

        private void UpdatePause(bool pause) {
            if(Time.realtimeSinceStartup <= 5) { return; } // In startup
            if(bridge != null) { bridge.PauseWithState(pause); }
            if(dbgLevel == DebugLevel.Verbose) { Debug.LogFormat("{0} >> {1} - {2} ({3})", DateTime.Now, "UpdatePause", "", ""); }
        }

        private void CreateBle() {
			
            //Determine which native IBleBridge to use based on the runtime platform; Android or iOS
            switch(Application.platform) {
                case RuntimePlatform.Android:
                    bridge = new AndroidBleBridge();
                    break;
                case RuntimePlatform.IPhonePlayer:
					bridge = new iOSBleBridge();
                    break;
                default:
					bridge = new DummyBleBridge(); //modify this class if you want to emulate ble interaction in the editor...
                    break;
            }

			//Startup the native side of the code and include our callbacks...
			bridge.Startup(true, StartupAction, ErrorAction, StateUpdateAction, DidUpdateRssiAction);

        }

        public void ScanForPeripherals(string[] serviceIds, Action<IPeripheral> onPeripheralFound) {
            Scan(serviceIds, onPeripheralFound);
        }

        public void StopScan() {
            bridge.StopScanning();
        }

        /**
        * Connected to the Scan button in the Unity Editor.
        */
        public void Scan(string[] serviceuuids, Action<Peripheral> callback) {
            bridge.StopScanning();
            // onActionCalled.Invoke("Applicaton: Scanning for ble devices...");
            bridge.ScanForPeripheralsWithServiceUUIDs(serviceuuids, delegate (string peripheralId, string peripheralName) {
                var p = GetPeripheral(peripheralId);

                if(p != null) {
                    if(dbgLevel == DebugLevel.Verbose) {
                        Debug.LogFormat("{0} >> Peripheral Discovery -- Found peripheral again -- id: {1}, name {2}", DateTime.Now,peripheralId, peripheralName);
                    }

                    // Found peripheral again?
                    p.lastFound = DateTime.Now;
                } else {
                    if(dbgLevel == DebugLevel.Verbose) {
                        Debug.LogFormat("{0} >> Peripheral Discovery -- Found new peripheral -- id: {1}, name: {2}", DateTime.Now, peripheralId, peripheralName);
                    }

                    var newP = new Peripheral(bridge, peripheralId, peripheralName);
                    newP.lastFound = DateTime.Now;
                    peripherals.Add(newP);
                    callback?.Invoke(newP);
                }
            });

            if(dbgLevel == DebugLevel.Verbose) {
                Debug.LogFormat("{0} >> Scan Started", DateTime.Now);
            }
        }

        Peripheral GetPeripheral(string id) {
            for(int i = 0; i < peripherals.Count; ++i) {
                var p = peripherals[i];

                if(p.id == id) {
                    return p;
                }
            }
            return null;
        }

		/**
        * Called when the Bluetooth adapter changes states, such as enabled by the user after the app has started.
        */
        private void StateUpdateAction(string state) {
			if (dbgLevel == DebugLevel.Verbose) {
                Debug.LogFormat("{0} >> {1} - {2} ({3})", DateTime.Now, "StateUpdateAction", state, "");
            }

			onStateChanged?.Invoke(state);
        }

        /**
         * Called when the IBleBridge.Startup() function has finished creating all the native resources and is ready to start connecting to BLE devices.
         */
        private void StartupAction() {
			if (dbgLevel == DebugLevel.Verbose) {
                Debug.LogFormat("{0} >> {1} - {2} ({3})", DateTime.Now, "StartupAction", "", "");
			}
		}

        /**
         * Called when the IBleBridge.Shutdown() function has finished and the native resources are ready to be released. 
         */
        private void ShutdownAction() {
            if(dbgLevel == DebugLevel.Verbose) {
                Debug.LogFormat("{0} >> {1} - {2} ({3})", DateTime.Now, "ShutdownAction", "", "");
            }
        }

        /**
         * Called when there is an error on the native side of the code.
         */
        private void ErrorAction(string error) {
			if (dbgLevel == DebugLevel.Verbose) {
                Debug.LogFormat("{0} >> {1} - {2} ({3})", DateTime.Now, "Error", error, "");
            }
		}

        /**
        * Called when the RSSI or Received Signal Strength Indicator and changed, either during a scan or after a call to IBleBridge.ReadRssiWithIdentifier()
        */
        private void DidUpdateRssiAction(string peripheralId, string rssi) {
            if(dbgLevel == DebugLevel.Verbose) {
                Debug.LogFormat("{0} >> {1} - {2} ({3})", DateTime.Now, "DidUpdateRssiAction", peripheralId, rssi);
            }
        }
    }
}
