# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
# See the LICENSE file in the project root for more information.

project(SampleCppApp)
set(DOTNET_PROJECT_NAME "SampleCppApp")

set(CMAKE_CONFIGURATION_TYPES "Debug;Release"
    CACHE STRING "Configuration types" FORCE)

set(SKIP_VERSIONING 1)

set(SOURCES
    ./SampleCppApp.cpp
)

include(../exe.cmake)

add_definitions(-DFEATURE_APPHOST=1)

# Disable manifest generation into the file .exe on Windows
if(CLR_CMAKE_TARGET_WIN32)
    set_property(TARGET ${PROJECT_NAME} PROPERTY
            LINK_FLAGS "/MANIFEST:NO"
        )
endif()

