using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ElectroNetwork.Models;

namespace ElectroNetwork;

public static class DataAccess
{
        private static void InitWires(XmlDocument xmlDocument, List<Wire> wires)
        {
            XmlNode wireNodes = xmlDocument.SelectSingleNode("//Lines");

            foreach (XmlNode wire in wireNodes.ChildNodes)
            {
                GetNodeValue<string>(wire, "Id", out string id);
                GetNodeValue<string>(wire, "Name", out string name);
                GetNodeValue<bool>(wire, "IsUnderground", out bool isUnderground);
                GetNodeValue<double>(wire, "R", out double r);
                GetNodeValue<string>(wire, "ConductorMaterial", out string conductorMaterial);
                GetNodeValue<string>(wire, "LineType", out string lineType);
                GetNodeValue<int>(wire, "ThermalConstantHeat", out int thermalConstantHeat);
                GetNodeValue<int>(wire, "FirstEnd", out int firstEnd);
                GetNodeValue<int>(wire, "SecondEnd", out int secondEnd);
                XmlNode verticesNode = wire.SelectSingleNode("Vertices");
                XmlNodeList pointNodes = verticesNode.SelectNodes("Point");
                
                if (pointNodes.Count >= 2)
                    wires.Add(new Wire(id, name,
                        isUnderground, r, lineType,
                        thermalConstantHeat, firstEnd, secondEnd,
                        new Point(double.Parse(pointNodes[0].SelectSingleNode("X").InnerText),
                            double.Parse(pointNodes[0].SelectSingleNode("Y").InnerText)),
                        new Point(double.Parse(pointNodes[pointNodes.Count - 1].SelectSingleNode("X").InnerText),
                            double.Parse(pointNodes[pointNodes.Count - 1].SelectSingleNode("Y").InnerText))));
            }
        }


        private static void InitSwitches(XmlDocument xmlDocument, List<Switch> switches)
        {
            XmlNode swNodes = xmlDocument.SelectSingleNode("//Switches");

            foreach(XmlNode sw in swNodes.ChildNodes)
            {
                GetNodeValue<string>(sw, "Id", out string? id);
                GetNodeValue<string>(sw, "Name", out string? name);
                var r1 = GetNodeValue<double>(sw, "X", out double x);
                var r2 = GetNodeValue<double>(sw, "Y", out double y);

                if (r1 && r2)
                    switches.Add(new Switch(id, name, new Point(x, y)));
                
            } 

        }


        private static void InitNodes(XmlDocument xmlDocument, List<Node> nodes)
        {
            XmlNode nodesNode = xmlDocument.SelectSingleNode("//Nodes");

            foreach(XmlNode node in nodesNode.ChildNodes)
            {
                GetNodeValue<string>(node, "Id", out string id);
                GetNodeValue<string>(node, "Name", out string name);
                var r1 = GetNodeValue<double>(node, "X", out double x);
                var r2 = GetNodeValue<double>(node, "Y", out double y);

                if (r1 && r2)
                    nodes.Add(new Node(id, name, new Point(x, y)));
            } 
        }



        private static void InitSubstations(XmlDocument xmlDocument, List<Substation> substations)
        {
            XmlNode substationsNode = xmlDocument.SelectSingleNode("//Substations");

            foreach(XmlNode substationNode in substationsNode.ChildNodes)
            {
                GetNodeValue<string>(substationNode, "Id", out string id);
                GetNodeValue<string>(substationNode, "Name", out string name);
                var r1 = GetNodeValue<double>(substationNode, "X", out double x);
                var r2 = GetNodeValue<double>(substationNode, "Y", out double y);
                
                if(r1 && r2)
                    substations.Add(new Substation(id, name, new Point(x, y)));
            }
        }


        public static bool TryParse<T>(string s, out T result)
        {
            result = default(T);

            Type type = typeof(T);

            if (typeof(T) == typeof(string))
            {
                result = (T)(object)s;
                return true;
            }
            
            MethodInfo tryParseMethod = type.GetMethod("TryParse", new[] { typeof(string), type.MakeByRefType() });

            

            if (tryParseMethod != null)
            {
                object[] args = { s, result };
                bool success = (bool)tryParseMethod.Invoke(null, args);

                result = (T)args[1];
                return success;
            }

            return false;
        }
        
        private static bool GetNodeValue<T>(XmlNode node,string name, out T value)
        {
            var dataContainer= node.SelectSingleNode(name);

            if (dataContainer == null)
            {
                value = default(T);
                return false;
            }

            return TryParse<T>(dataContainer.InnerText, out value);
        }
        
        public static void InitData(List<Node> nodes, List<Substation> substations,
            List<Switch> switches, List<Wire> wires)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            string path = System.IO.Path.Combine(solutionDirectory, "Content", "Geographic.xml");
            XmlDocument xmlDocument = new XmlDocument();
            if (File.Exists(path))
            {
                xmlDocument.Load(path);
            }

            XmlNode rootNode = xmlDocument.SelectSingleNode("NetworkModel");

            InitSwitches(xmlDocument, switches);
            InitSubstations(xmlDocument, substations);
            InitNodes(xmlDocument, nodes);
            InitWires(xmlDocument, wires);
        }
}