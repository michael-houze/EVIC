﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_DATABASE.Models.ViewModels
{
    public class AdminDashboard
    {
        // The number of vendors who has a status
        public int pendingVendors;

        public string calendarEvents;

        // An array of CONTRACTs with an expired status
        public List<ContractSummary> contractSummaries;

    }
}