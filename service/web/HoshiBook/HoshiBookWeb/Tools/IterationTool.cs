using System;
using System.Collections.Generic;
using HoshiBookWeb.Tools;


namespace HoshiBookWeb.Tools {
    public class IterationTool {
        public IterationTool() {
        }

        public static void ReadTwoDimListDictObj(List<List<Dictionary<string, object>>> twoDimListDictObj) {
            foreach (var readCurrDimList in twoDimListDictObj){
                foreach (var readCurrList in readCurrDimList){
                    foreach (var readCurrDict in readCurrList) {
                        string dictStr = "[Key: " + readCurrDict.Key + ",Value: " + readCurrDict.Value + "]";
                        Console.WriteLine(dictStr);
                    }
                    Console.WriteLine("----------------------------------------------------------------------------------");
                }
                Console.WriteLine("-------------------------------------------------------------------------------------------");
            }
            Console.WriteLine($"twoDimListDictObj count is: {twoDimListDictObj.Count}");
        }

        public static void ReadListDictObj(List<Dictionary<string, object>> dictObjList) {
            foreach (var cell in dictObjList) {
                // Console.WriteLine($"row.Key: {cell.Keys.Count}, row.Value: {cell.Values.Count}");
                foreach (var row in cell) {
                    Console.WriteLine($"{row.Key} : {row.Value}");
                }
                Console.WriteLine("----------------------------------------------------------------------------------");
            }
            Console.WriteLine($"List items count: {dictObjList.Count}");
        }

        // public static void ReadDictObj(Dictionary<string,object> dictObj) {
        //     foreach (var item in dictObj) {
        //         Console.WriteLine($"{item.Key} : {item.Value}");
        //     }
        // }

        public static void ReadDictItems<T>(Dictionary<string,T> dictObj) {
            foreach (var item in dictObj) {
                Console.WriteLine($"{item.Key} : {item.Value}");
            }
        }

        public static void ReadListContent<T>(List<T> queryData) {
            foreach (var currData in queryData) {
                Console.WriteLine(currData);
            }
        }
    }
}