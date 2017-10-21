using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ATMIT.Web.Utility.Data;

namespace Models.Database.Tables {
    public class UserEventNotification : IEntity<Guid> {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public bool IsSended { get; set; }
        public string IdPlace { get; set; }
        public DateTime Date { get; set; }
        [ForeignKey(nameof(User))]
        public int IdUser { get; set; }
        public User User { get; set; }

    }
}
