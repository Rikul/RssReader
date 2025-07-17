using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RssReader.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(300)]
        public string Title { get; set; }
        
        [MaxLength(1000)]
        public string Summary { get; set; }
        
        public string Content { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Link { get; set; }
        
        public DateTime PublishDate { get; set; }
        
        public bool IsRead { get; set; }
        
        public bool IsFavorite { get; set; }
        
        public int SourceId { get; set; }
        
        [ForeignKey("SourceId")]
        public virtual Source Source { get; set; }
        
        public Article()
        {
            PublishDate = DateTime.Now;
            IsRead = false;
            IsFavorite = false;
        }
    }
}