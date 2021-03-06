#include "Logger.h"
#include <stdio.h>

#define DEFAULT_LOG_FILE_NAME_FORMAT L"dd_NetCoreCheck_%I64u.log"

extern Logger* g_log;

void Logger::CreateLog(LPCWSTR pwszLogFile)
{
    if (pwszLogFile)
    {
        g_log = new Logger(pwszLogFile);
    }
    else
    {
        g_log = new Logger();
    }
}

void Logger::CloseLog()
{
    if (g_log)
    {
        delete g_log;
    }
}

Logger::Logger() throw() : m_file(NULL)
{
    WCHAR logFilePath[MAX_PATH];

    // Use default log-file path under %TEMP%.
    DWORD len = ::GetTempPath(MAX_PATH, logFilePath);
    if (len != 0)
    {
        if (logFilePath[len - 1] != L'\\')
        {
            ::_tcscat_s(logFilePath, MAX_PATH, TEXT("\\"));
        }

        WCHAR fileName[MAX_PATH];
        ::_stprintf_s(fileName, MAX_PATH, DEFAULT_LOG_FILE_NAME_FORMAT, GetTickCount64());
        ::_tcscat_s(logFilePath, MAX_PATH, fileName);

        Initialize(logFilePath);
    }
}

Logger::Logger(LPCWSTR filePath) throw() : m_file(NULL)
{
    // If log path was passed in as a parameter, use it
    Initialize(filePath);
}

void Logger::Initialize(LPCWSTR filePath)
{
    ::_tfopen_s(&m_file, (LPCWSTR)filePath, L"a+");
    Log(L"============= NetCoreCheck Start ===============");
}

Logger::~Logger(void) noexcept
{
    Log(L"=============  NetCoreCheck End  ===============");
    if (m_file)
    {
        ::fclose(m_file);
        m_file = NULL;
    }
}

void Logger::Log(LPCWSTR format, ...) const throw()
{
    if (!m_file)
    {
        // Instead of having the tool fail if we were unable to create the
        // log file we'll just have all logging calls silently fail.
        return;
    }

    // Start the log line with a timestamp
    const size_t dateTimeCharCount = 12;
    WCHAR date[dateTimeCharCount] = { TEXT('\0') };
    WCHAR time[dateTimeCharCount] = { TEXT('\0') };
    ::_tstrdate_s(date, dateTimeCharCount);
    ::_tstrtime_s(time, dateTimeCharCount);
    ::_ftprintf(m_file, TEXT("[%s,%s] "), date, time);

    // Then add the formatted message
    va_list args;
    va_start(args, format);
    ::_vftprintf_p(m_file, format, args);
    va_end(args);

    // Finally, start a new line and flush the buffer
    ::_ftprintf(m_file, TEXT("\n"));
    ::fflush(m_file);
}

