//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MVC_DATABASE.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class PRODUCTCATEGORY
    {
        public PRODUCTCATEGORY()
        {
            this.ANALYTICS = new HashSet<ANALYTIC>();
        }
        [Display(Name = "Category")]
        public string CATEGORY { get; set; }

        public virtual ICollection<ANALYTIC> ANALYTICS { get; set; }
    }
}
