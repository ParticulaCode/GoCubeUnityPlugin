using UnityEngine;
using System;

namespace BLE
{
	//Modify this to suite your needs for BLE Device emulation in the Editor

	public class DummyBleBridge : IBleBridge
	{
		private static BluetoothLeDevice bluetoothDevice;

		private bool lastOn = false;


		public BluetoothLeDevice Startup (bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction)
		{

			bluetoothDevice = null;
						
			if (GameObject.Find ("BleBridge") == null)
			{
				
				GameObject bleBridgeObj = new GameObject ("BleBridge");
				bluetoothDevice = bleBridgeObj.AddComponent<BluetoothLeDevice> ();
				
				if (bluetoothDevice != null)
				{
					bluetoothDevice.StartupAction = action;
					bluetoothDevice.ErrorAction = errorAction;
					bluetoothDevice.StateUpdateAction = stateUpdateAction;
					bluetoothDevice.DidUpdateRssiAction = rssiUpdateAction;
				}
			}
			
			bluetoothDevice.OnStartup("Startup");
			bluetoothDevice.OnBleStateUpdate("Powered On");
			
			return bluetoothDevice;
		}
		
		public void Shutdown (Action action)
		{
			
			if (bluetoothDevice != null)
				bluetoothDevice.ShutdownAction = action;

			bluetoothDevice.OnStartup("Shutdown");

		}
		
		public void Cleanup ()
		{
			GameObject bleBridgeObj = GameObject.Find ("BleBridge");
			
			if (bleBridgeObj != null)
				GameObject.Destroy (bleBridgeObj);
		}
		
		public void PauseWithState(bool isPaused){}
		
		public void ScanForPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			bluetoothDevice.DiscoveredPeripheralAction = action;
			bluetoothDevice.OnDiscoveredPeripheral("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6617:Star Technologies");
			bluetoothDevice.OnRssiUpdate("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a662:94");
		}
		
		public void StopScanning(){}
		
		public void ConnectToPeripheralWithIdentifier(string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string>disconnectAction)
		{
			bluetoothDevice.ConnectedPeripheralAction = connectAction;
			bluetoothDevice.DiscoveredServiceAction = serviceAction;
			bluetoothDevice.DiscoveredCharacteristicAction = characteristicAction;
			bluetoothDevice.DiscoveredDescriptorAction = descriptorAction;
			bluetoothDevice.DisconnectedPeripheralAction = disconnectAction;

			bluetoothDevice.OnConnectedPeripheral("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6617:Star Technologies");
			bluetoothDevice.OnDiscoveredService("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6636:6be6bc00-5c9a-11e4-8ed6-0800200c9a66");
			bluetoothDevice.OnDiscoveredCharacteristic("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6636:43ECE40F-412E-4F68-9062-3B7C4DED1580");
			bluetoothDevice.OnDiscoveredCharacteristic("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6636:45A634DC-B675-4EC2-A1F9-8FDAFF8D17F5");
			bluetoothDevice.OnDiscoveredCharacteristic("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6636:3E9883BD-A699-4ECC-88B8-28DE32292DD8");
		}
		
		public void DisconnectFromPeripheralWithIdentifier(string peripheralId, Action<string, string> action){}
		
		public void RetrieveListOfPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			bluetoothDevice.RetrievedPeripheralWithServiceAction = action;
			bluetoothDevice.OnRetrievedPeripheralWithServiceUUIDs("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a674:Acme");
		}
		
		public void RetrieveListOfPeripheralsWithUUIDs(string[] uuids, Action<string, string> action)
		{
			bluetoothDevice.RetrievedPeripheralWithUUIDAction = action;
			bluetoothDevice.OnRetrievedPeripheralWithUUID("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a684:Acme");
		}
		
		public void SubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication)
		{
			if (bluetoothDevice != null)
			{
				bluetoothDevice.DidUpdateNotificationStateForCharacteristicAction = notificationAction;
				bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
		}
		
		public void UnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action){}
		
		public void ReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action)
		{
			if (bluetoothDevice != null)
				bluetoothDevice.DidUpdateCharacteristicValueAction = action;
		}
		
		public void WriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse, Action<string, string, string> action)
		{

			if(bluetoothDevice !=null)
				bluetoothDevice.DidWriteCharacteristicAction = action;

			byte[] packet = new byte[5];

			lastOn = !lastOn;

			packet[0] = 0x80;
			packet[1] = 0x08;
			packet[2] = (byte)(lastOn ? 255 : 0);

			string base64string = System.Convert.ToBase64String(packet);

			bluetoothDevice.OnDidWriteCharacteristic("36:" + peripheralId + "36:" + characteristicId);
			bluetoothDevice.OnBluetoothData("36:"+ peripheralId +"36:3E9883BD-A699-4ECC-88B8-28DE32292DD8"+ base64string.Length + ":" + base64string);
		}

		public void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action)
		{

		}
		
		public void WriteDescriptorWithIdentifiers(string identifier, string service, string characteristic, string descriptor, byte[] data, int length, Action<string, string, string, string> action)
		{

		}

		public void ReadRssiWithIdentifier(string peripheralId)
		{
			bluetoothDevice.OnRssiUpdate("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a662:94");
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

		}

	}
}