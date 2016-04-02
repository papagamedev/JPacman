#include "JPacman.h"

SpriteData *iPac,*iGoblins[4],*iPoints[100];

int StepX,ChgTick,CurTick,ChgMode,byei;

int PointsPos[3][100][2]={{
	{110,40},{110,50},{110,60},{110,70},{110,80},{110,90},{110,100},
	{110,30},{120,30},{130,30},{140,30},{150,30},{160,30},{170,30},{180,30},{190,30},{200,30},{210,30},{220,30},
	{230,30},{240,30},{250,30},{260,30},{270,30},{280,30},{290,30},{300,30},{310,30},{320,30},{330,30},{340,30},
	{110,110},{120,110},{130,110},{140,110},{150,110},{160,110},{170,110},{180,110},{190,110},{200,110},{210,110},{220,110},
	{230,110},{240,110},{250,110},{260,110},{270,110},{280,110},{290,110},{300,110},{310,110},{320,110},{330,110},{340,110},
	{350,110},{360,110},{370,110},{380,110},{390,110},{400,110},{410,110},{420,110},{430,110},{440,110},{450,110},{460,110},
	{350,30},{360,30},{370,30},{380,30},{390,30},{400,30},{410,30},{420,30},{430,30},{440,30},{450,30},{460,30},
	{470,30},{480,30},{490,30},{500,30},{510,30},{520,30},{530,30},
	{470,110},{480,110},{490,110},{500,110},{510,110},{520,110},{530,110},
	{530,40},{530,50},{530,60},{530,70},{530,80},{530,90},{530,100}
},{
	{150,130},{150,140},{150,150},{150,160},{150,170},{150,180},{150,190},{150,200},{140,210},{130,210},{120,210},{110,200},			// J
	{170,130},{170,140},{170,150},{170,160},{170,170},{170,180},{170,190},{170,200},{170,210},{180,130},{190,130},{200,140},{200,150},{200,160},{190,170},{180,170}, // P
	{220,130},{220,140},{220,150},{220,160},{220,170},{220,180},{220,190},{220,200},{220,210},{230,210},{240,210},{250,210},{260,210}, // L
	{360,140},{350,130},{340,130},{330,130},{320,140},{320,150},{320,160},{330,170},{340,170},{350,170},{360,180},{360,190},{360,200},{350,210},{340,210},{330,210},{320,200}, // S
	{380,180},{380,190},{380,200},{390,210},{400,210},{410,210},{420,200},{420,190},{420,180},{410,170},{400,170},{390,170},  // o
	{450,210},{450,200},{450,190},{450,180},{450,170},{440,160},{450,160},{460,160},{450,150},{450,140},{460,130},{470,130}, // f
	{510,130},{510,140},{490,150},{500,150},{510,150},{520,150},{530,150},{510,160},{510,170},{510,180},{510,190},{510,200},{520,210},{530,210}, // t
	{530,210},{530,210},{530,210},{530,210}
},{
	{140,370},{140,380},{140,390},{140,400},{140,410},{140,420},{140,430},{140,440},{150,370},{160,370},{170,380},{170,390},{170,400},{160,410},{150,410}, // p
	{190,370},{190,380},{190,390},{190,400},{190,410},{200,380},{210,370},{220,370}, // r
	{250,390},{260,390},{270,390},{270,380},{260,370},{250,370},{240,380},{240,390},{240,400},{250,410},{260,410},{270,410}, //e
	{320,370},{310,370},{300,370},{290,380},{300,390},{310,390},{320,400},{310,410},{300,410},{290,410}, // s
	{350,390},{360,390},{370,390},{370,380},{360,370},{350,370},{340,380},{340,390},{340,400},{350,410},{360,410},{370,410}, //e
	{390,370},{390,380},{390,390},{390,400},{390,410},{400,370},{410,370},{420,380},{420,390},{420,400},{420,410}, // n
	{440,340},{440,350},{430,360},{440,360},{450,360},{440,370},{440,380},{440,390},{440,400},{450,410},{460,410}, // t
	{480,370},{490,370},{500,370},{510,380},{510,390},{510,400},{510,410},{500,410},{490,410},{480,400},{490,390},{500,390}, // a
	{490,410},{490,410},{490,410},{490,410},{490,410},{490,410},{490,410},{490,410},{490,410}
}};

void Intro_Setup()
{
	DPF(0, "Intro setup");
	int i;

	iPac=AddSprite(SP_PACMAN);
	iPac->xpos=-80;
	iPac->ypos=240;
	
	for (i=0;i<4;i++)
	{
		iGoblins[i]=AddSprite(SP_GOBLIN_RED+i);
		iGoblins[i]->xpos=(-4-i)*40;
		iGoblins[i]->ypos=240;
		iGoblins[i]->dir=DIR_RIGHT;
	}

	for (i=0;i<100;i++)
	{
		iPoints[i]=AddSprite(SP_POINT);
		iPoints[i]->xpos=PointsPos[0][i][0];
		iPoints[i]->ypos=PointsPos[0][i][1];
		iPoints[i]->framespeed=0.5;
	}
	
	// INTRO DURA 9600ms con tempo=200

	StepX=7; // 420/(4.8*TICKS_SEC) = ancho pantalla dividido por numero de ticks en 4.8 segundos
	ChgTick = 157;// 144;
	CurTick=0;
	ChgMode=0;

	byei=0;

	InputMode=1;
	TickMode=1;

	PlayMusic(MUS_INTRO);
}

void Intro_UpdateFrame()
{

}

void Intro_DoTick()
{
	int i;

	if (byei==2)
	{
		FadeOut();
		byei=1;
		return;
	}
	else if (byei==1)
	{
		PlayMusic(MUS_MENU);
		SetGameMode(MODE_MENU);
		return;
	}
	
	if (dwKeyState & JPACMAN_KEY_ESC)
	{
		for (i=0;i<4;i++)
			RemoveSprite(&iGoblins[i]);
		RemoveSprite(&iPac);
		byei=2;
		PlayMusic(MUS_NONE);
		return;
	}

	iPac->xpos+=StepX;
	for (i=0;i<4;i++)
		iGoblins[i]->xpos+=StepX;

	if (ChgMode>0)
	{
		for (i=0;i<100;i++)
		{
			if (iPoints[i]->xpos>PointsPos[ChgMode][i][0]) iPoints[i]->xpos-=10;
			if (iPoints[i]->ypos>PointsPos[ChgMode][i][1]) iPoints[i]->ypos-=10;
			if (iPoints[i]->xpos<PointsPos[ChgMode][i][0]) iPoints[i]->xpos+=10;
			if (iPoints[i]->ypos<PointsPos[ChgMode][i][1]) iPoints[i]->ypos+=10;
		}
	}


	CurTick++;

	if (CurTick==15)
		ChgMode=1;
	else if (CurTick==ChgTick-5)
	{
		ChgMode=2;
		StepX=-StepX;
		iPac->dir=DIR_LEFT;
		iPac->xpos=800;
		for (i=0;i<4;i++)
		{
			RemoveSprite(&iGoblins[i]);
			iGoblins[i]=AddSprite(SP_GOBLIN_PURPLE);
			iGoblins[i]->ypos=240;
			iGoblins[i]->xpos=650+35*i;
		}
	}
	else if (CurTick==ChgTick*2-25)
	{
		CurTick=ChgTick*2;
		byei=1;
		FadeOut();
	}
}
