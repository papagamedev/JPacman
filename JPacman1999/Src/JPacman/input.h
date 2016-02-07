/*==========================================================================
 *
 *  Copyright (C) 1995-1997 Microsoft Corporation. All Rights Reserved.
 *
 *  File:       input.h
 *
 *
 ***************************************************************************/

#ifndef _INPUT_H
#define _INPUT_H

#include <dinput.h>

//--------------------------------------------------------------------------


// keyboard buffer size
#define KEYBUFSIZE 32

/*
 * keyboard commands
 */
#define KEY_ESC    0x00000001l
#define KEY_DOWN   0x00000002l
#define KEY_LEFT   0x00000004l
#define KEY_RIGHT  0x00000008l
#define KEY_UP     0x00000010l
#define KEY_ENTER  0x00000020l
#define KEY_SPACE  0x00000040l

//--------------------------------------------------------------------------

// external variables
extern BOOL bKeyboardAcquired;

extern DWORD dwKeyState;

extern int InputMode;

extern char CurrentKey;

extern void (*ReadGameInput)(void);

//--------------------------------------------------------------------------

// prototypes
BOOL InitInput(HINSTANCE hInst, HWND hWnd);
void CleanupInput(void);
BOOL ReacquireInput(void);
BOOL PickInputDevice(int);

//--------------------------------------------------------------------------
#endif // _INPUT_H









