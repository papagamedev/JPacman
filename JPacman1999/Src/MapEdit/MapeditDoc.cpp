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

	// TODO: add reinitialization code here
	// (SDI documents will reuse this document)

	int i;

	for (i=0;i<28;i++)
		Map[i]="                                        ";

/*	Map[0]="                                        ";
	Map[1]=" ########$$$$$ ########## $$$$$######## ";
	Map[2]=" ########$E$$c ########## $d$$E######## ";
	Map[3]=" ##            ##  ##  ##            ## ";
	Map[4]=" ################  ##  ################ ";
	Map[5]=" ################  ##  ################ ";
	Map[6]="         ##        ##        ##         ";
	Map[7]=" $###############  ##  ###############$ ";
	Map[8]=" C###############  ##  ###############C ";
	Map[9]="        ## ##  ##  ##  ##  ## ##        ";
	Map[10]=" ######### ##  ##  ##  ##  ## ######### ";
	Map[11]=" ######### ##  ##  ##  ##  ## ######### ";
	Map[12]=" ##        ##  ##  ##  ##  ##        ## ";
	Map[13]=" ## ##$$$$ ## $$$  ##  $$$ ## $$$$## ## ";
	Map[14]=" ## ##$E$e ## $fE  ##  $Eg ## $h$E## ## ";
	Map[15]=" ## ##     ##      ##      ##     ## ## ";
	Map[16]=" ## ## $###########$$###########$ ## ## ";
	Map[17]=" ## ## C###########$P###########C ## ## ";
	Map[18]=" ## ##    ##                ##    ## ## ";
	Map[19]=" ## ##########$$$$$$L$$$$$########## ## ";
	Map[20]=" ## ##########$$$$$$F$$$$$########## ## ";
	Map[21]=" ##         ##            ##         ## ";
	Map[22]=" ###################################### ";
	Map[23]=" ###################################### ";
	Map[24]=" ##                 S                ## ";
	Map[25]=" ########$$$$$ %%%%%%%%%% $$$$$######## ";
	Map[26]=" ########$E$$a %%%%%H%%%% $b$$E######## ";
	Map[27]="                                        ";
*/
	return TRUE;
}



/////////////////////////////////////////////////////////////////////////////
// CMapeditDoc serialization

void CMapeditDoc::Serialize(CArchive& ar)
{
	int i;

	if (ar.IsStoring())
	{
		for (i=0;i<28;i++)
			ar << Map[i];
	}
	else
	{
		for (i=0;i<28;i++)
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
