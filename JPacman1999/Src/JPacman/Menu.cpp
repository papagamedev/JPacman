#include "JPacman.h"

#include <stdio.h>
#include <string.h>

typedef struct {
	char Item[20];
	int Data;
} ItemData;

SpriteData *Pac,*Points[30];
int bye=0,cp=-1,crsr=0;
char NewName[20];

#define ID_SALIR		-1
#define ID_JUGAR		-2
#define ID_PUNTAJES		-3
#define ID_DIF			-4
#define ID_MUSIC		-5
#define ID_SFX			-6
#define ID_IDENT		-7
#define ID_MOVPOINTS	-8
#define ID_MAPA			-9
#define ID_CHGPLYR		-10
#define ID_NEWPLYR		-11
#define ID_DELPLYR		-12

#define MENU_JPACMAN	0
#define ITEMS_JPACMAN	5
#define MENU_INFO		MENU_JPACMAN+ITEMS_JPACMAN+1
#define ITEMS_INFO		1
#define MENU_OPCIONES	MENU_INFO+ITEMS_INFO+1
#define ITEMS_OPCIONES	4
#define MENU_AUDIO		MENU_OPCIONES+ITEMS_OPCIONES+1
#define ITEMS_AUDIO		3
#define MENU_JUEGO		MENU_AUDIO+ITEMS_AUDIO+1
#define ITEMS_JUEGO		3 // 4
#define MENU_IDENT		MENU_JUEGO+ITEMS_JUEGO+1
#define ITEMS_IDENT		4
#define MENU_DELPLYR	MENU_IDENT+ITEMS_IDENT+1
#define ITEMS_DELPLYR	2
#define MENU_NEWPLYR	MENU_DELPLYR+ITEMS_DELPLYR+1
#define ITEMS_NEWPLYR	2

ItemData Menu[]={
	{"JPacman",ITEMS_JPACMAN},				// JPACMAN
	{"Jugar",ID_JUGAR},
	{"Opciones",MENU_OPCIONES},
	{"Elegir Jugador",MENU_IDENT},
	{"Puntajes",ID_PUNTAJES},
	{"Salir",ID_SALIR},
	
	{"Informacion",ITEMS_INFO},				// INFORMACION
	{"Volver",MENU_OPCIONES},

	{"Opciones",ITEMS_OPCIONES},				// OPCIONES
	{"Audio",MENU_AUDIO},
	{"Juego",MENU_JUEGO},
	{"Informacion",MENU_INFO},
	{"Volver",MENU_JPACMAN},

	{"Op. de Audio",ITEMS_AUDIO},	// AUDIO
	{"Musica",ID_MUSIC},
	{"Efectos",ID_SFX},
	{"Volver",MENU_OPCIONES},

	{"Op. de Juego",ITEMS_JUEGO},	// JUEGO
	{"Ronda",ID_DIF},
	{"Mov. Puntos",ID_MOVPOINTS},
//	{"Mapa del Nivel",ID_MAPA},
	{"Volver",MENU_OPCIONES},

	{"Elegir Jugador",ITEMS_IDENT},	// IDENTIFICACION
	{"Cambiar:",ID_CHGPLYR},
	{"Nuevo jugador",MENU_NEWPLYR},
	{"Borrar jugador",MENU_DELPLYR},
	{"Volver",MENU_JPACMAN},

	{"$Esta Seguro?",ITEMS_DELPLYR},	// DELPLYR
	{"Si",ID_DELPLYR},
	{"No",MENU_IDENT},

	{"Nuevo Jugador",ITEMS_NEWPLYR},   // NEWPLYR
	{"",ID_NEWPLYR},
	{"Cancelar",MENU_IDENT}
};

int CurSel,CurMenuItems,CurMenu,EscMenu;

void InitMenu(int menu)
{
	CurMenuItems=Menu[menu].Data;
	CurMenu=menu;
	CurSel=1;
	cp=-1;
	EscMenu=Menu[CurMenu+CurMenuItems].Data;
	TickMode=1;
}

void Menu_Setup()
{
	DPF(0, "Menu setup");

	bye=0;
	InitMenu(0);
	Pac=AddSprite(SP_PACMAN);
	Pac->xpos=200;
	Pac->ypos=272;

	InputMode=1;
	PlayMusic(MUS_MENU);
	FadeIn();
}

void Menu_UpdateFrame()
{
	int i;
	char temp[50];

	DrawText(240,210,Menu[CurMenu].Item);
	for (i=1;i<=CurMenuItems;i++)
		DrawText(240,220+i*36,Menu[CurMenu+i].Item);

	switch (CurMenu)
	{
	case MENU_JUEGO:
		temp[0]=StartRound+'1';
		temp[1]=0;
		DrawText(470,256,temp);
		sprintf_s(temp,(MovPointsOn)?"Si":"No");
		DrawText(470,292,temp);
		break;
	case MENU_IDENT:
		DrawText(400,256,Idents[CurIdent]);
		break;
	case MENU_AUDIO:
		sprintf_s(temp,(!MusicEnabled)?"N/A":(MusicOn)?"Si":"No");
		DrawText(430,256,temp);
		sprintf_s(temp,(!SoundEnabled)?"N/A":(SoundOn)?"Si":"No");
		DrawText(430,292,temp);
		break;
	case MENU_INFO:
		// 1.0 sep/1998
		// 1.2 ago/1999
		// 1.3 abr/2016
		{
			DrawText(530, 170, "v 1.3");
			const int lineSkip = 36;
			int yPos = 300;
			int xPos = 5;
			DrawText(xPos, yPos, "Desarrollado por Juan Pablo Lastra");
			yPos += lineSkip;
			DrawText(xPos, yPos, "entre 1998-2023 para Papa Game Dev");
			yPos += lineSkip;
			DrawText(xPos, yPos, "Proyecto sin fines comerciales.");
			yPos += lineSkip;
			DrawText(xPos, yPos, "Pac-Man es marca de Namco Bandai");
			yPos += lineSkip;
			DrawText(xPos, yPos, "Desarrollado usando Axmol");
		}
		break;
	case MENU_NEWPLYR:
		if (nIdents==20)
			sprintf_s(temp,"No mas de 20");
		else
			sprintf_s(temp,"%s%c",NewName,(crsr<16)?' ':'-');
		DrawText(240,256,temp);
		break;
	}
	if ((CurMenu!=MENU_INFO) && (CurMenu!=MENU_IDENT) &&
		(CurMenu!=MENU_NEWPLYR) && (CurIdent!=0))
	{
		sprintf_s(temp,"Jug: %s", Idents[CurIdent]);
		DrawText(150,445,temp);
	}
}

void Menu_DoTick()
{
	int op,i,j;

	if (bye<0)
	{
		switch (bye)
		{
			case ID_SALIR:
#ifdef JPACMAN_AXMOL
				axmol::Director::getInstance()->end();
#else
				PostMessage( hWndMain, WM_CLOSE, 0, 0 );
#endif
				SaveConfig();
				DPF(0,"Quitting...");
				break;
			case ID_JUGAR:
				SetGameMode(MODE_GAME);
				break;
			case ID_PUNTAJES:
				PuntajesMode=1;
				SetGameMode(MODE_PUNTAJES);
				break;
		}
		return;
	}

	if (dwKeyState & JPACMAN_KEY_DOWN)
	{
		CurSel++;
		if (CurSel>CurMenuItems) CurSel=1;
		PlaySound(SND_POINT);
	}
	if (dwKeyState & JPACMAN_KEY_UP)
	{
		CurSel--;
		if (CurSel<1) CurSel=CurMenuItems;
		PlaySound(SND_POINT);
	}
	if (dwKeyState & JPACMAN_KEY_BACK)
	{
		if (EscMenu==ID_SALIR) goto salir;
		InitMenu(EscMenu);
	}
	op=Menu[CurMenu+CurSel].Data;
	if ((dwKeyState & JPACMAN_KEY_LEFT) && (op<0))
		switch(op)
		{
		case ID_DIF:
			PlaySound(SND_POINT);
			StartRound--;
			if (StartRound==-1) StartRound=2;
			break;
		case ID_CHGPLYR:
			PlaySound(SND_POINT);
			CurIdent--;
			if (CurIdent==-1) CurIdent=nIdents-1;
			UpdateConfig();
			break;
		case ID_MOVPOINTS:
			PlaySound(SND_POINT);
			MovPointsOn^=TRUE;
			break;
		case ID_MUSIC:
			if (MusicEnabled)
			{
				PlaySound(SND_POINT);
				if (MusicOn)
					PlayMusic(MUS_NONE);
				MusicOn^=TRUE;
				if (MusicOn)
					PlayMusic(MUS_MENU);
			}
			break;
		case ID_SFX:
			if (SoundEnabled)
			{
				SoundOn^=TRUE;
				if (SoundOn)
					PlaySound(SND_POINT);
			}
			break;
		}
	else if ((dwKeyState & JPACMAN_KEY_OK) && (op>=0))
	{
		if ((op==MENU_DELPLYR) && (CurIdent==0)) goto caca;
		PlaySound(SND_POINT);
		InitMenu(op);
	}
	else if (((dwKeyState & JPACMAN_KEY_OK) || (dwKeyState & JPACMAN_KEY_RIGHT)) && (op<0))
		switch(op)
		{
		case ID_SALIR:
salir:
			PlaySound(SND_FRUIT);
			PlayMusic(MUS_NONE);
			FadeOut();
			bye=ID_SALIR;
			break;
		case ID_JUGAR:
			PlaySound(SND_FRUIT);
			PlayMusic(MUS_NONE);
			FadeOut();
			bye=ID_JUGAR;
			break;
		case ID_PUNTAJES:
			PlaySound(SND_FRUIT);
			FadeOut();
			bye=ID_PUNTAJES;
			break;
		case ID_DIF:
			PlaySound(SND_POINT);
			StartRound++;
			if (StartRound==3) StartRound=0;
			break;
		case ID_CHGPLYR:
			PlaySound(SND_POINT);
			UpdateConfig(TRUE);
			CurIdent++;
			if (CurIdent==nIdents) CurIdent=0;
			UpdateConfig();
			break;
		case ID_NEWPLYR:
			UpdateConfig(TRUE);
			if (cp<1) goto caca;
			SaveScores();
			for (CurIdent=1;CurIdent<nIdents;CurIdent++)
				if (_stricmp(Idents[CurIdent],NewName)==0)
					goto caca;
				else if (_stricmp(Idents[CurIdent],NewName)>0)
					break;
			for (i=nIdents;i>CurIdent;i--)
			{
				strcpy_s(Idents[i],Idents[i-1]);
				for (j=0;j<NUM_OPTS;j++)
					Opts[j][i]=Opts[j][i-1];
			}
			strcpy_s(Idents[i],NewName);
			for (j=0;j<NUM_OPTS;j++)
				Opts[j][i]=0;    
			nIdents++;
			cp=-1;
			UpdateConfig();
			LoadScores();
			PlaySound(SND_POINT);
			InitMenu(MENU_IDENT);
			break;
		case ID_DELPLYR:
			SaveScores();
			nIdents--;
			for (i=CurIdent;i<nIdents;i++)
			{
				strcpy_s(Idents[i],Idents[i+1]);
				for (j=0;j<NUM_OPTS;j++)
					Opts[j][i]=Opts[j][i+1];
			}
			for (j=0;j<NUM_OPTS;j++)
				Opts[j][i]=0;
			memset(Idents[i],0,16);
			CurIdent--;
			UpdateConfig();
			PlaySound(SND_POINT);
			LoadScores();
			InitMenu(MENU_IDENT);
			break;
		case ID_MOVPOINTS:
			PlaySound(SND_POINT);
			MovPointsOn^=TRUE;
			break;
		case ID_MUSIC:
			if (MusicEnabled)
			{
				PlaySound(SND_POINT);
				if (MusicOn)
					PlayMusic(MUS_NONE);
				MusicOn^=TRUE;
				if (MusicOn)
					PlayMusic(MUS_MENU);
			}
			break;
		case ID_SFX:
			if (SoundEnabled)
			{
				SoundOn^=TRUE;
				if (SoundOn)
					PlaySound(SND_POINT);
			}
			break;
		}
	else if (op==ID_NEWPLYR)
	{
		if (cp==-1)
		{
			if (nIdents==20)
				goto caca;
			cp=0;
			NewName[0]=0;
		}
		crsr++;
		crsr&=31;
		if (CurrentKey)
		{
			if ((CurrentKey=='\b') && (cp>0))
			{
				PlaySound(SND_POINT);
				cp--;
				NewName[cp]=0;
			}
			else if ((cp<12) &&
					(((CurrentKey>='A') && (CurrentKey<='Z')) ||
					((CurrentKey>='a') && (CurrentKey<='z')) ||
					((CurrentKey>=48) && (CurrentKey<=57)) ||
					(CurrentKey==' ')))
			{
				PlaySound(SND_POINT);
				NewName[cp]=CurrentKey;
				cp++;
				NewName[cp]=0;
			}
		}
	}
caca:
	Pac->ypos=236+CurSel*36;
}
