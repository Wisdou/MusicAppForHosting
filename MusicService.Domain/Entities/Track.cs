using System.ComponentModel.DataAnnotations;

namespace MusicService.Domain.Entities;

public class Track : Entity
{
    public required DateTimeOffset CreationDateTime { get; init; }
    
    public required string Title { get; init; }
    
    public required TrackGenre Genre { get; init; }
    
    public required byte[] Audio { get; init; }
    
    public required TimeSpan Duration { get; init; }
    
    public required string Performer { get; init; }
    
    public byte[]? Cover { get; init; }
}

public enum TrackGenre
{
    [Display(Name = "Не определен")]
    Undefined,
    
    [Display(Name = "Поп-музыка")]
    Pop,

    [Display(Name = "Рок")]
    Rock,

    [Display(Name = "Джаз")]
    Jazz,

    [Display(Name = "Блюз")]
    Blues,

    [Display(Name = "Классическая музыка")]
    Classical,

    [Display(Name = "Хип-хоп")]
    HipHop,

    [Display(Name = "Рэп")]
    Rap,

    [Display(Name = "Электронная музыка")]
    Electronic,

    [Display(Name = "Танцевальная музыка")]
    Dance,

    [Display(Name = "R&B")]
    RnB,

    [Display(Name = "Кантри")]
    Country,

    [Display(Name = "Метал")]
    Metal,

    [Display(Name = "Панк")]
    Punk,

    [Display(Name = "Соул")]
    Soul,

    [Display(Name = "Диско")]
    Disco,

    [Display(Name = "Техно")]
    Techno,

    [Display(Name = "Хаус")]
    House,

    [Display(Name = "K-Pop")]
    KPop,

    [Display(Name = "Опера")]
    Opera,
}