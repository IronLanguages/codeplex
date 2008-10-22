// OutParams.h : Declaration of the COutParams

#pragma once
#include "resource.h"       // main symbols

#include "DlrComLibrary_i.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// COutParams

class ATL_NO_VTABLE COutParams :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<COutParams, &CLSID_OutParams>,
	public ISupportErrorInfo,
	public IDispatchImpl<IOutParams, &IID_IOutParams, &LIBID_DlrComLibraryLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	COutParams()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_OUTPARAMS)


BEGIN_COM_MAP(COutParams)
	COM_INTERFACE_ENTRY(IOutParams)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);


	DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct()
    {
        s_cConstructed++;

        return S_OK;
    }

    void FinalRelease()
    {
        s_cReleased++;
    }

public:

	STDMETHOD(mBstr)(BSTR a, BSTR* b);
	STDMETHOD(mByte)(BYTE a, BYTE* b);
	STDMETHOD(mChar)(CHAR a, CHAR* b);
	STDMETHOD(mCy)(CY a, CY* b);
	STDMETHOD(mDate)(DATE a, DATE* b);
	STDMETHOD(mDouble)(DOUBLE a, DOUBLE* b);
	STDMETHOD(mFloat)(FLOAT a, FLOAT* b);
	STDMETHOD(mIDispatch)(IDispatch* a, IDispatch** b);
	STDMETHOD(mIFontDisp)(IFontDisp* a, IDispatch** b);
	STDMETHOD(mIPictureDisp)(IPictureDisp* a, IDispatch** b);
	STDMETHOD(mIUnknown)(IUnknown* a, IUnknown** b);
	STDMETHOD(mLong)(LONG a, LONG* b);
	STDMETHOD(mLongLong)(LONGLONG a, LONGLONG* b);
	STDMETHOD(mOleColor)(OLE_COLOR a, OLE_COLOR* b);
	STDMETHOD(mOleXposHimetric)(OLE_XPOS_HIMETRIC a, OLE_XPOS_HIMETRIC* b);
	STDMETHOD(mOleYposHimetric)(OLE_YPOS_HIMETRIC a, OLE_YPOS_HIMETRIC* b);
	STDMETHOD(mOleXsizeHimetric)(OLE_XSIZE_HIMETRIC a, OLE_XSIZE_HIMETRIC* b);
	STDMETHOD(mOleYsizeHimetric)(OLE_YSIZE_HIMETRIC a, OLE_YSIZE_HIMETRIC* b);
	STDMETHOD(mOleXposPixels)(OLE_XPOS_PIXELS a, OLE_XPOS_PIXELS* b);
	STDMETHOD(mOleYposPixels)(OLE_YPOS_PIXELS a, OLE_YPOS_PIXELS* b);
	STDMETHOD(mOleXsizePixels)(OLE_XSIZE_PIXELS a, OLE_XSIZE_PIXELS* b);
	STDMETHOD(mOleYsizePixels)(OLE_YSIZE_PIXELS a, OLE_YSIZE_PIXELS* b);
	STDMETHOD(mOleHandle)(OLE_HANDLE a, OLE_HANDLE* b);
	STDMETHOD(mOleOptExclusive)(OLE_OPTEXCLUSIVE a, OLE_OPTEXCLUSIVE* b);
	STDMETHOD(mOleTristate)(enum OLE_TRISTATE a, enum OLE_TRISTATE* b);
	STDMETHOD(mScode)(SCODE a, SCODE* b);
	STDMETHOD(mShort)(SHORT a, SHORT* b);
	STDMETHOD(mUlong)(ULONG a, ULONG* b);
	STDMETHOD(mULongLong)(ULONGLONG a, ULONGLONG* ab);
	STDMETHOD(mUShort)(USHORT a, USHORT* b);
	STDMETHOD(mVariant)(VARIANT a, VARIANT* b);
	STDMETHOD(mVariantBool)(VARIANT_BOOL a, VARIANT_BOOL* b);

	static BOOL AllDestructed() { return s_cConstructed == s_cReleased; }

private:
    static int s_cConstructed;
    static int s_cReleased;
};

OBJECT_ENTRY_AUTO(__uuidof(OutParams), COutParams)
