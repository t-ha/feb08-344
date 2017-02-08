using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private static NBAPlayerStats[] nbaplayers;
        private static CloudTable table;

        [WebMethod]
        public void ReadCSV()
        {
            string filename = System.Web.HttpContext.Current.Server.MapPath(@"/2015-2016.nba.stats.csv");
            string[] filedata = File.ReadAllLines(filename);
            nbaplayers = filedata.Skip(9)
                .Select(x => x.Split(','))
                .Select(x => new NBAPlayerStats(x[1], x[24]))
                .Take(30)
                .ToArray();
        }
        
        [WebMethod]
        public void InsertPlayerData()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("nbaplaerstats");
            table.CreateIfNotExists();

            foreach (NBAPlayerStats player in nbaplayers)
            {
                TableOperation insertOperation = TableOperation.Insert(player);
                table.Execute(insertOperation);
            }
        }

        [WebMethod]
        public void ReadPlayerData()
        {
            TableQuery<NBAPlayerStats> rangeQuery = new TableQuery<NBAPlayerStats>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, "A"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, "C"))
                );

            foreach (NBAPlayerStats entity in table.ExecuteQuery(rangeQuery))
            {
                Console.WriteLine(entity.Name + " | " + entity.PPG);
            }
        }
    }
}
