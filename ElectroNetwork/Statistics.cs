using System;
using System.Collections.Generic;
using System.Linq;
using ElectroNetwork.Models;

namespace ElectroNetwork;

public static class Statistics
{
    public static int ElementCount(List<Node> nodes, List<Substation> substations, List<Switch> switches, List<Wire> wires)
    {
        return nodes.Count + substations.Count + switches.Count + wires.Count;
    }


    // ReSharper disable once InconsistentNaming
    public static Tuple<Tuple<double, double>, Tuple<double, double>> MinMaxXY(List<Node> nodes, List<Substation> substations,
        List<Switch> switches, List<Wire> wires)
    {
        List<Point> points = new List<Point>();
        foreach (Node node in nodes)
        {
            points.Add(node.Point);
        }
        foreach (Substation substation in substations)
        {
            points.Add(substation.Point);
        }
        foreach (Switch sw in switches)
        {
            points.Add(sw.Point);
        }
        foreach (Wire wire in wires)
        {
            points.Add(wire.StartPoint);
            points.Add(wire.EndPoint);
        }

        double minX = points.Min(p => p.X);
        double minY = points.Min(p => p.Y);
        double maxX = points.Max(p => p.X);
        double maxY = points.Max(p => p.Y);

        return new Tuple<Tuple<double, double>, Tuple<double, double>>(
            new Tuple<double, double>(minX, maxX), new Tuple<double, double>(minY, maxY));
    }
}