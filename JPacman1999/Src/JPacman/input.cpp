#include "JPacman.h"

#ifndef JPACMAN_COCOS2DX

#include "input.h"

#ifdef DIRECTINPUT_SUPPORT
#include <dinput.h>
#endif

extern HWND hWndMain;

#define MAX_SCANCODE 85

char ScantoAsc[MAX_SCANCODE+1]={
	0,
	27,'1','2','3','4','5','6','7','8','9','0','-','=','\b',0,'q',
	'w','e','r','t','y','u','i','o','p','[',']',13,0,'a','s','d',
	'f','g','h','j','k','l',';',39,0,0,0,'z','x','c','v','b',
	'n','m',',','.','/',0,'*',0,' ',0,0,0,0,0,0,0,
	0,0,0,0,0,0,'7','8','9','-','4','5','6',0,'1','2',
	'3','0',','
};

char ScantoAscShifted[MAX_SCANCODE+1]={
	0,
	0,'!',0,0,0,0,0,'&','*','(',')',0,'+',0,0,'Q',
	'W','E','R','T','Y','U','I','O','P',0,0,0,0,'A','S','D',
	'F','G','H','J','K','L',':',0,0,0,0,'Z','X','C','V','B',
	'N','M'
};

#endif

DWORD dwKeyState = 0;
int InputMode=0;
char CurrentKey;
int ShiftMode=FALSE;


extern void ReadKeyboardInput(void);

// allocate external variables
void (*ReadGameInput)(void) = ReadKeyboardInput;

#ifndef JPACMAN_COCOS2DX

#ifdef DIRECTINPUT_SUPPORT

/*
 *  We'll use up to the first ten input devices.
 *
 *  c_cpdiFound is the number of found devices.
 *  g_rgpdiFound[0] is the array of found devices.
 *  g_pdevCurrent is the device that we are using for input.
 */

#define MAX_DINPUT_DEVICES 10
int g_cpdevFound;
LPDIRECTINPUTDEVICE2 g_rgpdevFound[MAX_DINPUT_DEVICES];
LPDIRECTINPUTDEVICE2 g_pdevCurrent;

#endif


/*--------------------------------------------------------------------------
| AddInputDevice
|
| Records an input device in the array of found devices.
|
| In addition to stashing it in the array, we also add it to the device
| menu so the user can pick it.
|
*-------------------------------------------------------------------------*/

#ifdef DIRECTINPUT_SUPPORT

void AddInputDevice(LPDIRECTINPUTDEVICE pdev, LPCDIDEVICEINSTANCE pdi)
{

    if (g_cpdevFound < MAX_DINPUT_DEVICES) {

        HRESULT hRes;

        
        //  Convert it to a Device2 so we can Poll() it.
        

        hRes = pdev->QueryInterface(IID_IDirectInputDevice2,
                    (LPVOID *)&g_rgpdevFound[g_cpdevFound]);

        if (SUCCEEDED(hRes)) {
/*
            HMENU hmenu;

        
             //  Add its description to the menu.
        
            hmenu = GetSubMenu(GetMenu(hWndMain), 0);

            InsertMenu(hmenu, g_cpdevFound, MF_BYPOSITION | MF_STRING,
                       IDC_DEVICES + g_cpdevFound,
                       pdi->tszInstanceName);
*/
            g_cpdevFound++;
        }
    }
}

/*--------------------------------------------------------------------------
|
| SetDIDwordProperty
|
| Set a DWORD property on a DirectInputDevice.
|
*-------------------------------------------------------------------------*/

HRESULT
SetDIDwordProperty(LPDIRECTINPUTDEVICE pdev, REFGUID guidProperty,
                   DWORD dwObject, DWORD dwHow, DWORD dwValue)
{
   DIPROPDWORD dipdw;

   dipdw.diph.dwSize       = sizeof(dipdw);
   dipdw.diph.dwHeaderSize = sizeof(dipdw.diph);
   dipdw.diph.dwObj        = dwObject;
   dipdw.diph.dwHow        = dwHow;
   dipdw.dwData            = dwValue;

   return pdev->SetProperty(guidProperty, &dipdw.diph);

}

/*--------------------------------------------------------------------------
| InitKeyboardInput
|
| Initializes DirectInput for the keyboard.  Creates a keyboard device,
| sets the data format to our custom format, sets the cooperative level and
| adds it to the menu.
|
*-------------------------------------------------------------------------*/

BOOL InitKeyboardInput(LPDIRECTINPUT pdi)
{
   LPDIRECTINPUTDEVICE pdev;
   DIDEVICEINSTANCE di;

   // create the DirectInput keyboard device
   if(pdi->CreateDevice(GUID_SysKeyboard, &pdev, NULL) != DI_OK)
   {
      OutputDebugString("IDirectInput::CreateDevice FAILED\n");
      return FALSE;
   }

   // set keyboard data format
   if(pdev->SetDataFormat(&c_dfDIKeyboard) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetDataFormat FAILED\n");
      pdev->Release();
      return FALSE;
   }

   // set the cooperative level
   if(pdev->SetCooperativeLevel(hWndMain,
      DISCL_NONEXCLUSIVE | DISCL_FOREGROUND) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetCooperativeLevel FAILED\n");
      pdev->Release();
      return FALSE;
   }

   // set buffer size
   if (SetDIDwordProperty(pdev, DIPROP_BUFFERSIZE, 0, DIPH_DEVICE, KEYBUFSIZE) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetProperty(DIPH_DEVICE) FAILED\n");
      pdev->Release();
      return FALSE;
   }

   //
   // Get the name of the primary keyboard so we can add it to the menu.
   //
   di.dwSize = sizeof(di);
   if (pdev->GetDeviceInfo(&di) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::GetDeviceInfo FAILED\n");
      pdev->Release();
      return FALSE;
   }

   //
   // Add it to our list of devices.  If AddInputDevice succeeds,
   // he will do an AddRef.
   //
 
   
   AddInputDevice(pdev, &di);
   pdev->Release();

   return TRUE;
}
#endif

/*--------------------------------------------------------------------------
| InitJoystickInput
|
| Initializes DirectInput for an enumerated joystick device.
| Creates the device, device, sets the standard joystick data format,
| sets the cooperative level and adds it to the menu.
|
| If any step fails, just skip the device and go on to the next one.
|
*-------------------------------------------------------------------------*/
/*
BOOL FAR PASCAL InitJoystickInput(LPCDIDEVICEINSTANCE pdinst, LPVOID pvRef)
{
   LPDIRECTINPUT pdi = pvRef;
   LPDIRECTINPUTDEVICE pdev;
   DIPROPRANGE diprg;

   // create the DirectInput joystick device
   if(pdi->lpVtbl->CreateDevice(pdi, &pdinst->guidInstance, &pdev, NULL) != DI_OK)
   {
      OutputDebugString("IDirectInput::CreateDevice FAILED\n");
      return DIENUM_CONTINUE;
   }

   // set joystick data format
   if(pdev->lpVtbl->SetDataFormat(pdev, &c_dfDIJoystick) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetDataFormat FAILED\n");
      pdev->lpVtbl->Release(pdev);
      return DIENUM_CONTINUE;
   }

   // set the cooperative level
   if(pdev->lpVtbl->SetCooperativeLevel(pdev, hWndMain,
      DISCL_NONEXCLUSIVE | DISCL_FOREGROUND) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetCooperativeLevel FAILED\n");
      pdev->lpVtbl->Release(pdev);
      return DIENUM_CONTINUE;
   }

   // set X-axis range to (-1000 ... +1000)
   // This lets us test against 0 to see which way the stick is pointed.

   diprg.diph.dwSize       = sizeof(diprg);
   diprg.diph.dwHeaderSize = sizeof(diprg.diph);
   diprg.diph.dwObj        = DIJOFS_X;
   diprg.diph.dwHow        = DIPH_BYOFFSET;
   diprg.lMin              = -1000;
   diprg.lMax              = +1000;

   if(pdev->lpVtbl->SetProperty(pdev, DIPROP_RANGE, &diprg.diph) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetProperty(DIPH_RANGE) FAILED\n");
      pdev->lpVtbl->Release(pdev);
      return FALSE;
   }

   //
   // And again for Y-axis range
   //
   diprg.diph.dwObj        = DIJOFS_Y;

   if(pdev->lpVtbl->SetProperty(pdev, DIPROP_RANGE, &diprg.diph) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetProperty(DIPH_RANGE) FAILED\n");
      pdev->lpVtbl->Release(pdev);
      return FALSE;
   }

   // set X axis dead zone to 50% (to avoid accidental turning)
   // Units are ten thousandths, so 50% = 5000/10000.
   if (SetDIDwordProperty(pdev, DIPROP_DEADZONE, DIJOFS_X, DIPH_BYOFFSET, 5000) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetProperty(DIPH_DEADZONE) FAILED\n");
      pdev->lpVtbl->Release(pdev);
      return FALSE;
   }


   // set Y axis dead zone to 50% (to avoid accidental thrust)
   // Units are ten thousandths, so 50% = 5000/10000.
   if (SetDIDwordProperty(pdev, DIPROP_DEADZONE, DIJOFS_Y, DIPH_BYOFFSET, 5000) != DI_OK)
   {
      OutputDebugString("IDirectInputDevice::SetProperty(DIPH_DEADZONE) FAILED\n");
      pdev->lpVtbl->Release(pdev);
      return FALSE;
   }


   //
   // Add it to our list of devices.  If AddInputDevice succeeds,
   // he will do an AddRef.
   //
   AddInputDevice(pdev, pdinst);
   pdev->lpVtbl->Release(pdev);

   return DIENUM_CONTINUE;
}
*/
/*--------------------------------------------------------------------------
| InitInput
|
| Initializes DirectInput for the keyboard and all joysticks.
|
| For each input device, add it to the menu.
|
*-------------------------------------------------------------------------*/

BOOL InitInput(HINSTANCE hInst, HWND hWnd)
{
#ifdef DIRECTINPUT_SUPPORT
   LPDIRECTINPUT pdi;
   BOOL fRc;

   // Note: Joystick support is a DirectX 5.0 feature.
   // Since we also want to run on DirectX 3.0, we will start out
   // with DirectX 3.0 to make sure that at least we get the keyboard.

   // create the DirectInput interface object
   if(DirectInputCreate(hInst, 0x0300, &pdi, NULL) != DI_OK)
   {
      OutputDebugString("DirectInputCreate 3.0 FAILED\n");
      return FALSE;
   }

   fRc = InitKeyboardInput(pdi);
   pdi->Release();       // Finished with DX 3.0

   if (!fRc) {
      return FALSE;
   }
/*
   // create the DirectInput 5.0 interface object
   if(DirectInputCreate(hInst, DIRECTINPUT_VERSION, &pdi, NULL) == DI_OK)
   {

      //
      // Enumerate the joystick devices.  If it doesn't work, oh well,
      // at least we got the keyboard.
      //

      pdi->lpVtbl->EnumDevices(pdi, DIDEVTYPE_JOYSTICK,
                               InitJoystickInput, pdi, DIEDFL_ATTACHEDONLY);

      pdi->lpVtbl->Release(pdi);    // Finished with DX 5.0.

   } else {
      OutputDebugString("DirectInputCreate 5.0 FAILED - no joystick support\n");
   }
*/
   // Default device is the keyboard
   PickInputDevice(0);
#endif
   // if we get here, we were successful
   return TRUE;
}

/*--------------------------------------------------------------------------
| CleanupInput
|
| Cleans up all DirectInput objects.
*-------------------------------------------------------------------------*/
void CleanupInput(void)
{
#ifdef DIRECTINPUT_SUPPORT
   int idev;

   // make sure the device is unacquired
   // it doesn't harm to unacquire a device that isn't acquired

   if (g_pdevCurrent)
   {
      IDirectInputDevice_Unacquire(g_pdevCurrent);
   }

   // release all the devices we created
   for (idev = 0; idev < g_cpdevFound; idev++)
   {
      if (g_rgpdevFound[idev]) {
         IDirectInputDevice_Release(g_rgpdevFound[idev]);
         g_rgpdevFound[idev] = 0;
      }
   }
#endif
}


/*--------------------------------------------------------------------------
| ReacquireInput
|
| Reacquires the current input device.  If Acquire() returns S_FALSE,
| that means
| that we are already acquired and that DirectInput did nothing.
*-------------------------------------------------------------------------*/
BOOL ReacquireInput(void)
{
#ifdef DIRECTINPUT_SUPPORT
	HRESULT hRes;

    // if we have a current device
    if(g_pdevCurrent)
    {
       // acquire the device
       hRes = IDirectInputDevice_Acquire(g_pdevCurrent);
       if(SUCCEEDED(hRes))
       {
          // acquisition successful
          return TRUE;
       }
       else
       {
          // acquisition failed
          return FALSE;
       }
    }
    else
#endif
	{
       // we don't have a current device
       return FALSE;
    }

}
#endif
/*--------------------------------------------------------------------------
| ReadKeyboardInput
|
| Requests keyboard data and performs any needed processing.
*-------------------------------------------------------------------------*/
void ReadKeyboardInput(void)
{
#ifdef DIRECTINPUT_SUPPORT

   DIDEVICEOBJECTDATA      rgKeyData[KEYBUFSIZE];
   DWORD                   dwEvents;
   DWORD                   dw;
   HRESULT                 hRes;

   // get data from the keyboard
   dwEvents = KEYBUFSIZE;
   hRes = IDirectInputDevice_GetDeviceData(g_pdevCurrent,
                                           sizeof(DIDEVICEOBJECTDATA),
                                           rgKeyData, &dwEvents, 0);

   if (InputMode)
	   dwKeyState=0;

   CurrentKey=0;

   if(hRes != DI_OK)
   {
      // did the read fail because we lost input for some reason?
      // if so, then attempt to reacquire.  If the second acquire
      // fails, then the error from GetDeviceData will be
      // DIERR_NOTACQUIRED, so we won't get stuck an infinite loop.
      if(hRes == DIERR_INPUTLOST)
         ReacquireInput();

      // return the fact that we did not read any data
      return;
   }

     // process the data
	for(dw = 0; dw < dwEvents; dw++)
    {
		switch(rgKeyData[dw].dwOfs)
        {
        case DIK_ESCAPE:
            if(rgKeyData[dw].dwData & 0x80)
				dwKeyState |= KEY_ESC;
            else
                dwKeyState &= ~KEY_ESC;
            break;

        case DIK_SPACE:
            if(rgKeyData[dw].dwData & 0x80)
				dwKeyState |= KEY_SPACE;
            else
                dwKeyState &= ~KEY_SPACE;
            break;

        case DIK_RETURN:
            if(rgKeyData[dw].dwData & 0x80)
                dwKeyState |= KEY_ENTER;
            else
                dwKeyState &= ~KEY_ENTER;
            break;

        case DIK_UP:
        case DIK_NUMPAD8:
            if(rgKeyData[dw].dwData & 0x80)
                dwKeyState |= KEY_UP;
            else
                dwKeyState &= ~KEY_UP;
            break;

        case DIK_DOWN:
        case DIK_NUMPAD2:
            if(rgKeyData[dw].dwData & 0x80)
                dwKeyState |= KEY_DOWN;
            else
                dwKeyState &= ~KEY_DOWN;
            break;

        case DIK_LEFT:
        case DIK_NUMPAD4:
            if(rgKeyData[dw].dwData & 0x80)
                dwKeyState |= KEY_LEFT;
            else
                dwKeyState &= ~KEY_LEFT;
            break;

        case DIK_RIGHT:
        case DIK_NUMPAD6:
            if(rgKeyData[dw].dwData & 0x80)
                dwKeyState |= KEY_RIGHT;
            else
                dwKeyState &= ~KEY_RIGHT;
            break;
		case DIK_LSHIFT:
		case DIK_RSHIFT:
            if(rgKeyData[dw].dwData & 0x80)
                ShiftMode=TRUE;
            else
                ShiftMode=FALSE;
            break;
		}
		if (rgKeyData[dw].dwData & 0x80)
			if (rgKeyData[dw].dwOfs<128)
				if (ShiftMode)
					CurrentKey=ScantoAscShifted[rgKeyData[dw].dwOfs];
				else
					CurrentKey=ScantoAsc[rgKeyData[dw].dwOfs];
	}
#endif
}

/*--------------------------------------------------------------------------
| ReadJoystickInput
|
| Requests joystick data and performs any needed processing.
|
*-------------------------------------------------------------------------
DWORD ReadJoystickInput(void)
{
   DWORD                   dwKeyState;
   HRESULT                 hRes;
   DIJOYSTATE              js;

   // poll the joystick to read the current state
   hRes = IDirectInputDevice2_Poll(g_pdevCurrent);

   // get data from the joystick
   hRes = IDirectInputDevice_GetDeviceState(g_pdevCurrent,
                                            sizeof(DIJOYSTATE), &js);

   if(hRes != DI_OK)
   {
      // did the read fail because we lost input for some reason?
      // if so, then attempt to reacquire.  If the second acquire
      // fails, then the error from GetDeviceData will be
      // DIERR_NOTACQUIRED, so we won't get stuck an infinite loop.
      if(hRes == DIERR_INPUTLOST)
         ReacquireInput();

      // return the fact that we did not read any data
      return 0;
   }

   //
   // Now study the position of the stick and the buttons.
   //

   dwKeyState = 0;

   if (js.lX < 0) {
      dwKeyState |= KEY_LEFT;
   } else if (js.lX > 0) {
      dwKeyState |= KEY_RIGHT;
   }

   if (js.lY < 0) {
      dwKeyState |= KEY_UP;
   } else if (js.lY > 0) {
      dwKeyState |= KEY_DOWN;
   }

   if (js.rgbButtons[0] & 0x80) {
      dwKeyState |= KEY_FIRE;
   }

   if (js.rgbButtons[1] & 0x80) {
      dwKeyState |= KEY_SHIELD;
   }

   if (js.rgbButtons[2] & 0x80) {
      dwKeyState |= KEY_STOP;
   }

   return dwKeyState;

}
*/
/*--------------------------------------------------------------------------
| PickInputDevice
|
| Make the n'th input device the one that we will use for game play.
|
*-------------------------------------------------------------------------*/

#ifdef DIRECTINPUT_SUPPORT

BOOL PickInputDevice(int n)
{

    if (n < g_cpdevFound) {

        
    //     *  Unacquire the old device.
         
        if (g_pdevCurrent) {
            IDirectInputDevice_Unacquire(g_pdevCurrent);
        }

        
  //         Move to the new device.
         
        g_pdevCurrent = g_rgpdevFound[n];
/*
        
//           Set ReadGameInput to the appropriate handler.
         
/        if (n == 0) {
            ReadGameInput = ReadKeyboardInput;
        } else {
            ReadGameInput = ReadJoystickInput;
        }

        CheckMenuRadioItem(GetSubMenu(GetMenu(hWndMain), 0),
                           IDC_DEVICES, IDC_DEVICES + g_cpdevFound - 1,
                           IDC_DEVICES + n, MF_BYCOMMAND);
*/
        ReacquireInput();

        return TRUE;
    } else {
        return FALSE;
    }
}
#endif


