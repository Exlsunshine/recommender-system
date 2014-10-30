using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RecommenderSystem
{
    class Program
    {
        static private int noOfUsers;
        static private int noOfItems;
        static private byte[,] ratingMatirx;
        static private byte[,] ratingFlag;
        static private float[] avgRatings;

        static void Main(string[] args)
        {
            initSettings();
            ReadRatings(@"ratings2.txt", @"ratingFlag.txt");
            preProcess();

            foo();
        }

        static private void foo() 
        {
            Console.WriteLine("Data load completed!");

            double[] simi = new double[noOfUsers];
            for (int i = 1; i < noOfUsers; i++)
                simi[i] = NormalizeCosineDistance(0, i);

            Array.Sort<double>(simi);
            for (int i = 0; i < 10; i++)
                Console.WriteLine("Top " + (i + 1).ToString() + ":\t"+simi[noOfUsers - 1 - i]);
            
            Console.ReadKey();
        }

        static private void preProcess() 
        {
            for (int i = 0; i < noOfUsers; i++)
            {
                int ratingNO = 0;
                double avgValue = 0;
                
                for (int j = 0; j < noOfItems; j++)
                {
                    if (ratingFlag[i, j] != 0)
                    {
                        avgValue += ratingMatirx[i, j];
                        ratingNO++;
                    }
                }

                avgRatings[i] = (float)(avgValue / ratingNO); 
            }
        }

        static private void initSettings() 
        {
            noOfUsers = 0;
            noOfItems = 0;
            ratingMatirx = new byte[Settings.maximumOfUserAmount, Settings.maximumOfItemAmount];
            ratingFlag = new byte[Settings.maximumOfUserAmount, Settings.maximumOfItemAmount];
            avgRatings = new float[Settings.maximumOfUserAmount];

            for (int i = 0; i < Settings.maximumOfUserAmount; i++)
                avgRatings[i] = 0;

            for (int i = 0; i < Settings.maximumOfUserAmount; i++)
            {
                for (int j = 0; j < Settings.maximumOfItemAmount; j++)
                {
                    ratingMatirx[i, j] = 0;
                    ratingFlag[i, j] = 0;
                }
            }
        }

        static private int ReadRatings(string ratingMatrixPath,string ratingFlagPath)
        {
            try
            {
                FileStream aFile = new FileStream(ratingMatrixPath, FileMode.Open);
                StreamReader sr = new StreamReader(aFile);
                string rowOfData = sr.ReadLine();

                while (rowOfData != null)
                {
                    if (rowOfData.Length == 0 || rowOfData[0] == '#')
                    {
                        rowOfData = sr.ReadLine();
                        continue;
                    }
                    
                    string[] columns = rowOfData.Substring(1).Split(' ');
                    for (int i = 0; i < columns.Length; i++)
                        ratingMatirx[noOfUsers, i] = Byte.Parse(columns[i]);

                    noOfUsers++;
                    rowOfData = sr.ReadLine();
                }
                sr.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            
            try
            {
                FileStream aFile = new FileStream(ratingFlagPath, FileMode.Open);
                StreamReader sr = new StreamReader(aFile);
                string rowOfData = sr.ReadLine();

                int userIndex = 0;
                while (rowOfData != null)
                {
                    if (rowOfData.Length == 0 || rowOfData[0] == '#')
                    {
                        rowOfData = sr.ReadLine();
                        continue;
                    }

                    string[] columns = rowOfData.Substring(1).Split(' ');
                    noOfItems = columns.Length;

                    for (int i = 0; i < columns.Length; i++)
                        ratingFlag[userIndex, i] = Byte.Parse(columns[i]);

                    userIndex++;
                    rowOfData = sr.ReadLine();
                }
                sr.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return -2;
            }
            return 0;
        }

        /*
         *                   sum( (Ri,y - avg(Ri)) * (Ra,y - avg(Ra)) )
         *   sim(a,i) =        ------------------------------------------
         *                 sqrt(sum(Ra,y - avg(Ra))) * sqrt(sum(Ri,y - avg(Ri))
         * 
         */
        static private double NormalizeCosineDistance(int user1Id, int user2Id) 
        {
            double coeffector = 0;
            double devation1 = 0;
            double devation2 = 0;
            
            for (int i = 0; i < noOfItems; i++)
            {
                if (ratingFlag[user1Id, i] != 0 && ratingFlag[user2Id, i] != 0)
                    coeffector += (ratingMatirx[user1Id, i]/* - avgRatings[user1Id]*/) * (ratingMatirx[user2Id, i]/* - avgRatings[user2Id]*/);
            }

            for (int i = 0; i < noOfItems; i++) 
            {
                if (ratingFlag[user1Id, i] != 0)
                    devation1 += Math.Pow(ratingMatirx[user1Id, i]/* - avgRatings[user1Id]*/, 2);
                if (ratingFlag[user2Id, i] != 0)
                    devation2 += Math.Pow(ratingMatirx[user2Id, i]/* - avgRatings[user2Id]*/, 2);
            }

            devation1 = Math.Sqrt(devation1);
            devation2 = Math.Sqrt(devation2);

            //Console.WriteLine(devation1);
            //if (devation2 == 0)
            //    Console.Write(noOfItems + "\t" + user2Id + "\t");
            Console.WriteLine(devation2);

            return coeffector / devation1 / devation2;
        }
    }
}
