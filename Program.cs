using Microsoft.Office.Interop.Word;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ConvertWordToPDF {
  class Program {
    static void Main(string[] args) {
      Program p = new Program();
      p.hashtable = p.GetArgs(args);
      p.Execute();
    }

    private Hashtable hashtable;
    private bool verb = false;

    private void Execute() {
      string source = null;
      string dest = null;

      if (this.hashtable.ContainsKey("h")) {
        this.PrintInfo();
        return;
      }

      if (this.hashtable.ContainsKey("v")) {
        verb = true;
      }

      if (this.hashtable.ContainsKey("s")) {
        source = (string)this.hashtable["s"];
      }

      if (this.hashtable.ContainsKey("d")) {
        dest = (string)this.hashtable["d"];
      }

      if (string.IsNullOrEmpty(source)) {
        source = Directory.GetCurrentDirectory();
        PrintVerbose("Set the default source with current folder. Folder:" + source);
      }

      List<string> files = new List<string>();
      if (File.Exists(source)) {
        files.Add(source);
        PrintVerbose("Add the file " + source);
      } else if (Directory.Exists(source)) {
        foreach (string item in Directory.GetFiles(source)) {
          PrintVerbose("Add the file " + item);
          files.Add(item);
        }
      } else {
        Console.WriteLine("Invalid Source File. File:" + source);
        return;
      }

      if (string.IsNullOrEmpty(dest)) {
        dest = source;
        PrintVerbose("Set the default destination with source. Folder:" + dest);
      }

      if (!Directory.Exists(dest)) {
        Directory.CreateDirectory(dest);
      }

      PrintVerbose("Source:      " + source);
      PrintVerbose("Destination: " + dest);
      int count = 0;
      for (int i = 0; i < files.Count; i++) {
        string sourceFile = files[i];
        string extension = this.ToLower(Path.GetExtension(sourceFile));
        if (!".doc".Equals(extension) && !".docx".Equals(extension)) {
          continue;
        }

        string destFile = Path.Combine(dest, Path.GetFileNameWithoutExtension(sourceFile) + ".pdf");
        count++;
        Console.WriteLine("Processing " + sourceFile);
        ConvertToPDF(sourceFile, destFile);
      }

      Console.WriteLine("All file is finished. Count: " + count);
    }

    private void ConvertToPDF(string source, string dest) {
      if (!File.Exists(source)) {
        Console.WriteLine("The file \"{0}\" does not exist.", source);
        return;
      }

      Application application = null;
      Documents documents = null;
      Document doc = null;
      try {
        application = new Application();
        documents = application.Documents;
        doc = application.Documents.Open(source);
        doc.Activate();
        doc.SaveAs2(dest, WdSaveFormat.wdFormatPDF);
        doc.Close();
      } finally {
        SafeRelease(doc);
        SafeRelease(documents);
        SafeRelease(application);
      }
    }

    private Hashtable GetArgs(string[] args) {
      Hashtable hashtable = new Hashtable();
      string key = null;
      for (int i = 0; i < args.Length; i++) {
        string str = args[i];
        if (str.StartsWith("-")) {
          if (key != null) {
            hashtable.Add(key, "");
          }

          key = str.Substring(1).ToLower();
        } else {
          if (key != null) {
            hashtable.Add(key, str);
            key = null;
          }
        }
      }

      if (key != null) {
        hashtable.Add(key, "");
      }

      return hashtable;
    }

    private void SafeRelease(object comObj) {
      if (comObj == null) {
        return;
      }

      try {
        Marshal.ReleaseComObject(comObj);
      } catch (Exception ex) {
        Console.WriteLine("Release COM error. Error:" + ex.Message);
      }
    }

    private void PrintVerbose(string message) {
      if (verb) {
        Console.WriteLine(message);
      }
    }

    private void PrintInfo() {
      Console.WriteLine("usage: ConvertWordToPDF [options]");
      Console.WriteLine("        -s <folder>|<file>   The Source file or Directory");
      Console.WriteLine("        -d <folder>          The Destination file or Directory");
      Console.WriteLine("        -v                   Print verbose.");
      Console.WriteLine("        -h                   Print this file.");
      Console.WriteLine();
    }

    private string ToLower(string str) {
      if (str == null) {
        return str;
      }

      return str.ToLower();
    }
  }
}
