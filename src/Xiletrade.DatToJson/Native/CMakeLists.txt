# Builds the "ooz" native library used to decompress the game bundles.
#
# ooz is an open source implementation of the Oodle decompressors (Kraken, Mermaid, Leviathan...),
# licensed under GPL v3. Its sources are not copied in this repository: they are downloaded at configure time,
# at a pinned commit, with its simde submodule (MIT, portable SIMD intrinsics).
#
# oodle_shim.cpp exposes, on top of ooz, the three Oodle functions that libGGPK3 calls to read bundles.
#
# Built by Xiletrade.DatToJson.csproj into its bin/ooz folder, nothing is written in this source folder:
#   cmake -S <this folder> -B <project>/bin/ooz -DCMAKE_BUILD_TYPE=Release
#   cmake --build <project>/bin/ooz --config Release
# The library is written to <build folder>/lib (ooz.dll on Windows, libooz.so on Linux).

cmake_minimum_required(VERSION 3.20)
project(xiletrade_ooz CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

include(FetchContent)

# Sources are downloaded once at the pinned commit, then never updated.
set(FETCHCONTENT_UPDATES_DISCONNECTED ON)

# https://github.com/zao/ooz (maintained, portable fork of https://github.com/powzix/ooz).
# Its simde submodule is fetched with it, at the commit ooz pins.
FetchContent_Declare(ooz
    GIT_REPOSITORY https://github.com/zao/ooz.git
    GIT_TAG ff5aeb9e45e362e8d6bb1199aa82406285dd2a18
    GIT_SUBMODULES simde
    SOURCE_SUBDIR sources-only)

# Only the sources are needed: SOURCE_SUBDIR points to a folder without CMakeLists.txt,
# so the projects of ooz itself are not added to the build.
FetchContent_MakeAvailable(ooz)

# Decompression only: no compressor, no command line tool.
add_library(ooz SHARED
    ${ooz_SOURCE_DIR}/kraken.cpp
    ${ooz_SOURCE_DIR}/lzna.cpp
    ${ooz_SOURCE_DIR}/bitknit.cpp
    oodle_shim.cpp)

# OOZ_BUILD_DLL removes the command line entry point of kraken.cpp.
target_compile_definitions(ooz PRIVATE OOZ_BUILD_DLL=1 OOZ_DYNAMIC=1)
target_include_directories(ooz PRIVATE ${ooz_SOURCE_DIR} ${ooz_SOURCE_DIR}/simde)

if(MSVC)
    target_compile_options(ooz PRIVATE /O2 /W0)
else()
    target_compile_options(ooz PRIVATE -O2 -w)
endif()

# Same file name on every configuration, in the build folder.
set(OOZ_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set_target_properties(ooz PROPERTIES
    OUTPUT_NAME ooz
    RUNTIME_OUTPUT_DIRECTORY ${OOZ_OUTPUT_DIRECTORY}
    RUNTIME_OUTPUT_DIRECTORY_RELEASE ${OOZ_OUTPUT_DIRECTORY}
    RUNTIME_OUTPUT_DIRECTORY_DEBUG ${OOZ_OUTPUT_DIRECTORY}
    LIBRARY_OUTPUT_DIRECTORY ${OOZ_OUTPUT_DIRECTORY}
    LIBRARY_OUTPUT_DIRECTORY_RELEASE ${OOZ_OUTPUT_DIRECTORY}
    LIBRARY_OUTPUT_DIRECTORY_DEBUG ${OOZ_OUTPUT_DIRECTORY})
