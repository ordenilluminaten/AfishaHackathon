using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ATMIT.Web.Utility.Data;

namespace Models {
    public class User : IEntity {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Avatar { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool CanRecieveGroupMessages { get; set; }
        public bool IsFamiliarWithBot { get; set; }
        public DateTime LastEnter { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public int? IdCity {get;set;}
        public float? Longitude {get;set;}
        public float? Latitude {get;set;}
        public virtual ICollection<UserEvent> UserEvents { get; set; }

        public void SetUserData(User _user) {
            Avatar = _user.Avatar;
            FirstName = _user.FirstName;
            LastName = _user.LastName;
            CanRecieveGroupMessages = _user.CanRecieveGroupMessages;
            IsFamiliarWithBot = _user.IsFamiliarWithBot;
            LastEnter = _user.LastEnter;
            Longitude = _user.Longitude;
            Latitude = _user.Latitude;
            IdCity = _user.IdCity;
        }
    }
}