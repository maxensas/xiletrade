using Xiletrade.Json;
using LibBundledGGPK3;
using LibDat2;
using System.Diagnostics;
using System.Reflection;
using System.Text;

try
{
    bool isPoe2 = false;
    string inputGgpk = string.Empty;
    bool exportCsv = false;
    bool exportDat = false;
    
    Assembly a = typeof(Program).Assembly;

    Console.WriteLine($"Launching Xiletrade JSON generator v{a.GetName().Version?.ToString()} ..");
    Console.WriteLine();
    try
    {
#pragma warning disable IL2026
        var refAssemblies = a.GetReferencedAssemblies();
#pragma warning restore IL2026
        if (refAssemblies is not null)
        {
            Console.WriteLine("[Dependencies]");
            foreach (AssemblyName an in refAssemblies)
            {
                if (an.Name is "LibBundledGGPK3" or "CsvHelper" or "Utf8Json")
                {
                    Console.WriteLine($"Nuget : {an.Name} v{an.Version}");
                }
                if (an.Name is "LibDat2")
                {
                    Console.WriteLine($"Lib   : {an.Name} v{an.Version}");
                }
            }
            Console.WriteLine();
        }
    }
    catch(Exception) 
    { 
    }
    
    Console.WriteLine("This program will create small JSON files consumed by Xiletrade.");
    Console.WriteLine();
    Console.Write("Do you want to export POE 2 files ? ");

    if (Console.ReadLine()!.ToLower() is "yes" or "y")
    {
        isPoe2 = true;
    }

    GameStrings game = new(isPoe2);
    
    foreach (var path in game.PathGgpk) // look if we can find without asking.
    {
        if (File.Exists(path))
        {
            inputGgpk = path;
            break;
        }
    }

    if (inputGgpk.Length is 0)
    {
        Console.Write("Please specify a path for 'Content.ggpk' : ");
        string path = Console.ReadLine()!;
        if (!File.Exists(path))
        {
            Console.WriteLine();
            Console.WriteLine(path + " not found . . . ending program.");
            Console.ReadLine();
            Environment.Exit(0);
        }
        inputGgpk = path;
    }

    string dataDirectory = "Data\\";
    string outputDir = Path.GetFullPath(dataDirectory); // not set as an entry
    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }

    Console.WriteLine();
    Console.WriteLine("[Settings]");
    Console.WriteLine("POE version       : " + game.GetVersion());
    Console.WriteLine("GGPK path         : " + inputGgpk);
    Console.WriteLine("DAT Schemas used  : " + Path.GetFullPath(game.GetDefinition()));
    Console.WriteLine("DAT64 targets     : " + string.Join(" + ", game.Names.Keys));
    Console.WriteLine("Output directory  : " + outputDir);
    Console.WriteLine("Output JSON files : " + string.Join(" + ", game.Names.Values.Where(v => !string.IsNullOrWhiteSpace(v))));

    Console.WriteLine();

    Console.Write("Do you want to export CSV files aswell ? ");
    if (Console.ReadLine()!.ToLower() is "yes" or "y")
    {
        exportCsv = true;
    }
    Console.Write("Do you want to export DAT files aswell ? ");
    if (Console.ReadLine()!.ToLower() is "yes" or "y")
    {
        exportDat = true;
    }

    Stopwatch watch = new();
    watch.Start();
    int files = 0;
    Console.WriteLine();
    Console.WriteLine("Reading ggpk file . . .");

    BundledGGPK? ggpk = null;
    LibBundle3.Index? index = null;

    var failed = await Task.Run(() => {
        ggpk = new(inputGgpk, false); 
        index = ggpk.Index;
        return index.ParsePaths();
    });

    if (ggpk is null || index is null)
    {
        Console.WriteLine($"Errors {failed} : GGPK or Index is null !");
        Console.WriteLine("Ending program . . .");
        return;
    }
    
    List<LibBundle3.Records.FileRecord> lFiles = new();
    foreach (var bundle in index.Bundles.ToArray())
    {
        foreach (var file in bundle.Files)
        {
            if (file.Path is null)
            {
                break;
            }
            if (file.Path.StartsWith("data/"))
            {
                lFiles.Add(file);
            }
        }
    }

    bool tencentGgpk = lFiles.Any(x => x.Path.StartsWith("data/" + Strings.TencentLang[1].Key));
    var langs = tencentGgpk ? Strings.TencentLang : Strings.GlobalLang;

    Console.WriteLine("Exporting files . . .");
    foreach (var lang in langs)
    {
        Console.WriteLine();
        Console.WriteLine("Language selected : " + lang.Key);
        foreach (var datName in game.Names.Keys)
        {
            string dat = datName + game.GetDatExtension();
            string langDir = lang.Key is "english" ? string.Empty : lang.Key + "/";
            string datDir = "data/" + (isPoe2 ? "balance/" : string.Empty) + langDir + dat;

            var fileRecord = lFiles.Where(x => x.Path.Contains(datDir)).FirstOrDefault();
            if (fileRecord is null)
            {
                Console.WriteLine("Not found in GGPK: " + datDir);
                Console.WriteLine("Skipping file . . .");
                Console.WriteLine();
                continue;
            }
            FileLib file = new(fileRecord, dat);

            string jsonPath = outputDir + "Lang\\";
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            jsonPath += lang.Value + "\\";
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }

            if (exportDat)
            {
                string datPath = outputDir + "dat64\\";
                if (!Directory.Exists(datPath))
                {
                    Directory.CreateDirectory(datPath);
                }
                datPath += lang.Value + "\\";
                if (!Directory.Exists(datPath))
                {
                    Directory.CreateDirectory(datPath);
                }

                var filePath = datPath + dat;

                LibBundle3.Index.Extract(file, datPath);
                Console.WriteLine("DAT64 created : " + filePath.Replace(outputDir, string.Empty));

                files++;
            }

            if (file is LibBundle3.Nodes.IFileNode fn)
            {
                var data = fn.Record.Read().ToArray();
                DatContainer dc;

                try // to handle DatDefinitions errors
                {
                    dc = new DatContainer(fileData: data, fileName: dat, poe2: isPoe2);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[{e.Source} error] {e.Message}"); 
                    continue;
                }

                
                StringBuilder sbCsv = new(dc.ToCsv());
                if (datName == game.Mods) // bugfixes
                {
                    if (lang.Key is "Japanese")
                    {
                        sbCsv.Replace("\r\n信心深さの", "信心深さの");
                    }
                    if (lang.Key is "Spanish")
                    {
                        sbCsv.Replace("daño de fuego de Golpe infernal 1\r\n", "daño de fuego de Golpe infernal 1");
                    }
                }
                sbCsv.Replace("\r", string.Empty);

                if (exportCsv)
                {
                    string csvPath = outputDir + "csv\\";
                    if (!Directory.Exists(csvPath))
                    {
                        Directory.CreateDirectory(csvPath);
                    }
                    csvPath += lang.Value + "\\";
                    if (!Directory.Exists(csvPath))
                    {
                        Directory.CreateDirectory(csvPath);
                    }

                    var csvFilePath = csvPath + datName + ".csv";
                    File.WriteAllText(csvFilePath, sbCsv.ToString());
                    Console.WriteLine("CSV created   : " + dataDirectory + csvFilePath.Replace(outputDir, string.Empty));
                    files++;
                }

                var jsonFilePath = Util.CreateJson(game, sbCsv.ToString(), datName, jsonPath, lang.Key);
                if (jsonFilePath?.Length > 0)
                {
                    Console.WriteLine("JSON created  : " + dataDirectory + jsonFilePath.Replace(outputDir, string.Empty));
                    files++;
                }
            }
            GC.Collect();
        }
    }
    ggpk.Dispose();
    watch.Stop();
    var ts = watch.Elapsed;
    Console.WriteLine();
    Console.WriteLine("Export Done !");
    Console.WriteLine("Total files created : " + files);
    Console.WriteLine($"Execution time : {ts.Minutes}m {ts.Seconds}s");
}
catch(Exception e)
{
    Console.WriteLine();
    Console.WriteLine("Error encounterd !");
    Console.WriteLine();
    Console.Error.WriteLine(e);
}
finally
{
    Console.WriteLine("Pressa a key to exit Xiletrade JSON generator . . .");
    Console.ReadKey();
}




