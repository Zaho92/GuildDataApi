﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GuildDataApi.Models
{
    public partial class User
    {
        public int IdUser { get; set; }
        public string Username { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        [JsonIgnore]
        public string Salt { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Phonenumber { get; set; }
        public int FkRightsTemplates { get; set; }

        public virtual RightsTemplate FkRightsTemplatesNavigation { get; set; }
    }
}