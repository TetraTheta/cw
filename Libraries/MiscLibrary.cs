using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace cw.Libraries {
  public class MiscLibrary {
    public static bool CheckCwebp() {
      string[] paths = Environment.GetEnvironmentVariable("PATH").Split(';');
      foreach (string path in paths) {
        string fullPath = Path.Combine(path, "cwebp.exe");
        if (File.Exists(fullPath)) {
          return true;
        }
      }
      return false;
    }

    public static bool CheckMagick() {
      string[] paths = Environment.GetEnvironmentVariable("PATH").Split(';');
      foreach (string path in paths) {
        string fullPath = Path.Combine(path, "magick.exe");
        if (File.Exists(fullPath)) {
          return true;
        }
      }
      return false;
    }

    public static string[] GetImageFileArray(string path) {
      if (File.Exists(path)) {
        return new string[] { Path.GetFullPath(path) };
      } else if (Directory.Exists(path)) {
        string[] extensions = { ".bmp", ".jpg", ".jpeg", ".png", ".webp" };
        return NaturalSort.Sort(Directory.GetFiles(path).Where(file => extensions.Contains(Path.GetExtension(file).ToLower())).ToArray());
      } else {
        Console.Error.WriteLine("\u001b[31mERROR\u001b[0m The specified path does not exist: '" + path + "'");
        Environment.Exit(1);
        return null;
      }
    }

    public static int GetImageWidth(string path) {
      // Start magick
      ProcessStartInfo startInfo = new ProcessStartInfo {
        FileName = "magick.exe",
        Arguments = $"identify -ping -format \"%w\" \"{path}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };
      Process process = Process.Start(startInfo);
      _ = process.Start();
      process.WaitForExit();
      string output = process.StandardOutput.ReadLine();
      // Check error
      if (process.ExitCode != 0 || string.IsNullOrEmpty(output)) {
        Console.Error.WriteLine("\x1b[31mERROR\x1b[0m Failed to get width of image: '" + path + "'");
        Console.Error.WriteLine(process.StandardError.ReadToEnd());
        Environment.Exit(1);
        return 0;
      }
      // Convert output to int
      int result = int.Parse(output);
      return result;
    }

    public static string GetParentDirectory(string path) {
      if (File.Exists(path)) {
        return Path.GetDirectoryName(path);
      } else if (Directory.Exists(path)) {
        return Path.GetFullPath(path);
      } else {
        Console.Error.WriteLine("\u001b[31mERROR\u001b[0m The specified path does not exist: '" + path + "'");
        Environment.Exit(1);
        return null;
      }
    }
  }
}
