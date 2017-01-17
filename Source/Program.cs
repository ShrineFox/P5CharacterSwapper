using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P5FileRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("P5FileRenamer by ShrineFox");
            Console.WriteLine("Compares 2 model folders, then copies renamed models and animations to a new folder.");
            Console.WriteLine("------------------------------------------------------------------------------------\n");
            Console.WriteLine("Character 1 Directory: ");
            string cOneDir = Console.ReadLine();

            if (cOneDir == "")
            {
                cOneDir = Directory.GetCurrentDirectory(); //Character 1's data.cvm/model/character directory
            }
            Console.WriteLine("Character 2 Directory: ");
            string cTwoDir = Console.ReadLine();
            if (cTwoDir == "")
            {
                cTwoDir = Directory.GetCurrentDirectory(); //Character 2's data.cvm/model/character directory
            }
            //start input validation
            int fileCount1 = Directory.EnumerateFiles(@cOneDir, "*.GMD").Count();
            int fileCount2 = Directory.EnumerateFiles(@cTwoDir).Count();
            if (fileCount1 == 0 || fileCount2 == 0)
            {
                Console.WriteLine("No GMD files detected in one or more of the supplied directories.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            //end input validation
            Console.WriteLine("------------------------------------------------------------------------------------\n");
            Console.WriteLine("Comparing models...");
            CompareModels(cOneDir, cTwoDir);
            Console.WriteLine("------------------------------------------------------------------------------------\n");
            Console.WriteLine("Models renamed! Comparing animations...");
            Console.WriteLine("------------------------------------------------------------------------------------\n\n");
            CompareAnims(cOneDir, cTwoDir);
            Console.WriteLine("------------------------------------------------------------------------------------\n");
            Console.WriteLine("Animations renamed! Press any key to exit.");
            Console.ReadKey();
        }

        public static void CompareModels(string cOneDir, string cTwoDir)
        {
            //Add paths to all files in directory to model list if they're GMD files
            List<string> cOneModels = new List<string>();
            foreach (string modelFile in Directory.GetFiles(@cOneDir))
            {
                bool isGMD = (modelFile.Contains(".GMD"));
                if (isGMD == true)
                {
                    cOneModels.Add(modelFile);
                }
            }
            List<string> cTwoModels = new List<string>();
            foreach (string modelFile in Directory.GetFiles(@cTwoDir))
            {
                bool isGMD = (modelFile.Contains(".GMD"));
                if (isGMD == true)
                {
                    cTwoModels.Add(modelFile);
                }
            }

            //Get the model ID from first model from each folder
            string cOneId = Path.GetFileName(cOneModels.ElementAt(0)).TrimStart(new Char[] { 'c' }).Remove(4,11);
            string cTwoId = Path.GetFileName(cTwoModels.ElementAt(0)).TrimStart(new Char[] { 'c' }).Remove(4, 11);

            //Get ready to compare
            string renFolder = cOneDir + "\\" + cOneId +"_renamed";
            DirectoryInfo di = Directory.CreateDirectory(renFolder);
            string newFileName = "";
            foreach (string modelPath in cTwoModels)
            {
                string cTwoId2 = Path.GetFileName(modelPath).TrimStart(new Char[] { 'c' }).Remove(0, 5).Remove(3, 7);
                string cTwoId3 = Path.GetFileName(modelPath).TrimStart(new Char[] { 'c' }).Remove(0, 9).Remove(2, 4);
                for (int i = 0; i < cOneModels.Count; i++)
                {
                    string cOneId2 = Path.GetFileName(cOneModels.ElementAt(i)).TrimStart(new Char[] { 'c' }).Remove(0, 5).Remove(3, 7);
                    string cOneId3 = (Path.GetFileName(cOneModels.ElementAt(i)).TrimStart(new Char[] { 'c' }).Remove(0, 9)).Remove(2, 4);
                    if (cOneId2 == cTwoId2 && cOneId3 == cTwoId3)
                    {
                        newFileName = renFolder + "\\" + "c" + cOneId + "_" + cTwoId2 + "_" + cTwoId3 + ".GMD";
                        File.Copy(modelPath, newFileName, true);
                    }
                }
            }
        }

        public static void CompareAnims(string cOneDir, string cTwoDir)
        {
            //Add paths to all files in directory to animation list if they're GAP files
            List<string> cOneBattle = new List<string>();
            List<string> cOneEvent = new List<string>();
            List<string> cOneField = new List<string>();

            List<string> cTwoBattle = new List<string>();
            List<string> cTwoEvent = new List<string>();
            List<string> cTwoField = new List<string>();
            int gapCount = Directory.EnumerateFiles(@cTwoDir, "*.GAP", SearchOption.AllDirectories).Where(d => !d.Contains("renamed") && !d.Contains("life")).Count();
            int renCount = 0;
            int procCount = 1; //Ignore GAP file in character's model directory
            //Get the model ID from first model from each folder
            List<string> cOneModels = new List<string>();
            foreach (string modelFile in Directory.GetFiles(@cOneDir))
            {
                bool isGMD = (modelFile.Contains(".GMD"));
                if (isGMD == true)
                {
                    cOneModels.Add(modelFile);
                }
            }
            List<string> cTwoModels = new List<string>();
            foreach (string modelFile in Directory.GetFiles(@cTwoDir))
            {
                bool isGMD = (modelFile.Contains(".GMD"));
                if (isGMD == true)
                {
                    cTwoModels.Add(modelFile);
                }
            }
            //Get IDs from models and name directories after them
            string cOneId = Path.GetFileName(cOneModels.ElementAt(0)).TrimStart(new Char[] { 'c' }).Remove(4, 11);
            string cTwoId = Path.GetFileName(cTwoModels.ElementAt(0)).TrimStart(new Char[] { 'c' }).Remove(4, 11);
            string newBattle = cOneDir + "\\" + cOneId + "_renamed\\battle";
            string newEvent = cOneDir + "\\" + cOneId + "_renamed\\event";
            string newField = cOneDir + "\\" + cOneId + "_renamed\\field";

            //If a folder exists, add animations to list if they're GAP files and make new folder
            if (Directory.Exists(@cTwoDir + "\\battle") && Directory.Exists(cOneDir + "\\battle"))
            {
                foreach (string gapFile in Directory.GetFiles(@cOneDir + "\\battle"))
                {
                    bool isGAP = (gapFile.Contains(".GAP"));
                    if (isGAP == true)
                    {
                        cOneBattle.Add(gapFile);
                    }
                }
                Directory.CreateDirectory(newBattle);
            }
            if (Directory.Exists(@cTwoDir + "\\event") && Directory.Exists(cOneDir + "\\event"))
            {
                foreach (string gapFile in Directory.GetFiles(@cOneDir + "\\event"))
                {
                    bool isGAP = (gapFile.Contains(".GAP"));
                    if (isGAP == true)
                    {
                        cOneEvent.Add(gapFile);
                    }
                }
                Directory.CreateDirectory(newEvent);
            }
            if (Directory.Exists(@cTwoDir + "\\field") && Directory.Exists(cOneDir + "\\field"))
            {
                foreach (string gapFile in Directory.GetFiles(@cOneDir + "\\field"))
                {
                    bool isGAP = (gapFile.Contains(".GAP"));
                    if (isGAP == true)
                    {
                        cOneField.Add(gapFile);
                    }
                }
                Directory.CreateDirectory(newField);
            }
            if (Directory.Exists(@cTwoDir + "\\battle") && Directory.Exists(cOneDir + "\\battle")) 
            {
                foreach (string gapFile in Directory.GetFiles(@cTwoDir + "\\battle"))
                {
                    bool isGAP = (gapFile.Contains(".GAP"));
                    if (isGAP == true)
                    {
                        cTwoBattle.Add(gapFile);
                    }
                }
            }
            if (Directory.Exists(@cTwoDir + "\\event") && Directory.Exists(cOneDir + "\\event"))
            {
                foreach (string gapFile in Directory.GetFiles(@cTwoDir + "\\event"))
                {
                    bool isGAP = (gapFile.Contains(".GAP"));
                    if (isGAP == true)
                    {
                        cTwoEvent.Add(gapFile);
                    }
                }
            }
            if (Directory.Exists(@cTwoDir + "\\field") && Directory.Exists(cOneDir + "\\field"))
            {
                foreach (string gapFile in Directory.GetFiles(@cTwoDir + "\\field"))
                {
                    bool isGAP = (gapFile.Contains(".GAP"));
                    if (isGAP == true)
                    {
                        cTwoField.Add(gapFile);
                    }
                }
            }
            //Get ready to compare
            string newFileName = "";
            foreach (string animPath in cTwoBattle) //For each battle animation in 02,
            {
                string cTwoAnimName = Path.GetFileName(animPath); //Save the filename of the animation
                for (int i = 0; i < cOneBattle.Count; i++) //and for each battle animation in 01,
                {
                    //Take the next animation filename in 01, change the id to 02, and compare.
                    string cOneAnimName = Path.GetFileName(cOneBattle.ElementAt(i)).Replace(cOneId,cTwoId); 
                    if (cTwoAnimName == cOneAnimName)
                    {
                        newFileName = newBattle + "\\" + cTwoAnimName.Replace(cTwoId,cOneId);
                        File.Copy(animPath, newFileName, true);
                        renCount++;
                    }
                }
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                ClearCurrentConsoleLine();
                procCount++;
                Console.WriteLine("GAP files processed: " + procCount + "/" + gapCount);
                Console.WriteLine("Files renamed: " + renCount + "/" + gapCount);
            }
            foreach (string animPath in cTwoEvent) 
            {
                string cTwoAnimName = Path.GetFileName(animPath); 
                for (int i = 0; i < cOneEvent.Count; i++) 
                {
                    string cOneAnimName = Path.GetFileName(cOneEvent.ElementAt(i)).Replace(cOneId, cTwoId);
                    if (cTwoAnimName == cOneAnimName)
                    {
                        newFileName = newEvent + "\\" + cTwoAnimName.Replace(cTwoId, cOneId);
                        File.Copy(animPath, newFileName, true);
                        renCount++;
                    }
                }
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                ClearCurrentConsoleLine();
                procCount++;
                Console.WriteLine("GAP files processed: " + procCount + "/" + gapCount);
                Console.WriteLine("Files renamed: " + renCount + "/" + gapCount);
            }
            foreach (string animPath in cTwoField)
            {
                string cTwoAnimName = Path.GetFileName(animPath);
                for (int i = 0; i < cOneField.Count; i++)
                {
                    string cOneAnimName = Path.GetFileName(cOneField.ElementAt(i)).Replace(cOneId, cTwoId);
                    if (cTwoAnimName == cOneAnimName)
                    {
                        newFileName = newField + "\\" + cTwoAnimName.Replace(cTwoId, cOneId);
                        File.Copy(animPath, newFileName, true);
                        renCount++;
                    }
                }
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                ClearCurrentConsoleLine();
                procCount++;
                Console.WriteLine("GAP files processed: " + procCount + "/" + gapCount);
                Console.WriteLine("Files renamed: " + renCount + "/" + gapCount);
            }
        }


        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }


    }
}
