using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XmlDupplicateCleaner
{
    /// <summary>
    /// Simple Duplicate in XML finder.
    /// </summary>
    public class XmlDuplicateCleaner
    {
        static void Main(string[] args)
        {
            Console.WriteLine("In what node should we search for duplicates (ex Old) ?");
            var nodeToLookFor  = Console.ReadLine();
            Console.WriteLine("Provide Filename for the XML file to search.");
            var fileName = Console.ReadLine();

            try
            {
                var xmlDoc = XDocument.Load(String.IsNullOrEmpty(fileName) ? "xmlfile.xml" : fileName);

                if (xmlDoc.Root != null)
                {
                    var grouped = xmlDoc.Root.Elements().Descendants(nodeToLookFor);
                    List<string> resultList = grouped.Select(groupItem => groupItem.Value).ToList();

                    var duplicateKeys = resultList.GroupBy(x => x)
                        .Where(group => @group.Count() > 1)
                        .Select(group => @group.Key).ToList();

                    if (!duplicateKeys.Any())
                    {
                        Console.WriteLine("\n No duplicates found.");
                        return;
                    }

                    Console.WriteLine("\nDuplicates found:\n");
                    duplicateKeys.ForEach(item => Console.WriteLine(item));
                    foreach (var duplicateKey in duplicateKeys)
                    {
                        Console.WriteLine(duplicateKey);
                    }
                }

                Console.ReadLine();
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("Could Not find the file please restart.");
                Console.ReadLine();
            }
         
        }
    }
}
