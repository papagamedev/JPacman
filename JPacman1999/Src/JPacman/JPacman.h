#ifndef JPACMAN_INCLUDED
#define JPACMAN_INCLUDED

#ifndef JPACMAN_COCOS2DX

#define INITGUID

#undef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <windowsx.h>
#include <mmsystem.h>
#include <ddraw.h>
#include <dsound.h>
#ifdef DIRECTMUSIC_SUPPORT
# include <dmusici.h>
#endif
#include <stdlib.h>
#include <stdio.h>
#include <io.h>
#include <fcntl.h>
#include <sys\stat.h>
#include <sys\types.h>
#include "resource.h"
#include "ddutil.h"
#include "dsutil.h"
#include "debug.h"

extern LPDIRECTSOUND           lpDS;
extern HWND                    hWndMain;

#else // JPACMAN_COCOS2DX

typedef int BOOL;
typedef unsigned int DWORD;
const BOOL TRUE = 1;
const BOOL FALSE = 0;
void DPF(int priority, char *fmt, ...);
#define NULL  0

#endif // JPACMAN_COCOS2DX

#include "input.h"

BOOL    InitializeGame( void );
BOOL    CleanupAndExit( char *err );
int  randInt( int low, int high );
double  inline randDouble( double low, double high );

extern char GFXFile[10];

enum
{
    MODE_MENU=1,
    MODE_GAME,
	MODE_INTRO,
	MODE_PUNTAJES
};

enum
{
	DIR_RIGHT,
	DIR_LEFT,
	DIR_DOWN,
	DIR_UP,
};

typedef struct SSpr {
	int idx;
	int kind;
	int dir;
	float frame;
	float framespeed;
	int xpos;
	int ypos;
	struct SSpr *next;
	struct SSpr *prev;
} SpriteData;

typedef struct {
	int lastframe;
	int animreverse;
	double speedframes;
	int width;
	int height;
	int right_x[8];
	int left_x[8];
	int down_x[8];
	int up_x[8];
	int right_y[8];
	int left_y[8];
	int down_y[8];
	int up_y[8];
} SpriteKind;

enum
{
	SP_GOBLIN_RED,
	SP_GOBLIN_PINK,
	SP_GOBLIN_GREEN,
	SP_GOBLIN_ORANGE,
	SP_GOBLIN_PURPLE,
	SP_GOBLIN_EYES,
	SP_PACMAN,
	SP_FRUIT,
	SP_COOKIE,
	SP_POINT
};

void SetGameMode(int Mode);

BOOL InitGFX();
BOOL RestoreGFX();
void UpdateGFX();
void UninitGFX();

extern void (*Setup)(void);
extern void (*UpdateFrame)(void);
extern void (*DoTick)(void);
extern BOOL TickMode;

extern SpriteData *Sprite1;

void Menu_Setup();
void Menu_UpdateFrame();
void Menu_DoTick();

void Game_Setup();
void Game_UpdateFrame();
void Game_DoTick();

void Intro_Setup();
void Intro_UpdateFrame();
void Intro_DoTick();

void Puntajes_Setup();
void Puntajes_UpdateFrame();
void Puntajes_DoTick();

void InitText();
void DrawText(int x,int y,char *text);

void ClearSprites();
SpriteData *AddSprite(int kind);
void RemoveSprite(SpriteData **sp);
int CheckCollision(SpriteData *spr1,SpriteData *spr2);
int DrawSprite(int x,int y,int kind,int dir,int frame);
void FlipScreen( void );
void EraseScreen( void );

#define TICKS_SEC	30

extern int StartRound;
extern int MovPointsOn;
extern int MusicEnabled,MusicOn;
extern int SoundEnabled,SoundOn;

extern int frames_sec;
extern int frame_updated;
extern int frames_count;
extern int ticks_count;

extern double fademode;
extern double fadestate;
extern int skipflip;

void StampPalette(void);
void FadeIn(void);
void FadeOut(void);

void LoadScores(void);
void SaveScores(void);
void LoadConfig(void);
void SaveConfig(void);
void UpdateConfig(int save=FALSE);

extern unsigned Score;
extern int PuntajesMode;

BOOL InitMusic(void);
void UninitMusic(void);
void PlayMusic(int mus);

int InitSound(void);
void UninitSound(void);
void PlaySound(int mus);
void StopSound(int mus);

#define NUM_OPTS 4

extern int CurIdent,nIdents;
extern char Idents[20][16];
extern char Opts[NUM_OPTS][20];

enum {
	MUS_NONE,
	MUS_MENU,
	MUS_GAMEX,
	MUS_GAMEGRN,
	MUS_GAMEBONUS,
	MUS_GAMEFINAL,
	MUS_INTRO,
	MUS_GAMEINTRO,
	MUS_OOPS,
	MUS_WIN
};

enum {
	SND_FRUIT=1,
	SND_POINT,
	SND_REVENGE,
	SND_REVENGETIME,
	SND_EYES
};

#endif
