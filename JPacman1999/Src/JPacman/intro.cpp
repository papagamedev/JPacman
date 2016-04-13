#include "JPacman.h"

const int sShapesMaxLines = 10;
const int sShapesCount = 3;
const int sMaxPoints = 131;
const int sPointsSpacing = 10;

SpriteData *iPac,*iGoblins[4],*iPoints[sMaxPoints];

int StepX,ChgTick,CurTick,ChgMode,byei;

struct sPointPos
{
	int xPos;
	int yPos;
};

struct sIntroShapes
{
	int xOriginPos;
	int yOriginPos;
	const char *shapeInText[sShapesMaxLines];
	sPointPos pointPos[sMaxPoints];
};

sIntroShapes sShapes[sShapesCount]=
{
	{
		5,3,
		{
		"########################################################",
		"##                                                    ##",
		"##                                                    ##",
		"##                                                     #",
		"##                                                    ##",
		"##                                                    ##",
		"########################################################",
		nullptr
		}
	},
	{
		5,10,
		{
		"###                 ###                  ###            ",
		"#  #               #                     #  #           ",
		"#  # ##  ###  ##   #     ##  ## #   ##   #  #  ##  #   #",
		"###    # #  #   #  # ##    # # # # #  #  #  # #  # #   #",
		"#    ### ###  ###  #  #  ### # # # ####  #  # #### #   #",
		"#   #  # #   #  #  #  # #  # #   # #     #  # #     # # ",
		"#    ### #    ###   ###  ### #   #  ##   ###   ##    #  ",
		nullptr
		}
	},
	{
		10,30,
		{
		"   ###                           #          ",
		"   #  #                         ###         ",
		"   #  # # ##  ##   ###  ##  ###  #    ##    ",
		"   ###  ##   #  # #    #  # #  # #      #   ",
		"   #    #    ####  ##  #### #  # #    ###   ",
		"   #    #    #       # #    #  # #   #  #   ",
		"   #    #     ### ###   ### #  #  ##  ###   ",
		"                                            ",
		"############################################",
		nullptr
		}
	}
};

int ComparePoints(const void *a, const void *b)
{
	const sPointPos* pointA = (const sPointPos*)a;
	const sPointPos* pointB = (const sPointPos*)b;
	if (pointA->xPos < pointB->xPos)
	{
		return -1;
	}
	if (pointA->xPos > pointB->xPos)
	{
		return 1;
	}
	if (pointA->yPos < pointB->yPos)
	{
		return -1;
	}
	if (pointA->yPos > pointB->yPos)
	{
		return 1;
	}
	return 0;
}

void Intro_Setup()
{
	DPF(0, "Intro setup");

	iPac=AddSprite(SP_PACMAN);
	iPac->xpos=-80;
	iPac->ypos=240;
	
	for (int i=0;i<4;i++)
	{
		iGoblins[i]=AddSprite(SP_GOBLIN_RED+i);
		iGoblins[i]->xpos=(-4-i)*40;
		iGoblins[i]->ypos=240;
		iGoblins[i]->dir=DIR_RIGHT;
	}

	for (int shape = 0; shape < sShapesCount; shape++)
	{
		int pointsCount = 0;

		int yPos = sShapes[shape].yOriginPos;
		for (int line = 0; line < sShapesMaxLines && pointsCount < sMaxPoints; line++)
		{
			const char *p = sShapes[shape].shapeInText[line];
			if (p == nullptr)
			{
				break;
			}

			int xPos = sShapes[shape].xOriginPos;
			while (*p != 0 && pointsCount < sMaxPoints)
			{
				char c = *p;
				if (c == '#')
				{
					sShapes[shape].pointPos[pointsCount].xPos = xPos * sPointsSpacing;
					sShapes[shape].pointPos[pointsCount].yPos = yPos * sPointsSpacing;
					pointsCount++;
				}
				p++;
				xPos++;
			}
			yPos++;
		}

		qsort(sShapes[shape].pointPos, pointsCount, sizeof(sPointPos), ComparePoints);
	}

	for (int i=0;i<sMaxPoints;i++)
	{
		iPoints[i]=AddSprite(SP_POINT);
		iPoints[i]->xpos = sShapes[0].pointPos[i].xPos;
		iPoints[i]->ypos = sShapes[0].pointPos[i].yPos;
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
		for (i=0;i<sMaxPoints;i++)
		{
			if (iPoints[i]->xpos > sShapes[ChgMode].pointPos[i].xPos)
			{
				iPoints[i]->xpos -= sPointsSpacing;
			}
			else if (iPoints[i]->xpos < sShapes[ChgMode].pointPos[i].xPos)
			{
				iPoints[i]->xpos += sPointsSpacing;
			}
			if (iPoints[i]->ypos > sShapes[ChgMode].pointPos[i].yPos)
			{
				iPoints[i]->ypos -= sPointsSpacing;
			}
			else if (iPoints[i]->ypos < sShapes[ChgMode].pointPos[i].yPos)
			{
				iPoints[i]->ypos += sPointsSpacing;
			}
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
