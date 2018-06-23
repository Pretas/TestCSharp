﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConsoleTest
{    
    class Program
    {
        static void Main(string[] args)
        {
            int row = 100000;
            int col = 50;

            double[,] sample = new double[row, col];
            Random rd = new Random();

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    sample[i, j] = rd.NextDouble();
                }
            }

            string path = @"D:\StreamTest.txt";

            //파일저장
            File.Delete(path);
            using (Stream stream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, sample);
            }

            //파일열기
            double[,] res;
            byte[] res2;
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                res2 = (byte[])bformatter.Deserialize(stream);
                //res = (double[,])bformatter.Deserialize(stream);                
            }

            long sum = 0;
            for (int i = 0; i < res2.GetLength(0); i++)
            {
                sum += res2[i];
            }


            Console.ReadLine();
        }
    }
}
