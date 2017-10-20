using System.ComponentModel.DataAnnotations;

namespace Models.Afisha {
    public enum PlaceType {
        [Display(Name = "Все места")]
        All,
        [Display(Name = "Рестораны")]
        Restaurant,
        [Display(Name = "Концертные залы")]
        ConcertHall,
        [Display(Name = "Спорткомплексы")]
        SportBuilding,        
        [Display(Name = "Кинотеатры")]
        Cinema,
        [Display(Name = "Музеи")]
        Museum, 
        [Display(Name = "Театры")]       
        Theatre,      
        [Display(Name = "Фитнес центры")]  
        FitnessCenter,        
        [Display(Name = "Отели")]
        Hotel,        
        [Display(Name = "Магазины")]
        Shop,        
        [Display(Name = "Клубы")]
        Club,
        [Display(Name = "Парки")]        
        Park,   
        [Display(Name = "Галереи")]     
        Gallery,        
        [Display(Name = "Выставочный зал")]     
        ShowRoom
    }
}