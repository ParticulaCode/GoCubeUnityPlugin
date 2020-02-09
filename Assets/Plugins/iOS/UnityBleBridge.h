//
//  UnityBleBridge.h
//  UnityBleBridge
//
//  Created by Jason Peterson on 10/22/14.
//  Copyright (c) 2014 Star Technologies. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

@interface UnityBleBridge : NSObject <CBCentralManagerDelegate, CBPeripheralDelegate>

@property(nonatomic, strong) NSString *gameObjName;

@property(nonatomic, strong) CBCentralManager *centralManager;

@property(nonatomic, strong) NSMutableDictionary *peripheralList;
@property(nonatomic, strong) NSMutableDictionary *serviceList;
@property(nonatomic, strong) NSMutableDictionary *characteristicList;

@end
