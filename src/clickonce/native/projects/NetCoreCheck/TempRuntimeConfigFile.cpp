#include "TempRuntimeConfigFile.h"
#include "Logger.h"
#include "shlwapi.h"

#define RUNTIMECONFIG_FORMAT_STR TEXT("{ \"runtimeOptions\": { \"framework\": { \"name\": \"%s\", \"version\": \"%s\" } } }")

extern Logger* g_log;
DWORD WriteFile(LPCWSTR filePath, LPCWSTR fileText);

DWORD GetTempRuntimeConfigPath(LPWSTR runtimeConfigPath)
{
    if (!GetModuleFileName(NULL, runtimeConfigPath, MAX_PATH))
    {
        g_log->Log(TEXT("Couldn't get module name."));
        return ::GetLastError();
    }

    PathRemoveFileSpec(runtimeConfigPath);
    if (!PathAppend(runtimeConfigPath, TEXT("Test.runtimeconfig.json")))
    {
        g_log->Log(TEXT("Couldn't append file."));
        return EXIT_FAILURE_TEMPRTJSONPATH;
    }

    g_log->Log(TEXT("Temporary runtime config file path: '%s'."), runtimeConfigPath);
    return 0;
}

DWORD CreateTempRuntimeConfigFile(LPCWSTR runtimeConfigPath, LPCWSTR frameworkName, LPCWSTR frameworkVersion)
{
    if (PathFileExists(runtimeConfigPath))
    {
        if (!DeleteFile(runtimeConfigPath))
        {
            g_log->Log(TEXT("Failed to delete existing file '%s'."), runtimeConfigPath);
            return EXIT_FAILURE_TEMPRTJSONFile;
        }
    }

    WCHAR fileText[MAX_PATH];
    if (swprintf_s(fileText, MAX_PATH, RUNTIMECONFIG_FORMAT_STR, frameworkName, frameworkVersion) <= 0)
    {
        g_log->Log(TEXT("Failed to format file text."));
        return EXIT_FAILURE_TEMPRTJSONFile;
    }

    g_log->Log(TEXT("Temp runtime config file text: '%s'."), fileText);
    return WriteFile(runtimeConfigPath, fileText);
}

DWORD WriteFile(LPCWSTR filePath, LPCWSTR fileText)
{
    int numBytes, textLength;
    DWORD bytesWritten;
    DWORD ret = 0;
    CHAR* fileContents = NULL;
    HANDLE file = INVALID_HANDLE_VALUE;

    file = CreateFile(filePath, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (file == INVALID_HANDLE_VALUE)
    {
        ret = ::GetLastError();
        g_log->Log(TEXT("CreateFile failed: '%d'."), ret);
        goto exit;
    }

    textLength = lstrlenW(fileText) + 1;
    fileContents = new CHAR[textLength];
    numBytes = WideCharToMultiByte(CP_ACP, 0, fileText, textLength, fileContents, textLength, NULL, NULL);
    if (numBytes == 0)
    {
        ret = ::GetLastError();
        g_log->Log(TEXT("WideCharToMultiByte failed: '%d'."), ret);
        goto exit;
    }

    if (!WriteFile(file, (LPCVOID)fileContents, numBytes, &bytesWritten, NULL))
    {
        ret = ::GetLastError();
        g_log->Log(TEXT("WriteFile failed: '%d'."), ret);
        goto exit;
    }

exit:
    if (fileContents != NULL)
    {
        delete[] fileContents;
    }

    if (file != INVALID_HANDLE_VALUE)
    {
        CloseHandle(file);
    }

    return ret;
}