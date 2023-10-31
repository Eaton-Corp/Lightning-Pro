using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using System.Configuration;

namespace LightningPRO.Models
{
    public class LightningProDatabaseContext : DataConnection
    {
        public LightningProDatabaseContext() : base("LPdatabase"){}
        public ITable<PRL123> Product => this.GetTable<PRL123>();
        public ITable<PRL4> PRL4Product => this.GetTable<PRL4>();
        public ITable<PRLCS> PRLCSProduct => this.GetTable<PRLCS>();

    }
}

