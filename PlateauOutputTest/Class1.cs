using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlateauOutputTest
{
    public interface ICFTable
    {   
        ICFTable GetCF(IModel model, Policy p, ScenarioManager sm);

        string TableName { get; }
        void SetTableName(string tn);

        void AddUp(ICFTable cf);
    }

    public class CFTable1 : ICFTable
    {
        public int Member1;
        public string Member2;
        public double Member3;

        private string _tableName = "N/A";
        public string TableName { get => _tableName; }

        public ICFTable GetCF(IModel model, Policy p, ScenarioManager sm)
        {
            CFTable1 cf = new CFTable1();

            if (model.GetType() == typeof(Model1))
            {
                Model1 modelFrom = model as Model1;
                cf.Member1 = modelFrom.member1;
                cf.Member2 = modelFrom.member2;
                cf.Member3 = modelFrom.member3;
            }

            return cf;
        }

        public void AddUp(ICFTable cf)
        {
            if (cf.GetType() == typeof(CFTable1))
            {
                CFTable1 modelFrom = cf as CFTable1;
                Member1 = modelFrom.Member1;
                Member2 = modelFrom.Member2;
                Member3 = modelFrom.Member3;
            }
        }

        public void SetTableName(string tn)
        {
            _tableName = tn;
        }
    }

    public class Accumlator
    {
        // 베이스가 되는 타입
        public Type CFTableType { get; private set; }
        
        // 그루핑된 데이터 저장소 : Dic<subGroup, ICFTable>
        Dictionary<string, ICFTable> GroupedData;

        public void Init(ICFTable cfT)
        {
            CFTableType = cfT.GetType();
        }

        public void SetCF(IModel model, Policy p, ScenarioManager sM)
        {

            throw new NotImplementedException();
        }
    }
}
