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

    public partial class VENDORCONTACT
    {
        public int PRIMARYKEY { get; set; }
        public string Id { get; set; }
        [Display(Name = "Secondary Contact's Name")]
        public string CONTACTNAME { get; set; }
        [Display(Name = "Secondary Contact's Phone")]
        public string CONTACTPHONE { get; set; }
        [Display(Name = "Secondary Contact's Email")]
        public string CONTACTEMAIL { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }
    }
}
