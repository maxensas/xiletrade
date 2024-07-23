using XiletradeJson;
using LibBundledGGPK;
using LibDat2;
using System.Diagnostics;
using System.Reflection;
using System.Text;

try
{
    string inputGgpk = string.Empty;
    bool exportCsv = false;
    bool exportDat = false;

    var versionString = Assembly.GetEntryAssembly()?
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                        .InformationalVersion
                        .ToString();

    Console.WriteLine($"Launching Xiletrade JSON v{versionString} generator . . .");
    Console.WriteLine();
    Console.WriteLine("[Dependencies]");

    Assembly a = typeof(Program).Assembly;
    foreach (AssemblyName an in a.GetReferencedAssemblies())
    {
        if (an.Name is "LibBundledGGPK3" or "CsvHelper")
        {
            Console.WriteLine($"Nuget : {an.Name} v{an.Version}");
        }
        if (an.Name is "LibDat2" or "Utf8Json")
        {
            Console.WriteLine($"Lib   : {an.Name} v{an.Version}");
        }
    }
    Console.WriteLine();
    Console.WriteLine("This program will create small json files consumed by Xiletrade.");
    Console.WriteLine();
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
    string outputDir = Path.GetFullPath("Data\\"); // not set as an entry
    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }

    Console.WriteLine();
    Console.WriteLine("[Settings]");
    Console.WriteLine("GGPK path        : " + inputGgpk);
    Console.WriteLine("DAT64 targets    : " + string.Join(" + ", Strings.DatNames));
    Console.WriteLine("Output directory : " + outputDir);
    Console.WriteLine("Export CSV       : " + exportCsv);
    Console.WriteLine("Export DAT       : " + exportDat);
    Console.WriteLine();

    Console.WriteLine("Reading ggpk file . . .");
    var ggpk = new BundledGGPK(inputGgpk);
    bool globalGgpk = ggpk.Index.FindNode("Data\\" + Strings.TencentLang[1].Key) is null;
    var langs = globalGgpk ? Strings.GlobalLang : Strings.TencentLang;
    //var languages = string.Join('/', (from kvp in langs select kvp.Key).Distinct().ToList());
    Console.WriteLine("DAT Schemas used : " + Path.GetFullPath("DatDefinitions.json"));
    Console.WriteLine("Exporting files . . .");
    
    foreach (var lang in langs)
    {
        Console.WriteLine();
        Console.WriteLine("Language selected : " + lang.Key);
        foreach (var datName in Strings.DatNames)
        {
            string dat = datName + ".dat64";
            string langDir = lang.Key is "english" ? string.Empty : lang.Key + "\\";
            string datDir = "data\\" + langDir + dat;

            var node = ggpk.Index.FindNode(datDir);
            if (node == null)
            {
                Console.WriteLine("Not found in GGPK: " + datDir);
                Console.WriteLine("Skipping file . . .");
                Console.WriteLine();
                continue;
            }

            string jsonPath = outputDir + "json\\";
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
                ggpk.Index.Extract(node, filePath);
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
                    Console.WriteLine("CSV created   : " + csvFilePath.Replace(outputDir, string.Empty));
                    files++;
                }

                var jsonFilePath = Util.CreateJson(sbCsv.ToString(), datName, jsonPath);
                if (jsonFilePath?.Length > 0)
                {
                    Console.WriteLine("JSON created  : " + jsonFilePath.Replace(outputDir, string.Empty));
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




