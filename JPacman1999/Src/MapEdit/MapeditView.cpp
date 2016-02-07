// MapeditView.cpp : implementation of the CMapeditView class
//

#include "stdafx.h"
#include "Mapedit.h"

#include "MapeditDoc.h"
#include "MainFrm.h"
#include "MapeditView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CMapeditView

IMPLEMENT_DYNCREATE(CMapeditView, CView)

BEGIN_MESSAGE_MAP(CMapeditView, CView)
	//{{AFX_MSG_MAP(CMapeditView)
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	ON_WM_RBUTTONDOWN()
	ON_WM_RBUTTONUP()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CMapeditView construction/destruction

CMapeditView::CMapeditView()
{
	// TODO: add construction code here

	CurrentTool=TOOL_SELECT;
	MousePress=FALSE;
	MouseRtPress=FALSE;
}

CMapeditView::~CMapeditView()
{
}

BOOL CMapeditView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: Modify the Window class or styles here by modifying
	//  the CREATESTRUCT cs

	return CView::PreCreateWindow(cs);
}

/////////////////////////////////////////////////////////////////////////////
// CMapeditView drawing

void CMapeditView::OnDraw(CDC* pDC)
{
	CMapeditDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	// TODO: add draw code for native data here

	int i,j;
	int lx,ly;
	int fx,fy;

	for (i=0;i<28;i++)
		MapView[i]=pDoc->Map[i];

	ApplyCursor(MapView);

	CBrush brBorde(RGB(0,0,96));
	CBrush brCamino(RGB(128,128,255));
	CBrush brFondo(RGB(48,48,96));
	CBrush brSalida(RGB(255,255,255));
	CBrush brPunto(RGB(255,255,0));
	CBrush brFruta(RGB(255,0,0));
	CBrush brEntrada(HS_FDIAGONAL,RGB(255,255,255));
	CBrush brLetras(HS_BDIAGONAL,RGB(0,255,255));
	
	pDC->SetBkColor(RGB(128,128,255));
	CRect rect(19,19,421,301);

	pDC->FrameRect(rect,&brBorde);
	rect.left--;
	rect.top--;
	rect.right++;
	rect.bottom++;
	pDC->FrameRect(rect,&brBorde);
	rect.left--;
	rect.top--;
	rect.right++;
	rect.bottom++;
	pDC->FrameRect(rect,&brBorde);
	for (i=0;i<28;i++)
		for (j=0;j<40;j++)
		{
			rect.left=j*10+20;
			rect.top=i*10+20;
			rect.right=rect.left+10;
			rect.bottom=rect.top+10;
			char c=MapView[i][j];

			if (c=='S')
			{
				rect.left-=10;
				pDC->FillRect(rect,&brSalida);
			}
			else if (c=='T')
			{
				rect.top-=10;
				pDC->FillRect(rect,&brSalida);
			}
			else if (c==' ')
				pDC->FillRect(rect,&brFondo);
			else if ((c>='a') && (c<='t'))
			{
				rect.left-=10;
				rect.top-=10;
				c-='a';
				c>>=1;
				CBrush br(RGB(224-64*(c&1),224-64*((c & 2) >> 1),224-64*((c & 12) >> 2)));
				pDC->FillRect(rect,&br);
			}
			else if ((c=='#') && (i>0) && (j>0) && (MapView[i-1][j]=='#') &&
				(MapView[i][j-1]=='#') && (MapView[i-1][j-1]=='#'))
			{
				pDC->FillRect(rect,&brCamino);
				rect.left-=1;
				rect.top-=1;
				rect.right-=9;
				rect.bottom-=9;
				pDC->FillRect(rect,&brPunto);
			}
			else if (c=='G')
			{
				pDC->FillRect(rect,&brCamino);
				rect.left-=3;
				rect.top+=3;
				rect.right-=7;
				rect.bottom-=3;
				pDC->FillRect(rect,&brPunto);
				rect.left++;
				rect.top--;
				rect.right--;
				rect.bottom++;
				pDC->FillRect(rect,&brPunto);
			}
			else if (c=='C')
			{
				pDC->FillRect(rect,&brCamino);
				rect.left+=3;
				rect.top-=3;
				rect.right-=3;
				rect.bottom-=7;
				pDC->FillRect(rect,&brPunto);
				rect.left--;
				rect.top++;
				rect.right++;
				rect.bottom--;
				pDC->FillRect(rect,&brPunto);
			}
			else if (c=='P')
			{
				pDC->FillRect(rect,&brCamino);
				rect.left-=9;
				rect.top-=9;
				rect.bottom--;
				rect.right--;
				CBrush *obr=pDC->SelectObject(&brPunto);
				pDC->Ellipse(rect);
				pDC->SelectObject(obr);
			}
			else if (c=='E')
			{
				pDC->FillRect(rect,&brCamino);
				rect.left-=10;
				rect.top-=10;
				pDC->FillRect(rect,&brEntrada);
			}
			else if (c=='L')
			{
				lx=rect.left-60;
				ly=rect.top;
			}
			else if (c=='F')
			{
				pDC->FillRect(rect,&brCamino);
				fx=rect.left-5;
				fy=rect.top-5;
			}
			else if ((c=='%') || (c=='V') || (c=='H'))
				pDC->FillRect(rect,&brSalida);
			else
				pDC->FillRect(rect,&brCamino);
		}
	rect.left=lx;
	rect.bottom=ly+20;
	rect.right=lx+120;
	rect.top=ly;
	pDC->FillRect(rect,&brLetras);
	rect.left=fx;
	rect.top=fy;
	rect.bottom=fy+10;
	rect.right=fx+10;
	CBrush *obr=pDC->SelectObject(&brFruta);
	pDC->Ellipse(rect);
	pDC->SelectObject(obr);
}

/////////////////////////////////////////////////////////////////////////////
// CMapeditView diagnostics

#ifdef _DEBUG
void CMapeditView::AssertValid() const
{
	CView::AssertValid();
}

void CMapeditView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CMapeditDoc* CMapeditView::GetDocument() // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CMapeditDoc)));
	return (CMapeditDoc*)m_pDocument;
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CMapeditView message handlers

void CMapeditView::OnMouseMove(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default

	xpos=point.x/10-2;
	ypos=point.y/10-2;

//	if ((xpos<0) || (xpos>39) || (ypos<0) || (ypos>27)) return;
	if (MousePress+MouseRtPress)
	{
		CMapeditDoc* pDoc = GetDocument();
		ASSERT_VALID(pDoc);
	
		ApplyCursor(pDoc->Map);
	}
	InvalidateRect(NULL,FALSE);

	CView::OnMouseMove(nFlags, point);
}

void CMapeditView::OnLButtonDown(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	
	CMapeditDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	
	MousePress=TRUE;
	if ((xpos<0) || (xpos>39) || (ypos<0) || (ypos>27)) return;
	ApplyCursor(pDoc->Map);
	InvalidateRect(NULL,FALSE);

	CView::OnLButtonDown(nFlags, point);
}

void CMapeditView::OnLButtonUp(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	
	MousePress=FALSE;

	CView::OnLButtonUp(nFlags, point);
}

void CMapeditView::ApplyCursor(CString map[])
{
	if ((xpos<0) || (xpos>39) || (ypos<0) || (ypos>27)) return;
	
	if (MouseRtPress) goto pared;

	switch (CurrentTool)
	{
	case TOOL_PARED:
pared:
		map[ypos].SetAt(xpos,' ');
		break;
	case TOOL_CAMINO:
		map[ypos].SetAt(xpos,'$');
		break;
	case TOOL_PUNTOS:
		if ((xpos==0) || (ypos==0)) return;
		map[ypos].SetAt(xpos,'#');
		map[ypos-1].SetAt(xpos,'#');
		map[ypos].SetAt(xpos-1,'#');
		map[ypos-1].SetAt(xpos-1,'#');
		break;
	case TOOL_FRUTA:
		if ((xpos==0) || (ypos==0)) return;
		map[ypos].SetAt(xpos,'F');
		break;
	case TOOL_LETRAS:
		if ((xpos<6) || (xpos>34) || (ypos==27)) return;
		map[ypos].SetAt(xpos,'L');
		map[ypos].SetAt(xpos-1,'$');
		map[ypos].SetAt(xpos-2,'$');
		map[ypos].SetAt(xpos-3,'$');
		map[ypos].SetAt(xpos-4,'$');
		map[ypos].SetAt(xpos-5,'$');
		map[ypos].SetAt(xpos-6,'$');
		map[ypos].SetAt(xpos+1,'$');
		map[ypos].SetAt(xpos+2,'$');
		map[ypos].SetAt(xpos+3,'$');
		map[ypos].SetAt(xpos+4,'$');
		map[ypos].SetAt(xpos+5,'$');
		map[ypos+1].SetAt(xpos,'$');
		map[ypos+1].SetAt(xpos-1,'$');
		map[ypos+1].SetAt(xpos-2,'$');
		map[ypos+1].SetAt(xpos-3,'$');
		map[ypos+1].SetAt(xpos-4,'$');
		map[ypos+1].SetAt(xpos-5,'$');
		map[ypos+1].SetAt(xpos-6,'$');
		map[ypos+1].SetAt(xpos+1,'$');
		map[ypos+1].SetAt(xpos+2,'$');
		map[ypos+1].SetAt(xpos+3,'$');
		map[ypos+1].SetAt(xpos+4,'$');
		map[ypos+1].SetAt(xpos+5,'$');
		break;
	case TOOL_PACMAN:
		if ((xpos==0) || (ypos==0)) return;
		map[ypos].SetAt(xpos,'P');
		break;
	case TOOL_GALLETAVERT:
		if (ypos==0) return;
		map[ypos].SetAt(xpos,'C');
		break;
	case TOOL_GALLETAHORZ:
		if (xpos==0) return;
		map[ypos].SetAt(xpos,'G');
		break;
	case TOOL_SALIDAHORZ:
		if (xpos==0) return;
		map[ypos].SetAt(xpos,'S');
		break;
	case TOOL_SALIDAVERT:
		if (ypos==0) return;
		map[ypos].SetAt(xpos,'T');
		break;
	case TOOL_ENTRADA:
		if ((xpos==0) || (ypos==0)) return;
		map[ypos].SetAt(xpos,'E');
		break;
	case TOOL_GOBLINSHORZ:
		if ((xpos<5) || (xpos>35) || (ypos==0)) return;
		map[ypos].SetAt(xpos,'H');
		map[ypos].SetAt(xpos-1,'%');
		map[ypos].SetAt(xpos-2,'%');
		map[ypos].SetAt(xpos-3,'%');
		map[ypos].SetAt(xpos-4,'%');
		map[ypos].SetAt(xpos-5,'%');
		map[ypos].SetAt(xpos+1,'%');
		map[ypos].SetAt(xpos+2,'%');
		map[ypos].SetAt(xpos+3,'%');
		map[ypos].SetAt(xpos+4,'%');
		map[ypos-1].SetAt(xpos,'%');
		map[ypos-1].SetAt(xpos-1,'%');
		map[ypos-1].SetAt(xpos-2,'%');
		map[ypos-1].SetAt(xpos-3,'%');
		map[ypos-1].SetAt(xpos-4,'%');
		map[ypos-1].SetAt(xpos-5,'%');
		map[ypos-1].SetAt(xpos+1,'%');
		map[ypos-1].SetAt(xpos+2,'%');
		map[ypos-1].SetAt(xpos+3,'%');
		map[ypos-1].SetAt(xpos+4,'%');
		break;
	case TOOL_GOBLINSVERT:
		if ((ypos<5) || (ypos>23) || (xpos==0)) return;
		map[ypos].SetAt(xpos,'V');
		map[ypos-1].SetAt(xpos,'%');
		map[ypos-2].SetAt(xpos,'%');
		map[ypos-3].SetAt(xpos,'%');
		map[ypos-4].SetAt(xpos,'%');
		map[ypos-5].SetAt(xpos,'%');
		map[ypos+1].SetAt(xpos,'%');
		map[ypos+2].SetAt(xpos,'%');
		map[ypos+3].SetAt(xpos,'%');
		map[ypos+4].SetAt(xpos,'%');
		map[ypos].SetAt(xpos-1,'%');
		map[ypos-1].SetAt(xpos-1,'%');
		map[ypos-2].SetAt(xpos-1,'%');
		map[ypos-3].SetAt(xpos-1,'%');
		map[ypos-4].SetAt(xpos-1,'%');
		map[ypos-5].SetAt(xpos-1,'%');
		map[ypos+1].SetAt(xpos-1,'%');
		map[ypos+2].SetAt(xpos-1,'%');
		map[ypos+3].SetAt(xpos-1,'%');
		map[ypos+4].SetAt(xpos-1,'%');
		break;
	}
}

void CMapeditView::OnRButtonDown(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	
	CMapeditDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	
	MouseRtPress=TRUE;
	if ((xpos<0) || (xpos>39) || (ypos<0) || (ypos>27)) return;
	ApplyCursor(pDoc->Map);
	InvalidateRect(NULL,FALSE);

	CView::OnRButtonDown(nFlags, point);
}

void CMapeditView::OnRButtonUp(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	
	MouseRtPress=FALSE;
	CView::OnRButtonUp(nFlags, point);
}
