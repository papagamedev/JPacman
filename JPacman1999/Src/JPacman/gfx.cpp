#include "JPacman.h"

#include <math.h>

#define MAX_SPRITES	1000

#ifdef JPACMAN_COCOS2DX

cocos2d::Sprite* sSpriteFondo;
cocos2d::Sprite* sSprites[MAX_SPRITES];

#else // !JPACMAN_COCOS2DX

LPDIRECTDRAW7           lpDD=NULL;
LPDIRECTDRAWSURFACE7    lpFrontBuffer=NULL;
LPDIRECTDRAWSURFACE7    lpBackBuffer=NULL;
LPDIRECTDRAWPALETTE     lpPalette=NULL;
LPDIRECTDRAWSURFACE7    lpFondo=NULL;
LPDIRECTDRAWSURFACE7    lpGfx=NULL;
DWORD                   dwTransType;

PALETTEENTRY Palette[256];
#endif // !JPACMAN_COCOS2DX

int frames_sec=0;
int frame_updated=1;
int frames_count=0;
int ticks_count=0;

double fadestate=1,fademode=0;
int skipflip=FALSE;

const char *GFXFile;

SpriteData *Sprites,*Sprite1;
int nSprites;
int SpriteOcc[MAX_SPRITES];

static int CharTable[128]={
	-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
	-1,64,-1,63,65,66,67,-1,69,70,68,-1,74,75,73,76,
	52,56,53,54,57,58,59,60,61,62,71,72,-1,-1,-1,55,
	-1,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,
	15,17,18,16,19,20,21,22,23,24,25,-1,-1,-1,-1,-1,
	-1,26,27,28,29,30,31,32,33,35,36,37,34,38,39,40,
	41,42,43,44,45,46,47,48,49,50,51,-1,-1,-1,-1,-1
};

static int CharsLeftPos[77];

static int CharsTopPos[77]={
	0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,	// 17
	32,32,32,32,32,32,32,32,32,32,32,32,32,32,32,32,32,32, // 18
	64,64,64,64,64,64,64,64,64,64,64,64,64,64,64,64,64,64,64,64,64,  // 21
	96,96,96,96,96,96,96,96,96,96,96,96,96,96,96,96,96,96,96,96,96   // 21
};

static int CharsWidth[77]={
	26,24,24,24,22,21,24,24,9,14,23,20,31,26,26,23,23,
	26,24,25,25,24,32,24,24,24,18,19,18,19,19,14,23,18,8,
	6,11,19,30,19,20,18,20,16,19,13,19,19,27,19,19,17,20,19,19,15,
	10,22,19,20,20,22,21,9,9,16,28,34,16,13,13,9,9,9,10,9,18
};

SpriteKind SpriteInfo [] = {{
	2,0,0.3,32,32,{0,32},{64,96},{128,160},{192,224},    // FANTASMA ROJO
		{128,128},{128,128},{128,128},{128,128}
	},{
	2,0,0.3,32,32,{0,32},{64,96},{128,160},{192,224},	// FANTASMA ROSADO
		{160,160},{160,160},{160,160},{160,160}
	},{
	2,0,0.3,32,32,{256,288},{320,352},{384,416},{448,480},	// FANTASMA VERDE
		{128,128},{128,128},{128,128},{128,128}
	},{
	2,0,0.3,32,32,{256,288},{320,352},{384,416},{448,480},	// FANTASMA NARANJO
		{160,160},{160,160},{160,160},{160,160}
	},{
	2,0,0.3,32,32,{512,544},{512,544},{512,544},{512,544},	// FANTASMA MORADO
		{128,160},{128,160},{160,128},{160,128}
	},{
	2,0,0.3,32,16,{608,608},{608,608},{608,608},{608,608},	// OJOS DE FANTASMA
		{96,112},{96,112},{96,112},{96,112}
	},{
	7,1,1,32,32,{0,32,64,96,128,160,192,224},{224,192,160,128,96,64,32,0},  //PACMAN
		{224,192,160,128,96,64,32,0},{0,32,64,96,128,160,192,224},
		{0,0,0,0,0,0,0,0},{32,32,32,32,32,32,32,32},
		{64,64,64,64,64,64,64,64},{96,96,96,96,96,96,96,96}
	},{
	0,0,0,16,16,
		{576,592,608,624,624,576,608,624},					// FRUTAS
		{576,592,608,624,624,576,592,608},{608,624,
		576,592,608,624,624,576},{592,608,624,624,624,608,624,608},
        {176,176,176,128,176,160,128,128},
		{176,176,176,176,128,160,160,160},{128,128,
		176,176,176,176,128,160},{160,160,128,160,144,128,128,144},
	},{
	4,1,0.3,16,16,{576,576,592,576,576},{576,576,592,576,576}, // GALLETAS
		{576,576,592,576,576},{576,576,592,576,576},
	    {128,128,128,144,144},{128,128,128,144,144},
		{128,128,128,144,144},{128,128,128,144,144}
	},{
	4,0,0,8,8,{592,600,592,600},{592,600,592,600}, // PUNTOS
		{592,600,592,600},{592,600,592,600},
		{144,144,152,152},{144,144,152,152},
		{144,144,152,152},{144,144,152,152}
}};

BOOL InitGFX()
{
#ifndef JPACMAN_COCOS2DX

    HRESULT	ddrval;
    DDSURFACEDESC2  ddsd;
    DDSCAPS2        ddscaps;
	int i;
	
	ddrval = DirectDrawCreateEx( NULL, (VOID**)&lpDD, IID_IDirectDraw7, NULL);

    if (ddrval != DD_OK)
        return FALSE;

    ddrval = lpDD->SetCooperativeLevel(hWndMain,
                            DDSCL_EXCLUSIVE | DDSCL_FULLSCREEN );
    if( ddrval != DD_OK )
        return FALSE;


    // set the mode
    ddrval = lpDD->SetDisplayMode(640, 480, 8, 0, 0 );
    if( ddrval != DD_OK )
        return FALSE;

    // check the color key hardware capabilities
    dwTransType = DDBLTFAST_SRCCOLORKEY;

    // Create surfaces
    memset( &ddsd, 0, sizeof( ddsd ) );
    ddsd.dwSize = sizeof( ddsd );
    ddsd.dwFlags = DDSD_CAPS | DDSD_BACKBUFFERCOUNT;
    ddsd.ddsCaps.dwCaps = DDSCAPS_PRIMARYSURFACE |
                          DDSCAPS_FLIP |
                          DDSCAPS_COMPLEX;
    ddsd.dwBackBufferCount = 1;
    ddrval = lpDD->CreateSurface( &ddsd, &lpFrontBuffer, NULL );
    if( ddrval != DD_OK )
        return FALSE;

	DPF(1,"front ok!");

    // get a pointer to the back buffer
    memset( &ddscaps, 0, sizeof( ddscaps ) );
    ddscaps.dwCaps = DDSCAPS_BACKBUFFER;
    ddrval = lpFrontBuffer->GetAttachedSurface(&ddscaps,&lpBackBuffer );
    if( ddrval != DD_OK )
        return FALSE;

	DPF(1,"back ok!");

    memset( &ddsd, 0, sizeof( ddsd ) );
    ddsd.dwSize = sizeof( ddsd );
    ddsd.dwFlags = DDSD_CAPS | DDSD_WIDTH | DDSD_HEIGHT;
    ddsd.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN;
    ddsd.dwWidth = 640;
    ddsd.dwHeight = 480;
    ddrval = lpDD->CreateSurface( &ddsd, &lpFondo, NULL );
    if( ddrval != DD_OK )
        return FALSE;

    ddsd.dwFlags = DDSD_CAPS | DDSD_WIDTH | DDSD_HEIGHT;
    ddsd.ddsCaps.dwCaps = DDSCAPS_OFFSCREENPLAIN;
    ddsd.dwWidth = 640;
    ddsd.dwHeight = 192;
    ddrval = lpDD->CreateSurface( &ddsd, &lpGfx, NULL );
    if( ddrval != DD_OK )
        return FALSE;

	InitText();
#endif

	if ((Sprites=(SpriteData *) LocalAlloc(LPTR,MAX_SPRITES*sizeof(SpriteData)))==NULL)
        return FALSE;

	nSprites=0;
	Sprite1=NULL;
	for (int i=0;i<MAX_SPRITES;i++)
		SpriteOcc[i]=FALSE;

	DPF(0,"InitGFX OK");
	return TRUE;
}

void UninitGFX()
{
	ClearSprites();
	

	if (Sprites!=NULL)
	{
		LocalFree(Sprites);
		Sprites=NULL;
	}
#ifndef JPACMAN_COCOS2DX

    if( lpPalette != NULL )
    {
		lpPalette->Release();
		lpPalette=NULL;
	}

    if( lpGfx != NULL )
    {
		lpGfx->Release();
		lpGfx=NULL;
	}

    if( lpFondo != NULL )
    {
		lpFondo->Release();
		lpFondo=NULL;
	}

    if( lpFrontBuffer != NULL )
    {
		lpFrontBuffer->Release();
		lpFrontBuffer=NULL;
	}

    if( lpDD != NULL )
    {
		lpDD->Release();
		lpDD=NULL;
	}
#endif
	DPF(0,"UninitGFX OK");
}

BOOL RestoreGFX()
{
#ifdef JPACMAN_COCOS2DX
	if (sSpriteFondo != nullptr)
	{
		gAppScene->removeChild(sSpriteFondo);
	}

	char fileToLoad[MAX_PATH];
	sprintf_s(fileToLoad, "%s.png", GFXFile);
	sSpriteFondo = cocos2d::Sprite::create(fileToLoad, cocos2d::Rect(0, 0, 640, 480));
	if (sSpriteFondo)
	{
		gAppScene->addChild(sSpriteFondo);
		sSpriteFondo->setPosition(320, 240);
	}

#else
	HRESULT	ddrval;
    HBITMAP     hbm;

    ddrval = lpFrontBuffer->Restore();
    if( ddrval != DD_OK )
        return FALSE;

	EraseScreen();

    ddrval = lpFondo->Restore();
    if( ddrval != DD_OK )
        return FALSE;

    hbm = (HBITMAP)LoadImage(GetModuleHandle(NULL), GFXFile, IMAGE_BITMAP, 0, 0, LR_CREATEDIBSECTION );

    if( NULL == hbm )
        return FALSE;

    if( lpPalette != NULL )
        lpPalette->Release();

	lpPalette = DDLoadPalette( lpDD, GFXFile );
    if( NULL == lpPalette )
        return FALSE;

    ddrval=lpPalette->GetEntries(0,0,256,Palette);

	lpFrontBuffer->SetPalette( lpPalette );

	skipflip=TRUE;

    ddrval = DDCopyBitmap( lpFondo, hbm, 0, 0, 640, 480 );
    if( ddrval != DD_OK )
    {
        DeleteObject( hbm );
        return FALSE;
    }

    ddrval = DDCopyBitmap( lpGfx, hbm, 0, 480, 640, 192);
    if( ddrval != DD_OK )
    {
        DeleteObject( hbm );
        return FALSE;
    }

    DeleteObject( hbm );

    DDSetColorKey( lpFondo, 0 );
    DDSetColorKey( lpGfx, 0 );

#endif
	DPF(0,"RestoreGFX OK");

	return TRUE;
}

void InitText()
{
	int i,x=0,y=0;

	if (CharsLeftPos[1]!=0) return;
	for (i=0;i<77;i++)
	{
		if (CharsTopPos[i]!=y)
		{
			x=0;
			y=CharsTopPos[i];
		}
		CharsLeftPos[i]=256+x;
		x+=CharsWidth[i];
	}
	DPF(0,"InitText OK");
}

void DrawText(int x,int y,char *text)
{
#ifndef JPACMAN_COCOS2DX

	char *p=text,c,ct;
    HRESULT ddrval;
    RECT src;

    while ((c=(*p++))!=0)
	{
		ct=CharTable[c];
		if (ct!=-1)	
		{
			src.left=CharsLeftPos[ct];
			src.top=CharsTopPos[ct];
			src.right=src.left+CharsWidth[ct];
			src.bottom=src.top+32;
			while( 1 )
			{
			    ddrval = lpBackBuffer->BltFast(x,y,lpGfx, &src, dwTransType);
				if( ddrval == DD_OK )
		            break;
				if( ddrval == DDERR_SURFACELOST )
				{
		            if(!RestoreGFX())
						return;
				}
				if( ddrval != DDERR_WASSTILLDRAWING )
		            return;
			}
			x+=CharsWidth[ct]+2;
		}
		else
			x+=5;
	}
#endif
}

void UpdateGFX()
{

    HRESULT     ddrval;
    RECT    src;
	SpriteData *act;

	if (fademode>0)
	{
		fadestate+=fademode;
		if (fadestate>=1)
		{
			fadestate=1;
			fademode=0;
		}
		StampPalette();
	}
	else if (fademode<0)
	{
		fadestate+=fademode;
		if (fadestate<=0)
		{
			fadestate=0;
			fademode=0;
		}
		StampPalette();
	}


#ifndef JPACMAN_COCOS2DX

	src.left=0;
	src.top=0;
	src.right=640;
	src.bottom=480;
    while(1)
	{
	    ddrval = lpBackBuffer->BltFast(0,0,lpFondo, &src, 0 );
        if( ddrval == DD_OK )
        {
            break;
        }
        if( ddrval == DDERR_SURFACELOST )
        {
            if( !RestoreGFX() )
                return;
        }
        if( ddrval != DDERR_WASSTILLDRAWING )
            return;
	}
#endif
	act=Sprite1;
	while (act!=NULL)
	{
		if (act->dir==-1)
			DrawSprite(act, (int)act->xpos,(int)act->ypos,act->kind,0,(int) (act->frame));
		else
			DrawSprite(act, (int)act->xpos,(int)act->ypos,act->kind,act->dir,(int) (act->frame));
		act->frame+=act->framespeed;
		if (act->framespeed>0)
		{
			if (act->frame>=SpriteInfo[act->kind].lastframe)
				if (SpriteInfo[act->kind].animreverse)
					act->framespeed*=-1;
				else
					act->frame=0;
		}
		else if (act->framespeed<0)
		{
			if (act->frame<=0)
				if (SpriteInfo[act->kind].animreverse)
					act->framespeed*=-1;
				else
					act->frame=(float) SpriteInfo[act->kind].lastframe;
		}
		act=act->next;
	}

#ifndef JPACMAN_COCOS2DX

	UpdateFrame();

#ifdef _DEBUG
	char temp[10];
	sprintf(temp,"FPS: %3d",frames_sec);		// FRAMES POR SEGUNDO
	DrawText(0,0,temp);
#endif

    if (skipflip)
		skipflip=FALSE;
	else
		FlipScreen();
	frames_count++;
	frame_updated=1;
#endif
}

SpriteData *AddSprite(int kind)
{
	SpriteData *spnew,*act,*prev=NULL;
	int i;

	if (nSprites==MAX_SPRITES)
		CleanupAndExit("Demasiados sprites!");

	for (i=0;i<MAX_SPRITES;i++)
		if (!SpriteOcc[i]) break;

	spnew=Sprites+i;
	nSprites++;
	spnew->idx=i;
	SpriteOcc[i]=TRUE;

#ifdef JPACMAN_COCOS2DX
	sSprites[i] = cocos2d::Sprite::create("gfx_sprites.png");
	gAppScene->addChild(sSprites[i]);
#endif
	spnew->kind=kind;
	spnew->framespeed=(float) SpriteInfo[kind].speedframes;
	spnew->frame=0;
	spnew->dir=-1;
	spnew->xpos=0;
	spnew->ypos=0;
	act=Sprite1;
	while (act!=NULL)
	{
		if (act->kind<=kind) break;
		prev=act;
		act=act->next;
	}
	spnew->next=act;
	spnew->prev=prev;
	if (act!=NULL)
		act->prev=spnew;
	if (prev!=NULL)
		prev->next=spnew;
	else
		Sprite1=spnew;

	DPF(0,"AddSprite %d OK",spnew->idx);
	return (spnew);
}

void RemoveSprite(SpriteData **sp)
{
	SpriteData *spr=(*sp);
	
	if (spr==NULL) return;
	
	if (spr->prev!=NULL)
		spr->prev->next=spr->next;
	else
		Sprite1=spr->next;
	if (spr->next!=NULL)
		spr->next->prev=spr->prev;
	SpriteOcc[spr->idx]=FALSE;
#ifdef JPACMAN_COCOS2DX
	gAppScene->removeChild(sSprites[spr->idx]);
#endif
	nSprites--;
	DPF(0,"RemoveSprite %d OK",spr->idx);
	*sp=NULL;
}

void ClearSprites()
{
	SpriteData *act=Sprite1;

	Sprite1=NULL;
	while (act!=NULL)
	{
		SpriteOcc[act->idx]=FALSE;
#ifdef JPACMAN_COCOS2DX
		gAppScene->removeChild(sSprites[act->idx]);
#endif
		DPF(0,"RemoveSprite %d OK (Clearall)",act->idx);
		nSprites--;
		act=act->next;
	}
	DPF(0,"ClearSprites OK (nSprites=%d)",nSprites);
}

int CheckCollision(SpriteData *spr1,SpriteData *spr2)
{
	int i1,j1,i2,j2;
	
	i1=abs((int)(spr1->xpos-spr2->xpos));
	j1=((SpriteInfo[spr1->kind].width+SpriteInfo[spr2->kind].width)>>1)-8;
	i2=abs((int)(spr1->ypos-spr2->ypos));
	j2=((SpriteInfo[spr1->kind].height+SpriteInfo[spr2->kind].height)>>1)-8;
	if ((i1<j1) && (i2<j2)) return TRUE;
	return FALSE;
}

int DrawSprite(SpriteData* spr,int x,int y,int kind,int dir,int frame)
{
#ifdef JPACMAN_COCOS2DX

	sSprites[spr->idx]->setPosition(x ,480 - y);
	sSprites[spr->idx]->setTextureRect(
		cocos2d::Rect(
			SpriteInfo[kind].right_x[dir * 8 + frame] + 0.25f, 
			SpriteInfo[kind].right_y[dir * 8 + frame] + 0.25f, 
			SpriteInfo[kind].width - 0.5f, 
			SpriteInfo[kind].height - 0.5f));

#else // !JPACMAN_COCOS2DX
    HRESULT ddrval;
    RECT src;
	int i,j;

	src.left=SpriteInfo[kind].right_x[dir*8+frame];
	src.top=SpriteInfo[kind].right_y[dir*8+frame];
	i=SpriteInfo[kind].width;
	j=SpriteInfo[kind].height;
	src.right=src.left+i;
	src.bottom=src.top+j;
	while(1)
	{
		ddrval = lpBackBuffer->BltFast(x-i/2,y-j/2,lpGfx,&src,dwTransType);
		if( ddrval == DD_OK )
		{
		    break;
		}
		if( ddrval == DDERR_SURFACELOST )
		{
	        if( !RestoreGFX() )
				return FALSE;
		}
		if( ddrval != DDERR_WASSTILLDRAWING )
	        return FALSE;
	}
#endif // !JPACMAN_COCOS2DX
	return TRUE;
}

void FadeIn()
{
	fademode=0.04;
	fadestate=0;
}

void FadeOut()
{
	fademode=-0.04;
	fadestate=1;
}


void StampPalette()
{
	DPF(0, "StampPalette: fadestate=%d fademode=%d", (int)(fadestate*10000.0), (int)(fademode*10000.0));

#ifdef JPACMAN_COCOS2DX
	gAppScene->setCascadeOpacityEnabled(true);
	gAppScene->setOpacity(fadestate * 255);
#else // !JPACMAN_COCOS2DX
	PALETTEENTRY Temp[256];
	int i;


	for (i=0;i<256;i++)
	{
		Temp[i].peRed=(BYTE)((double)(Palette[i].peRed)*fadestate);
		Temp[i].peBlue=(BYTE)((double)(Palette[i].peBlue)*fadestate);
		Temp[i].peGreen=(BYTE)((double)(Palette[i].peGreen)*fadestate);
		Temp[i].peFlags=0;
	}

    lpPalette->SetEntries(0,0,256,Temp);
#endif
}

#ifndef JPACMAN_COCOS2DX

void FlipScreen( void )
{
    HRESULT     ddrval;

    // Flip the surfaces
    while( 1 )
    {
        ddrval = lpFrontBuffer->Flip( NULL, 0 );
        if( ddrval == DD_OK )
        {
            break;
        }
        if( ddrval == DDERR_SURFACELOST )
        {
            if( !RestoreGFX() )
            {
                return;
            }
        }
        if( ddrval != DDERR_WASSTILLDRAWING )
        {
            break;
        }
    }
}

void EraseScreen( void)
{
    DDBLTFX     ddbltfx;
    HRESULT     ddrval;

    // Erase the background
    ZeroMemory(&ddbltfx, sizeof(ddbltfx));
    ddbltfx.dwSize = sizeof( ddbltfx );
#ifdef NONAMELESSUNION
    ddbltfx.u5.dwFillColor = 0;
#else
    ddbltfx.dwFillColor = 0;
#endif
    while( 1 )
    {
		ddrval = lpFrontBuffer->Blt( NULL, NULL, NULL, DDBLT_COLORFILL, &ddbltfx );

        if( ddrval == DD_OK )
        {
            break;
        }
        if( ddrval == DDERR_SURFACELOST )
        {
            if( !RestoreGFX() )
                return;
        }
        if( ddrval != DDERR_WASSTILLDRAWING )
        {
            return;
        }
    }
}

#endif
