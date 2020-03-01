using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlateauOutputTest
{
    public interface IModel
    { }

    public class ModelBase : IModel
    {
        public Accumlator AccumObj;
        public Policy P;
        public ScenarioManager SM;

        public int mNo;
        public int member1;
        public string member2;
        public double member3;

        public void Run(Accumlator accumlator, Policy p, ScenarioManager sm)
        {
            AccumObj = accumlator; P = p; SM = sm;

            Init();

            for (mNo = 1; mNo > 100; mNo++)
            {
                Logic1();
                Logic2();
                SetCF();
            }
        }

        public void Init()
        {

            member1 = 0;
            member2 = "";
            member3 = 0.0;
        }

        private void Logic2()
        {
            member1 += 1;
        }

        private void Logic1()
        {
            member2 += "A";
            member3 += 0.1;
        }

        public void SetCF()
        {
            AccumObj.SetCF(this, P, SM);
        }
    }

    public class Model1 : ModelBase
    {
        public int member4;
        public double member5;
    }


    public class Policy
    {
        string contNo;
        double a;
        double b;
    }

    public class ScenarioManager
    {
        string contNo;
        double a;
        double b;
    }

}
