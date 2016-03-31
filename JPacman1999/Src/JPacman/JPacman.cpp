#include "JPacman.h"
#include <stdlib.h>
#include <string.h>

#ifndef JPACMAN_COCOS2DX

HWND                    hWndMain;
HINSTANCE               hInst;

#endif

BOOL                    bIsActive;
BOOL                    bMouseVisible;
DWORD                   dwFrameCount;
DWORD                   dwFrameTime;
DWORD                   dwFrames;
DWORD                   dwFramesLast;
DWORD                   objTickCount;
BOOL					TickMode=1;

void (*Setup)(void);
void (*UpdateFrame)(void);
void (*DoTick)(void);


int randInt( int low, int high )
{
    int range = high - low;
    int num = rand() % range;
    return( num + low );
}

double inline randDouble( double low, double high )
{
    double range = high - low;
    double num = range * (double)rand()/(double)RAND_MAX;
    return( num + low );
}

#ifndef JPACMAN_COCOS2DX

/*
 * MainWndproc
 *
 * Callback for all Windows messages
 */
long FAR PASCAL MainWndproc( HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam )
{
    switch( message )
    {
    case WM_ACTIVATEAPP:
        bIsActive = (BOOL) wParam;
		if( bIsActive )
    	{
		    bMouseVisible = FALSE;
            ReacquireInput();
			objTickCount=GetTickCount();
		}
		else
		{
		    bMouseVisible = TRUE;
		}
        break;

    case WM_CREATE:
        break;

    case WM_SETCURSOR:
		if( !bMouseVisible )
		{
			SetCursor(NULL);
		}
		else
		{
		    SetCursor(LoadCursor( NULL, IDC_ARROW ));
		}
        return TRUE;

	case WM_DESTROY:
        CleanupAndExit(NULL);
        PostQuitMessage( 0 );
        break;
    default:
        break;
    }
    return DefWindowProc(hWnd, message, wParam, lParam);

} /* MainWndproc */

/*
 * initApplication
 *
 * Do that Windows initialization stuff...
 */
static BOOL initApplication( HINSTANCE hInstance, int nCmdShow )
{
    WNDCLASS    wc;
    BOOL        rc;

    wc.style = CS_DBLCLKS;
    wc.lpfnWndProc = MainWndproc;
    wc.cbClsExtra = 0;
    wc.cbWndExtra = 0;
    wc.hInstance = hInstance;
    wc.hIcon = LoadIcon( hInstance, MAKEINTRESOURCE(JPACMAN_ICON));
    wc.hCursor = LoadCursor( NULL, IDC_ARROW );
    wc.hbrBackground = (HBRUSH) GetStockObject( BLACK_BRUSH );
    wc.lpszMenuName =  NULL;
    wc.lpszClassName = "JPacmanClass";
    rc = RegisterClass( &wc );
    if( !rc )
    {
        return FALSE;
    }

    hWndMain = CreateWindowEx(0,  // WS_EX_TOPMOST,
        "JPacmanClass",
        "JPacman",
        WS_VISIBLE | // so we don't have to call ShowWindow
        WS_POPUP |   // non-app window
        WS_CAPTION | // so our menu doesn't look ultra-goofy
        WS_SYSMENU,  // so we get an icon in the tray
        0,
        0,
        GetSystemMetrics(SM_CXSCREEN),
        GetSystemMetrics(SM_CYSCREEN),
        NULL,
        NULL,
        hInstance,
        NULL );

    if( !hWndMain )
    {
        return FALSE;
    }

//    UpdateWindow( hWndMain );

    return TRUE;

} /* initApplication */

/*
 * WinMain
 */
int PASCAL WinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine,
                        int nCmdShow )
{
    MSG     msg;
	DWORD tk;

    // save off application instance
    hInst = hInstance;

    if( !initApplication(hInstance, nCmdShow) )
    {
        return FALSE;
    }

    if((!InitializeGame()) || (!InitInput(hInst, hWndMain)))
    {
		DestroyWindow(hWndMain);
        return FALSE;
    }

    while( 1 )
    {
        if( PeekMessage( &msg, NULL, 0, 0, PM_NOREMOVE ) )
        {
            if( !GetMessage( &msg, NULL, 0, 0 ) )
            {
                return msg.wParam;
            }
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
        else if ( bIsActive )
        {
			tk=GetTickCount();
			while (tk>=objTickCount)
			{
				objTickCount+=(1000/TICKS_SEC);
				ReadGameInput();
				if ((!fademode) && (frame_updated || TickMode)) DoTick();
				ticks_count++;
				if (ticks_count==TICKS_SEC)
				{
					ticks_count=0;
					frames_sec=frames_count;
					frames_count=0;
				}
				frame_updated=0;
			}
			if (!frame_updated) 
				UpdateGFX();
		}
		else
			WaitMessage();
	}

} /* WinMain */

#endif // JPACMAN_COCOS2DX

BOOL InitializeGame( void )
{

	LoadConfig();
	UpdateConfig();

	LoadScores();

    InitSound();	// ????????

	InitMusic();

	if (!InitGFX())
        return CleanupAndExit("DirectDraw initialazing error!");

	SetGameMode(MODE_INTRO);

    return TRUE;
}

BOOL CleanupAndExit( char *err)
{

	if (err)
		DPF(0,"CleanupAndExit  err = %s", err );
	else
		DPF(0,"Cleaning up...");
#ifndef JPACMAN_COCOS2DX
	// make the cursor visible
    SetCursor(LoadCursor( NULL, IDC_ARROW ));
#endif
    bMouseVisible = TRUE;

	bIsActive=FALSE;

	UninitGFX();

	UninitMusic();

	DPF(0,"UninitMusic OK");

	UninitSound();

	DPF(0,"UninitSound OK");

#ifndef JPACMAN_COCOS2DX
	// clean up DirectInput objects
    CleanupInput();
#endif // JPACMAN_COCOS2DX

	DPF(0,"CleanupInput OK");
    //
    // warn user if there is one
    //
	if (err)
	{
#ifndef JPACMAN_COCOS2DX
		MessageBox(hWndMain, err, "ERROR", MB_OK);
#endif // JPACMAN_COCOS2DX
	}
	return FALSE;
}

void SetGameMode(int Mode)
{
	ClearSprites();
	switch(Mode)
	{
	case MODE_INTRO:
		Setup=Intro_Setup;
		UpdateFrame=Intro_UpdateFrame;
		DoTick=Intro_DoTick;
		strcpy(GFXFile,"GFX_INTRO");
		break;
	case MODE_MENU:
		Setup=Menu_Setup;
		UpdateFrame=Menu_UpdateFrame;
		DoTick=Menu_DoTick;
		strcpy(GFXFile,"GFX_MENU");
		break;
	case MODE_PUNTAJES:
		Setup=Puntajes_Setup;
		UpdateFrame=Puntajes_UpdateFrame;
		DoTick=Puntajes_DoTick;
		strcpy(GFXFile,"GFX_PUNTAJES");
		break;
	case MODE_GAME:
		Setup=Game_Setup;
		UpdateFrame=Game_UpdateFrame;
		DoTick=Game_DoTick;
		strcpy(GFXFile,"GFX_GAME");
		break;
	}
	RestoreGFX();
	Setup();
#ifndef JPACMAN_COCOS2DX
	objTickCount=GetTickCount();
#endif // JPACMAN_COCOS2DX
}

