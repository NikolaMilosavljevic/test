#pragma once

#include "NetCoreCheck.h"
#include <tchar.h>

class Logger
{
public:
    void Log(LPCWSTR pszFormat, ...) const throw();

    // Static methods to create and close the logger
    static void CreateLog(LPCWSTR logFilePath);
    static void CloseLog();

private:
    Logger(void) throw();
    Logger(LPCWSTR logFilePath) throw();
    ~Logger(void) noexcept;

    void Initialize(LPCWSTR logFilePath);
    FILE* m_file;
};