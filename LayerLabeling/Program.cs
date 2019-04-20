using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayerLabeling
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public class LayerManager
    {
        private int NumberOfNodes;
        private int Depth = 4;
        private int[,] LabelTable;

        public LayerManager(string[] args)
        {
            this.NumberOfNodes = Int32.Parse(args[0]);
            this.Depth = Int32.Parse(args[1]);
            this.LabelTable = new int[this.NumberOfNodes, 5];
            for (int i = 0; i < this.NumberOfNodes; i++)
            {
                this.LabelTable[i, 0] = i;
            }
        }


    }
}
