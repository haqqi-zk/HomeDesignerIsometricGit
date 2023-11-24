//
//  TopGamesSocial.m
//  activitysheetplugin
//
//  Created by Robert on 2014.07.25.. ---> 1.0
//  Modified by Robert on 2014.10.21. ---> 1.1
//  Copyright (c) 2014 Top Games. All rights reserved.
//

#import "TopGamesSocial.h"
#import <UIKit/UIKit.h> // 1.1
#import "SSCWhatsAppActivity.h"
#import "VKSdk.h"


/* 1.1 */
#define SYSTEM_VERSION_EQUAL_TO(v)                  ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] == NSOrderedSame)
#define SYSTEM_VERSION_GREATER_THAN(v)              ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] == NSOrderedDescending)
#define SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(v)  ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] != NSOrderedAscending)
#define SYSTEM_VERSION_LESS_THAN(v)                 ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] == NSOrderedAscending)
#define SYSTEM_VERSION_LESS_THAN_OR_EQUAL_TO(v)     ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] != NSOrderedDescending)
/* 1.1 */

@interface TopGamesSocial (){
UIActivityIndicatorView *spinner;
}
@end

@implementation TopGamesSocial


- (void)_presentActivitySheetWithData :(id)data{
    
    if ( [self isVersionSupported] == NO) // 1.1
        return;                           // 1.1
    
    UIActivityViewController *av = [[UIActivityViewController alloc]initWithActivityItems:[[NSArray alloc] initWithObjects:data,nil] applicationActivities:nil];
    [[[UIApplication sharedApplication]keyWindow].rootViewController presentViewController:av animated:YES completion:^{
          [self dismissLoadingSprite];
    }];
    
    if (SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(@"8.0"))
    {
        av.popoverPresentationController.sourceView = [[UIApplication sharedApplication]keyWindow].rootViewController.view; // 1.1
        av.popoverPresentationController.sourceRect = CGRectMake(100,100,5,5); // 1.1
    }
    
    spinner = [[UIActivityIndicatorView alloc]initWithFrame: [[UIApplication sharedApplication]keyWindow].frame];
    [spinner startAnimating];
    [[[UIApplication sharedApplication]keyWindow].rootViewController.view addSubview:spinner];
}



- (void)_presentActivitySheetWithArray : (NSArray*) data{
    
    
    
    
    if ( [self isVersionSupported] == NO) // 1.1
        return;                           // 1.1
    
    
    
    
    //VK
    /*NSArray *items = @[[UIImage imageNamed:@"apple"], @"Check out information about VK SDK" , [NSURL URLWithString:@"https://vk.com/dev/ios_sdk"]]; //1*/
    
   /* UIActivityViewController *activityViewController = [[UIActivityViewController alloc]
                                                        initWithActivityItems:items
                                                        applicationActivities:@[[VKActivity new]]]; //2*/
   
    
    
    
    SSCWhatsAppActivity *whatsAppActivity = [[SSCWhatsAppActivity alloc] init];
    
    UIActivityViewController *av =
    [[UIActivityViewController alloc]initWithActivityItems:data applicationActivities:@[whatsAppActivity,[VKActivity new]]];
    [[[UIApplication sharedApplication]keyWindow].rootViewController presentViewController:av animated:YES completion:^{
        [self dismissLoadingSprite];
    }];
    
    if (SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(@"8.0"))
    {
        av.popoverPresentationController.sourceView = [[UIApplication sharedApplication]keyWindow].rootViewController.view;// 1.1
        av.popoverPresentationController.sourceRect = CGRectMake(100,100,5,5); // 1.1
    }
    
    spinner = [[UIActivityIndicatorView alloc]initWithFrame: [[UIApplication sharedApplication]keyWindow].frame];
    [spinner startAnimating];
    [[[UIApplication sharedApplication]keyWindow].rootViewController.view addSubview:spinner];

}

- (void)dismissLoadingSprite{
    NSLog(@"Dismissing loading sprite");
    [spinner stopAnimating];
    [spinner removeFromSuperview];
}
/* 1.1 */
-(BOOL)isVersionSupported{
    
    if ( SYSTEM_VERSION_LESS_THAN(@"6.0") ){
        NSLog(@"This version of iOS is not supported activity sheet is only present with devices iOS 6+!");
        UIAlertView *av = [[UIAlertView alloc]initWithTitle:@"Share is not supported" message:@"Share is not supported on devices with software older than iOS 6, please update to the most current iOS software if your device is eligible to use sharing!" delegate:nil cancelButtonTitle:@"Ok" otherButtonTitles:nil, nil];
        
        [av show];
        return NO;
    }
    else{
        
        return YES;
    }
    
}/* 1.1 */

@end

extern "C"
{
    TopGamesSocial *social;
    
    void pluginInit(char *message)
    {
        [VKSdk initializeWithAppId:[NSString stringWithUTF8String:message]];
         NSLog(@"VK initialized");
    }
    
    void presentActivitySheetWithString(Byte *socialData,int _length){

      social = [[TopGamesSocial alloc]init];
      NSUInteger n = (unsigned long) _length;
      NSLog(@"Length is %lu",(unsigned long)n);
      NSData *d = [[NSData alloc]initWithBytes:socialData length:n*sizeof(char)];
      NSLog(@"data length %lu",(unsigned long)[d length]);
      NSString *s = [[NSString alloc]initWithData:(NSData*)d encoding:NSUTF8StringEncoding];
      [social _presentActivitySheetWithData:s];
    }
    void presentActivitySheetWithImage(Byte *socialData,int _length){
        social = [[TopGamesSocial alloc]init];
        NSUInteger n = (unsigned long) _length;
        NSLog(@"Length is %lu",(unsigned long)n);
        NSData *d = [[NSData alloc]initWithBytes:socialData length:n];
        UIImage *img = [[UIImage alloc]initWithData:d];
        [social _presentActivitySheetWithData:img];
    }
    void presentActivitySheetWithImageAndString(char *message,Byte *imgData,int _length){
  
        social = [[TopGamesSocial alloc]init];
        NSUInteger n = (unsigned long)_length;
        NSData *_imgData =[[NSData alloc]initWithBytes:imgData length:n];
        UIImage *img = [[UIImage alloc]initWithData:_imgData];
        NSString *_message = [NSString stringWithUTF8String:message];
        NSArray *data = [[NSArray alloc]initWithObjects:img,_message, nil];
        [social _presentActivitySheetWithArray:data];
        
    }
}


/*
#pragma mark - Navigation

// In a storyboard-based application, you will often want to do a little preparation before navigation
- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    // Get the new view controller using [segue destinationViewController].
    // Pass the selected object to the new view controller.
}
*/


