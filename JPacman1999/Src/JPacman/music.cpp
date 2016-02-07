#include "JPacman.h"

int curmus=-1;

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

int MusicEnabled=TRUE,MusicOn=TRUE;

BOOL InitMusic()
{
	IDirectMusicLoader* lpLoader;

	if (!lpDS)
	{
bye:
		MusicEnabled=FALSE;
		MusicOn=FALSE;
		return FALSE;
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
}

void UninitMusic()
{
	if (!MusicEnabled) return;
	
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
}

void PlayMusic(int mus)
{
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

}
