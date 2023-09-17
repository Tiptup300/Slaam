﻿using System.Collections.Generic;

namespace Tiptup300.Slaam.States.Credits;

public struct CreditsListing
{
   public string Name;
   public List<string> Credits;
   public CreditsListing(string name, List<string> credits)
   {
      Name = name;
      Credits = credits;
   }
}