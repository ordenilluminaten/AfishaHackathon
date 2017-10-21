using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ATMIT.Web.Utility.Data;

namespace Models.Database.Tables {
    public class UserNotification : IEntity<Guid> {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        [ForeignKey(nameof(UserEvent))]
        public Guid IdUserEvent { get; set; }
        public UserEvent UserEvent { get; set; }
        public DateTime Date { get; set; }
        public UserNotificationType Type { get; set; }
        [ForeignKey(nameof(User))]
        public int IdUser { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(UserFrom))]
        public int? IdUserFrom { get; set; }
        public User UserFrom { get; set; }
    }

    public enum UserNotificationType {
        NewOffer,
        OfferAccepted,
        OfferRejected
    }
}
