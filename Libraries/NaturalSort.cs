﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace cw.Libraries {
  internal class NaturalSort {
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string psz1, string psz2);

    public static string[] Sort(string[] arr) {
      Array.Sort(arr, new NaturalComparer());
      return arr;
    }

    private class NaturalComparer : IComparer<string> {
      public int Compare(string x, string y) {
        return StrCmpLogicalW(x, y);
      }
    }
  }
}
