using CommandLine;
using CommandLine.Text;
using cw.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace cw {
  internal class Program {
    public class Options {
      public Options(string input, int width) {
        Input = string.IsNullOrEmpty(input) ? Directory.GetCurrentDirectory() : input;
        Width = width <= 0 ? 1280 : width;
      }
      public Options(string input) {
        Input = string.IsNullOrEmpty(input) ? Directory.GetCurrentDirectory() : input;
        Width = 1280;
      }

      [Value(0, MetaValue = "FILE|DIR", HelpText = "File or directory to search and process image files.")]
      public string Input { get; } = Directory.GetCurrentDirectory();
      [Option('w', "width", Default = 1280, HelpText = "Width of output WebP image in pixel.\nWill set to 1280 if 0 or negative value is provided.", MetaValue = "POSITIVE_INTEGER", Required = false)]
      public int Width { get; } = 1280;

      [Usage(ApplicationAlias = "cw.exe")]
      public static IEnumerable<Example> Examples => new List<Example>() {
            new Example("Convert all image files in the directory to 1280px-width WebP images", new Options(Directory.GetCurrentDirectory(), 1280))
          };
    }

    private static void Main(string[] args) {
      Parser parser = new Parser(config => config.HelpWriter = Console.Out);

      _ = parser.ParseArguments<Options>(args).WithParsed(Run).WithNotParsed(HandleParseError);
    }

    private static void HandleParseError(IEnumerable<Error> e) {
      if (e.IsVersion() || e.IsHelp()) {
        return;
      }

      Console.Error.WriteLine("\x1b[31mERROR\x1b[0m Failed to parse arguments.");
    }

    private static void Run(Options opts) {
      // Abort program if cwebp and magick is not found
      if (!MiscLibrary.CheckCwebp()) {
        Console.Error.WriteLine("\u001b[31mERROR\u001b[0m 'cwebp.exe' is not found. Aborting...");
      }
      if (!MiscLibrary.CheckMagick()) {
        Console.Error.WriteLine("\u001b[31mERROR\u001b[0m 'magick.exe' is not found. Aborting...");
      }
      // Create 'converted' subdirectory if not exists
      string convertedPath = Path.Combine(MiscLibrary.GetParentDirectory(opts.Input), "converted");
      if (!Directory.Exists(convertedPath)) {
        _ = Directory.CreateDirectory(convertedPath);
      }
      // Check file list is empty
      string[] files = MiscLibrary.GetImageFileArray(opts.Input);
      if (files.Length == 0) {
        Console.WriteLine("\u001b[33mWARN\u001b[0m There is no image file to process.");
      }
      // Convert image files
      foreach ((string file, int idx) in files.Select((file, idx) => (file, idx))) {
        string[] argument = MiscLibrary.GetImageWidth(file) <= opts.Width
          ? (new[] {
            "-preset default",
            "-q 85",
            "-m 6",
            "-pass 10",
            "-mt",
            "-quiet",
            $"-o \"{convertedPath}\\{Path.GetFileNameWithoutExtension(file)}.webp\"",
            $"-- \"{file}\""
          })
          : (new[] {
            "-preset default",
            "-q 85",
            "-m 6",
            "-pass 10",
            "-resize 1280 0",
            "-mt",
            "-quiet",
            $"-o \"{convertedPath}\\{Path.GetFileNameWithoutExtension(file)}.webp\"",
            $"-- \"{file}\""
          });
        Console.Title = $"[{idx:D3}/{files.Length:D3}] Processing {Path.GetFileName(file)} ...";
        Console.WriteLine($"[{idx:D3}/{files.Length:D3}] Processing {Path.GetFileName(file)} ...");
        ProcessStartInfo startInfo = new ProcessStartInfo {
          FileName = "cwebp.exe",
          Arguments = string.Join(" ", argument),
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
          CreateNoWindow = true
        };
        Process process = Process.Start(startInfo);
        _ = process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0) {
          Console.Error.WriteLine("\x1b[31mERROR\x1b[0m Failed to process image: '" + file + "'");
          Console.Error.WriteLine(process.StandardError.ReadToEnd());
          _ = Console.ReadKey();
        }
      }
    }
  }
}
