// ParamsIn.cpp : Implementation of CParamsIn

#include "stdafx.h"
#include "ParamsInRetval.h"

int CParamsInRetval::s_cConstructed;
int CParamsInRetval::s_cReleased;
// CParamsIn


STDMETHODIMP CParamsInRetval::mBstr(BSTR a, BSTR* b)
{
	if(a==NULL) {
		*b = a;
	}
	else {
		*b = SysAllocString(a);
	}

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mByte(BYTE a, BYTE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mChar(CHAR a, CHAR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mCy(CY a, CY* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mDate(DATE a, DATE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mDouble(DOUBLE a, DOUBLE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mFloat(FLOAT a, FLOAT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mIDispatch(IDispatch* a, IDispatch** b)
{
	*b = *&a;
	if (a!=NULL) {
		a->AddRef();
	}
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mIFontDisp(IFontDisp* a, IDispatch** b)
{
	*b = *&a;
	if (a!=NULL) {
		a->AddRef();
	}
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mIPictureDisp(IPictureDisp* a, IDispatch** b)
{
	*b = *&a;
	if (a!=NULL) {
		a->AddRef();
	}
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mIUnknown(IUnknown* a, IUnknown** b)
{
	*b = *&a;
	if (a!=NULL) {
		a->AddRef();
	}
	return S_OK;
}

STDMETHODIMP CParamsInRetval::mLong(LONG a, LONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mLongLong(LONGLONG a, LONGLONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleColor(OLE_COLOR a, OLE_COLOR* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleXposHimetric(OLE_XPOS_HIMETRIC a, OLE_XPOS_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleYposHimetric(OLE_YPOS_HIMETRIC a, OLE_YPOS_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleXsizeHimetric(OLE_XSIZE_HIMETRIC a, OLE_XSIZE_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleYsizeHimetric(OLE_YSIZE_HIMETRIC a, OLE_YSIZE_HIMETRIC* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleXposPixels(OLE_XPOS_PIXELS a, OLE_XPOS_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleYposPixels(OLE_YPOS_PIXELS a, OLE_YPOS_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleXsizePixels(OLE_XSIZE_PIXELS a, OLE_XSIZE_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleYsizePixels(OLE_YSIZE_PIXELS a, OLE_YSIZE_PIXELS* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleHandle(OLE_HANDLE a, OLE_HANDLE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleOptExclusive(OLE_OPTEXCLUSIVE a, OLE_OPTEXCLUSIVE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mOleTristate(enum OLE_TRISTATE a, enum OLE_TRISTATE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mScode(SCODE a, SCODE* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mShort(SHORT a, SHORT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mUlong(ULONG a, ULONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mULongLong(ULONGLONG a, ULONGLONG* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mUShort(USHORT a, USHORT* b)
{
	*b = a;

	return S_OK;
}

STDMETHODIMP CParamsInRetval::mVariant(VARIANT a, VARIANT* b)
{
	return VariantCopy(b, &a);
}

STDMETHODIMP CParamsInRetval::mVariantBool(VARIANT_BOOL a, VARIANT_BOOL* b)
{
	*b = a;

	return S_OK;
}

