#include "JPacman.h"

#ifndef JPACMAN_COCOS2DX
LPDIRECTSOUND lpDS=NULL;
#else
typedef void* HSNDOBJ;
#endif
HSNDOBJ hsoFruit=NULL;			// Fruta
HSNDOBJ hsoPoint=NULL;			// Punto
HSNDOBJ hsoRevengeTime=NULL;	// Mientras fantasmas morados
HSNDOBJ hsoRevenge=NULL;		// Comerse un fantasma
HSNDOBJ hsoEyes=NULL;			// Ojitos huyendo

int SoundOn=TRUE,SoundEnabled=TRUE;

#ifndef JPACMAN_COCOS2DX
int InitSound()
{
	if (SUCCEEDED(DirectSoundCreate(NULL, &lpDS, NULL)))
	{
		if (SUCCEEDED(lpDS->SetCooperativeLevel(hWndMain,DSSCL_PRIORITY)))
		{
			if ((hsoPoint=SndObjCreate(lpDS,"POINT",1)) &&
				(hsoFruit=SndObjCreate(lpDS,"FRUIT",1)) &&
				(hsoRevenge=SndObjCreate(lpDS,"REVENGE",1)) &&
				(hsoRevengeTime=SndObjCreate(lpDS,"REVENGETIME",1)) &&
				(hsoEyes=SndObjCreate(lpDS,"EYES",1)))
					return TRUE;
		}
        UninitSound();
    }
	SoundEnabled=FALSE;
	SoundOn=FALSE;
	return FALSE;
}

void UninitSound()
{
	if (lpDS)
	{
		SndObjDestroy(hsoFruit);
        hsoFruit=NULL;
        SndObjDestroy(hsoPoint);
        hsoPoint=NULL;
        SndObjDestroy(hsoRevenge);
        hsoRevenge=NULL;
        SndObjDestroy(hsoRevengeTime);
        hsoRevengeTime=NULL;
        SndObjDestroy(hsoEyes);
        hsoEyes=NULL;
		lpDS->Release();
        lpDS = NULL;
    }
}
#else // JPACMAN_COCOS2DX

#define DSBPLAY_LOOPING 1

int InitSound()
{
	return TRUE;
}

void UninitSound()
{

}

void SndObjPlay(HSNDOBJ snd, int flags)
{

}

void SndObjStop(HSNDOBJ snd)
{

}

#endif // JPACMAN_COCOS2DX


void PlaySound(int snd)
{
	if (!SoundOn) return;
	if (!SoundEnabled) return;

	switch (snd)
	{
	case SND_FRUIT:
		SndObjPlay(hsoFruit,0);
		break;
	case SND_POINT:
		SndObjPlay(hsoPoint,0);
		break;
	case SND_REVENGE:
		SndObjPlay(hsoRevenge,0);
		break;
	case SND_REVENGETIME:
		SndObjPlay(hsoRevengeTime,DSBPLAY_LOOPING);
		break;
	case SND_EYES:
		SndObjPlay(hsoEyes,DSBPLAY_LOOPING);
		break;
	}
}

void StopSound(int snd)
{
	if (!SoundOn) return;
	if (!SoundEnabled) return;

	switch (snd)
	{
	case SND_FRUIT:
		SndObjStop(hsoFruit);
		break;
	case SND_POINT:
		SndObjStop(hsoPoint);
		break;
	case SND_REVENGE:
		SndObjStop(hsoRevenge);
		break;
	case SND_REVENGETIME:
		SndObjStop(hsoRevengeTime);
		break;
	case SND_EYES:
		SndObjStop(hsoEyes);
		break;
	}
}
