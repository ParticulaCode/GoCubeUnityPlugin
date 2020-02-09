using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace BLE
{

    public class iOSBleBridge : IBleBridge
    {

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeStartup(string gameObjName, bool isCentral);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeShutdown();

        [DllImport("__Internal")]
        private static extern void iOSBleBridgePauseWithState(bool isPaused);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeConnectToPeripheralWithIdentifier(string peripheralId);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeDisconnectPeripheralWithIdentifier(string peripheralId);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeScanForPeripheralsWithServiceUUIDs(string uuids);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeRetrieveListOfPeripheralsWithServiceUUIDs(string uuids);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeRetrieveListOfPeripheralsWithUUIDs(string uuids);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeStopScanning();

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeWriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeSubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeUnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeWriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length);

        [DllImport("__Internal")]
        private static extern void iOSBleBridgeReadRssiWithIdentifier(string peripheralId);

        private static BluetoothLeDevice bluetoothDevice;

        public bool debugAllCmd = true;

        public BluetoothLeDevice Startup(bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction)
        {
            bluetoothDevice = null;


            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"Startup", "", "");
            if (GameObject.Find("BleBridge") == null)
            {

                GameObject bleBridgeObj = new GameObject("BleBridge");
                bluetoothDevice = bleBridgeObj.AddComponent<BluetoothLeDevice>();

                if (bluetoothDevice != null)
                {
                    bluetoothDevice.StartupAction = action;
                    bluetoothDevice.ErrorAction = errorAction;
                    bluetoothDevice.StateUpdateAction = stateUpdateAction;
                    bluetoothDevice.DidUpdateRssiAction = rssiUpdateAction;
                }
            }

            iOSBleBridgeStartup("BleBridge", asCentral);

            return bluetoothDevice;
        }

        public void Shutdown(Action action)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"Shutdown", "", "");
            if (bluetoothDevice != null)
                bluetoothDevice.ShutdownAction = action;

            iOSBleBridgeShutdown();
        }

        public void Cleanup()
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"Cleanup", "", "");

            GameObject bleBridgeObj = GameObject.Find("BleBridge");

            if (bleBridgeObj != null)
                GameObject.Destroy(bleBridgeObj);
        }

        public void PauseWithState(bool isPaused)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"PauseWithState", isPaused, "");

            iOSBleBridgePauseWithState(isPaused);
        }

        public void ScanForPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"ScanForPeripheralsWithServiceUUIDs", "", "");

            if (bluetoothDevice != null)
            {
                bluetoothDevice.DiscoveredPeripheralAction = action;
            }

            string serviceUUIDsString = null;

            if (serviceUUIDs != null)
            {
                serviceUUIDsString = "";

                foreach (string serviceUUID in serviceUUIDs)
                    serviceUUIDsString += serviceUUID + "|";

                serviceUUIDsString = serviceUUIDsString.Substring(0, serviceUUIDsString.Length - 1);
            }

            iOSBleBridgeScanForPeripheralsWithServiceUUIDs(serviceUUIDsString);
        }

        public void ConnectToPeripheralWithIdentifier(string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string> disconnectAction)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"ConnectToPeripheralWithIdentifier", peripheralId, "");
            if (bluetoothDevice != null)
            {
                bluetoothDevice.ConnectedPeripheralAction = connectAction;
                bluetoothDevice.DiscoveredServiceAction = serviceAction;
                bluetoothDevice.DiscoveredCharacteristicAction = characteristicAction;
                bluetoothDevice.DiscoveredDescriptorAction = descriptorAction;
                bluetoothDevice.DisconnectedPeripheralAction = disconnectAction;
            }

            iOSBleBridgeConnectToPeripheralWithIdentifier(peripheralId);
        }

        public void DisconnectFromPeripheralWithIdentifier(string peripheralId, Action<string, string> action)
        {

            if (bluetoothDevice != null)
                bluetoothDevice.DisconnectedPeripheralAction = action;

            iOSBleBridgeDisconnectPeripheralWithIdentifier(peripheralId);

            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"DisconnectFromPeripheralWithIdentifier", peripheralId, "");
        }


        public void RetrieveListOfPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"RetrieveListOfPeripheralsWithServiceUUIDs", "", "");

            if (bluetoothDevice != null)
            {
                bluetoothDevice.RetrievedPeripheralWithServiceAction = action;
            }

            string serviceUUIDsString = null;

            if (serviceUUIDs != null)
            {
                serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;

                foreach (string serviceUUID in serviceUUIDs)
                    serviceUUIDsString += serviceUUID + "|";

                // strip the last delimeter
                serviceUUIDsString = serviceUUIDsString.Substring(0, serviceUUIDsString.Length - 1);
            }

            iOSBleBridgeRetrieveListOfPeripheralsWithServiceUUIDs(serviceUUIDsString);
        }

        public void RetrieveListOfPeripheralsWithUUIDs(string[] uuids, Action<string, string> action)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"RetrieveListOfPeripheralsWithUUIDs", "", "");

            if (bluetoothDevice != null)
            {
                bluetoothDevice.RetrievedPeripheralWithUUIDAction = action;

            }

            string uuidsString = null;

            if (uuids != null)
            {
                uuidsString = uuids.Length > 0 ? "" : null;
                foreach (string uuidString in uuids)
                    uuidsString += uuidString + "|";

                // strip the last delimeter
                uuidsString = uuidsString.Substring(0, uuidsString.Length - 1);
            }

            iOSBleBridgeRetrieveListOfPeripheralsWithUUIDs(uuidsString);
        }

        public void StopScanning()
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"iOSBleBridgeStopScanning", "", "");

            iOSBleBridgeStopScanning();
        }

        public void SubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"SubscribeToCharacteristicWithIdentifiers", "", "");

            if (bluetoothDevice != null)
            {
                bluetoothDevice.DidUpdateNotificationStateForCharacteristicAction = notificationAction;
                bluetoothDevice.DidUpdateCharacteristicValueAction = action;
            }

            iOSBleBridgeSubscribeToCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);
        }

        public void UnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action)
        {

            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"UnSubscribeFromCharacteristicWithIdentifiers", "", "");

            iOSBleBridgeUnSubscribeFromCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);

        }

        public void ReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action)
        {

            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"ReadCharacteristicWithIdentifiers", "", "");

            if (bluetoothDevice != null)
                bluetoothDevice.DidUpdateCharacteristicValueAction = action;

            iOSBleBridgeReadCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);

        }

        public void WriteCharacteristicWithIdentifiers(string peripheralId,
            string serviceId, string characteristicId,
            byte[] data, int length, bool withResponse, Action<string, string, string> action)
        {

            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})\n{4} {5} {6}\n{7} {8}", DateTime.Now,
"WriteCharacteristicWithIdentifiers", BitConverter.ToString(data), length,
peripheralId, serviceId,
characteristicId,
withResponse, action == null ? "NULL" : "ACTIOn");

            if (bluetoothDevice != null)
                bluetoothDevice.DidWriteCharacteristicAction = action;

            iOSBleBridgeWriteCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId, data, length, withResponse);

        }


        public void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"ReadDescriptorWithIdentifiers", "", "");

            if (bluetoothDevice != null)
                bluetoothDevice.DidReadDescriptorValueAction = action;

            iOSBleBridgeReadDescriptorWithIdentifiers(peripheralId, serviceId, characteristicId, descriptorId);
        }

        public void WriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length, Action<string, string, string, string> action)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"WriteDescriptorWithIdentifiers", "", "");

            if (bluetoothDevice != null)
                bluetoothDevice.DidWriteDescriptorAction = action;

            iOSBleBridgeWriteDescriptorWithIdentifiers(peripheralId, serviceId, characteristicId, descriptorId, data, length);
        }

        public void ReadRssiWithIdentifier(string peripheralId)
        {
            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"ReadRssiWithIdentifier", "", "");

            iOSBleBridgeReadRssiWithIdentifier(peripheralId);
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

            if (debugAllCmd) Debug.LogFormat("IOS_{0} >> {1} - {2} ({3})", DateTime.Now,
"AddAdvertisementDataListeners", "", "");

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
}
