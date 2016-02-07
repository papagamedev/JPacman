// Mapedit.h : main header file for the MAPEDIT application
//

#if !defined(AFX_MAPEDIT_H__F4339A39_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_)
#define AFX_MAPEDIT_H__F4339A39_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CMapeditApp:
// See Mapedit.cpp for the implementation of this class
//

class CMapeditApp : public CWinApp
{
public:
	CMapeditApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMapeditApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation
	//{{AFX_MSG(CMapeditApp)
	afx_msg void OnAppAbout();
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MAPEDIT_H__F4339A39_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_)
