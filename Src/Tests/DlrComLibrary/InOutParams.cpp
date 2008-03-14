// InOutParams.cpp : Implementation of CInOutParams

#include "stdafx.h"
#include "InOutParams.h"
#include <comutil.h>
#pragma comment(lib, "comsupp.lib")

// CInOutParams
int CInOutParams::s_cConstructed;
int CInOutParams::s_cReleased;


STDMETHODIMP CInOutParams::mBstr(BSTR* a)
{
	if(a!=NULL) 
	{
		_bstr_t temp = *a;
		_bstr_t newVal = L"a";
		SysFreeString(*a);
		*a = SysAllocString(temp + newVal);
	}

	return S_OK;
}

STDMETHODIMP CInOutParams::mByte(BYTE* a)
{
	*a += 2;
	return S_OK;
}

STDMETHODIMP CInOutParams::mDouble(DOUBLE *a)
{
	*a += 2;
	return S_OK;
}

STDMETHODIMP CInOutParams::mTwoInOutParams(DOUBLE* a, DOUBLE* b)
{	
	*b += *a + 2;
	*a += 2;
	return S_OK;
}

STDMETHODIMP CInOutParams::mInAndInOutParams(CY a, CY* b)
{
	*b = a;
	return S_OK;
}

STDMETHODIMP CInOutParams::mOutAndInOutParams(DATE* a, DATE* b)
{
	*b = *a;
	return S_OK;
}

STDMETHODIMP CInOutParams::mIDispatch(IDispatch** a)
{
	return S_OK;
}
STDMETHODIMP CInOutParams::mSingleRefParam(DOUBLE* a)
{
	*a += 2;
	return S_OK;
}

STDMETHODIMP CInOutParams::mTwoRefParams(BSTR* a, IDispatch** b)
{
	return S_OK;
}