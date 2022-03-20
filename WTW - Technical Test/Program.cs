using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Linq;
using System.Collections.Generic;

namespace WTW_Technical_Test
{
    class Program
    {      
        public static List<TriangleData> ReadCsvFile(string filePath)
        {
            var records = new List<TriangleData>();
            //"C:\dev\DataTest.csv"
            using (var stremReader = new StreamReader(@filePath))
            {
                using (var csvReader = new CsvReader(stremReader, CultureInfo.InvariantCulture))
                {
                     records = csvReader.GetRecords<TriangleData>().ToList();                                       
                }
            }
            return records;
        }
        
        public static int GetEarliestOriginYear(List<TriangleData> records)
        {
            int min = int.MaxValue;

            foreach(TriangleData record in records)
            { 
                if(record.OriginYear < min)
                {
                    min = record.OriginYear;
                }
            }

            return min;
        }

        public static int GetNumberOfDevelopmentYears(List<TriangleData> records)
        {
            int min = int.MaxValue;
            int max = int.MinValue;

            foreach (TriangleData record in records)
            {
                if (record.DevelopmentYear < min)
                {
                    min = record.DevelopmentYear;
                }
                if (record.DevelopmentYear > max)
                {
                    max = record.DevelopmentYear;
                }
            }
            return (max-min)+1;
        }

        static List<decimal> CalculateIncrementalValues(List<TriangleData> records)
        {
            List<decimal> incrementalValuesList = new List<decimal>();

            for (int i = 0; i< (records.Count); i++)
            {                
                if (records[i].OriginYear == records[i].DevelopmentYear) //start of the product
                {
                    incrementalValuesList.Add( records[i].IncrementalValue);
                }
                else
                {
                    int diff = records[i].DevelopmentYear - records[i].OriginYear;

                    if (diff == 1) // next record
                    {
                        incrementalValuesList.Add( records[i].IncrementalValue + records[i - 1].IncrementalValue);
                    }
                    else
                    {
                        List<int> acctualDates = new List<int>();
                        decimal incrementalValue = 0.0m;
                        var missingDates = Enumerable.Range(records[i].OriginYear, diff + 1).ToList();

                        for (int j = 0; j < diff;j++)
                        {
                            if(records[i].Product == records[i - j].Product)
                            {
                                incrementalValue = incrementalValue + records[i - j].IncrementalValue;
                            }                             
                            acctualDates.Add(records[i].DevelopmentYear);
                            missingDates.RemoveAll(r => r == records[i-j].DevelopmentYear);
                        }

                        for(int k = 0; k < missingDates.Count; k++)
                        {
                            if (missingDates[k] == records[i].DevelopmentYear - k - 1)
                            {
                                incrementalValuesList.Add(incrementalValue - records[i].IncrementalValue);
                            }
                        }

                        incrementalValuesList.Add(incrementalValue);
                    }
                }                         
            }
            return incrementalValuesList;
        }

        static List<List<TriangleData>> SeperateProductListsTest(List<TriangleData> records)
        {
            List<List<TriangleData>> myList = new List<List<TriangleData>>();

            for (int i = 0; i < records.Count; i++)
            {
                if (i == 0)
                {
                    myList.Add(new List<TriangleData>());
                    myList[0].Add(records[i]);

                }
                else if (records[i].Product == records[i - 1].Product)
                {
                    myList[(myList.Count - 1)].Add(records[i]);
                }
                else
                {
                    myList.Add(new List<TriangleData>());
                    myList[(myList.Count) - 1].Add(records[i]);
                }
            }

            return myList;
        }

        static int GetLongestList(List<List<TriangleData>> records)
        {
            int maxLength = 0;

            for(int i = 0; i < records.Count; i++)
            {
                maxLength = records[i].Count;
                if (maxLength < records[i].Count)
                {
                    maxLength = records[i].Count;
                }
            }
            return maxLength;
        }

        static void Main(string[] args)
        {
            bool run = true;
            string filePath = "";

            while (run)
            {
                Console.Clear();
                Console.WriteLine("Enter Path:");
                filePath = Console.ReadLine();

                if (File.Exists(filePath))
                {
                    run = false;
                }
                else
                {
                    Console.WriteLine("Path Does not exists. Try again. Press enter to continue...");
                    Console.ReadLine();
                }
            }

            var records = ReadCsvFile(filePath);
            List<List<TriangleData>> productsList = SeperateProductListsTest(records);
            var csvPath = Path.Combine(Environment.CurrentDirectory, $"calculations-{DateTime.Now.ToFileTime()}.csv");

            try
            {
                using (StreamWriter file = new StreamWriter(csvPath, true))
                {
                    file.WriteLine(GetEarliestOriginYear(records) +", "+ GetNumberOfDevelopmentYears(records));

                    for (int i = 0; i < productsList.Count; i++)
                    {
                        var csvLine = new System.Text.StringBuilder();
                        List<decimal> valuesList = CalculateIncrementalValues(productsList[i]);

                        if (valuesList.Count < GetLongestList(productsList))
                        {
                            int test = GetLongestList(productsList) - valuesList.Count + 1;

                            for (int k = 0; k < test; k++)
                            {
                                valuesList.Insert(0, 0);
                            }
                        }
                        Console.WriteLine(productsList[i][0].Product);

                        csvLine.Append(productsList[i][0].Product.ToString());

                        for (int j = 0; j < valuesList.Count; j++)
                        {                           
                            if (valuesList[j] % 1 == 0)
                            {
                                Console.WriteLine(valuesList[j].ToString("0"));
                                csvLine.Append(", " + valuesList[j].ToString("0"));
                            }
                            else
                            {
                                Console.WriteLine(valuesList[j].ToString("0.#"));
                                csvLine.Append(", " + valuesList[j].ToString("0.#"));
                            }
                        }
                        file.WriteLine(csvLine);
                        Console.WriteLine("New CSV file has beeen created in"+Environment.CurrentDirectory);
                    }
 
                }
            }
            catch(Exception ex)
            {
                throw new ApplicationException("Wrtire new CSV file failed",ex);
            }
        }
    }
}
