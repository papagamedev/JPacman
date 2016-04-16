#include "JPacman.h"

SpriteData *Pacman,*Goblins[4],*Cookies[4],*Fruit;
int nPoints,fFruit,Map=0,Lives,rev_time,Level,levR,levL,PauseMode,ByeMode,TotalPoints;
int GoblinSpeed,BonusMode,RevengeTimeout,CookiesMove,PointsMove,PointsMult,GoblinCI,GoblinStart,GobPurp[4];
int GameTime,LiveTime,LossTime,WinTime,ScoreX,ScoreY,ScoreSt,ScoreStX,ScoreStY,GameTune;
unsigned ScoretoAdd,FruitScore,rev_score,Score,cnteyes,cntprp;
int TunelsX[Level::MaxTunnels*2],TunelsY[Level::MaxTunnels * 2],nTunels;
int EntTunX[Level::MaxEntries],EntTunY[Level::MaxEntries],nEnts,GobInTun[4],GobMove;
int PacmanX,PacmanY,ExitX,ExitY,HouseX,HouseY,HouseKind,HouseExitDir,FruitX,FruitY,CookiesX[4],CookiesY[4],TextX,TextY; // pos. iniciales
int MovPointsOn=TRUE;	// para habilitar el movimiento de puntos
int StartRound=0;	// ronda inicial - 1


char Maps[2][Level::Height*Level::Width+1] = { 
	 "                                        " 
	 " ##################  ################## " 
	 " ##################  ################## " 
	 " ##      ##      ##  ##      ##      ## " 
	 " ###################################### " 
	 " ###################################### " 
	 " $G      ##  ##          ##  ##      $G " 
	 " ##      ##  ######  ######  ##      ## " 
	 " ##########  ######  ######  ########## " 
	 " ##########      ##  ##      ########## " 
	 "         ## ################ ##         " 
	 "         ## ################ ##         " 
	 "         ## ##      S     ## ##         " 
	 "$$$$$$$$$##### $$$$$$$$$$ #####$$$$$$$$$" 
	 "$a$$$$$$E##### $$$$$H$$$$ #####$E$$$$$$b" 
	 "         ## ##            ## ##         " 
	 " ####### ## ##$$$$$$L$$$$$## ## ####### " 
	 " ####### ## ##$$$$$$F$$$$$## ## ####### " 
	 " $G   ## ## ##            ## ## ##   $G " 
	 " #### #############$$############# #### " 
	 " #### #############$P############# #### " 
	 "   ## ##   ##              ##   ## ##   " 
	 " #######   #######    #######   ####### " 
	 " #######   #######    #######   ####### " 
	 " ##             ##    ##             ## " 
	 " ###################################### " 
	 " ###################################### " 
	 "                                        " 
	 };

bool LoadLevel(const char* fileName,char *mapBuf)
{
	FILE* fileHandle;
	int err = fopen_s(&fileHandle, fileName, "rt");
	if (err != NOERROR)
	{
		DPF(0, "Error loading map from %s", fileName);
		return false;
	}

	// map file is written by MFC application as map height (28) CStrings
	// each CString is written as 1 byte for the length and then the string without null ending char
	// in this case, CString length is always the map width (40)

	const int mapSize = Level::Height*(Level::Width+1);
	char tempBuf[mapSize+1];
	int readCount = fread_s(tempBuf, mapSize, 1, mapSize, fileHandle);
	if (readCount != mapSize)
	{
		DPF(0, "Not enough data in %s", fileName);
		return false;
	}
	tempBuf[mapSize] = 0;

	// now "read" each line confirming the CString's structure

	char *p = tempBuf;
	for (int row = 0; row < Level::Height; row++)
	{
		char colWidth = *p++;
		if (colWidth != Level::Width)
		{
			DPF(0, "Incorrect map format in %s", fileName);
			return false;
		}

		memcpy(&mapBuf[Level::Width*row], p, Level::Width);
		p += Level::Width;
	}

	DPF(0, "Map data read ok from %s", fileName);
	fclose(fileHandle);

	return true;
}

void AddScore(int x,int y,int s)
{
	int as=Score/10000;
	
	if ((s>=100) || (s==0))
	{
		if (ScoretoAdd)
			Score+=ScoretoAdd;
		ScoretoAdd=s;
		if (s>0)
		{
			ScoreX=x;
			ScoreY=y;
			ScoreSt=0;
			ScoreStX=(130-ScoreX)/TICKS_SEC;
			ScoreStY=(446-ScoreY)/TICKS_SEC;
		}
	}
	else
		Score+=s;
	while ((int)(Score/10000)>as)
	{
		Lives++;
		as++;
	}
}

void InitLive()
{
	int i;
	SpriteData *act;
	
	LiveTime=0;
	Pacman=AddSprite(SP_PACMAN);
	Pacman->xpos=PacmanX;
	Pacman->ypos=PacmanY;
	Pacman->dir=-1;
	cnteyes=cntprp=0;
	for (i=0;i<4;i++)
	{
		if (Goblins[i]!=NULL) RemoveSprite(&Goblins[i]);   // POSICIONAR FANTASMAS
		Goblins[i]=AddSprite(SP_GOBLIN_RED+i);
		Goblins[i]->xpos=(HouseKind)?HouseX-60+40*i:HouseX;
		Goblins[i]->ypos=(HouseKind)?HouseY:HouseY-60+40*i;
		Goblins[i]->dir=(HouseKind)?i>>1:(i>>1)+2;
		GobInTun[i]=FALSE;
		GobPurp[i]=FALSE;
		if (Cookies[i]!=NULL)					// POSICIONAR GALLETAS
		{
			Cookies[i]->xpos=CookiesX[i];
			Cookies[i]->ypos=CookiesY[i];
			Cookies[i]->dir=-1;
		}
	}
	for (act=Sprite1;act!=NULL;act=act->next)
		if ((act->kind==SP_POINT) && (act->framespeed!=1))
		{
			act->framespeed=0;
			act->dir=-1;
		}

	PauseMode=0;
}

/*
	  32 NIVELES: A,B,C,*,D,E,J,*               RONDA 1
	              A,B,C,D,*,E,F,G,J,*,          RONDA 2
		          A,B,C,D,*,E,F,G,*,H,I,J,*,K   RONDA 3
	
	    A -> I: GALLETAS DURAN MENOS TIEMPO, MONOS MAS RAPIDOS Y ASTUTOS
		A -> 1er Bonus: NORMAL
		1er -> 2do Bonus: PUNTOS SE MUEVEN
		2do -> 3er Bonus: GALLETAS SE MUEVEN
		*: solo galletas, puntaje por fantasmas en incremento...
		J: SIN GALLETAS, PUNTOS SE MULTIPLICAN!!!
		K: SIN GALLETAS, PUNTOS SE MULTIPLICAN MAS AUN!!
	
*/

char GetMap(int row, int col)
{
	return Maps[Map][row*Level::Width + col];
}

void InitLevel()
{
	int b, ck;
	SpriteData *act;

	PlayMusic(MUS_NONE);
	ClearSprites();

	act = AddSprite(SP_POINT);
	act->xpos = 8;
	act->ypos = 8;
	act->framespeed = 1;
	act = AddSprite(SP_POINT);
	act->xpos = 632;
	act->ypos = 8;
	act->framespeed = 1;
	act = AddSprite(SP_POINT);
	act->xpos = 632;
	act->ypos = 440;
	act->framespeed = 1;
	act = AddSprite(SP_POINT);
	act->xpos = 8;
	act->ypos = 440;
	act->framespeed = 1;

	act = AddSprite(SP_PACMAN);
	act->xpos = 18;
	act->ypos = 462;
	act->dir = DIR_RIGHT;
	act->frame = 5;
	act->framespeed = 0;

	for (int i = 0; i < Level; i++)
	{
		act = AddSprite(SP_FRUIT);
		act->dir = 0;
		act->frame = (float)i;
		act->framespeed = 0;
		act->xpos = 620 - 18 * (i & 15);
		act->ypos = (i & 16) ? 470 : 452;
	}

	GameTime = LossTime = WinTime = 0;
	b = Level;
	BonusMode = CookiesMove = PointsMove = PointsMult = FALSE;
	if (b > 18)							// NIVELES DIFICILES
	{
		levR = 3;
		b -= 18;
		levL = b;
		if (b > 9)
		{
			levL -= 2;
			GoblinSpeed = 4;  //5;
			FruitScore = 500 * (b - 2);
			PointsMove = 2;
			CookiesMove = 1;
			GameTune = MUS_GAMEGRN;
		}
		else if (b > 5)
		{
			levL--;
			GoblinSpeed = 4;
			FruitScore = 500 * (b - 1);
			PointsMove = 1;
			GameTune = MUS_GAMEGRN;
		}
		else
		{
			GoblinSpeed = 3;
			FruitScore = 500 * b;
			GameTune = MUS_GAMEX;
		}
		if ((b == 5) || (b == 9) || (b == 13))
		{
			BonusMode = 1;
			FruitScore = 5000;
			GameTune = MUS_GAMEBONUS;
		}
		else if (b == 12)
		{
			FruitScore = 5000;
			PointsMult = 1;
			GameTune = MUS_GAMEFINAL;
		}
		else if (b == 14)
		{
			FruitScore = 7000;
			PointsMult = 2;
			GameTune = MUS_GAMEFINAL;
		}
		RevengeTimeout = (b == 12) ? 1 : (14 - b)*TICKS_SEC + 1;
		GoblinCI = (b * 3) / 2 + 8;
		GoblinStart = 5 - (b >> 2);
	}
	else if (b > 8)					// NIVELES MEDIOS
	{
		levR = 2;
		b -= 8;
		levL = b;
		if (b > 5)
		{
			levL--;
			GoblinSpeed = 4;
			FruitScore = 500 * (b - 1);
			PointsMove = 1;
			GameTune = MUS_GAMEGRN;
		}
		else
		{
			GoblinSpeed = 3;
			FruitScore = 500 * b;
			GameTune = MUS_GAMEX;
		}
		if ((b == 5) || (b == 10))
		{
			BonusMode = 1;
			FruitScore = 5000;
			GameTune = MUS_GAMEBONUS;
		}
		else if (b == 9)
		{
			FruitScore = 5000;
			PointsMult = 1;
			GameTune = MUS_GAMEFINAL;
		}
		RevengeTimeout = (b == 9) ? 1 : (14 - b)*TICKS_SEC + 1;
		GoblinCI = b + 2;
		GoblinStart = 6 - (b >> 1);
	}
	else							// NIVELES FACILES
	{
		levR = 1;
		levL = b;
		if (b > 4)
		{
			levL--;
			GoblinSpeed = 4;
			FruitScore = 500 * (b - 1);
			PointsMove = TRUE;
			GameTune = MUS_GAMEGRN;
		}
		else
		{
			GoblinSpeed = 3;
			FruitScore = 500 * b;
			GameTune = MUS_GAMEX;
		}
		if ((b == 4) || (b == 8))
		{
			BonusMode = 1;
			FruitScore = 5000;
			GameTune = MUS_GAMEBONUS;
		}
		else if (b == 7)
		{
			FruitScore = 5000;
			PointsMult = 1;
			GameTune = MUS_GAMEFINAL;
		}
		RevengeTimeout = (b == 7) ? 1 : (14 - b)*TICKS_SEC + 1;
		GoblinCI = b - 1;
		GoblinStart = 8 - b;
	}

	if (BonusMode)
	{
		RevengeTimeout += (5 * TICKS_SEC);
		GoblinStart = 1;
	}

	rev_score = 200;

	nPoints = 0;
	for (int i = 0; i < 4; i++)					// AGREGAR GALLETAS
	{
		Cookies[i] = AddSprite(SP_COOKIE);
		nPoints++;
		Goblins[i] = NULL;
	}

	nEnts = 0;

	for (int i = 0; i < 10; i++)
		TunelsX[i] = TunelsY[i] = 0;

	TotalPoints = 0;
	ck = 0;

	//	traducir mapa

	if (!LoadLevel("map01.map",Maps[0]))
	{
		CleanupAndExit("Error reading map!");
		return;
	}

	for (int i = 0; i < 40; i++)
	{
		for (int j = 0; j < 28; j++)
		{
			if ((GetMap(j,i) == '#') && (GetMap(j,i-1) == '#') &&
				(GetMap(j-1,i) == '#') && (GetMap(j-1,i-1) == '#')
				&& (!BonusMode))
			{
				if ((act = AddSprite(SP_POINT)) == NULL)
				{
					CleanupAndExit("Error al poner un punto! (NULL)");
					return;
				}
				act->xpos = i * 16;
				act->ypos = j * 16;
				nPoints++;
				TotalPoints++;
			}
			else if (GetMap(j,i) == 'E') // Entrada de "TUNELES"
			{
				EntTunX[nEnts] = i;
				EntTunY[nEnts++] = j;
			}
			else if (GetMap(j,i) == 'F') // Ubicacion Fruta
			{
				FruitX = i * 16;
				FruitY = j * 16;
			}
			else if (GetMap(j,i) == 'P') // Ubicacion Pacman
			{
				PacmanX = i * 16;
				PacmanY = j * 16;
			}
			else if (GetMap(j,i) == 'S') // Salida Fantasmas
			{
				ExitX = i * 16;
				ExitY = j * 16;
			}
			else if (GetMap(j,i) == 'L') // Letras
			{
				TextX = i * 16;
				TextY = j * 16 + 8;
			}
			else if (GetMap(j,i) == 'H') // "Casa" horizontal
			{
				HouseX = i * 16;
				HouseY = j * 16;
				HouseKind = 1;
			}
			else if (GetMap(j,i) == 'V') // "Casa" vertical
			{
				HouseX = i * 16;
				HouseY = j * 16;
				HouseKind = 0;
			}
			else if (GetMap(j,i) == 'G') // Galletas (vertical)
			{
				CookiesX[ck] = i * 16;
				CookiesY[ck++] = j * 16 + 8;
			}
			else if (GetMap(j,i) == 'C') // Galletas (horizontal)
			{
				CookiesX[ck] = i * 16 + 8;
				CookiesY[ck++] = j * 16;
			}
			else if (GetMap(j,i) >= 'a') // AGREGAR "TUNELES"
			{
				TunelsX[GetMap(j,i) - 'a'] = i;
				TunelsY[GetMap(j,i) - 'a'] = j;
				DPF(0, "Nuevo tunel #%d: x=%d y=%d", GetMap(j,i) - 'a', i, j);
			}
		}
	}
	for (nTunels=0;(TunelsX[nTunels]!=0) || (TunelsY[nTunels]!=0);nTunels++); // CONTAR TUNELES

	DPF(0,"nTunels=%d",nTunels);

	if (HouseX==ExitX)
	{
		if (HouseY>ExitY)
		{
			HouseExitDir=DIR_UP;
			ExitY-=16;
		}
		else
		{
			HouseExitDir=DIR_DOWN;
			ExitY+=32;
		}
	}
	else
	{
		if (HouseX>ExitX)
		{
			HouseExitDir=DIR_LEFT;
			ExitX-=16;
		}
		else
		{
			HouseExitDir=DIR_RIGHT;
			ExitX+=32;
		}
	}


	FadeIn();
}

void DirsCanMove(SpriteData *spr,char *dirs)
{
	int mxpos,mypos;

	dirs[0]=dirs[1]=dirs[2]=dirs[3]=0;

	mxpos=(spr->xpos) & 15;
	mypos=(spr->ypos) & 15;
	if (mxpos)
		dirs[DIR_LEFT]=dirs[DIR_RIGHT]=1;
	else
	{
		if (mypos)
			dirs[DIR_UP]=dirs[DIR_DOWN]=1;
		else
		{
			mxpos=(spr->xpos) >> 4;
			mypos=(spr->ypos) >> 4;
			if ((GetMap(mypos - 2, mxpos) >= '!') &&
				(GetMap(mypos - 2, mxpos - 1) >= '!'))
			{
				dirs[DIR_UP] = 1;
			}
			if ((GetMap(mypos, mxpos - 2) >= '!') &&
				(GetMap(mypos - 1, mxpos - 2) >= '!'))
			{
				dirs[DIR_LEFT] = 1;
			}
			if ((GetMap(mypos + 1, mxpos) >= '!') &&
				(GetMap(mypos + 1, mxpos - 1) >= '!'))
			{
				dirs[DIR_DOWN] = 1;
			}
			if ((GetMap(mypos, mxpos + 1) >= '!') &&
				(GetMap(mypos - 1, mxpos + 1) >= '!'))
			{
				dirs[DIR_RIGHT] = 1;
			}
		}
	}
}

void Game_Setup()
{
	switch (StartRound)
	{
	case 0:
		Level=1;
		break;
	case 1:
		Level=9;
		break;
	case 2:
		Level=19;
		break;
	}
	Lives=3;
	Score=0;
	Map=0;
	PauseMode=0;
	ByeMode=0;
	InputMode=0;
	ScoretoAdd=0;
	InitLevel();


}

void Game_UpdateFrame()
{
	char buf[100];

	sprintf_s(buf,"x%d",Lives);
	DrawText(36,446,buf);

	sprintf_s(buf,"%d",Score);
	DrawText(130,446,buf);

	if (ScoretoAdd)
	{
		sprintf_s(buf,"%d",ScoretoAdd);
		DrawText(ScoreX,ScoreY,buf);
	}

	if (PauseMode==5)
	{
		DrawText(TextX-52,256,"Pausa");
	}
	else if ((Lives==1) && (LossTime>TICKS_SEC))
	{
		DrawText(TextX-107,256,"Fin del Juego");
	}
	else if (GameTime<TICKS_SEC*2)
	{
		sprintf_s(buf,"Ronda %d",levR);
		DrawText(TextX-63,256,buf);
	}
	else if (GameTime<TICKS_SEC*4)
	{
		if (BonusMode)
			DrawText(TextX-54,256,"Nivel X");
		else if (PointsMult==1)
			DrawText(TextX-85,256,"Nivel Final");		
		else if (PointsMult==2)
			DrawText(TextX-95,256,"jajajajaja...");		
		else
		{
			sprintf_s(buf,"Nivel %d",levL);
			DrawText(TextX-53,256,buf);
		}
	}
}

void MakeMove(SpriteData *spr,char *dirs)
{
	int mxpos,mypos,i;

	if (spr->dir==DIR_UP)
	{
		if (dirs[DIR_UP])
			spr->ypos--;
		else
			spr->dir=-1;
	}
	else if (spr->dir==DIR_DOWN)
	{
		if (dirs[DIR_DOWN])
			spr->ypos++;
		else
			spr->dir=-1;
	}
	else if (spr->dir==DIR_LEFT)
	{
		if (dirs[DIR_LEFT])
			spr->xpos--;
		else
			spr->dir=-1;
	}
	else if (spr->dir==DIR_RIGHT)
	{
		if (dirs[DIR_RIGHT])
			spr->xpos++;
		else
			spr->dir=-1;
	}
	if ((spr->xpos & 15)+(spr->ypos & 15)==0)
	{
		mxpos=spr->xpos >> 4;
		mypos=spr->ypos >> 4;
		for (i=0;i<nTunels;i++) // VERIFICAR TUNELES
			if ((mxpos==TunelsX[i]) && (mypos==TunelsY[i]))
			{
				i^=1;
				spr->xpos=TunelsX[i] << 4;
				spr->ypos=TunelsY[i] << 4;
				break;
			}
		if (GobMove>=0)
			for (i=0;i<nEnts;i++) // VERIFICAR TUNELES
				if ((mxpos==EntTunX[i]) && (mypos==EntTunY[i]))
				{
					GobInTun[GobMove]^=TRUE;
					break;
				}
	}
}

void FollowPos(SpriteData *spr,int xt,int yt,char *dirs,int ci)
{
	int d,tr=0;
	int dx,dy;
	
	if (dirs[0]+dirs[1]+dirs[2]+dirs[3]==1)
	{
		for (d=0;d<4;d++)
			if (dirs[d]) break;
	}
	else if ((ci<4) || (randInt(0,ci)<4))
	{
azar:	
		d=randInt(0,3);
		while ((!dirs[d]) || (spr->dir==(d^1)))
		{
			d++;
			d&=3;
		}
	}
	else
	{
		dx=spr->xpos-xt;
		dy=spr->ypos-yt;
		d=spr->dir;
		if (abs((int)dx)<abs((int)dy))
		{
checkvert:
			if (dy<0)
			{
				if ((!dirs[DIR_DOWN]) || ((d==DIR_UP) && (ci<14)))
				{
					if (tr) goto azar;
					tr=1;
					goto checkhoriz;
				}
				else
					d=DIR_DOWN;
			}
			else
			{
				if ((!dirs[DIR_UP]) || ((d==DIR_DOWN) && (ci<14)))
				{
					if (tr) goto azar;
					tr=1;
					goto checkhoriz;
				}
				else
					d=DIR_UP;
			}
		}
		else
		{
checkhoriz:
			if (dx<0)
			{
				if ((!dirs[DIR_RIGHT]) || ((d==DIR_LEFT) && (ci<14)))
				{
					if (tr) goto azar;
					tr=1;
					goto checkvert;
				}
				else
					d=DIR_RIGHT;
			}
			else
			{
				if ((!dirs[DIR_LEFT]) || ((d==DIR_RIGHT) && (ci<14)))
				{
					if (tr) goto azar;
					tr=1;
					goto checkvert;
				}
				else
					d=DIR_LEFT;
			}
		}
	}
	spr->dir=d;
}

void Game_DoTick()
{
	SpriteData *act,*temp;
	char dirs[4];
	int i,k;

	if (ByeMode)
	{
		if (ByeMode>10)
		{
			ByeMode-=10;
			for (i=0;i<4;i++)
				RemoveSprite(&Goblins[i]);
			RemoveSprite(&Pacman);
			FadeOut();
		}
		else switch (ByeMode)
		{
		case 1:
			SetGameMode(MODE_MENU);
			break;
		case 2:
			SetGameMode(MODE_PUNTAJES);
			break;
		}
		return;
	}

	if (dwKeyState & JPACMAN_KEY_ESC)
	{
		PlayMusic(MUS_NONE);
		StopSound(SND_EYES);
		StopSound(SND_REVENGETIME);
		ByeMode=11;
		return;
	}

	if (Lives==0) 
		return;

	if (PauseMode==5)
	{
		if (dwKeyState & JPACMAN_KEY_SPACE)
		{
			PauseMode=4;
			InputMode=0;
		}
		return;
	}

	GameTime++;

	if (GameTime==1)
	{
		InitLive();
		PlayMusic(MUS_GAMEINTRO);
		TickMode=1;
		return;
	}

	if (ScoretoAdd)
	{
		ScoreSt++;
		if (ScoreSt>TICKS_SEC*2)
		{
			ScoreX+=ScoreStX;
			ScoreY+=ScoreStY;
			if (ScoreSt==TICKS_SEC*3) AddScore(0,0,0);
		}
	}
	
	if (LossTime)
	{
		LossTime--;
		if (LossTime>TICKS_SEC)
		{
			if (!(LossTime & 3))
			{
				i=Pacman->dir;
				if ((i==0) || (i==-1)) i=1;
				else if (i==1) i=3;
				else if (i==3) i=2;
				else if (i==2) i=0;
				Pacman->dir=i;
			}
		}
		else if (LossTime==TICKS_SEC)
		{
			AddScore(0,0,0);
			RemoveSprite(&Pacman);
			if (!BonusMode)
			{
				Lives--;
				if (Lives>0)
					InitLive();
				else
				{
					PuntajesMode=0;
					ByeMode=12;
				}
			}
			else
			{
				LossTime=0;
				goto nextlevel;
			}
		}
		return;
	}

	if (WinTime)
	{
		WinTime--;
		if (WinTime==4)
		{
			for (i=0;i<4;i++)
				RemoveSprite(&Goblins[i]);
			RemoveSprite(&Pacman);
		}
		else if (WinTime==1)
			FadeOut();
		else if (WinTime==0)
		{
			AddScore(0,0,0);
			if (Level<32) Level++;
			InitLevel();
		}
		else if ((!(WinTime & 3)) && (WinTime>4))
		{
			for (k=0;k<4;k++)
			{
				i=Goblins[k]->dir;
				if (i==0) i=1;
				else if (i==1) i=3;
				else if (i==3) i=2;
				else if (i==2) i=0;
				Goblins[k]->dir=i;
			}
		}
		return;
	}

	if (GameTime==TICKS_SEC*((BonusMode)?5:60))
	{
		Fruit=AddSprite(SP_FRUIT);
		Fruit->xpos=FruitX;
		Fruit->ypos=FruitY;
		Fruit->frame=(float)(Level-1);
	}
	else if ((Fruit!=NULL) && (GameTime==80*TICKS_SEC))
		RemoveSprite(&Fruit);

	if (GameTime==TICKS_SEC*4.1)
		TickMode=0;
	if (GameTime<TICKS_SEC*4.1)
		return;

#ifdef _DEBUG
	if (dwKeyState & JPACMAN_KEY_ENTER)
	{
		goto nextlevel;
	}
#endif

	if (PauseMode)
		PauseMode--;
	else if (dwKeyState & JPACMAN_KEY_SPACE)
	{
		PauseMode=5;
		InputMode=1;
	}

	if (LiveTime==0)
		PlayMusic(GameTune);


	LiveTime++;

	if (MovPointsOn)
	{
	// movimiento de galletas

		if ((LiveTime>TICKS_SEC*(7+4*(11-levL+levR))) && (CookiesMove))
			for (i=0;i<4;i++)
				if (Cookies[i]!=NULL)
				{
					DirsCanMove(Cookies[i],dirs);
					if ((dirs[0]+dirs[1]+dirs[2]+dirs[3]>2) || (!dirs[Cookies[i]->dir]))
						FollowPos(Cookies[i],randInt(0,640),randInt(0,480),dirs,1);
					MakeMove(Cookies[i],dirs);
				}

	// movimiento de puntos

		if ((LiveTime>TICKS_SEC*(5+4*(11-levL+levR))) && (PointsMove))
			for (act=Sprite1;act!=NULL;act=act->next)
				if ((act->kind==SP_POINT) && (act->framespeed!=1))
				{
					for (i=0;i<PointsMove;i++)
					{
						DirsCanMove(act,dirs);
						if ((dirs[0]+dirs[1]+dirs[2]+dirs[3]>2) || (!dirs[act->dir]))
							FollowPos(act,randInt(0,640),randInt(0,480),dirs,1);
						MakeMove(act,dirs);
						act->framespeed=0.5;
					}
					if ((nPoints<(TotalPoints*5)/6) && (PointsMult) && (!randInt(0,nPoints*50/PointsMult)))
					{
	// multiplicacion de puntos
						temp=AddSprite(SP_POINT);
						nPoints++;
						temp->dir=act->dir^1;
						temp->xpos=act->xpos;
						temp->ypos=act->ypos;
					}
				}
	
	}

	//mov . pacman

	DirsCanMove(Pacman,dirs);

	if ((dwKeyState & JPACMAN_KEY_UP) && (dirs[DIR_UP]))
		Pacman->dir=DIR_UP;
	else if ((dwKeyState & JPACMAN_KEY_DOWN) && (dirs[DIR_DOWN]))
		Pacman->dir=DIR_DOWN;
	if ((dwKeyState & JPACMAN_KEY_LEFT) && (dirs[DIR_LEFT]))
		Pacman->dir=DIR_LEFT;
	else if ((dwKeyState & JPACMAN_KEY_RIGHT) && (dirs[DIR_RIGHT]))
		Pacman->dir=DIR_RIGHT;
	
	// mov. fantasmas

	GobMove=-1;
	for (k=0;k<4;k++)
		MakeMove(Pacman,dirs);

	for (i=0;i<4;i++)
	{
		GobMove=i;
		if (GobPurp[i])				// morados,huyen
			for (k=0;k<2;k++)
			{
				DirsCanMove(Goblins[i],dirs);
				if ((dirs[0]+dirs[1]+dirs[2]+dirs[3]>2) || (!dirs[Goblins[i]->dir]))
					FollowPos(Goblins[i],HouseX,HouseY,dirs,5);
				MakeMove(Goblins[i],dirs);
			}
		else if (Goblins[i]->kind==SP_GOBLIN_EYES)
			for (k=0;k<8;k++)		// ojos, entran
			{
				if ((Goblins[i]->xpos==ExitX) && (Goblins[i]->ypos==ExitY))
					{
						Goblins[i]->dir=HouseExitDir^1;
						dirs[HouseExitDir^1]=1;
					}
				else				// dan vueltas
				{
					DirsCanMove(Goblins[i],dirs);
					if ((dirs[0]+dirs[1]+dirs[2]+dirs[3]>2) || (!dirs[Goblins[i]->dir]))
						FollowPos(Goblins[i],ExitX,ExitY,dirs,13);
				}
				MakeMove(Goblins[i],dirs);
				if ((Goblins[i]->xpos==HouseX) && (Goblins[i]->ypos==HouseY))
				{
					cnteyes--;
					if (!cnteyes) StopSound(SND_EYES);
					temp=AddSprite(SP_GOBLIN_RED+i);
					temp->xpos=Goblins[i]->xpos;
					temp->ypos=Goblins[i]->ypos;
					temp->dir=Goblins[i]->dir;
					RemoveSprite(&Goblins[i]);
					Goblins[i]=temp;
					break;
				}
			}
		else
			for (k=0;k<((GobInTun[i])?2:GoblinSpeed);k++)
			{
				if ((Goblins[i]->xpos==HouseX) && (Goblins[i]->ypos==HouseY)
					&& (LiveTime>=TICKS_SEC*GoblinStart*i))
					{
						Goblins[i]->dir=HouseExitDir;
						dirs[HouseExitDir]=1;
					}
				else
				{
					DirsCanMove(Goblins[i],dirs);
					if ((dirs[0]+dirs[1]+dirs[2]+dirs[3]>2) || (!dirs[Goblins[i]->dir]))
						FollowPos(Goblins[i],Pacman->xpos,Pacman->ypos,dirs,i+GoblinCI);
				}
				MakeMove(Goblins[i],dirs);
			}
	}
	GobMove=-1;

	act=Sprite1;
	while (act!=NULL)
	{
		if (CheckCollision(Pacman,act))
			switch (act->kind)
			{
			case SP_POINT:
				PlaySound(SND_POINT);
				temp=act;
				if (temp->dir==-1)
					AddScore(0,0,10);
				else
					AddScore(0,0,15);
				RemoveSprite(&temp);
				nPoints--;
				if (nPoints==0)
				{
nextlevel:
					if (rev_time>0) StopSound(SND_REVENGETIME);
					if (cnteyes) StopSound(SND_EYES);
					PlayMusic(MUS_NONE);
					WinTime=TICKS_SEC*4;
					PlayMusic(MUS_WIN);
					return;
				}
				break;
			case SP_GOBLIN_RED:
			case SP_GOBLIN_PINK:
			case SP_GOBLIN_GREEN:
			case SP_GOBLIN_ORANGE:
				for (i=0;i<4;i++)
					if ((Goblins[i]==act) && (GobPurp[i]))
						goto killpurp;
				if (rev_time>0) StopSound(SND_REVENGETIME);
				if (cnteyes) StopSound(SND_EYES);
				PlayMusic(MUS_NONE);
				LossTime=TICKS_SEC*3;
				Pacman->framespeed=0;
				PlayMusic(MUS_OOPS);
				break;
			case SP_COOKIE:
				for (i=0;i<4;i++)
					if (Cookies[i]==act) break;
				PlaySound(SND_POINT);
				nPoints--;
				AddScore(0,0,50);
				RemoveSprite(&Cookies[i]);
				if (nPoints==0)
				{
					if (BonusMode)
						AddScore(Pacman->xpos,Pacman->ypos,5000);
					goto nextlevel;
				}
				for (i=0;i<4;i++)
					if ((Goblins[i]->kind>=SP_GOBLIN_RED) &&
						(Goblins[i]->kind<=SP_GOBLIN_ORANGE))
					{
						GobPurp[i]=TRUE;
						temp=AddSprite(SP_GOBLIN_PURPLE);
						temp->xpos=Goblins[i]->xpos;
						temp->ypos=Goblins[i]->ypos;
						temp->dir=Goblins[i]->dir^1;
						RemoveSprite(&Goblins[i]);
						Goblins[i]=temp;
						cntprp++;
					}
				if (!cntprp) break;
				if (rev_time<1) PlaySound(SND_REVENGETIME);
				rev_time=RevengeTimeout;
				if (!BonusMode)
					rev_score=200;
				else if (rev_score>400)
					rev_score/=4;
				else
					rev_score=200;
				break;
			case SP_GOBLIN_PURPLE:
				for (i=0;i<4;i++)
					if (Goblins[i]==act)
						break;
killpurp:
				GobPurp[i]=FALSE;
				PlaySound(SND_REVENGE);
				if (!cnteyes) PlaySound(SND_EYES);
				cnteyes++;
				cntprp--;
				if (!cntprp) rev_time=1;
				DPF(0,"Fantasma Muerto: act=%u , i=%d",act,i);
				temp=AddSprite(SP_GOBLIN_EYES);
				DPF(0,"                 temp=%u",temp);
				temp->xpos=Goblins[i]->xpos;
				temp->ypos=Goblins[i]->ypos;
				temp->dir=Goblins[i]->dir;
				RemoveSprite(&Goblins[i]);
				DPF(0,"                 goblins[i] (antes) =%u",Goblins[i]);
				Goblins[i]=temp;
				DPF(0,"                 goblins[i] (despues) =%u",Goblins[i]);
				AddScore(temp->xpos,temp->ypos,rev_score);
				rev_score<<=1;
				break;
			case SP_FRUIT:
				PlaySound(SND_FRUIT);
				AddScore(Fruit->xpos,Fruit->ypos,FruitScore);
				RemoveSprite(&Fruit);
				break;
			}
		act=act->next;
	}

	if (rev_time>0)
	{
		rev_time--;
		if ((rev_time<TICKS_SEC*3) && (!(rev_time%(TICKS_SEC/4))))
		{
			for (i=0;i<4;i++)
				if (GobPurp[i])
				{
					if (Goblins[i]->kind==SP_GOBLIN_PURPLE)
						temp=AddSprite(SP_GOBLIN_RED+i);
					else
						temp=AddSprite(SP_GOBLIN_PURPLE);
					temp->xpos=Goblins[i]->xpos;
					temp->ypos=Goblins[i]->ypos;
					temp->dir=Goblins[i]->dir;
					RemoveSprite(&Goblins[i]);
					Goblins[i]=temp;
				}			
		}
		if (!rev_time)
		{
			rev_time=-1;
			cntprp=0;
			StopSound(SND_REVENGETIME);
			for (i=0;i<4;i++)
				if (GobPurp[i])
				{
					GobPurp[i]=FALSE;
					temp=AddSprite(SP_GOBLIN_RED+i);
					temp->xpos=Goblins[i]->xpos;
					temp->ypos=Goblins[i]->ypos;
					temp->dir=Goblins[i]->dir;
					RemoveSprite(&Goblins[i]);
					Goblins[i]=temp;
				}
		}
	}
}
