using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ATMIT.Web.Utility.Data;

namespace Models {
    public class UserEventOffer : IEntity<Guid> {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(User))]
        public int IdUser { get; set; }
        public virtual User User { get; set; }

        [ForeignKey(nameof(UserEvent))]
        public Guid IdUserEvent { get; set; }
        public virtual UserEvent UserEvent { get; set; }

        public DateTime Date { get; set; }

        public string Comment { get; set; }

        public CompanionState State { get; set; }
    }

    public enum CompanionState {
        Pending,
        Accepted,
        Rejected
    }
}