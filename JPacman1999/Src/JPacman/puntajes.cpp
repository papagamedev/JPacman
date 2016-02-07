#include "JPacman.h"

int PuntajesMode,PosIns,CurPos,Cursor,byep,CurScr;

unsigned MaxPuntajes[20][10]={
	10000,9999,9696,6969,6666,5432,2345,2323,2222,2000
};

char MaxNames[20][10][16]={
	"Pinkasso",
	"The Master",
	"Cucurucho",
	"Flanshop",
	"Point",
	"Paulline",
	"Esponcho",
	"EsPanchi",
	"Pocha",
	"Gismy"
};

int CurIdent=0,nIdents=1;
char Idents[20][16]={"Invitado"};   // 20 jugadores máximo!!
char Opts[NUM_OPTS][20];  // sfx, musica, ronda, movpuntos

void Puntajes_Setup()
{
	SpriteData *temp;
	int i;

	byep=0;
	CurScr=CurIdent;

	temp=AddSprite(SP_PACMAN);
	temp->xpos=30;
	temp->ypos=30;
	temp->dir=DIR_RIGHT;
	temp=AddSprite(SP_PACMAN);
	temp->xpos=610;
	temp->ypos=30;
	temp->dir=DIR_LEFT;
	InputMode=1;

	for (i=10;i<640;i+=10)
	{
		temp=AddSprite(SP_POINT);
		temp->xpos=i;
		temp->ypos=61;
		temp=AddSprite(SP_POINT);
		temp->xpos=i;
		temp->ypos=111;
	}
	for (i=71;i<111;i+=10)
	{
		temp=AddSprite(SP_POINT);
		temp->xpos=10;
		temp->ypos=i;
		temp=AddSprite(SP_POINT);
		temp->xpos=630;
		temp->ypos=i;		
	}

	if (PuntajesMode==0)
	{
		if (CurScr>0)
		{
			for (PosIns=0;PosIns<10;PosIns++)
				if (MaxPuntajes[0][PosIns]<Score) break;
			if (PosIns<10)
			{
				for (i=9;i>PosIns;i--)
				{
					memcpy(MaxNames[0][i],MaxNames[0][i-1],16);
					MaxPuntajes[0][i]=MaxPuntajes[0][i-1];
				}
				MaxPuntajes[0][PosIns]=Score;
				strcpy(MaxNames[0][PosIns],Idents[CurScr]);
			}
		}
		for (PosIns=0;PosIns<10;PosIns++)
			if (MaxPuntajes[CurScr][PosIns]<Score) break;
		if (PosIns==10)
			PuntajesMode=1;
		else
		{
			for (i=9;i>PosIns;i--)
			{
				memcpy(MaxNames[CurScr][i],MaxNames[CurScr][i-1],16);
				MaxPuntajes[CurScr][i]=MaxPuntajes[CurScr][i-1];
			}
			MaxPuntajes[CurScr][PosIns]=Score;
			CurPos=0;
			MaxNames[CurScr][PosIns][CurPos]=0;
		}
	}
	Cursor=0;
	InputMode=1;
	TickMode=1;
	FadeIn();
}

void Puntajes_UpdateFrame()
{
	int i;
	char temp[50];
	
	if (CurScr==0)
		DrawText(165,14,"Mejores Puntajes");
	else
	{
		sprintf(temp,"Puntajes: %s",Idents[CurScr]);
		DrawText(50,14,temp);
	}
	for (i=0;i<10;i++)
	{
		sprintf(temp,"%d.",i+1);
		DrawText(45,70+i*40+10*(i>0),temp);
		if ((i==PosIns) && (Cursor & 16))
		{
			sprintf(temp,"%s-",MaxNames[CurScr][i]);
			DrawText(95,70+i*40+10*(i>0),temp);
		}
		else
			DrawText(95,70+i*40+10*(i>0),MaxNames[CurScr][i]);
		sprintf(temp,"%d",MaxPuntajes[CurScr][i]);
		DrawText(480,70+i*40+10*(i>0),temp);
	}
}

void Puntajes_DoTick()
{
	if (byep==1)
		SetGameMode(MODE_MENU);
	else if (PuntajesMode==1)
	{
		if ((dwKeyState & KEY_ESC) || (dwKeyState & KEY_ENTER))
		{
			PlaySound(SND_FRUIT);
			byep=1;
			FadeOut();
		}
		else if (dwKeyState & KEY_LEFT)
		{
			CurScr--;
			if (CurScr==-1) CurScr+=nIdents;
		}
		else if (dwKeyState & KEY_RIGHT)
		{
			CurScr++;
			if (CurScr==nIdents) CurScr=0;
		}
	}
	else
	{
		Cursor++;
		Cursor &= 31;
		if ((dwKeyState & KEY_ESC) || (dwKeyState & KEY_ENTER))
		{
			SaveScores();
			PuntajesMode=1;
			PlaySound(SND_FRUIT);
			if (dwKeyState & KEY_ENTER)
			{
				Cursor=0;
			}
			else
			{
				byep=1;
				FadeOut();
			}
			return;
		}
		if ((CurrentKey))
		{
			if ((CurrentKey=='\b') && (CurPos>0))
			{
				PlaySound(SND_POINT);
				CurPos--;
				MaxNames[CurScr][PosIns][CurPos]=0;
			}
			else if ((CurPos<15) &&
					(((CurrentKey>='A') && (CurrentKey<='Z')) ||
					((CurrentKey>='a') && (CurrentKey<='z')) ||
					((CurrentKey>=40) && (CurrentKey<=59)) ||
					(CurrentKey=='&') || (CurrentKey=='%') ||
					(CurrentKey=='!') || (CurrentKey=='?') ||
					(CurrentKey==' ')))
			{
				PlaySound(SND_POINT);
				MaxNames[CurScr][PosIns][CurPos]=CurrentKey;
				CurPos++;
				MaxNames[CurScr][PosIns][CurPos]=0;
			}
		}
	}
}

void LoadScores()
{
	int f,i,j,c,id;
	unsigned char buf[240];
	char fname[50];

	for (id=0;id<nIdents;id++)
	{	
		for (i=0;i<10;i++)
		{
			memset(MaxNames[id][i],0,16);
			MaxPuntajes[id][i]=0;
		}

		if (id==0)
			strcpy(fname,"General.ptj");
		else
			sprintf(fname,"%s.ptj",Idents[id]);
	
		if ((f=_open(fname,_O_RDONLY | _O_BINARY))==-1)
		{
			DPF(0,"Error al abrir archivo con puntajes");
			continue;
		}
		if (_read(f,buf,240)<240)
		{
			DPF(0,"Error al leer archivo con puntajes");
			_close(f);
			continue;
		}
		_close(f);

		c=0;
		for (j=0;j<10;j++)
		{
			for (i=0;i<20;i++)
				c+=buf[j*20+i+40];
			if (c!=*((int *)buf+j))
			{
				DPF(0,"Archivo de puntajes corrupto!");
				continue;
			}
		}
	
		for (i=0;i<10;i++)
		{
			memcpy(MaxNames[id][i],buf+40+16*i,16);
			MaxPuntajes[id][i]=*((int *) (buf+200+4*i));
		}
	}

	// puntajes a la chancha
/*
	strcpy(MaxNames[1][0],"pera1");
	strcpy(MaxNames[2][0],"pina2");
	strcpy(MaxNames[2][1],"pina1");
	MaxPuntajes[1][0]=239825;
	MaxPuntajes[2][0]=275430;
	MaxPuntajes[2][1]=106150;
	SaveScores();*/
}

void SaveScores()
{
	int f,i,j,id;
	unsigned c;
	unsigned char buf[240];
	char fname[50];

	for (id=0;id<nIdents;id++)
	{	
		if (id==0)
			strcpy(fname,"General.ptj");
		else
			sprintf(fname,"%s.ptj",Idents[id]);
	
		for (i=0;i<10;i++)
		{
			memcpy(buf+40+16*i,MaxNames[id][i],16);
			*((int *) (buf+200+4*i))=MaxPuntajes[id][i];
		}
	
		c=0;
		for (j=0;j<10;j++)
		{
			for (i=0;i<20;i++)
				c+=buf[j*20+i+40];
			*((int *)buf+j)=c;
		}

		if ((f=_open(fname,_O_RDWR | _O_BINARY | _O_CREAT | _O_TRUNC, _S_IREAD | _S_IWRITE))==-1)
		{
			DPF(0,"Error al crear archivo con puntajes");
			continue;
		}
		if (_write(f,buf,240)!=240)
		{
			DPF(0,"Error al escribir archivo con puntajes");
			_close(f);
			continue;
		}
		_close(f);
	}
}

void LoadConfig()
{	
	int f,i,j;
	unsigned char buf[320];

	if ((f=_open("Config.dat",_O_RDONLY | _O_BINARY))==-1)
	{
		DPF(0,"Error al abrir archivo de configuracion");
		return;
	}
	if (_read(f,buf,320)<320)
	{
		DPF(0,"Error al leer archivo de configuracion");
		_close(f);
		return;
	}
	memcpy(Idents[0],buf,320);
	for (nIdents=0;nIdents<20;nIdents++)
		if (!Idents[nIdents][0]) break;
	if (nIdents==0)
	{
		strcpy(Idents[0],"Invitado");
		nIdents++;
	}
	if (_read(f,buf,20*NUM_OPTS)<20*NUM_OPTS)
	{
		DPF(0,"Error al leer archivo de configuracion");
		_close(f);
		return;
	}
	memcpy(Opts[0],buf,20*NUM_OPTS);
	for (i=nIdents;i<20;i++)
		for (j=0;j<NUM_OPTS;j++)
			Opts[j][i]=0;

	_close(f);
}

void UpdateConfig(int save)
{
	if (!save)
	{
		SoundOn=1-(Opts[0][CurIdent]&1);
		MusicOn=1-(Opts[1][CurIdent]&1);
		MovPointsOn=1-(Opts[2][CurIdent]&1);
		StartRound=Opts[3][CurIdent]%3;
	}
	else
	{
		Opts[0][0]=(unsigned char) (1-((SoundEnabled)?SoundOn:TRUE));
		Opts[1][0]=(unsigned char) (1-((MusicEnabled)?MusicOn:TRUE));
		Opts[2][0]=(unsigned char) (1-MovPointsOn);
		Opts[3][0]=(unsigned char) (StartRound);
		Opts[0][CurIdent]=Opts[0][0];
		Opts[1][CurIdent]=Opts[1][0];
		Opts[2][CurIdent]=Opts[2][0];
		Opts[3][CurIdent]=Opts[3][0];
	}
}

void SaveConfig()
{
	int f;
	
	UpdateConfig(TRUE);

	if ((f=_open("Config.dat",_O_RDWR | _O_BINARY | _O_CREAT | _O_TRUNC, _S_IREAD | _S_IWRITE))==-1)
	{
		DPF(0,"Error al crear archivo de configuracion");
		return;
	}
	if (_write(f,Idents,320)!=320)
	{
		DPF(0,"Error al escribir archivo de configuracion");
		_close(f);
		return;
	}
	if (_write(f,Opts,NUM_OPTS*20)!=NUM_OPTS*20)
	{
		DPF(0,"Error al escribir archivo de configuracion");
		_close(f);
		return;
	}
	_close(f);
}
