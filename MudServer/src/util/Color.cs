using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameCore.Util {
public static class Color {
    public const string RedD = "\x1b[0;31m";
    public const string Red = "\x1b[1;31m";
    public const string GreenD = "\x1b[0;32m";
    public const string Green = "\x1b[1;32m";
    public const string Gold = "\x1b[0;33m";
    public const string Yellow = "\x1b[1;33m";
    public const string BlueD = "\x1b[0;34m";
    public const string Blue = "\x1b[1;34m";
    public const string Magenta = "\x1b[;35m";
    public const string Cyan = "\x1b[1;36m";
    public const string White = "\x1b[1;37m";
    public const string Reset = "\x1b[0m";
}
}
