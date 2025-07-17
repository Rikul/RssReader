using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RssReader.Models
{
    public class Source
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Url { get; set; }
        
        [MaxLength(200)]
        public string Category { get; set; }
        
        public DateTime LastUpdated { get; set; }
        
        public virtual ICollection<Article> Articles { get; set; }
        
        public Source()
        {
            Articles = new HashSet<Article>();
            LastUpdated = DateTime.Now;
        }
    }
}