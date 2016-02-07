// MainFrm.cpp : implementation of the CMainFrame class
//

#include "stdafx.h"
#include "Mapedit.h"

#include "MainFrm.h"
#include "MapeditDoc.h"
#include "MapeditView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CMainFrame

IMPLEMENT_DYNCREATE(CMainFrame, CFrameWnd)

BEGIN_MESSAGE_MAP(CMainFrame, CFrameWnd)
	//{{AFX_MSG_MAP(CMainFrame)
	ON_WM_CREATE()
	ON_COMMAND(ID_HERRAMIENTAS_CAMINO, OnHerramientasCamino)
	ON_COMMAND(ID_HERRAMIENTAS_PARED, OnHerramientasPared)
	ON_COMMAND(ID_HERRAMIENTAS_PUNTOS, OnHerramientasPuntos)
	ON_COMMAND(ID_HERRAMIENTAS_SELECCIONAR, OnHerramientasSeleccionar)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_SELECCIONAR, OnUpdateHerramientasSeleccionar)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_PUNTOS, OnUpdateHerramientasPuntos)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_PARED, OnUpdateHerramientasPared)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_CAMINO, OnUpdateHerramientasCamino)
	ON_UPDATE_COMMAND_UI(ID_VIEW_TOOLS, OnUpdateViewTools)
	ON_UPDATE_COMMAND_UI(ID_INDICATOR_POS, OnUpdatePos)
	ON_COMMAND(ID_VIEW_TOOLS, OnViewTools)
	ON_COMMAND(ID_HERRAMIENTAS_FRUTA, OnHerramientasFruta)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_FRUTA, OnUpdateHerramientasFruta)
	ON_COMMAND(ID_HERRAMIENTAS_GALLETAHORZ, OnHerramientasGalletahorz)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_GALLETAHORZ, OnUpdateHerramientasGalletahorz)
	ON_COMMAND(ID_HERRAMIENTAS_GALLETAVERT, OnHerramientasGalletavert)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_GALLETAVERT, OnUpdateHerramientasGalletavert)
	ON_COMMAND(ID_HERRAMIENTAS_GOBLINSHORZ, OnHerramientasGoblinshorz)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_GOBLINSHORZ, OnUpdateHerramientasGoblinshorz)
	ON_COMMAND(ID_HERRAMIENTAS_GOBLINSVERT, OnHerramientasGoblinsvert)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_GOBLINSVERT, OnUpdateHerramientasGoblinsvert)
	ON_COMMAND(ID_HERRAMIENTAS_LETRAS, OnHerramientasLetras)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_LETRAS, OnUpdateHerramientasLetras)
	ON_COMMAND(ID_HERRAMIENTAS_PACMAN, OnHerramientasPacman)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_PACMAN, OnUpdateHerramientasPacman)
	ON_COMMAND(ID_HERRAMIENTAS_SALIDAHORZ, OnHerramientasSalidahorz)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_SALIDAHORZ, OnUpdateHerramientasSalidahorz)
	ON_COMMAND(ID_HERRAMIENTAS_SALIDAVERT, OnHerramientasSalidavert)
	ON_UPDATE_COMMAND_UI(ID_HERRAMIENTAS_SALIDAVERT, OnUpdateHerramientasSalidavert)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

static UINT indicators[] =
{
	ID_SEPARATOR,           // status line indicator
	ID_INDICATOR_POS,
	ID_INDICATOR_CAPS,
	ID_INDICATOR_NUM,
	ID_INDICATOR_SCRL,
};

int CurrentTool;
CMainFrame *FrameWnd;

/////////////////////////////////////////////////////////////////////////////
// CMainFrame construction/destruction

CMainFrame::CMainFrame()
{
	// TODO: add member initialization code here
	
}

CMainFrame::~CMainFrame()
{
}

int CMainFrame::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CFrameWnd::OnCreate(lpCreateStruct) == -1)
		return -1;
	
	if (!m_wndToolBar.CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_TOP
		| CBRS_GRIPPER | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) ||
		!m_wndToolBar.LoadToolBar(IDR_MAINFRAME))
	{
		TRACE0("Failed to create toolbar\n");
		return -1;      // fail to create
	}

	if (!m_wndTools.CreateEx(this, TBSTYLE_FLAT, WS_CHILD | WS_VISIBLE | CBRS_LEFT
		| CBRS_GRIPPER | CBRS_TOOLTIPS | CBRS_FLYBY | CBRS_SIZE_DYNAMIC) ||
		!m_wndTools.LoadToolBar(IDR_TOOLS))
	{
		TRACE0("Failed to create toolbar\n");
		return -1;      // fail to create
	}

	if (!m_wndStatusBar.Create(this) ||
		!m_wndStatusBar.SetIndicators(indicators,
		  sizeof(indicators)/sizeof(UINT)))
	{
		TRACE0("Failed to create status bar\n");
		return -1;      // fail to create
	}

	// TODO: Delete these three lines if you don't want the toolbar to
	//  be dockable
	m_wndToolBar.EnableDocking(CBRS_ALIGN_ANY);
	m_wndTools.EnableDocking(CBRS_ALIGN_ANY);
	EnableDocking(CBRS_ALIGN_ANY);
	DockControlBar(&m_wndToolBar);
	DockControlBar(&m_wndTools);
	m_wndToolBar.SetWindowText("General");
	m_wndTools.SetWindowText("Herramientas");

	FrameWnd=this;
	
	return 0;
}

BOOL CMainFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	if( !CFrameWnd::PreCreateWindow(cs) )
		return FALSE;
	// TODO: Modify the Window class or styles here by modifying
	//  the CREATESTRUCT cs

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// CMainFrame diagnostics

#ifdef _DEBUG
void CMainFrame::AssertValid() const
{
	CFrameWnd::AssertValid();
}

void CMainFrame::Dump(CDumpContext& dc) const
{
	CFrameWnd::Dump(dc);
}

#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CMainFrame message handlers


void CMainFrame::OnHerramientasCamino() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_CAMINO;
}

void CMainFrame::OnHerramientasPared() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_PARED;
}

void CMainFrame::OnHerramientasPuntos() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_PUNTOS;
}

void CMainFrame::OnHerramientasSeleccionar() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_SELECT;
}

void CMainFrame::OnUpdateHerramientasSeleccionar(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_SELECT));
}

void CMainFrame::OnUpdateHerramientasPuntos(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_PUNTOS));
}

void CMainFrame::OnUpdateHerramientasPared(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_PARED));
}

void CMainFrame::OnUpdateHerramientasCamino(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_CAMINO));
}

void CMainFrame::OnViewTools() 
{
	// TODO: Add your command handler code here
	
	ShowControlBar(&m_wndTools, (m_wndTools.GetStyle() & WS_VISIBLE) == 0, FALSE);
}

void CMainFrame::OnUpdateViewTools(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((m_wndTools.GetStyle() & WS_VISIBLE) != 0);
}

void CMainFrame::OnUpdatePos(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->Enable();

	CMapeditView *v=(CMapeditView *) GetActiveView();

	if ((v!=NULL) && (v->xpos>=0) && (v->xpos<=39) && (v->ypos>=0) && (v->ypos<=27))
	{
		char temp[100];
		sprintf(temp,"Pos: %2d,%2d",v->xpos,v->ypos);
		m_wndStatusBar.SetPaneText(1,temp);
	}
}

void CMainFrame::OnHerramientasFruta() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_FRUTA;	
}

void CMainFrame::OnUpdateHerramientasFruta(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_FRUTA));
}

void CMainFrame::OnHerramientasGalletahorz() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_GALLETAHORZ;	
}

void CMainFrame::OnUpdateHerramientasGalletahorz(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_GALLETAHORZ));
}

void CMainFrame::OnHerramientasGalletavert() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_GALLETAVERT;	
}

void CMainFrame::OnUpdateHerramientasGalletavert(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_GALLETAVERT));
}

void CMainFrame::OnHerramientasGoblinshorz() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_GOBLINSHORZ;	
}

void CMainFrame::OnUpdateHerramientasGoblinshorz(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_GOBLINSHORZ));
}

void CMainFrame::OnHerramientasGoblinsvert() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_GOBLINSVERT;	
}

void CMainFrame::OnUpdateHerramientasGoblinsvert(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_GOBLINSVERT));
}

void CMainFrame::OnHerramientasLetras() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_LETRAS;	
}

void CMainFrame::OnUpdateHerramientasLetras(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_LETRAS));
}

void CMainFrame::OnHerramientasPacman() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_PACMAN;	
}

void CMainFrame::OnUpdateHerramientasPacman(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_PACMAN));
}

void CMainFrame::OnHerramientasSalidahorz() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_SALIDAHORZ;	
}

void CMainFrame::OnUpdateHerramientasSalidahorz(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_SALIDAHORZ));
}

void CMainFrame::OnHerramientasSalidavert() 
{
	// TODO: Add your command handler code here
	
	CurrentTool=TOOL_SALIDAVERT;	
}

void CMainFrame::OnUpdateHerramientasSalidavert(CCmdUI* pCmdUI) 
{
	// TODO: Add your command update UI handler code here
	
	pCmdUI->SetCheck((CurrentTool==TOOL_SALIDAVERT));
}
