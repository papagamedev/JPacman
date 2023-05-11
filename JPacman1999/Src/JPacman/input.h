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

//--------------------------------------------------------------------------


// keyboard buffer size
#define KEYBUFSIZE 32

/*
 * keyboard commands
 */
#define JPACMAN_KEY_BACK					0x00000001l
#define JPACMAN_KEY_DOWN				0x00000002l
#define JPACMAN_KEY_LEFT				0x00000004l
#define JPACMAN_KEY_RIGHT				0x00000008l
#define JPACMAN_KEY_UP					0x00000010l
#define JPACMAN_KEY_OK				0x00000020l
#define JPACMAN_KEY_PAUSE				0x00000040l
#ifdef _DEBUG
#define JPACMAN_KEY_CHEAT_NEXT_LEVEL	0x00000080l
#endif

//--------------------------------------------------------------------------

// external variables
extern BOOL bKeyboardAcquired;

extern DWORD dwKeyState;

extern int InputMode;

extern char CurrentKey;

extern void (*ReadGameInput)(void);

#ifdef JPACMAN_AXMOL

axmol::EventListener* InitInput();

#else

//--------------------------------------------------------------------------

// prototypes
BOOL InitInput(HINSTANCE hInst, HWND hWnd);
void CleanupInput(void);
BOOL ReacquireInput(void);
BOOL PickInputDevice(int);


//--------------------------------------------------------------------------

#endif // JPACMAN_AXMOL

#endif // _INPUT_H









