﻿using Domain.Models.ProspectList;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.Networking.GatherProspects.Interfaces
{
    public interface IGatherProspectsInteractionHandler : IInteractionHandler
    {
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; }
        public IList<IWebElement> Prospects { get; }
    }
}