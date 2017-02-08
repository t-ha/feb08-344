using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class NBAPlayerStats : TableEntity
    {
        public NBAPlayerStats(string name, string ppg)
        {
            this.PartitionKey = name;
            this.RowKey = Guid.NewGuid().ToString();

            this.Name = name;
            this.PPG = double.Parse(ppg);
        }

        public NBAPlayerStats() { }

        public string Name { get; set; }
        public double PPG { get; set; }
    }
}