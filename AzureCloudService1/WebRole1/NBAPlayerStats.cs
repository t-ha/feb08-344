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
            name = name.Substring(1, name.Length - 1);
            ppg = ppg.Substring(0, ppg.Length - 2);
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