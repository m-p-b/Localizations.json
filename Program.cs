﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Localizations
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.UTF8Encoding.UTF8;

            var types = new HashSet<Guid>();
            types.Add(new Guid("7f368d97-8b7f-4b39-b156-dc66afd9496a")); // TaxCode
            types.Add(new Guid("dcb382dc-a4e0-4354-a845-b7d647f610f7")); // CustomField
            types.Add(new Guid("73af4c68-c347-4088-8846-758f1e7bc5bb")); // PayslipContributionItem
            types.Add(new Guid("0444eb18-6fc5-4d1f-be8b-c114da01832c")); // PayslipDeductionItem
            types.Add(new Guid("ab02f6ab-c91c-4fc2-b979-66a6682c200e")); // PayslipEarningsItem
            types.Add(new Guid("91c2bcbb-1f8c-4aa1-82fd-0ab38c97fb14")); // ReportTransformation
            types.Add(new Guid("6ef13e42-ad89-4d42-9480-546e0c04a411")); // BalanceSheetAccount

            var projectDir = new DirectoryInfo(Directory.GetCurrentDirectory());
#if DEBUG
            projectDir = projectDir.Parent.Parent.Parent;
#endif
            Console.WriteLine("Project Directory: " + projectDir.FullName);

            var output = new Dictionary<string, Dictionary<string, object>>();

            var keys = new HashSet<Guid>();

            foreach (var e in projectDir.GetDirectories().OrderBy(x => x.Name))
            {
                if (e.Name.StartsWith('.')) continue;
                if (e.Name == "obj") continue;
                if (e.Name == "bin") continue;

                output.Add(e.Name, new Dictionary<string, object>());

                foreach (var e2 in e.GetFiles("*.json").OrderBy(x => x.Name))
                {
                    Console.WriteLine("Input: "+e2.FullName);

                    var json = JsonConvert.DeserializeObject<Dictionary<Guid, Dictionary<Guid, object>>>(File.ReadAllText(e2.FullName))
                        .Where(x => types.Contains(x.Key))
                        .ToDictionary(x => x.Key, x => x.Value);

                    foreach (var type in json)
                    {
                        foreach (var o in type.Value)
                        {
                            if (type.Key == o.Key) continue;
                            if (keys.Contains(o.Key))
                            {
                                throw new Exception("Duplicate: "+o.Key.ToString());
                            }
                            keys.Add(o.Key);
                        }
                    }

                    var name = Path.GetFileNameWithoutExtension(e2.Name);
                    output[e.Name].Add(name, json);
                }
            }

            var outputJson = JsonConvert.SerializeObject(output, Formatting.Indented);
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Localizations.json");
            Console.WriteLine("Output: " + outputPath);
            File.WriteAllText(outputPath, outputJson);
        }
    }
}