// MainFrm.h : interface of the CMainFrame class
//
/////////////////////////////////////////////////////////////////////////////

#if !defined(AFX_MAINFRM_H__F4339A3D_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_)
#define AFX_MAINFRM_H__F4339A3D_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class CMainFrame : public CFrameWnd
{
	
protected: // create from serialization only
	CMainFrame();
	DECLARE_DYNCREATE(CMainFrame)

// Attributes
public:

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMainFrame)
	public:
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	//}}AFX_VIRTUAL

// Implementation
public:
	CStatusBar  m_wndStatusBar;
	virtual ~CMainFrame();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:  // control bar embedded members
	CToolBar    m_wndToolBar;
	CToolBar    m_wndTools;

// Generated message map functions
protected:
	//{{AFX_MSG(CMainFrame)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnHerramientasCamino();
	afx_msg void OnHerramientasPared();
	afx_msg void OnHerramientasPuntos();
	afx_msg void OnHerramientasSeleccionar();
	afx_msg void OnUpdateHerramientasSeleccionar(CCmdUI* pCmdUI);
	afx_msg void OnUpdateHerramientasPuntos(CCmdUI* pCmdUI);
	afx_msg void OnUpdateHerramientasPared(CCmdUI* pCmdUI);
	afx_msg void OnUpdateHerramientasCamino(CCmdUI* pCmdUI);
	afx_msg void OnUpdateViewTools(CCmdUI* pCmdUI);
	afx_msg void OnUpdatePos(CCmdUI* pCmdUI);
	afx_msg void OnViewTools();
	afx_msg void OnHerramientasFruta();
	afx_msg void OnUpdateHerramientasFruta(CCmdUI* pCmdUI);
	afx_msg void OnHerramientasGalletahorz();
	afx_msg void OnUpdateHerramientasGalletahorz(CCmdUI* pCmdUI);
	afx_msg void OnHerramientasGalletavert();
	afx_msg void OnUpdateHerramientasGalletavert(CCmdUI* pCmdUI);
	afx_msg void OnHerramientasGoblinshorz();
	afx_msg void OnUpdateHerramientasGoblinshorz(CCmdUI* pCmdUI);
	afx_msg void OnHerramientasGoblinsvert();
	afx_msg void OnUpdateHerramientasGoblinsvert(CCmdUI* pCmdUI);
	afx_msg void OnHerramientasLetras();
	afx_msg void OnUpdateHerramientasLetras(CCmdUI* pCmdUI);
	afx_msg void OnHerramientasPacman();
	afx_msg void OnUpdateHerramientasPacman(CCmdUI* pCmdUI);
	afx_msg void OnHerramientasSalidahorz();
	afx_msg void OnUpdateHerramientasSalidahorz(CCmdUI* pCmdUI);
	afx_msg void OnHerramientasSalidavert();
	afx_msg void OnUpdateHerramientasSalidavert(CCmdUI* pCmdUI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MAINFRM_H__F4339A3D_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_)
