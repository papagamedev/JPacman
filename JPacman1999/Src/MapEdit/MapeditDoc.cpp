// MapeditDoc.cpp : implementation of the CMapeditDoc class
//

#include "stdafx.h"
#include "Mapedit.h"

#include "MapeditDoc.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CMapeditDoc

IMPLEMENT_DYNCREATE(CMapeditDoc, CDocument)

BEGIN_MESSAGE_MAP(CMapeditDoc, CDocument)
	//{{AFX_MSG_MAP(CMapeditDoc)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CMapeditDoc construction/destruction

CMapeditDoc::CMapeditDoc()
{
	// TODO: add one-time construction code here

}

CMapeditDoc::~CMapeditDoc()
{
}

BOOL CMapeditDoc::OnNewDocument()
{
	if (!CDocument::OnNewDocument())
		return FALSE;

	for (int i = 0; i < Level::Height; i++)
	{
		Map[i] = "                                        ";
	}

	return TRUE;
}



/////////////////////////////////////////////////////////////////////////////
// CMapeditDoc serialization

void CMapeditDoc::Serialize(CArchive& ar)
{
	if (ar.IsStoring())
	{
		for (int i=0;i<Level::Height;i++)
			ar << Map[i];
	}
	else
	{
		for (int i=0;i<Level::Height;i++)
			ar >> Map[i];
	}
}

/////////////////////////////////////////////////////////////////////////////
// CMapeditDoc diagnostics

#ifdef _DEBUG
void CMapeditDoc::AssertValid() const
{
	CDocument::AssertValid();
}

void CMapeditDoc::Dump(CDumpContext& dc) const
{
	CDocument::Dump(dc);
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CMapeditDoc commands
