using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ATMIT.Web.Utility.Data;

namespace Models {
    public enum Gender {
        [Display(Name = "Мужской")]
        Male = 0,
        [Display(Name = "Женский")]
        Female,
        [Display(Name = "Любой")]
        Any
    }

    public class UserEventVM {
        public string Id { get; set; }
        
        public int IdUser { get; set; }

        [Required]
        public string IdEvent { get; set; }

        [Range(1, 10)]
        public int UserCount { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }        
    }

    public class UserEvent : IEntity<Guid> {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(User))]
        public int IdUser { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int IdEvent { get; set; }

        [Range(1, 10)]
        public int UserCount { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }

        public virtual ICollection<UserEventOffer> Offers { get; set; }
    }
}
