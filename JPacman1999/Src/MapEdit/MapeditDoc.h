// MapeditDoc.h : interface of the CMapeditDoc class
//
/////////////////////////////////////////////////////////////////////////////

#if !defined(AFX_MAPEDITDOC_H__F4339A3F_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_)
#define AFX_MAPEDITDOC_H__F4339A3F_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000


class CMapeditDoc : public CDocument
{
protected: // create from serialization only
	CMapeditDoc();
	DECLARE_DYNCREATE(CMapeditDoc)

// Attributes
public:

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMapeditDoc)
	public:
	virtual BOOL OnNewDocument();
	virtual void Serialize(CArchive& ar);
	//}}AFX_VIRTUAL

// Implementation
public:
	int Cookies;
	CString Map[28];
	virtual ~CMapeditDoc();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	//{{AFX_MSG(CMapeditDoc)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MAPEDITDOC_H__F4339A3F_4837_11D3_B3B8_B5D1F94DA22F__INCLUDED_)
