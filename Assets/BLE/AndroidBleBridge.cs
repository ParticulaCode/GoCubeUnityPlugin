using UnityEngine;
using System;

namespace BLE
{

#if UNITY_ANDROID
	public class AndroidBleBridge : IBleBridge {

		private AndroidJavaObject bridge = null;
		private AndroidJavaObject playerActivityContext = null;
		private static BluetoothLeDevice bluetoothDevice;

		public string isNull;
		
		public BluetoothLeDevice Startup (bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction)
		{
			bluetoothDevice = null;

			if (GameObject.Find ("BleBridge") == null)
			{

				GameObject bleBridgeObj = new GameObject ("BleBridge");
				bluetoothDevice = bleBridgeObj.AddComponent<BluetoothLeDevice> ();
				
				if (bluetoothDevice != null)
				{
					bluetoothDevice.isLowerCaseUUID = true;
					bluetoothDevice.StartupAction = action;
					bluetoothDevice.ErrorAction = errorAction;
					bluetoothDevice.StateUpdateAction = stateUpdateAction;
					bluetoothDevice.DidUpdateRssiAction = rssiUpdateAction;
				}
			}

			// First, obtain the current activity context
			using (var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
				playerActivityContext = actClass.GetStatic<AndroidJavaObject>("currentActivity");
			}

			// Pass the context to a newly instantiated TestUnityProxy object
			using (var pluginClass = new AndroidJavaClass("com.startechplus.unityblebridge.Bridge")) {
				if (pluginClass != null) {
					bridge = pluginClass.CallStatic<AndroidJavaObject>("instance");
					bridge.Call("setContext", playerActivityContext);
					bridge.Call ("startup", bluetoothDevice.gameObject.name, asCentral);
				}
				else
				{
					Debug.LogError("AndroidBleBridge: Error creating Android objects...");
				}
			}

			return bluetoothDevice;
		}
		
		public void Shutdown (Action action)
		{
			
			if (bluetoothDevice != null)
				bluetoothDevice.ShutdownAction = action;
			
			bridge.Call("shutdown");
		}
		
		public void Cleanup ()
		{
			GameObject bleBridgeObj = GameObject.Find ("BleBridge");
			
			if (bleBridgeObj != null)
				GameObject.Destroy (bleBridgeObj);
		}
		
		public void PauseWithState (bool isPaused)
		{
			bridge.Call("pauseWithState", isPaused);	
		}
		
		public void ScanForPeripheralsWithServiceUUIDs (string[] serviceUUIDs, Action<string, string> action)
		{
			if (bluetoothDevice != null) 
			{
				bluetoothDevice.DiscoveredPeripheralAction = action;
			}
			
			string serviceUUIDsString = null;
			
			if (serviceUUIDs != null) 
			{
				serviceUUIDsString = "";
				
				foreach (string serviceUUID in serviceUUIDs)
					serviceUUIDsString += serviceUUID.ToLower() + "|";
				
				serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
			}
			
			bridge.Call("scanForPeripheralsWithServiceUUIDs", serviceUUIDsString);
		}
		
		public void ConnectToPeripheralWithIdentifier (string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string>disconnectAction)
		{				
			if (bluetoothDevice != null)
			{
				bluetoothDevice.ConnectedPeripheralAction = connectAction;
				bluetoothDevice.DiscoveredServiceAction = serviceAction;
				bluetoothDevice.DiscoveredCharacteristicAction = characteristicAction;
				bluetoothDevice.DiscoveredDescriptorAction = descriptorAction;
				bluetoothDevice.DisconnectedPeripheralAction = disconnectAction;
			}

			bridge.Call("connectToPeripheralWithIdentifier", peripheralId);
		}
		
		public void DisconnectFromPeripheralWithIdentifier (string peripheralId, Action<string, string> action)
		{		
			if (bluetoothDevice != null)
				bluetoothDevice.DisconnectedPeripheralAction = action;
			
			bridge.Call("disconnectFromPeripheralWithIdentifier", peripheralId);
		}
				
		public void RetrieveListOfPeripheralsWithServiceUUIDs (string[] serviceUUIDs, Action<string, string> action)
		{
			if (bluetoothDevice != null)
			{
				bluetoothDevice.RetrievedPeripheralWithServiceAction = action;
			}
			
			string serviceUUIDsString = null;
			
			if(serviceUUIDs != null)
			{
				serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;
				
				foreach (string serviceUUID in serviceUUIDs)
					serviceUUIDsString += serviceUUID.ToLower() + "|";
				
				// strip the last delimeter
				serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
			}
			
			bridge.Call("retrieveListOfPeripheralsWithServiceUUIDs", serviceUUIDsString);
		}
		
		public void RetrieveListOfPeripheralsWithUUIDs (string[] uuids, Action<string, string> action)
		{
			if (bluetoothDevice != null)
			{
				bluetoothDevice.RetrievedPeripheralWithUUIDAction = action;
			}
			
			string uuidsString = null;
			
			if(uuids != null)
			{
				uuidsString = uuids.Length > 0 ? "" : null;
				foreach (string pUUID in uuids)
					uuidsString += pUUID.ToLower() + "|";
				
				// strip the last delimeter
				uuidsString = uuidsString.Substring (0, uuidsString.Length - 1);
			}
			
			bridge.Call ("retrieveListOfPeripheralsWithUUIDs", uuidsString);
		}
		
		public void StopScanning ()
		{
			bridge.Call("stopScanning");
		}
		
		public void SubscribeToCharacteristicWithIdentifiers (string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication)
		{
			Debug.Log("in SubscribeToCharacteristicWithIdentifiers");
			if (bluetoothDevice != null)
			{
				Debug.Log("in if of SubscribeToCharacteristicWithIdentifiers");
				bluetoothDevice.DidUpdateNotificationStateForCharacteristicAction = notificationAction;
				bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
			
			bridge.Call ("subscribeToCharacteristicWithIdentifiers", peripheralId, serviceId.ToLower(), characteristicId.ToLower(), isIndication);
		}
		
		public void UnSubscribeFromCharacteristicWithIdentifiers (string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action)
		{
			
			bridge.Call ("unSubscribeFromCharacteristicWithIdentifiers",peripheralId, serviceId.ToLower(), characteristicId.ToLower());
			
		}
		
		public void ReadCharacteristicWithIdentifiers (string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action)
		{
			
			if (bluetoothDevice != null)
				bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			
			bridge.Call ("readCharacteristicWithIdentifiers", peripheralId, serviceId.ToLower(), characteristicId.ToLower());		
		}
		
		public void WriteCharacteristicWithIdentifiers (string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse, Action<string, string, string> action)
		{
			if (bluetoothDevice != null)
			{
				bluetoothDevice.DidWriteCharacteristicAction = action;
			}

			bridge.Call("writeCharacteristicWithIdentifiers", peripheralId, serviceId.ToLower(), characteristicId.ToLower(), data, length, withResponse);//BitConverter.ToString(data)"0x33"
		}

		public void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action)
		{
			if(bluetoothDevice != null)
				bluetoothDevice.DidReadDescriptorValueAction = action;

			bridge.Call ("readDescriptorWithIdentifiers", peripheralId, serviceId.ToLower(), characteristicId.ToLower(), descriptorId.ToLower());
		}
		
		public void WriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length, Action<string, string, string, string> action)
		{
			if(bluetoothDevice != null)
				bluetoothDevice.DidWriteDescriptorAction = action;

			bridge.Call("writeDescriptorWithIdentifiers", peripheralId, serviceId.ToLower(), characteristicId.ToLower(), descriptorId.ToLower(), data, length);

		}

		public void ReadRssiWithIdentifier(string peripheralId)
		{
			bridge.Call("readRssiWithIdentifier", peripheralId);
		}


		public void AddAdvertisementDataListeners(Action<string, string> localNameAction, 
		                                           Action<string, byte[]> manufactureDataAction,
		                                           Action<string, string, byte[]> serviceDataAction,
		                                           Action<string, string> serviceAction,
		                                           Action<string, string> overflowServiceAction,
		                                           Action<string, string> txPowerLevelAction,
		                                           Action<string, string> isConnectable,
		                                           Action<string, string> solicitedServiceAction)
		{
			if (bluetoothDevice != null)
			{
				bluetoothDevice.DidAdvertiseLocalNameAction = localNameAction;
				bluetoothDevice.DidAdvertiseManufactureDataAction = manufactureDataAction;
				bluetoothDevice.DidAdvertiseServiceDataAction = serviceDataAction;
				bluetoothDevice.DidAdvertiseServiceAction = serviceAction;
				bluetoothDevice.DidAdvertiseOverflowServiceAction = overflowServiceAction;
				bluetoothDevice.DidAdvertiseTxPowerLevelAction = txPowerLevelAction;
				bluetoothDevice.DidAdvertiseIsConnectable = isConnectable;
				bluetoothDevice.DidAdvertiseSolicitedServiceAction = solicitedServiceAction;				
			}
		}		
	}
#else
	public class AndroidBleBridge : IBleBridge 
	{
		public BluetoothLeDevice Startup(bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction){ return null; }

		public void Shutdown(Action action){}
		
		public void Cleanup(){}
		
		public void PauseWithState(bool isPaused){}
		
		public void ScanForPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action){}
		
		public void StopScanning(){}
		
		public void ConnectToPeripheralWithIdentifier(string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string>disconnectAction){}
		
		public void DisconnectFromPeripheralWithIdentifier(string peripheralId, Action<string, string> action){}
		
		public void RetrieveListOfPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action){}
		
		public void RetrieveListOfPeripheralsWithUUIDs(string[] uuids, Action<string, string> action){}
		
		public void SubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication){}
		
		public void UnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action){}
		
		public void ReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action){}
		
		public void WriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse, Action<string, string, string> action){}
		
		public void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action){}
		
		public void WriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length, Action<string, string, string, string> action){}

		public void ReadRssiWithIdentifier(string peripheralId){}

		public void AddAdvertisementDataListeners(Action<string, string> localNameAction, 
		                                                 Action<string, byte[]> manufactureDataAction,
		                                                 Action<string, string, byte[]> serviceDataAction,
		                                                 Action<string, string> serviceAction,
		                                                 Action<string, string> overflowServiceAction,
		                                                 Action<string, string> txPowerLevelAction,
		                                                 Action<string, string> isConnectable,
		                                          Action<string, string> solicitedServiceAction){}
	}

#endif
}
