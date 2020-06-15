#pragma once

#include <Windows.h>

// Return Codes
#define EXIT_SUCCESS                0  // Required runtime is installed
#define EXIT_FAILURE_LOADHOSTFXR    1  // No runtime is installed
#define EXIT_FAILURE_INITHOSTFXR    2  // Required runtime is not installed
#define EXIT_FAILURE_HOSTFXREXPORTS 3  // Failed to get hostfxr exports
#define EXIT_FAILURE_INVALIDARGS    4  // Invalid Arguments
#define EXIT_FAILURE_TEMPRTJSONPATH 5  // Failed to construct temp json file path
#define EXIT_FAILURE_TEMPRTJSONFile 6  // Failed to create temp json file