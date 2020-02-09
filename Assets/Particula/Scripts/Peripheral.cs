using System;
using System.Collections.Generic;
using BLE;
using UnityEngine;

namespace Particula.Bluetooth {

    public class Peripheral : IPeripheral {

        class CharacteristicDescription {
            public string id, service;
        }

        public class Characteristic : ICharacteristic {

            public string id { get; private set; }
            public Action<Characteristic> onReady;
            public event Action<byte[]> onData;

            string serviceId;
            Peripheral peripheral;

            public Characteristic(Peripheral peripheral, string serviceId, string id, Action<Characteristic> onReady) {
                this.peripheral = peripheral;
                this.serviceId = serviceId;
                this.id = id;
                this.onReady = onReady;
            }

            public void OnStatusChanged(string peripheralId, string service, string characteristic) {
				onReady?.Invoke(this);
            }

            public void OnDataReceived(string peripheralId, string service, string characteristic, byte[] data) {
                onData?.Invoke(data);
            }

            public void Publish(byte[] data) {
				peripheral.bridge.WriteCharacteristicWithIdentifiers(peripheral.id, serviceId, id, data, data.Length, false, null);
            }
        }

        public string id { get; private set; }
        public string name { get; private set; }

        public DateTime lastFound;

        IBleBridge bridge;
        bool connected = false;

        List<CharacteristicDescription> discoveredCharacteristics = new List<CharacteristicDescription>();
        event Action<string, string> _characteristicDiscovered;
        public event Action<string, string> characteristicDiscovered {
            add {
                _characteristicDiscovered += value;
                foreach(var c in discoveredCharacteristics) {
                    value(c.service, c.id);
                }
            }
            remove {
				_characteristicDiscovered -= value;
            }
        }
		
        List<Characteristic> subscribedCharacteristics = new List<Characteristic>();

        public Peripheral(IBleBridge bridge, string id, string name) {
            this.id = id;
            this.name = name;
            this.bridge = bridge;
        }

        public void Connect() {
            bridge.ConnectToPeripheralWithIdentifier(id, OnConnected, OnDiscoveredService, OnDiscoveredCharacteristic, OnDiscoveredDescription, OnDisconnected);
        }

        public void Disconnect() {
            bridge.DisconnectFromPeripheralWithIdentifier(id, OnDisconnected);
        }

		private bool ShouldSubscribeToCharacteristicWithIdentifiers(string device, string service, string characteristic)
		{
			var ret = (service == GoCube.UART_UUID && characteristic == GoCube.RX_UUID);
			return ret;
		}

		public void SubscribeToCharacteristic(string service, string characteristic, Action<ICharacteristic> callback) {
            var c = new Characteristic(this, service, characteristic, callback);

			if(ShouldSubscribeToCharacteristicWithIdentifiers(id, service, characteristic))
			{
				bridge.SubscribeToCharacteristicWithIdentifiers(id, service, characteristic, c.OnStatusChanged, c.OnDataReceived, false);				
			}
			else
			{
				callback(c);
			}
		}

        /**
        * Called when a successful connection has been established with a Bluetooth device.  This is usually do to a call to IBleBridge.ConnectToPeripheralWithIdentifier()
        */
        void OnConnected(string peripheralId, string name) {
            connected = true;
        }

        /**
        * Called when a Bluetooth device has been disconnected, 
        * either from a call to IBleBridge.DisconnectFromPeripheralWithIdentifier() 
        * or the device has been shut off or gone out of range.
        */
        void OnDisconnected(string peripheralId, string name) {
            Debug.Log("OnDisconnected");
            connected = false;
        }

        /**
        * Called when a Service has been discovered.  
        * I dont know about to bottom part, as it seems like its gonna be called when a service is discovered on this peripheral.
        * 
        * Services are automatically scanned for with a call to IBleBridge.ScanForPeripheralsWithServiceUUIDs() 
        * or IBleBridge.RetrieveListOfPeripheralsWithServiceUUIDs().
        */
        void OnDiscoveredService(string peripheralId, string service) {
            
        }
        /**
        * Called when a Characteristic has been discovered. 
        * I dont know about to bottom part, as it seems like its gonna be called when a characaristic is discovered on this peripheral.
        * 
        * Characteristic are automatically scanned for with a call to IBleBridge.ScanForPeripheralsWithServiceUUIDs() 
        * or IBleBridge.RetrieveListOfPeripheralsWithServiceUUIDs().
        */
        void OnDiscoveredCharacteristic(string peripheralId, string service, string characteristic) {
			discoveredCharacteristics.Add(new CharacteristicDescription() { service = service, id = characteristic });

			if (_characteristicDiscovered != null)
			{
				_characteristicDiscovered.Invoke(service, characteristic);
			}
		}

        /**
        * Called when a Descriptor has been discovered.
        * I dont know about to bottom part, as it seems like its gonna be called when a description is discovered on this peripheral.
        * 
        * Descriptor are automatically scanned for with a call to IBleBridge.ScanForPeripheralsWithServiceUUIDs() 
        * or IBleBridge.RetrieveListOfPeripheralsWithServiceUUIDs().
        */
        void OnDiscoveredDescription(string peripheralId, string service, string characteristic, string descriptor) {
            
        }
    }
}