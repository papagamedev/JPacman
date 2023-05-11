#include "JPacman.h"

int SoundOn = TRUE, SoundEnabled = TRUE;

#ifndef JPACMAN_AXMOL

LPDIRECTSOUND lpDS = NULL;

HSNDOBJ hsoFruit = NULL;			// Fruta
HSNDOBJ hsoPoint = NULL;			// Punto
HSNDOBJ hsoRevengeTime = NULL;	// Mientras fantasmas morados
HSNDOBJ hsoRevenge = NULL;		// Comerse un fantasma
HSNDOBJ hsoEyes = NULL;			// Ojitos huyendo

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

void PauseAllSounds()
{

}

void ResumeAllSounds()
{

}

#else // JPACMAN_AXMOL

#include "Audio/AudioEngine.h"

struct jpacmanSndInfo
{
	const char *fileName;
	bool loop;
	long durationInMiliseconds;
	unsigned engineId;
	long endTimeInMiliseconds;
};

jpacmanSndInfo sSoundInfo[SND_MAX]=
{
	{
		"fruit.wav",
		false,
		2000,
	},
	{
		"point.wav",
		false,
		170,
	},
	{
		"revenge.wav",
		false,
		1000,
	},
	{
		"revengetime.wav",
		true,
		0,
	},
	{
		"eyes.wav",
		true,
		0,
	},
};


int InitSound()
{
	for (int i = 0; i < SND_MAX; i++)
	{
		axmol::AudioEngine::preload(sSoundInfo[i].fileName);
	}
	return TRUE;
}

void UninitSound()
{

}

void PauseAllSounds()
{
	axmol::AudioEngine::pauseAll();
}

void ResumeAllSounds()
{
	axmol::AudioEngine::resumeAll();
}



#endif // JPACMAN_AXMOL


void PlaySound(int snd)
{
	if (!SoundOn) return;
	if (!SoundEnabled) return;

#if JPACMAN_AXMOL
	jpacmanSndInfo& sound = sSoundInfo[snd];
	if (sound.loop)
	{
		// if sound is looping and already playing, do not start it over again
//		if (sound.engineId)
//			return;
	}
	else
	{
		// if sound is not looping, make sure it already finished to avoid aborting it in the middle
		if (sound.endTimeInMiliseconds > axmol::utils::getTimeInMilliseconds())
			return;
	}

	sound.engineId = axmol::AudioEngine::play2d(sound.fileName, sound.loop);
	sound.endTimeInMiliseconds = axmol::utils::getTimeInMilliseconds() + sound.durationInMiliseconds;
	DPF(0, "play %s", sound.fileName);

#else // !JPACMAN_AXMOL
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
#endif // !JPACMAN_AXMOL
}

void StopSound(int snd)
{
	if (!SoundOn) return;
	if (!SoundEnabled) return;

#if JPACMAN_AXMOL
	jpacmanSndInfo& sound = sSoundInfo[snd];
	if (sound.engineId)
	{
		axmol::AudioEngine::stop(sound.engineId);
		sound.engineId = 0;
	}
#else // !JPACMAN_AXMOL

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
#endif // !JPACMAN_AXMOL

}
