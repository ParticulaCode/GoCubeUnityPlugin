//
//  UnityBleBridge.mm
//  UnityBleBridge
//
//  Created by Jason Peterson on 10/22/14.
//  Copyright (c) 2014 Star Technologies. All rights reserved.
//

#import "UnityBleBridge.h"

/*
 @interface CBUUID (StringExtraction)
 
 - (NSString *)representativeString;
 
 @end
 
 @implementation CBUUID (StringExtraction)
 
 - (NSString *)representativeString;
 {
 NSData *data = [self data];
 
 NSUInteger bytesToConvert = [data length];
 const unsigned char * uuidBytes = (const unsigned char *)[data bytes];
 NSMutableString *outputString = [NSMutableString stringWithCapacity:16];
 
 for (NSUInteger currentByteIndex = 0; currentByteIndex < bytesToConvert; currentByteIndex++)
 {
 switch (currentByteIndex)
 {
 case 3:
 case 5:
 case 7:
 case 9:[outputString appendFormat:@"%02x-", uuidBytes[currentByteIndex]]; break;
 default:[outputString appendFormat:@"%02x", uuidBytes[currentByteIndex]];
 }
 
 }
 
 if(outputString.length == 4)
 //00002901-0000-1000-8000-00805f9b34fb
 return [[NSString stringWithFormat:@"0000%@-0000-1000-8000-00805f9b34fb", outputString] uppercaseString];
 else
 return [outputString uppercaseString];
 }
 
 @end
 */

extern void UnitySendMessage(const char *, const char *, const char *);

@implementation UnityBleBridge

@synthesize gameObjName;
@synthesize centralManager;
@synthesize peripheralList;
@synthesize serviceList;
@synthesize characteristicList;



- (id)init
{
    return  [self initWithGameObjectName:@"BleBridge"];
}

- (id)initWithGameObjectName:(NSString *)name
{
    if ( self = [super init] ) {
        
        if(name.length > 0)
            [self setGameObjName:name];
        else
            [self setGameObjName:@"BleBridge"];
    }
    return self;
}

- (void) startupAs:(bool) isCentral
{
    if([self centralManager] == nil)
    {
        [self setCentralManager:[[CBCentralManager alloc] initWithDelegate:self queue:nil]];
        [self setPeripheralList:[NSMutableDictionary dictionaryWithCapacity:5]];
        [self setServiceList:[NSMutableDictionary dictionaryWithCapacity:5]];
        [self setCharacteristicList:[NSMutableDictionary dictionaryWithCapacity:5]];
    }
}

- (void) shutdown
{
    [[self centralManager] stopScan];
    
#if !__has_feature(objc_arc)
    [[self centralManager] release];
    [[self peripheralList] release];
    [[self serviceList] release];
    [[self characteristicList] release];
#endif
    
    [self setCentralManager:nil];
    [self setPeripheralList:nil];
    [self setCharacteristicList:nil];
    [self setServiceList:nil];
}

-(void) pauseWithState:(bool) isPaused
{
    
}

-(void) scanForPeripheralsWithServiceUUIDs:(NSArray *)uuids andOptions:(NSDictionary *) options
{
    if([self centralManager] != nil)
        [[self centralManager] scanForPeripheralsWithServices:uuids options:options];
}

-(void) stopScanning
{
    [[self centralManager] stopScan];
}
-(void) readRssiWithIdentifier:(NSString *) identifier
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:identifier];
        if(peripheral != nil)
        {
            
            if([peripheral RSSI] != nil)
            {
                NSString *rssi = [[peripheral RSSI] stringValue];
                NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@", (int)strlen([identifier UTF8String]), identifier, (int)strlen([rssi UTF8String]), rssi];
                UnitySendMessage ([[self gameObjName] UTF8String], "OnRssiUpdate", [message UTF8String]);
            }
            
        }
    }
    
}

-(void) retrieveListOfPeripheralsWithServiceUUIDs:(NSArray *)uuids
{
    if([self centralManager] != nil)
    {
        NSArray * peripherals = [[self centralManager] retrieveConnectedPeripheralsWithServices:uuids];
        
        for(id _peripheral in peripherals)
        {
            CBPeripheral *peripheral = _peripheral;
            [self onUpdatePeripheral:peripheral withEvent:@"OnRetrievedPeripheralWithServiceUUIDs" andIdentifer:[peripheral name]];
        }
    }
}


-(void) retrieveListOfPeripheralsWithIdentifiers:(NSArray *)uuids
{
    if([self centralManager] != nil)
    {
        NSArray * peripherals = [[self centralManager] retrievePeripheralsWithIdentifiers:uuids];
        
        for(id _peripheral in peripherals)
        {
            CBPeripheral *peripheral = _peripheral;
            [self onUpdatePeripheral:peripheral withEvent:@"OnRetrievedPeripheralWithUUID" andIdentifer:[peripheral name]];
        }
    }
}

-(void) connectToPeripheralWithIdentifier:(NSString *)identifier
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:identifier];
        if(peripheral != nil)
        {
            [[self centralManager] cancelPeripheralConnection:peripheral];
            [[self centralManager] connectPeripheral:peripheral options:nil];
        }
    }
}

-(void) disconnectPeripheralWithIdentifier:(NSString *)identifier
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:identifier];
        if(peripheral != nil)
            [[self centralManager] cancelPeripheralConnection:peripheral];
    }
}

-(void) readCharacteristicWithPeripherialId:(NSString *)peripheralId andServiceId:(NSString *) serviceId andCharacteristicId:(NSString *)characteristicId
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:peripheralId];
        if(peripheral != nil)
        {
            CBCharacteristic *characteristic = [[self characteristicList] objectForKey:[NSString stringWithFormat:@"%@%@%@", peripheralId, serviceId, characteristicId]];
            
            if(characteristic != nil)
                [peripheral readValueForCharacteristic:characteristic];
        }
    }
}

-(void) readDescriptorWithPeripherialId:(NSString *)peripheralId andServiceId:(NSString *) serviceId andCharacteristicId:(NSString *)characteristicId andDescriptorId:(NSString *)descriptorId
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:peripheralId];
        if(peripheral != nil)
        {
            CBCharacteristic *characteristic = [[self characteristicList] objectForKey:[NSString stringWithFormat:@"%@%@%@", peripheralId, serviceId, characteristicId]];
            
            if(characteristic != nil)
            {
                
                for(id obj in [characteristic descriptors])
                {
                    CBDescriptor *descriptor = obj;
                    
                    NSString *dUuid = [NSString stringWithFormat:@"%@", [descriptor UUID]];
                    
                    if ([dUuid isEqualToString:descriptorId]) {
                        
                        
                        NSString *dValue = [[descriptor value] base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed];
                        
                        NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId,
                                             (int)strlen([serviceId UTF8String]), serviceId,
                                             (int)strlen([characteristicId UTF8String]), characteristicId,
                                             (int)strlen([dUuid UTF8String]), dUuid,
                                             (int)strlen([dValue UTF8String]), dValue];
                        
                        
                        UnitySendMessage ([[self gameObjName] UTF8String], "OnDescriptorRead", [message UTF8String]);
                        
                        [peripheral readValueForDescriptor:descriptor];
                        
                    }
                    
                }
            }
        }
    }
}

- (void)peripheral:(CBPeripheral *)peripheral didUpdateValueForDescriptor:(CBDescriptor *)descriptor error:(NSError *)error
{
    if(error == nil)
    {
        CBCharacteristic *characteristic = [descriptor characteristic];
        
        NSString *dUuid = [NSString stringWithFormat:@"%@", [descriptor UUID]];
        NSString *characteristicId = [NSString stringWithFormat:@"%@", [characteristic UUID]];
        NSString *serviceId = [NSString stringWithFormat:@"%@", [[characteristic service] UUID]];
        
        NSString *peripheralId = @"Unknown";
        
        NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
        id key;
        while ((key = [enumerator nextObject]))
        {
            CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
            if ([listPeripheral isEqual:peripheral])
            {
                peripheralId = key;
                break;
            }
        }
        
        NSString *dValue = [[descriptor value] base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed];
        
        NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId,
                             (int)strlen([serviceId UTF8String]), serviceId,
                             (int)strlen([characteristicId UTF8String]), characteristicId,
                             (int)strlen([dUuid UTF8String]), dUuid,
                             (int)strlen([dValue UTF8String]), dValue];
        
        
        UnitySendMessage ([[self gameObjName] UTF8String], "OnDescriptorRead", [message UTF8String]);
    }
    
}

- (void)peripheral:(CBPeripheral *)peripheral didWriteValueForDescriptor:(CBDescriptor *)descriptor error:(NSError *)error
{
    if(error == nil)
    {
        CBCharacteristic *characteristic = [descriptor characteristic];
        
        NSString *dUuid = [NSString stringWithFormat:@"%@", [descriptor UUID]];
        NSString *characteristicId = [NSString stringWithFormat:@"%@", [characteristic UUID]];
        NSString *serviceId = [NSString stringWithFormat:@"%@", [[characteristic service] UUID]];
        
        
        NSString *peripheralId = @"Unknown";
        
        NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
        id key;
        while ((key = [enumerator nextObject]))
        {
            CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
            if ([listPeripheral isEqual:peripheral])
            {
                peripheralId = key;
                break;
            }
        }
        
        NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId,
                             (int)strlen([serviceId UTF8String]), serviceId,
                             (int)strlen([characteristicId UTF8String]), characteristicId,
                             (int)strlen([dUuid UTF8String]), dUuid];
        
        
        UnitySendMessage ([[self gameObjName] UTF8String], "OnDidWriteDescriptor", [message UTF8String]);
    }
}

-(void) writeCharacteristicWithPeripherialId:(NSString *)peripheralId andServiceId:(NSString *) serviceId andCharacteristicId:(NSString *)characteristicId withData:(NSData *)data withResponse:(bool) withResponse
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:peripheralId];
        if(peripheral != nil)
        {
            CBCharacteristic *characteristic = [[self characteristicList] objectForKey:[NSString stringWithFormat:@"%@%@%@", peripheralId, serviceId, characteristicId]];
            
            
            if(characteristic != nil)
            {
                if(withResponse)
                    [peripheral writeValue:data forCharacteristic:characteristic type:CBCharacteristicWriteWithResponse];
                else
                    [peripheral writeValue:data forCharacteristic:characteristic type:CBCharacteristicWriteWithoutResponse];
            }
        }
    }
}

-(void) writeDescriptorWithPeripherialId:(NSString *)peripheralId andServiceId:(NSString *) serviceId andCharacteristicId:(NSString *)characteristicId andDescriptorId:(NSString *)descriptorId withData:(NSData *)data
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:peripheralId];
        if(peripheral != nil)
        {
            CBCharacteristic *characteristic = [[self characteristicList] objectForKey:[NSString stringWithFormat:@"%@%@%@", peripheralId, serviceId, characteristicId]];
            
            if(characteristic != nil)
            {
                
                for(id obj in [characteristic descriptors])
                {
                    CBDescriptor *descriptor = obj;
                    
                    NSString *dUuid = [NSString stringWithFormat:@"%@", [descriptor UUID]];
                    
                    if ([dUuid isEqualToString:descriptorId]) {
                        
                        
                        [peripheral writeValue:data forDescriptor:descriptor];
                        
                    }
                    
                }
            }
        }
    }
    
}

-(void) subscribeToCharacteristicWithPeripherialId:(NSString *)peripheralId andServiceId:(NSString *) serviceId andCharacteristicId:(NSString *)characteristicId
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:peripheralId];
        if(peripheral != nil)
        {
            
            CBCharacteristic *characteristic = [[self characteristicList] objectForKey:[NSString stringWithFormat:@"%@%@%@", peripheralId, serviceId, characteristicId]];
            
            if(characteristic != nil)
                [peripheral setNotifyValue:true forCharacteristic:characteristic];
        }
    }
}

-(void) unsubscribeFromCharacteristicWithPeripherialId:(NSString *)peripheralId andServiceId:(NSString *) serviceId andCharacteristicId:(NSString *)characteristicId
{
    if([self centralManager] != nil)
    {
        CBPeripheral *peripheral = [[self peripheralList] objectForKey:peripheralId];
        if(peripheral != nil)
        {
            CBCharacteristic *characteristic = [[self characteristicList] objectForKey:[NSString stringWithFormat:@"%@%@%@", peripheralId, serviceId, characteristicId]];
            
            if(characteristic != nil)
                [peripheral setNotifyValue:false forCharacteristic:characteristic];
        }
    }
}

-(NSString *) onUpdatePeripheral:(CBPeripheral *)peripheral withEvent:(NSString *)event andIdentifer:(NSString *)identifier
{
    NSString *message;
    NSString *ident = identifier == nil ? @"Unknown" : identifier;
    
    NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
    id key;
    while ((key = [enumerator nextObject]))
    {
        CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
        if ([listPeripheral isEqual:peripheral])
        {
            
            message = [NSString stringWithFormat:@"%d:%@%d:%@", (int)strlen([(NSString*)key UTF8String]), (NSString *)key, (int)strlen([ident UTF8String]), ident];
            
            UnitySendMessage ([[self gameObjName] UTF8String], [event UTF8String], [message UTF8String]);
            
            return [NSString stringWithFormat:@"%@", key];
        }
    }
    
    NSString *newKey = [[NSUUID UUID] UUIDString];
    
    if([peripheral identifier] != nil && [[[peripheral identifier] UUIDString] length] > 0)
        newKey = [[peripheral identifier] UUIDString];
    
    [[self peripheralList] setObject:peripheral forKey:newKey];
    
    message = [NSString stringWithFormat:@"%d:%@%d:%@", (int)strlen([newKey UTF8String]), newKey, (int)strlen([ident UTF8String]), ident];
    
    UnitySendMessage ([[self gameObjName] UTF8String], [event UTF8String], [message UTF8String]);
    
    return [NSString stringWithFormat:@"%@", newKey];
    
}

- (void)peripheralDidUpdateRSSI:(CBPeripheral *)peripheral  error:(NSError *)error
{
    if(error == nil)
    {
        
        
        
        NSString *peripheralId = @"Unknown";
        
        NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
        id key;
        while ((key = [enumerator nextObject]))
        {
            CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
            if ([listPeripheral isEqual:peripheral])
            {
                peripheralId = key;
                break;
            }
        }
        
        if([peripheral RSSI] != nil)
        {
            NSNumber *RSSI = [peripheral RSSI];
            NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId, (int)strlen([[RSSI stringValue] UTF8String]), [RSSI stringValue]];
            UnitySendMessage ([[self gameObjName] UTF8String], "OnRssiUpdate", [message UTF8String]);
        }
    }
}

#pragma mark - CBCentralManagerDelegate

- (void)centralManager:(CBCentralManager *)central
  didConnectPeripheral:(CBPeripheral *)peripheral
{
    [self onUpdatePeripheral:peripheral withEvent:@"OnConnectedPeripheral" andIdentifer:[peripheral name]];
    
    [peripheral setDelegate:self];
    [peripheral discoverServices:nil];
    
    
}

- (void)centralManager:(CBCentralManager *)central
didDisconnectPeripheral:(CBPeripheral *)peripheral
                 error:(NSError *)error
{
    
    //if(error == nil)
    [self onUpdatePeripheral:peripheral withEvent:@"OnDisconnectedPeripheral" andIdentifer:[peripheral name]];
    
}

- (void)centralManager:(CBCentralManager *)central
didFailToConnectPeripheral:(CBPeripheral *)peripheral
                 error:(NSError *)error
{
    //if(error == nil)
    [self onUpdatePeripheral:peripheral withEvent:@"OnDisconnectedPeripheral" andIdentifer:[peripheral name]];
}

-(void)unityBleSendUpdateWithPeripheralId:(NSString *)peripheralId withKey:(NSString *)key andValue:(NSString *)value
{
    NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId, (int)strlen([value  UTF8String]), value];
    
    UnitySendMessage ([[self gameObjName] UTF8String], [key UTF8String], [message UTF8String]);
}

- (void)centralManager:(CBCentralManager *)central
 didDiscoverPeripheral:(CBPeripheral *)peripheral
     advertisementData:(NSDictionary *)advertisementData
                  RSSI:(NSNumber *)RSSI
{
    
    NSString *peripheralId = [self onUpdatePeripheral:peripheral withEvent:@"OnDiscoveredPeripheral" andIdentifer:[advertisementData objectForKey:CBAdvertisementDataLocalNameKey]];
    
    /*
     NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
     id key;
     while ((key = [enumerator nextObject]))
     {
     CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
     if ([listPeripheral isEqual:peripheral])
     {
     peripheralId = key;
     break;
     }
     }
     */
    
    
    
    NSString *localName = [advertisementData objectForKey:CBAdvertisementDataLocalNameKey];
    if(localName != nil)
    {
        [self unityBleSendUpdateWithPeripheralId:peripheralId withKey:@"OnAdvertisementDataLocalName" andValue:localName];
    }
    
    
    NSData *manufactureDataKey = [advertisementData objectForKey:CBAdvertisementDataManufacturerDataKey];
    if(manufactureDataKey != nil)
    {
        [self unityBleSendUpdateWithPeripheralId:peripheralId withKey:@"OnAdvertisementDataManufactureData" andValue:[manufactureDataKey base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed]];
    }
    
    
    NSDictionary *serviceData = [advertisementData objectForKey:CBAdvertisementDataServiceDataKey];
    if(serviceData != nil)
    {
        for(CBUUID *sUUID in serviceData)
        {
            NSString *serviceUUID = [sUUID UUIDString];
            
            NSData *serviceDataData = [serviceData objectForKey: sUUID];
            
            if(serviceDataData != nil)
            {
                
                NSString *dataString = [serviceDataData base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed];
                NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@",
                                     (int)strlen([peripheralId UTF8String]), peripheralId,
                                     (int)strlen([serviceUUID UTF8String]), serviceUUID,
                                     (int)strlen([dataString  UTF8String]), dataString];
                
                UnitySendMessage ([[self gameObjName] UTF8String], "OnAdvertisementDataServiceData", [message UTF8String]);
            }
        }
        
    }
    
    NSArray *serviceUUIDS = [advertisementData objectForKey:CBAdvertisementDataServiceUUIDsKey];
    if(serviceUUIDS != nil)
    {
        for(CBUUID *sUUID in serviceUUIDS)
        {
            NSString *serviceUUID = [sUUID UUIDString];
            [self unityBleSendUpdateWithPeripheralId:peripheralId withKey:@"OnAdvertisementDataServiceUUID" andValue:serviceUUID];
        }
    }
    
    
    NSArray *overflowServiceUUIDs = [advertisementData objectForKey:CBAdvertisementDataOverflowServiceUUIDsKey];
    if(overflowServiceUUIDs != nil)
    {
        for(CBUUID *sUUID in overflowServiceUUIDs)
        {
            NSString *serviceUUID = [sUUID UUIDString];
            [self unityBleSendUpdateWithPeripheralId:peripheralId withKey:@"OnAdvertisementDataOverflowServiceUUID" andValue:serviceUUID];
        }
    }
    
    
    NSNumber *txPowerLevel = [advertisementData objectForKey:CBAdvertisementDataTxPowerLevelKey];
    if(txPowerLevel)
    {
        [self unityBleSendUpdateWithPeripheralId:peripheralId withKey:@"OnAdvertisementDataTxPowerLevel" andValue:[txPowerLevel stringValue]];
    }
    
    NSNumber *isConnectable = [advertisementData objectForKey:CBAdvertisementDataIsConnectable];
    if(isConnectable)
    {
        [self unityBleSendUpdateWithPeripheralId:peripheralId withKey:@"OnAdvertisementDataIsConnectable" andValue:[isConnectable stringValue]];
    }
    
    NSArray * solicitedServiceUUIDs = [advertisementData objectForKey:CBAdvertisementDataSolicitedServiceUUIDsKey];
    if(solicitedServiceUUIDs != nil)
    {
        for(CBUUID *sUUID in solicitedServiceUUIDs)
        {
            NSString *serviceUUID = [sUUID UUIDString];
            [self unityBleSendUpdateWithPeripheralId:peripheralId withKey:@"OnAdvertisementDataSolicitedServiceUUID" andValue:serviceUUID];
        }
    }
    
    
    if(RSSI != nil)
    {
        [self unityBleSendUpdateWithPeripheralId:peripheralId withKey:@"OnRssiUpdate" andValue:[RSSI stringValue]];
    }
    
    /*
     NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId, (int)strlen([[RSSI stringValue] UTF8String]), [RSSI stringValue]];
     
     UnitySendMessage ([[self gameObjName] UTF8String], "OnRssiUpdate", [message UTF8String]);
     */
    
    
}

- (void)centralManager:(CBCentralManager *)central
didRetrieveConnectedPeripherals:(NSArray *)peripherals
{
    for(id peripheral in peripherals)
    {
        [self onUpdatePeripheral:peripheral withEvent:@"OnDiscoveredPeripheral" andIdentifer:[peripheral name]];
    }
}

- (void)centralManager:(CBCentralManager *)central
didRetrievePeripherals:(NSArray *)peripherals
{
    for(id peripheral in peripherals)
    {
        [self onUpdatePeripheral:peripheral withEvent:@"OnDiscoveredPeripheral" andIdentifer:[peripheral name]];
    }
}

// method called whenever the device state changes.
- (void)centralManagerDidUpdateState:(CBCentralManager *)central
{
    switch ([central state]) {
        case CBCentralManagerStatePoweredOff:
            UnitySendMessage ([[self gameObjName] UTF8String], "OnBleStateUpdate", "Powered Off");
            break;
        case CBCentralManagerStatePoweredOn:
            UnitySendMessage ([[self gameObjName] UTF8String], "OnBleStateUpdate", "Powered On");
            break;
        case CBCentralManagerStateResetting:
            UnitySendMessage ([[self gameObjName] UTF8String], "OnBleStateUpdate", "Resetting");
            break;
        case CBCentralManagerStateUnauthorized:
            UnitySendMessage ([[self gameObjName] UTF8String], "OnBleStateUpdate", "Unauthorized");
            break;
        case CBCentralManagerStateUnknown:
            UnitySendMessage ([[self gameObjName] UTF8String], "OnBleStateUpdate", "Unknown");
            break;
        case CBCentralManagerStateUnsupported:
            UnitySendMessage ([[self gameObjName] UTF8String], "OnBleStateUpdate", "Unsupported");
            break;
            
        default:
            UnitySendMessage ([[self gameObjName] UTF8String], "OnBleStateUpdate", "Unknown");
            break;
    }
}

- (void)centralManager:(CBCentralManager *)central
      willRestoreState:(NSDictionary *)dict
{
    
}

-(void)serviceDiscovered:(CBPeripheral *)peripheral
{
    for(id cbservice in [peripheral services])
    {
        CBService *service = cbservice;
        
        NSString *peripheralId = @"Unknown";
        
        NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
        id key;
        while ((key = [enumerator nextObject]))
        {
            CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
            if ([listPeripheral isEqual:peripheral])
            {
                peripheralId = key;
                break;
            }
        }
        NSString *uuid = [NSString stringWithFormat:@"%@", [service UUID]];
        NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId, (int)strlen([uuid UTF8String]), uuid];
        
        UnitySendMessage ([[self gameObjName] UTF8String], "OnDiscoveredService", [message UTF8String]);
        
        [peripheral discoverCharacteristics:nil forService:service];
    }
    
}

#pragma mark - CBPeripheralDelegate


- (void)peripheral:(CBPeripheral *)peripheral
didDiscoverServices:(NSError *)error
{
    if(error == nil)
    {
        [self serviceDiscovered:peripheral];
    }
}

-(void)characteristicDiscovered:(CBPeripheral *)peripheral forService:(CBService *)service
{
    for(id cbCharacteristic in [service characteristics])
    {
        CBCharacteristic *characteristic = cbCharacteristic;
        
        
        NSString *peripheralId = @"Unknown";
        
        NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
        id key;
        while ((key = [enumerator nextObject]))
        {
            CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
            if ([listPeripheral isEqual:peripheral])
            {
                peripheralId = key;
                break;
            }
        }
        
        NSString *cUuid = [NSString stringWithFormat:@"%@", [characteristic UUID]];
        NSString *sUuid = [NSString stringWithFormat:@"%@", [service UUID]];
        
        NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId, (int)strlen([sUuid UTF8String]), sUuid, (int)strlen([cUuid UTF8String]), cUuid];
        
        
        [[self characteristicList] setObject:characteristic forKey:[NSString stringWithFormat:@"%@%@%@", peripheralId, sUuid, cUuid]];
        
        UnitySendMessage ([[self gameObjName] UTF8String], "OnDiscoveredCharacteristic", [message UTF8String]);
        
        [peripheral discoverDescriptorsForCharacteristic:cbCharacteristic];
        
    }
}

- (void)peripheral:(CBPeripheral *)peripheral didDiscoverDescriptorsForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
{
    if(error == nil)
    {
        
        NSString *peripheralId = @"Unknown";
        NSString *cUuid = [NSString stringWithFormat:@"%@", [characteristic UUID]];
        NSString *sUuid = [NSString stringWithFormat:@"%@", [[characteristic service] UUID]];
        
        NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
        id key;
        while ((key = [enumerator nextObject]))
        {
            CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
            if ([listPeripheral isEqual:peripheral])
            {
                peripheralId = key;
                break;
            }
        }
        
        for(id obj in [characteristic descriptors])
        {
            CBDescriptor *descriptor = obj;
            
            NSString *dUuid = [NSString stringWithFormat:@"%@", [descriptor UUID]];
            
            NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId,
                                 (int)strlen([sUuid UTF8String]), sUuid,
                                 (int)strlen([cUuid UTF8String]), cUuid,
                                 (int)strlen([dUuid UTF8String]), dUuid];
            
            UnitySendMessage ([[self gameObjName] UTF8String], "OnDiscoveredDescriptor", [message UTF8String]);
        }
    }
    
}

// Invoked when you discover the characteristics of a specified service.
- (void)peripheral:(CBPeripheral *)peripheral didDiscoverCharacteristicsForService:(CBService *)service error:(NSError *)error
{
    if(error == nil)
    {
        [self characteristicDiscovered:peripheral forService:service];;
    }
}

- (void)peripheral:(CBPeripheral *)peripheral
 didModifyServices:(NSArray *)invalidatedServices
{
    [peripheral discoverServices:nil];
}

// Invoked when you retrieve a specified characteristic's value, or when the peripheral device notifies your app that the characteristic's value has changed.
- (void)peripheral:(CBPeripheral *)peripheral didUpdateValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error
{
    if(error == nil)
    {
        if([characteristic value] != nil)
        {
            
            NSString *peripheralId = @"Unknown";
            
            NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
            id key;
            while ((key = [enumerator nextObject]))
            {
                CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
                if ([listPeripheral isEqual:peripheral])
                {
                    peripheralId = key;
                    break;
                }
            }
            
            //NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), (int)strlen([(NSString*)[characteristic UUID] UTF8String]), peripheralId, [characteristic UUID], [[characteristic value] base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed]];
            
            NSString *cUuid = [NSString stringWithFormat:@"%@", [characteristic UUID]];
            NSString *sUuid = [NSString stringWithFormat:@"%@", [[characteristic service] UUID] ];
            NSString *cValue = [[characteristic value] base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed];
            
            NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId,
                                 (int)strlen([sUuid UTF8String]), sUuid,
                                 (int)strlen([cUuid UTF8String]), cUuid,
                                 (int)strlen([cValue UTF8String]), cValue];
            
            
            UnitySendMessage ([[self gameObjName] UTF8String], "OnBluetoothData", [message UTF8String]);
        }
    }
    
}

- (void)peripheral:(CBPeripheral *)peripheral
didWriteValueForCharacteristic:(CBCharacteristic *)characteristic
             error:(NSError *)error
{
    if(error == nil)
    {
        NSString *peripheralId = @"Unknown";
        
        NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
        id key;
        while ((key = [enumerator nextObject]))
        {
            CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
            if ([listPeripheral isEqual:peripheral])
            {
                peripheralId = key;
                break;
            }
        }
        
        NSString *cUuid = [NSString stringWithFormat:@"%@", [characteristic UUID]];
        NSString *sUuid = [NSString stringWithFormat:@"%@", [[characteristic service] UUID]];
        
        NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId, (int)strlen([sUuid UTF8String]), sUuid, (int)strlen([cUuid UTF8String]), cUuid ];
        
        
        [[self characteristicList] setObject:characteristic forKey:[NSString stringWithFormat:@"%@%@%@", peripheralId,  sUuid, cUuid]];
        
        UnitySendMessage ([[self gameObjName] UTF8String], "OnDidWriteCharacteristic", [message UTF8String]);
    }
    
}

- (void)peripheral:(CBPeripheral *)peripheral didUpdateNotificationStateForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error{
    
    if(error == nil)
    {
        NSString *peripheralId = @"Unknown";
        
        NSEnumerator *enumerator = [[self peripheralList] keyEnumerator];
        id key;
        while ((key = [enumerator nextObject]))
        {
            CBPeripheral *listPeripheral = [[self peripheralList] objectForKey:key];
            if ([listPeripheral isEqual:peripheral])
            {
                peripheralId = key;
                break;
            }
        }
        
        NSString *cUuid = [NSString stringWithFormat:@"%@", [characteristic UUID]];
        NSString *sUuid = [NSString stringWithFormat:@"%@", [[characteristic service] UUID]];
        
        NSString *message = [NSString stringWithFormat:@"%d:%@%d:%@%d:%@1:%d", (int)strlen([(NSString*)peripheralId UTF8String]), peripheralId, (int)strlen([sUuid UTF8String]), sUuid, (int)strlen([cUuid UTF8String]), cUuid, [characteristic isNotifying] ? 1 : 0 ];
        
        
        [[self characteristicList] setObject:characteristic forKey:[NSString stringWithFormat:@"%@%@%@", peripheralId, sUuid, cUuid]];
        
        UnitySendMessage ([[self gameObjName] UTF8String], "OnDidUpdateNotificationStateForCharacteristicAction", [message UTF8String]);
    }
}
static char base64EncodingTable[64] =
{
    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
    'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
    'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
    'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/'
};

+ (NSString *) base64StringFromData: (NSData *)data length: (int)length
{
    unsigned long ixtext, lentext;
    long ctremaining;
    unsigned char input[3], output[4];
    short i, charsonline = 0, ctcopy;
    const unsigned char *raw;
    NSMutableString *result;
    
    lentext = [data length];
    if (lentext < 1)
        return @"";
    result = [NSMutableString stringWithCapacity: lentext];
    raw = (const unsigned char *)[data bytes];
    ixtext = 0;
    
    while (true) {
        ctremaining = lentext - ixtext;
        if (ctremaining <= 0)
            break;
        for (i = 0; i < 3; i++) {
            unsigned long ix = ixtext + i;
            if (ix < lentext)
                input[i] = raw[ix];
            else
                input[i] = 0;
        }
        output[0] = (input[0] & 0xFC) >> 2;
        output[1] = ((input[0] & 0x03) << 4) | ((input[1] & 0xF0) >> 4);
        output[2] = ((input[1] & 0x0F) << 2) | ((input[2] & 0xC0) >> 6);
        output[3] = input[2] & 0x3F;
        ctcopy = 4;
        switch (ctremaining) {
            case 1:
                ctcopy = 2;
                break;
            case 2:
                ctcopy = 3;
                break;
        }
        
        for (i = 0; i < ctcopy; i++)
            [result appendString: [NSString stringWithFormat: @"%c", base64EncodingTable[output[i]]]];
        
        for (i = ctcopy; i < 4; i++)
            [result appendString: @"="];
        
        ixtext += 3;
        charsonline += 4;
        
        if ((length > 0) && (charsonline >= length))
            charsonline = 0;
    }
    return result;
}

@end



extern "C" {
    
    UnityBleBridge *unityBleBridge;
    
    void iOSBleBridgeStartup (char *gameObjName, BOOL isCentral) {
        
        NSString *name = [NSString stringWithFormat:@"%s", gameObjName];
        unityBleBridge = [[UnityBleBridge alloc] initWithGameObjectName:name];
        
        [unityBleBridge startupAs:isCentral];
        
        UnitySendMessage ([[unityBleBridge gameObjName] UTF8String], "OnStartup", "Active");
    }
    
    void iOSBleBridgeShutdown () {
        
        if (unityBleBridge != nil) {
            
            UnitySendMessage ([[unityBleBridge gameObjName] UTF8String], "OnShutdown", "Inactive");
            
            [unityBleBridge shutdown];
#if !__has_feature(objc_arc)
            [unityBleBridge release];
#endif
            unityBleBridge = nil;
        }
    }
    
    void iOSBleBridgePauseWithState (BOOL isPaused) {
        
        if (unityBleBridge != nil)
            [unityBleBridge pauseWithState:isPaused];
    }
    
    void iOSBleBridgeScanForPeripheralsWithServiceUUIDs (char *uuidsCString) {
        
        if (unityBleBridge != nil)
        {
            NSMutableArray *uuidArray = [[NSMutableArray alloc] init];
            
            if (uuidsCString != nil)
            {
                NSString *uuidsString = [NSString stringWithFormat:@"%s", uuidsCString];
                NSArray *serviceUUIDs = [uuidsString componentsSeparatedByString:@"|"];
                
                if (serviceUUIDs.count > 0)
                {
                    for (NSString* sUUID in serviceUUIDs)
                        [uuidArray addObject:[CBUUID UUIDWithString:sUUID]];
                }
            }
            
            [unityBleBridge scanForPeripheralsWithServiceUUIDs:uuidArray andOptions:nil];
#if !__has_feature(objc_arc)
            [uuidArray release];
#endif
        }
    }
    
    void iOSBleBridgeStopScanning () {
        
        if (unityBleBridge != nil)
            [unityBleBridge stopScanning];
    }
    
    void iOSBleBridgeRetrieveListOfPeripheralsWithServiceUUIDs (char *uuidsCString) {
        
        if (unityBleBridge != nil)
        {
            NSMutableArray *uuidArray = [[NSMutableArray alloc] init];
            
            if (uuidsCString != nil)
            {
                NSString *uuidsString = [NSString stringWithFormat:@"%s", uuidsCString];
                NSArray *serviceUUIDs = [uuidsString componentsSeparatedByString:@"|"];
                
                if (serviceUUIDs.count > 0)
                {
                    for (NSString* sUUID in serviceUUIDs)
                        [uuidArray addObject:[CBUUID UUIDWithString:sUUID]];
                }
            }
            
            [unityBleBridge retrieveListOfPeripheralsWithServiceUUIDs:uuidArray];
#if !__has_feature(objc_arc)
            [uuidArray release];
#endif
        }
    }
    
    void iOSBleBridgeRetrieveListOfPeripheralsWithUUIDs (char *uuidsCString) {
        
        if (unityBleBridge != nil)
        {
            NSMutableArray *uuidArray = [[NSMutableArray alloc] init];
            
            if (uuidsCString != nil)
            {
                NSString *uuidsString = [NSString stringWithFormat:@"%s", uuidsCString];
                NSArray *serviceUUIDs = [uuidsString componentsSeparatedByString:@"|"];
                
                if (serviceUUIDs.count > 0)
                {
                    for (NSString* sUUID in serviceUUIDs)
                        [uuidArray addObject:[CBUUID UUIDWithString:sUUID]];
                }
            }
            
            [unityBleBridge  retrieveListOfPeripheralsWithIdentifiers:uuidArray];
#if !__has_feature(objc_arc)
            [uuidArray release];
#endif
        }
    }
    
    void iOSBleBridgeConnectToPeripheralWithIdentifier (char *identifier) {
        
        if (unityBleBridge && identifier != nil)
        {
            [unityBleBridge connectToPeripheralWithIdentifier:[NSString stringWithFormat:@"%s", identifier]];
        }
    }
    
    void iOSBleBridgeDisconnectPeripheralWithIdentifier (char *identifier) {
        
        if (unityBleBridge && identifier != nil)
        {
            [unityBleBridge disconnectPeripheralWithIdentifier:[NSString stringWithFormat:@"%s", identifier]];
        }
    }
    
    void iOSBleBridgeReadCharacteristicWithIdentifiers (char *peripheralId, char *serviceId, char *characteristicId) {
        
        if (unityBleBridge && peripheralId != nil && serviceId != nil && characteristicId != nil)
        {
            [unityBleBridge readCharacteristicWithPeripherialId:[NSString stringWithFormat:@"%s", peripheralId] andServiceId:[NSString stringWithFormat:@"%s", serviceId] andCharacteristicId:[NSString stringWithFormat:@"%s", characteristicId]];
        }
    }
    
    void iOSBleBridgeWriteCharacteristicWithIdentifiers (char *peripheralId, char *serviceId, char *characteristicId, unsigned char *data, int length, BOOL withResponse) {
        
        if (unityBleBridge && peripheralId != nil && serviceId != nil && characteristicId != nil && data != nil && length > 0)
        {
            [unityBleBridge writeCharacteristicWithPeripherialId:[NSString stringWithFormat:@"%s", peripheralId] andServiceId:[NSString stringWithFormat:@"%s", serviceId] andCharacteristicId:[NSString stringWithFormat:@"%s", characteristicId] withData:[NSData dataWithBytes:data length:length] withResponse:withResponse];
        }
    }
    
    void iOSBleBridgeSubscribeToCharacteristicWithIdentifiers (char *peripheralId, char *serviceId, char *characteristicId) {
        
        if (unityBleBridge && peripheralId != nil && serviceId != nil && characteristicId != nil)
        {
            [unityBleBridge subscribeToCharacteristicWithPeripherialId:[NSString stringWithFormat:@"%s", peripheralId] andServiceId:[NSString stringWithFormat:@"%s", serviceId] andCharacteristicId:[NSString stringWithFormat:@"%s", characteristicId]];
        }
    }
    
    void iOSBleBridgeUnSubscribeFromCharacteristicWithIdentifiers (char *peripheralId, char *serviceId, char *characteristicId) {
        
        if (unityBleBridge && peripheralId != nil && serviceId != nil && characteristicId != nil)
        {
            [unityBleBridge unsubscribeFromCharacteristicWithPeripherialId:[NSString stringWithFormat:@"%s", peripheralId] andServiceId:[NSString stringWithFormat:@"%s", serviceId] andCharacteristicId:[NSString stringWithFormat:@"%s", characteristicId]];
        }
    }
    
    void iOSBleBridgeReadDescriptorWithIdentifiers (char * peripheralId, char * serviceId, char * characteristicId, char * descriptorId)
    {
        if (unityBleBridge && peripheralId != nil && serviceId != nil && characteristicId != nil && descriptorId != nil)
        {
            [unityBleBridge readDescriptorWithPeripherialId:[NSString stringWithFormat:@"%s", peripheralId] andServiceId:[NSString stringWithFormat:@"%s", serviceId] andCharacteristicId:[NSString stringWithFormat:@"%s", characteristicId] andDescriptorId:[NSString stringWithFormat:@"%s", characteristicId]];
        }
    }
    
    void iOSBleBridgeWriteDescriptorWithIdentifiers (char * peripheralId, char * serviceId, char * characteristicId, char * descriptorId, unsigned char * data, int length)
    {
        if (unityBleBridge && peripheralId != nil && serviceId != nil && characteristicId != nil && descriptorId != nil)
        {
            [unityBleBridge writeDescriptorWithPeripherialId:[NSString stringWithFormat:@"%s", peripheralId] andServiceId:[NSString stringWithFormat:@"%s", serviceId] andCharacteristicId:[NSString stringWithFormat:@"%s", characteristicId] andDescriptorId:[NSString stringWithFormat:@"%s", descriptorId] withData:[NSData dataWithBytes:data length:length]];
        }
    }
    
    void iOSBleBridgeReadRssiWithIdentifier(char *peripheralId)
    {
        if (unityBleBridge && peripheralId != nil)
        {
            [unityBleBridge readRssiWithIdentifier:[NSString stringWithFormat:@"%s", peripheralId] ];
        }
    }
    
}