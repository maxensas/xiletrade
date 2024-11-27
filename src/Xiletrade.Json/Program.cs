using Xiletrade.Json;
using LibBundledGGPK3;
using LibDat2;
using System.Diagnostics;
using System.Reflection;
using System.Text;

try
{
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
    foreach (var path in Strings.PathGgpk) // look if we can find without asking.
    {
        if (File.Exists(path))
        {
            inputGgpk = path;
            break;
        }
    }

    if (inputGgpk.Length == 0)
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
    Console.WriteLine("GGPK path         : " + inputGgpk);
    Console.WriteLine("DAT Schemas used  : " + Path.GetFullPath("DatDefinitions.json"));
    Console.WriteLine("DAT64 targets     : " + string.Join(" + ", Strings.DatNames));
    Console.WriteLine("Output directory  : " + outputDir);
    Console.WriteLine("Output JSON files : " + string.Join(" + ", Strings.JsonNames));

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
    var ggpk = new BundledGGPK(inputGgpk); 
    bool tencentGgpk = ggpk.Index.TryFindNode("data/" + Strings.TencentLang[1].Key, out var tencentNode);
    var langs = tencentGgpk ? Strings.TencentLang : Strings.GlobalLang;

    Console.WriteLine("Exporting files . . .");
    foreach (var lang in langs)
    {
        Console.WriteLine();
        Console.WriteLine("Language selected : " + lang.Key);
        foreach (var datName in Strings.DatNames)
        {
            string dat = datName + ".dat64";
            string langDir = lang.Key is "english" ? string.Empty : lang.Key + "/";
            string datDir = "data/" + langDir + dat;

            ggpk.Index.TryFindNode(datDir, out var node);
            if (node is null)
            {
                Console.WriteLine("Not found in GGPK: " + datDir);
                Console.WriteLine("Skipping file . . .");
                Console.WriteLine();
                continue;
            }

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
                //ggpk.Index.Extract(node, filePath);
                LibBundle3.Index.Extract(node, filePath);
                Console.WriteLine("DAT64 created : " + filePath.Replace(outputDir, string.Empty));

                files++;
            }

            if (node is LibBundle3.Nodes.IFileNode fn)
            {
                var data = fn.Record.Read().ToArray();
                DatContainer dc;
                try // to handle DatDefinitions errors mainly
                {
                    dc = new DatContainer(data, dat);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[{e.Source} error] {e.Message}"); 
                    continue;
                }
                
                StringBuilder sbCsv = new(dc.ToCsv());
                if (datName == Strings.Mods) // bugfixes
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

                var jsonFilePath = Util.CreateJson(sbCsv.ToString(), datName, jsonPath, lang.Key);
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




