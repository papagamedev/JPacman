#include "JPacman.h"

#ifdef JPACMAN_COCOS2DX

#include "Audio/include/SimpleAudioEngine.h"
typedef void* HSNDOBJ;

#else

LPDIRECTSOUND lpDS = NULL;

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

const char *sndFiles[SND_MAX] =
{
	"fruit.wav",
	"point.wav",
	"revenge.wav",
	"revengetime.wav",
	"eyes.wav"
};

bool sndLoop[SND_MAX] =
{
	false,
	false,
	false,
	true,
	true
};

unsigned sndId[SND_MAX];

int InitSound()
{
	auto audio = CocosDenshion::SimpleAudioEngine::getInstance();
	for (int i = 0; i < SND_MAX; i++)
	{
		audio->preloadEffect(sndFiles[i]);
	}
	return TRUE;
}

void UninitSound()
{

}

#endif // JPACMAN_COCOS2DX


void PlaySound(int snd)
{
	if (!SoundOn) return;
	if (!SoundEnabled) return;

#if JPACMAN_COCOS2DX
	auto audio = CocosDenshion::SimpleAudioEngine::getInstance();
	sndId[snd] = audio->playEffect(sndFiles[snd], sndLoop[snd]);
	DPF(0, "play %s", sndFiles[snd]);
#else // !JPACMAN_COCOS2DX
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
#endif // !JPACMAN_COCOS2DX
}

void StopSound(int snd)
{
	if (!SoundOn) return;
	if (!SoundEnabled) return;

#if JPACMAN_COCOS2DX
	if (sndId[snd])
	{
		auto audio = CocosDenshion::SimpleAudioEngine::getInstance();
		audio->stopEffect(sndId[snd]);
		sndId[snd] = 0;
	}
#else // !JPACMAN_COCOS2DX

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
#endif // !JPACMAN_COCOS2DX

}
