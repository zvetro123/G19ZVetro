using System;
using System.Collections.Generic;
using System.Text;

namespace G19ZVetro.Models;

public class Profile
{
    public string Name { get; set; } = "";

    public int Red { get; set; }

    public int Green { get; set; }

    public int Blue { get; set; }

    public Dictionary<string, GKeyAction> GKeys { get; set; } =
        new()
        {
            ["G1"] = new(),
            ["G2"] = new(),
            ["G3"] = new(),
            ["G4"] = new(),
            ["G5"] = new(),
            ["G6"] = new(),
            ["G7"] = new(),
            ["G8"] = new(),
            ["G9"] = new(),
            ["G10"] = new(),
            ["G11"] = new(),
            ["G12"] = new()
        };
}