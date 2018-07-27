﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SuiMerger
{
    public class PS3InstructionReader : IDisposable
    {
        public XmlReader reader;

        public PS3InstructionReader(System.IO.Stream stream)
        {
            reader = XmlReader.Create(stream, new XmlReaderSettings());
        }

        //returns true if there is an instruction left, false if no instructions left
        public bool AdvanceToNextInstruction()
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //Console.WriteLine("Attributes of <" + reader.Name + ">");

                        if (reader.Name == "ins")
                        {
                            return true;
                        }
                        break;
                    case XmlNodeType.Text:
                        if (reader.Value.Trim() != "")
                        {
                            Console.WriteLine("Text Node: {0}", reader.Value);
                            throw new Exception("non empty 'Text' node!");
                        }
                        break;
                    case XmlNodeType.EndElement:
                        //Console.WriteLine("End Element {0}", reader.Name);
                        break;

                    case XmlNodeType.XmlDeclaration:
                        Console.WriteLine("Skipping XML Declaration");
                        break;

                    default:
                        if (reader.Value.Trim() != "")
                        {
                            Console.WriteLine("Other node {0} with value {1}",
                                            reader.NodeType, reader.Value);
                            throw new Exception("non empty 'other' node!");
                        }
                        break;
                }
            }

            return false;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }


    public class PS3XMLReader
    {
        public static List<PS3DialogueInstruction> GetPS3DialoguesFromXML(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return GetPS3DialoguesFromXML(fs);
            }
        }

        public static List<PS3DialogueInstruction> GetPS3DialoguesFromXML(System.IO.Stream stream)
        {
            List<PS3DialogueInstruction> dialogueInstructions = new List<PS3DialogueInstruction>();

            List<string> previousXML = new List<string>();

            using (PS3InstructionReader ps3Reader = new PS3InstructionReader(stream))
            {
                while(ps3Reader.AdvanceToNextInstruction())
                {
                    if (ps3Reader.reader.GetAttribute("type") == "DIALOGUE")
                    {
                        int num = Convert.ToInt32(ps3Reader.reader.GetAttribute("num"));
                        int dlgtype = Convert.ToInt32(ps3Reader.reader.GetAttribute("dlgtype"));
                        string data = ps3Reader.reader.GetAttribute("data");
                        dialogueInstructions.Add(new PS3DialogueInstruction(num, dlgtype, data, previousXML, ps3Reader.reader.ReadOuterXml()));
                        previousXML.Clear();
                    }
                    else
                    {
                        //store previous xml nodes before each Dialogue line
                        string outerXML = ps3Reader.reader.ReadOuterXml();
                        previousXML.Add(outerXML);
                    }
                }
            }

            return dialogueInstructions;
        }

    }
}
