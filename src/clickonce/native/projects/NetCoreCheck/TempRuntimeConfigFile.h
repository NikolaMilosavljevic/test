#pragma once

#include "NetCoreCheck.h"

DWORD GetTempRuntimeConfigPath(LPWSTR runtimeConfigPath);
DWORD CreateTempRuntimeConfigFile(LPCWSTR runtimeConfigPath, LPCWSTR frameworkName, LPCWSTR frameworkVersion);
