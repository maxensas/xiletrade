// Xiletrade.DatToJson - GPL v3
//
// Exposes, on top of ooz, the Oodle functions that libGGPK3 (LibBundle3.Oodle) calls to READ bundles,
// with the same names and signatures. libGGPK3 is then used unchanged: the .NET side only redirects
// its "oo2core" library to this one.
//
// Compression functions are not provided: the tool never writes game files.

#include <cstddef>
#include <cstdint>
#include <cstring>
#include <vector>

#if defined(_WIN32)
#define SHIM_EXPORT __declspec(dllexport)
#else
#define SHIM_EXPORT __attribute__((visibility("default")))
#endif

// Implemented in ooz (kraken.cpp). Decodes a whole compressed stream.
// Returns the number of bytes written, or -1 on failure.
int Kraken_Decompress(const uint8_t* src, size_t src_len, uint8_t* dst, size_t dst_len);

namespace
{
    // ooz may write up to 64 bytes after the end of its output buffer.
    // libGGPK3 decodes chunks straight into its final buffer, so decoding is done in a slightly larger
    // buffer, one per thread, then copied: nothing is ever written outside the caller's buffer.
    constexpr size_t SafeSpace = 64;

    // Room for the compressed data of a chunk: a compressed chunk can be a little larger than the raw one,
    // and ooz may read a few bytes after the end of its input.
    constexpr intptr_t CompressedPadding = 64 * 1024;
}

extern "C"
{
    // Same signature as OodleLZ_Decompress. Only the first four parameters are used.
    // Returns the number of decompressed bytes, 0 on failure (as Oodle does).
    SHIM_EXPORT intptr_t OodleLZ_Decompress(
        const uint8_t* compressed, intptr_t compressedSize, uint8_t* output, intptr_t outputSize,
        int /*fuzzSafe*/, int /*checkCRC*/, int /*verbosity*/,
        uint8_t* /*dictionaryBase*/, intptr_t /*dictionarySize*/,
        void* /*callback*/, void* /*callbackUserData*/,
        void* /*decoderMemory*/, intptr_t /*decoderMemorySize*/,
        int /*threadPhase*/)
    {
        if (compressed == nullptr || output == nullptr || compressedSize <= 0 || outputSize <= 0)
        {
            return 0;
        }

        thread_local std::vector<uint8_t> buffer;
        const size_t needed = static_cast<size_t>(outputSize) + SafeSpace;
        if (buffer.size() < needed)
        {
            buffer.resize(needed);
        }

        const int written = Kraken_Decompress(compressed, static_cast<size_t>(compressedSize),
            buffer.data(), static_cast<size_t>(outputSize));
        if (written != outputSize)
        {
            return 0;
        }

        std::memcpy(output, buffer.data(), static_cast<size_t>(outputSize));
        return outputSize;
    }

    // ooz allocates its own decoder state: libGGPK3 does not need to give it any memory.
    SHIM_EXPORT int OodleLZDecoder_MemorySizeNeeded(int /*compressor*/, intptr_t /*rawSize*/)
    {
        return 0;
    }

    // Size of the buffer libGGPK3 rents to read one compressed chunk.
    SHIM_EXPORT intptr_t OodleLZ_GetCompressedBufferSizeNeeded(int /*compressor*/, intptr_t rawSize)
    {
        return rawSize + rawSize / 8 + CompressedPadding;
    }
}
