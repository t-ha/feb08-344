﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text;
using Microsoft.WindowsAzure.Storage.Queue;

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
        private static NBAPlayerStats[] results;
        private static CloudTable table;
        private static CloudQueue queue;

        [WebMethod]
        public NBAPlayerStats[] ReadCSV()
        {
            string filename = System.Web.HttpContext.Current.Server.MapPath(@"/2015-2016.nba.stats.csv");
            string[] filedata = File.ReadAllLines(filename);
            var nbaplayers = filedata.Skip(2)
                .Select(x => x.Split(','))
                .Select(x => new NBAPlayerStats(x[0], x[21]))
                .Take(30)
                .ToArray();
            results = nbaplayers;
            return nbaplayers;
        }
        
        [WebMethod]
        public void InsertPlayerData()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("nbaplayerstats");
            table.CreateIfNotExists();
            
            foreach (NBAPlayerStats player in results)
            {
                TableOperation insertOperation = TableOperation.Insert(player);
                table.Execute(insertOperation);
            }
            WebService1.table = table;
        }

        [WebMethod]
        public List<string> SearchPlayerData()
        {
            TableQuery<NBAPlayerStats> rangeQuery = new TableQuery<NBAPlayerStats>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, "A"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, "C"))
                );

            List<string> matchedSearches = new List<string>();
            foreach (NBAPlayerStats entity in WebService1.table.ExecuteQuery(rangeQuery))
            {
                matchedSearches.Add(entity.Name + " | " + entity.PPG);
            }

            return matchedSearches;
        }

        [WebMethod]
        public void InsertURL()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference("myurls");
            queue.CreateIfNotExists();

            CloudQueueMessage message = new CloudQueueMessage("http://www.cnn.com/index.html");
            queue.AddMessage(message);
        }

        [WebMethod]
        public void ReadURLFromQueue()
        {
            CloudQueueMessage message2 = queue.GetMessage(TimeSpan.FromMinutes(5));
            queue.DeleteMessage(message2);
        }
    }
}
