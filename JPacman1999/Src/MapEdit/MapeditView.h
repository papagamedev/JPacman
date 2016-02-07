// MapeditView.h : interface of the CMapeditView class
//
/////////////////////////////////////////////////////////////////////////////

#if !defined(AFX_MAPEDITVIEW_H__F4339A41_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_)
#define AFX_MAPEDITVIEW_H__F4339A41_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

extern int CurrentTool;
extern CMainFrame *FrameWnd;


class CMapeditView : public CView
{
protected: // create from serialization only
	CMapeditView();
	DECLARE_DYNCREATE(CMapeditView)

// Attributes
public:
	CMapeditDoc* GetDocument();

// Operations
public:
	int xpos,ypos;
	BOOL MousePress;
	BOOL MouseRtPress;
	CString MapView[28];

	void ApplyCursor(CString map[]);

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMapeditView)
	public:
	virtual void OnDraw(CDC* pDC);  // overridden to draw this view
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	protected:
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CMapeditView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	//{{AFX_MSG(CMapeditView)
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnRButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnRButtonUp(UINT nFlags, CPoint point);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#define TOOL_SELECT	0
#define TOOL_CAMINO	1
#define TOOL_PUNTOS	2
#define TOOL_PARED	3
#define TOOL_FRUTA	4
#define TOOL_LETRAS	5
#define TOOL_PACMAN 6
#define TOOL_GALLETAHORZ	7
#define TOOL_GALLETAVERT	8
#define TOOL_GOBLINSHORZ	9
#define TOOL_GOBLINSVERT	10
#define	TOOL_SALIDAHORZ	11
#define TOOL_SALIDAVERT	12
#define TOOL_ENTRADA 13

#ifndef _DEBUG  // debug version in MapeditView.cpp
inline CMapeditDoc* CMapeditView::GetDocument()
   { return (CMapeditDoc*)m_pDocument; }
#endif

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MAPEDITVIEW_H__F4339A41_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_)
