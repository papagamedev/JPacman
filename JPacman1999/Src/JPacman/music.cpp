#include "JPacman.h"

int curmus=-1;
int MusicEnabled = TRUE, MusicOn = TRUE;

#ifdef JPACMAN_AXMOL

#include "Audio/AudioEngine.h"

struct jpacmanMusicInfo
{
	const char *fileName;
	bool loop;
};

jpacmanMusicInfo sMusicInfo[MUS_MAX] =
{
	{ nullptr, false },
	{ "menu.mp3", true },
	{ "gamex.mp3", true },//MUS_GAMEX
	{ "bonus.mp3", true },//MUS_GAMEBONUS
	{ "intro.mp3", false },
	{ "gameintro.mp3", false },//MUS_GAMEINTRO
	{ "oops.mp3", false },//MUS_OOPS
	{ "win.mp3", false },//MUS_WIN
};

BOOL InitMusic()
{
	for (int i = 0; i < MUS_MAX; i++)
	{
		axmol::AudioEngine::preload(sMusicInfo[i].fileName);
	}

	return TRUE;
}

void UninitMusic()
{


}

int musicInstanceId=0;

void PlayMusic(int mus)
{
	if (sMusicInfo[mus].fileName == nullptr)
	{
		axmol::AudioEngine::stop(musicInstanceId);
	}
	else
	{
		musicInstanceId = axmol::AudioEngine::play2d(sMusicInfo[mus].fileName, sMusicInfo[mus].loop);
	}
}

void PauseMusic()
{
	axmol::AudioEngine::pause(musicInstanceId);
}

void ResumeMusic()
{
	axmol::AudioEngine::resume(musicInstanceId);
}

#else // !JPACMAN_AXMOL

#ifdef DIRECTMUSIC_SUPPORT


IDirectMusicPerformance* lpPerf=NULL;
IDirectMusicSegment* lpSgtIntro=NULL;
IDirectMusicSegment* lpSgtMenu=NULL;
IDirectMusicSegment* lpSgtGameX=NULL;
IDirectMusicSegment* lpSgtGameGrn=NULL;
IDirectMusicSegment* lpSgtGameBonus=NULL;
IDirectMusicSegment* lpSgtGameFinal=NULL;
IDirectMusicSegment* lpSgtOops=NULL;
IDirectMusicSegment* lpSgtWin=NULL;
IDirectMusicSegment* lpSgtGameIntro=NULL;

#endif

BOOL InitMusic()
{
#ifdef DIRECTMUSIC_SUPPORT
	IDirectMusicLoader* lpLoader;

	if (!lpDS)
	{
	bye:
#endif
		MusicEnabled=FALSE;
		MusicOn=FALSE;
		return FALSE;


#ifdef DIRECTMUSIC_SUPPORT
	}

	if (FAILED(CoInitialize(NULL)))
		goto bye;

    if (FAILED(CoCreateInstance(CLSID_DirectMusicPerformance,NULL,CLSCTX_INPROC, 
            IID_IDirectMusicPerformance,(void**)&lpPerf)))
		goto bye;

	if (FAILED(lpPerf->Init(NULL, lpDS, NULL)))
		goto bye;

	if (FAILED(lpPerf->AddPort(NULL)))
		goto bye;

	if (FAILED(CoCreateInstance(CLSID_DirectMusicLoader,NULL,CLSCTX_INPROC,
            IID_IDirectMusicLoader,(void **) &lpLoader)))
		goto bye;

	DMUS_OBJECTDESC ObjDesc;

#ifdef _DEBUG
    lpLoader->SetSearchDirectory( GUID_DirectMusicAllTypes, L"c:\\my documents\\myprojects\\jpacman\\jpacman music\\runtimefiles\\", FALSE );
#endif

    ObjDesc.guidClass = CLSID_DirectMusicSegment;
    ObjDesc.dwSize = sizeof(DMUS_OBJECTDESC);
    ObjDesc.dwValidData = DMUS_OBJ_CLASS | DMUS_OBJ_FILENAME;

    wcscpy( ObjDesc.wszFileName, L"Intro.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtIntro )))
		goto bye;

    wcscpy( ObjDesc.wszFileName, L"Menu.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtMenu )))
		goto bye;

    wcscpy( ObjDesc.wszFileName, L"GameX.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtGameX )))
		goto bye;

    wcscpy( ObjDesc.wszFileName, L"GameGrn.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtGameGrn )))
		goto bye;

    wcscpy( ObjDesc.wszFileName, L"GameBonus.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtGameBonus )))
		goto bye;

    wcscpy( ObjDesc.wszFileName, L"GameFinal.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtGameFinal )))
		goto bye;

    wcscpy( ObjDesc.wszFileName, L"Oops.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtOops )))
		goto bye;

    wcscpy( ObjDesc.wszFileName, L"Win.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtWin )))
		goto bye;

	wcscpy( ObjDesc.wszFileName, L"GameIntro.sgt" );
	if (FAILED(lpLoader->GetObject(&ObjDesc, IID_IDirectMusicSegment, 
            (void**) &lpSgtGameIntro )))
		goto bye;

	lpLoader->Release();
	return TRUE;
#endif
}

void UninitMusic()
{
	if (!MusicEnabled) return;

#ifdef DIRECTMUSIC_SUPPORT

	if (lpSgtIntro!=NULL)
	{
		lpSgtIntro->Release();
		lpSgtIntro=NULL;
	}
	if (lpSgtMenu!=NULL)
	{
		lpSgtMenu->Release();
		lpSgtMenu=NULL;
	}
	if (lpSgtGameX!=NULL)
	{
		lpSgtGameX->Release();
		lpSgtGameX=NULL;
	}
	if (lpSgtGameGrn!=NULL)
	{
		lpSgtGameGrn->Release();
		lpSgtGameGrn=NULL;
	}
	if (lpSgtGameBonus!=NULL)
	{
		lpSgtGameBonus->Release();
		lpSgtGameBonus=NULL;
	}
	if (lpSgtGameFinal!=NULL)
	{
		lpSgtGameFinal->Release();
		lpSgtGameFinal=NULL;
	}
	if (lpSgtOops!=NULL)
	{
		lpSgtOops->Release();
		lpSgtOops=NULL;
	}
	if (lpSgtWin!=NULL)
	{
		lpSgtWin->Release();
		lpSgtWin=NULL;
	}
	if (lpSgtGameIntro!=NULL)
	{
		lpSgtGameIntro->Release();
		lpSgtGameIntro=NULL;
	}


	if (lpPerf!=NULL)
	{
		lpPerf->CloseDown();
		lpPerf->Release();
		lpPerf=NULL;
	}

	CoUninitialize();
#endif

}

void PauseMusic()
{

}

void ResumeMusic()
{

}

void PlayMusic(int mus)
{
#ifdef DIRECTMUSIC_SUPPORT

	IDirectMusicSegment* lpSgt;

	if (!MusicOn) return;
	if (!MusicEnabled) return;
	
	if (curmus==-1)
	{
		lpSgtIntro->SetParam(GUID_Download,0xFFFFFFFF, 0, 0, lpPerf);
		lpPerf->PlaySegment(lpSgtIntro, 0 , 0, NULL);
		lpPerf->Stop(NULL,NULL,0,0);
	}

	if (curmus==mus) return;

	curmus=mus;
	switch(mus)
	{
	case MUS_INTRO:
		lpSgt=lpSgtIntro;
		break;
	case MUS_MENU:
		lpSgt=lpSgtMenu;
		break;
	case MUS_GAMEX:
		lpSgt=lpSgtGameX;
		break;
	case MUS_GAMEGRN:
		lpSgt=lpSgtGameGrn;
		break;
	case MUS_GAMEBONUS:
		lpSgt=lpSgtGameBonus;
		break;
	case MUS_GAMEFINAL:
		lpSgt=lpSgtGameFinal;
		break;
	case MUS_OOPS:
		lpSgt=lpSgtOops;
		break;
	case MUS_WIN:
		lpSgt=lpSgtWin;
		break;
	case MUS_GAMEINTRO:
		lpSgt=lpSgtGameIntro;
		break;
	default:
		lpPerf->Stop(NULL,NULL,0,0);
		break;		
	}
	if (mus!=MUS_NONE)
		lpPerf->PlaySegment(lpSgt, DMUS_SEGF_MEASURE , 0, NULL);
#endif
}

#endif // !JPACMAN_AXMOL
