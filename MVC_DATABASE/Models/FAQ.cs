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


    public partial class FAQ
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Question")]
        [DataType(DataType.Text)]
        public string QUESTION { get; set; }
        [Required]
        [Display(Name = "Answer")]
        [DataType(DataType.Text)]
        public string ANSWER { get; set; }
    }
}
