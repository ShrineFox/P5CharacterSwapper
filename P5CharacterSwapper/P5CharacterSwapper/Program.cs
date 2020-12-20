using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GFDLibrary;
using GFDLibrary.Animations;
using GFDLibrary.Models;
using GFDStudio.FormatModules;
using GFDStudio.GUI.Controls;
using GFDStudio.GUI.DataViewNodes;
using GFDStudio.IO;
using TGE.SimpleCommandLine;

namespace P5CharacterSwapper
{
    class Program
    {
        public static ProgramOptions Options { get; private set; }

        static void Main(string[] args)
        {
            //Get/validate input
            string about = SimpleCommandLineFormatter.Default.FormatAbout<ProgramOptions>("ShrineFox, TGE", "Batch-replaces P5 character models/animations by ID.\nCan retarget models and animations if specified, or fallback to a certain GMD.\nUses TGE's GFDLibrary and SimpleCommandLine. No input files will be overwritten.");
            Console.WriteLine(about);
            try
            {
                Options = SimpleCommandLineParser.Default.Parse<ProgramOptions>(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            //Create model/animation lists
            Input.ogModels = Directory.GetFiles(Options.Old, "*.GMD", SearchOption.AllDirectories);
            Input.newModels = Directory.GetFiles(Options.New, "*.GMD", SearchOption.AllDirectories);
            Input.ogAnims = Directory.GetFiles(Options.Old, "*.GAP", SearchOption.AllDirectories);
            Input.newAnims = Directory.GetFiles(Options.New, "*.GAP", SearchOption.AllDirectories);
            Input.retargetFolder = $"{ Options.New} Retargeted to {Path.GetFileName(Options.Old)}";
            string defaultGMDId = Options.DefaultGMDId.ToString("D3");
            string newModelDefaultGMD = Path.Combine(Options.New, $"c{Path.GetFileName(Options.New)}_{defaultGMDId}_00.GMD");
            string ogModelDefaultGMD = Path.Combine(Options.Old, $"c{Path.GetFileName(Options.Old)}_{defaultGMDId}_00.GMD");
            //Try to get default GMD for new model
            if (!File.Exists(newModelDefaultGMD))
            {
                //Fall back to 051 or 001 if they exist, otherwise use first available mmodel
                if (!File.Exists(newModelDefaultGMD))
                {
                    if (Input.newModels.Any(x => x.Contains("_051_")))
                        newModelDefaultGMD = Input.newModels.FirstOrDefault(x => x.Contains("_051_"));
                    else if (Input.newModels.Any(x => x.Contains("_000_")))
                        newModelDefaultGMD = Input.newModels.FirstOrDefault(x => x.Contains("_000_"));
                    else
                        newModelDefaultGMD = Input.newModels.FirstOrDefault();
                }
            }
            Input.newDefaultModel = newModelDefaultGMD;

            //Try to get default GMD for old model
            if (!File.Exists(ogModelDefaultGMD))
            {
                //Fall back to 051 or 001 if they exist, otherwise use first available mmodel
                if (!File.Exists(ogModelDefaultGMD))
                {
                    if (Input.ogModels.Any(x => x.Contains("_051_")))
                        ogModelDefaultGMD = Input.ogModels.FirstOrDefault(x => x.Contains("_051_"));
                    else if (Input.ogModels.Any(x => x.Contains("_000_")))
                        ogModelDefaultGMD = Input.ogModels.FirstOrDefault(x => x.Contains("_000_"));
                    else
                        ogModelDefaultGMD = Input.ogModels.FirstOrDefault();
                }
            }
            Input.ogDefaultModel = ogModelDefaultGMD;

            //Replacement models and animations output to new folder
            if (!Directory.Exists(Input.retargetFolder))
                Directory.CreateDirectory(Input.retargetFolder);
            if (Options.GMD.Replace || Options.GMD.Retarget)
                ReplaceModels();
            if (Options.GAP.Replace || Options.GAP.Retarget)
                ReplaceAnimations();
            if (Options.DeleteChunks)
                RemoveChunks();
        }

        private static void RemoveChunks()
        {
            foreach(var gmd in Directory.GetFiles(Input.retargetFolder, "*.GMD", SearchOption.TopDirectoryOnly))
            {
                var modelPack = ModuleImportUtilities.ImportFile<ModelPack>(gmd);
                var newModelPack = new ModelPack();
                newModelPack.Version = modelPack.Version;
                newModelPack.Textures = modelPack.Textures;
                newModelPack.Materials = modelPack.Materials;
                newModelPack.Model = modelPack.Model;
                newModelPack.AnimationPack = modelPack.AnimationPack;
                newModelPack.Save(gmd);
            }
        }

        private static void ReplaceAnimations()
        {
            var newCharDefaultModel = ModuleImportUtilities.ImportFile<ModelPack>(Input.newDefaultModel);
            var ogCharDefaultModel = ModuleImportUtilities.ImportFile<ModelPack>(Input.ogDefaultModel);
            for (int i = 0; i < Input.ogAnims.Length; i++)
            {
                bool foundMatch = false;
                //Set destination to GMD folder if name contains emt
                string newDestination = "";
                if (Input.ogAnims[i].Contains("emt"))
                    newDestination = Path.Combine(Input.retargetFolder, Path.GetFileName(Input.ogAnims[i]));
                else
                {
                    string gapFolder = Path.Combine(Input.retargetFolder, Path.GetFileName(Path.GetDirectoryName(Input.ogAnims[i])));
                    if (!Directory.Exists(gapFolder))
                        Directory.CreateDirectory(gapFolder);
                    newDestination = Path.Combine(gapFolder, Path.GetFileName(Input.ogAnims[i]));
                }

                for (int x = 0; x < Input.newAnims.Length; x++)
                {
                    //If filenames match except for character ID, replace new animation with old one
                    string renamedNewAnim = Path.GetFileName(Input.newAnims[x]).Replace(Path.GetFileName(Options.New), Path.GetFileName(Options.Old));
                    if (Path.GetFileName(Input.ogAnims[i]) == renamedNewAnim)
                    {
                        foundMatch = true;
                        //Delete new gap if it exists
                        if (File.Exists(newDestination))
                            File.Delete(newDestination);

                        if (Options.GAP.Replace)
                        {
                            //Copy gap to new location
                            File.Copy(Input.newAnims[x], newDestination);
                            Console.WriteLine($"Copying {Path.GetFileName(Input.newAnims[x])} to replace {Path.GetFileName(Input.ogAnims[i])}");
                        }
                        else if (Options.GAP.Retarget)
                        {
                            //Save new retargeted gap
                            var animationPack = Resource.Load<AnimationPack>(Input.ogAnims[i]);
                            animationPack.Retarget(ogCharDefaultModel.Model, newCharDefaultModel.Model, Options.GAP.FixArms);
                            animationPack.Save(newDestination);
                            Console.WriteLine($"Retargeting {Path.GetFileName(Input.ogAnims[i])} comparing {Path.GetFileName(Input.ogDefaultModel)} to {Path.GetFileName(Input.newDefaultModel)}");
                        }
                    }
                }
                if (!foundMatch && Options.GAP.Retarget)
                {
                    //Delete new gap if it exists
                    if (File.Exists(newDestination))
                        File.Delete(newDestination);
                    //Save new retargeted gap
                    var animationPack = Resource.Load<AnimationPack>(Input.ogAnims[i]);
                    animationPack.Retarget(ogCharDefaultModel.Model, newCharDefaultModel.Model, Options.GAP.FixArms);
                    animationPack.Save(newDestination);
                    Console.WriteLine($"Retargeting {Path.GetFileName(Input.ogDefaultModel)} to {Path.GetFileName(Input.ogAnims[i])}");
                }
            }
        }

        private static void ReplaceModels()
        {
            for (int i = 0; i < Input.ogModels.Length; i++)
            {
                bool foundMatch = false;
                for (int x = 0; x < Input.newModels.Length; x++)
                {
                    //If filenames match except for character ID, replace new model with old one
                    string renamedNewModel = Path.GetFileName(Input.newModels[x]).Replace(Path.GetFileName(Options.New), Path.GetFileName(Options.Old));
                    if (Path.GetFileName(Input.ogModels[i]) == renamedNewModel)
                    {
                        if (Options.GMD.Retarget)
                        {
                            //Retarget matching new model to old model's IDs, using default model when no match is found
                            var originalModelPack = ModuleImportUtilities.ImportFile<ModelPack>(Input.ogModels[i]);
                            var newModelPack = ModuleImportUtilities.ImportFile<ModelPack>(Input.newModels[x]);
                            originalModelPack.ReplaceWith(newModelPack);
                            originalModelPack.Save(Path.Combine(Input.retargetFolder, Path.GetFileName(Input.ogModels[i])));
                            Console.WriteLine($"Retargeting {Path.GetFileName(Input.newModels[x])} to {Path.GetFileName(Input.ogModels[i])}");
                        }
                        else if (Options.GMD.Replace)
                        {
                            //If not retargeting, copy file
                            foundMatch = true;
                            File.Copy(Input.newModels[x], Path.Combine(Input.retargetFolder, Path.GetFileName(Input.ogModels[i])));
                            Console.WriteLine($"Copying {Path.GetFileName(Input.newModels[x])} to replace {Path.GetFileName(Input.ogModels[i])}");
                        }
                    }
                }
                if (!foundMatch)
                {
                    if (Options.GMD.Retarget)
                    {
                        //Retarget default model
                        var originalModelPack = ModuleImportUtilities.ImportFile<ModelPack>(Input.ogModels[i]);
                        var newModelPack = ModuleImportUtilities.ImportFile<ModelPack>(Input.newDefaultModel);
                        originalModelPack.ReplaceWith(newModelPack);
                        originalModelPack.Save(Path.Combine(Input.retargetFolder, Path.GetFileName(Input.ogModels[i])));
                        Console.WriteLine($"Retargeting {Path.GetFileName(Input.newDefaultModel)} to {Path.GetFileName(Input.ogModels[i])}");
                    }
                    else if (Options.GMD.Replace)
                    {
                        //Copy default model to new location
                        File.Copy(Input.newDefaultModel, Path.Combine(Input.retargetFolder, Path.GetFileName(Input.ogModels[i])));
                        Console.WriteLine($"Copying {Path.GetFileName(Input.newDefaultModel)} to replace {Path.GetFileName(Input.ogModels[i])}");
                    }
                }
            }
        }

        public class ProgramOptions
        {
            [Option("o", "old", "directory", "The path to the P5 model/character/#### directory you want to replace.", Required = true)]
            public string Old { get; set; } = "";

            [Option("n", "new", "directory", "The path to the P5 model/character/#### directory you want to use as a replacement.", Required = true)]
            public string New { get; set; } = "";

            [Option("id", "default id", "integer", "When specified, the new character's GMD with a matching minor ID directory will be used for replacing/retargeting non-matching models. Otherwise, 051 is used (default battle model).")]
            public int DefaultGMDId { get; set; } = 51;

            [Option("d", "delete-chunks", "boolean", "When specified, new GMDs won't contain any physics chunks.")]
            public bool DeleteChunks { get; set; } = false;

            [Group("gmd")]
            public GMDOptions GMD { get; set; }

            public class GMDOptions
            {
                [Option("r", "replace", "boolean", "Replaces models with matching IDs.\nNon-matches will be replaced by the Default GMD (and retargeted if using --rt).")]
                public bool Replace { get; set; } = false;

                [Option("rt", "retarget", "boolean", "Retarget models with matching IDs. This improves compatibility with the original character's animations, but may break the new character's animations.\nNon-matches will be replaced/retargeted by the Default GMD.")]
                public bool Retarget { get; set; } = false;
            }

            [Group("gap")]
            public GAPOptions GAP { get; set; }

            public class GAPOptions
            {
                [Option("r", "replace", "boolean", "Replaces animation packs with matching IDs. Works well when not retargeting models.\nNon-matches will be left alone, unless using --rt.")]
                public bool Replace { get; set; } = false;

                [Option("rt", "retarget", "boolean", "Retargets new character's non-matching animation packs to the original character's model.\nUses the default GMD ID for retargeting.")]
                public bool Retarget { get; set; } = false;

                [Option("a", "fix-arms", "boolean", "Rotates arm bones 45 degrees when retargeting animations. Useful when replacing characters in a T-pose with characters in an A-pose.")]
                public bool FixArms { get; set; } = false;
            }
        }

        public static class Input
        {
            public static string[] ogModels { get; set; }
            public static string[] ogAnims { get; set; }
            public static string[] newModels { get; set; }
            public static string[] newAnims { get; set; }
            public static string retargetFolder { get; set; }
            public static int defaultModel { get; set; }
            public static string ogDefaultModel { get; set; }
            public static string newDefaultModel { get; set; }
        }
    }
}
