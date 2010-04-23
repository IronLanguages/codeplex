#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

# -----------------------------------------------------------------------------
# -- Initial Setup

if ($env:DLR_ROOT -eq $null) {
	echo "DLR_ROOT is not set.  Cannot continue!"
	exit 1
}

. "$env:DLR_ROOT\Test\Scripts\install_dlrcom.ps1"
if (! $?) {
	echo "Failed to source and run "$env:DLR_ROOT\Test\Scripts\install_dlrcom.ps1".  Cannot continue!"
	exit 1
}

if ("$env:ROWAN_BIN" -eq "") {
	log-critical "ROWAN_BIN is not set.  Cannot continue!"
}

if (! (test-path $env:ROWAN_BIN\ipy.exe)) {
	log-critical "$env:ROWAN_BIN\ipy.exe does not exist.  Cannot continue!"
}
set-alias rowipy $env:DLR_ROOT\Languages\IronPython\Internal\ipy.bat



# ------------------------------------------------------------------------------
# -- Run the test

pushd $env:DLR_ROOT\Languages\IronPython\Tests

log-info "Running the following COM test: $PWD> $env:ROWAN_BIN\ipy.exe $args"
rowipy $args[0].Split(" ")
$EC = $LASTEXITCODE

popd
exit $EC