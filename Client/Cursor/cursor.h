#pragma once
using namespace System;

namespace TerminalUtils {
    public ref class Cursor {
    public:
        static Tuple<int, int>^ GetCursorPosition();
    };
}