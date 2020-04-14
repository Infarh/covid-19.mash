using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace covid_19.mash
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string url = "https://coronavirus.mash.ru/";

            var client = new HttpClient();

            var response = await client.GetAsync(url);

            var html = await response.Content.ReadAsStringAsync();

            var parser = new HtmlParser();

            var doc = parser.ParseDocument(html);

            var tables = doc.QuerySelectorAll("table[class=\"table table-hover mb-0\"]").ToArray();

            var rows = tables
               .SelectMany(table => table.QuerySelectorAll("tbody tr"))
               .Select(row => row.Children.Select(cell => cell.TextContent).ToArray())
               .Select(row => (Address: row[0], Number: row[1], Date: DateTime.Parse(row[2])))
               .ToArray();

            const string data_file_name_base = "covid-19-addresses";
            var data_file_name = $"{data_file_name_base}[{DateTime.Now:yyyy-MM-dd}].csv";
            await using var writer = File.CreateText(data_file_name);
            var count = 0;
            foreach (var (address, number, date) in rows)
            {
                count++;
                var line = string.Join(";",
                    date.ToString("dd.MM.yyyy"),
                    address,
                    number
                    );

                await writer.WriteLineAsync(line);
                Console.WriteLine(line);
            }
            Console.WriteLine("Всего случаев {0}", count);
        }
    }
}
