using Microsoft.EntityFrameworkCore;
using RssReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RssReader.Data
{
    public class ArticleRepository
    {
        private readonly DatabaseContext _context;
        
        public ArticleRepository(DatabaseContext context)
        {
            _context = context;
        }
        
        public async Task<List<Article>> GetArticlesBySourceIdAsync(int sourceId)
        {
            return await _context.Articles
                .Where(a => a.SourceId == sourceId)
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }
        
        public async Task<List<Article>> GetUnreadArticlesAsync()
        {
            return await _context.Articles
                .Where(a => !a.IsRead)
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }
        
        public async Task<List<Article>> GetFavoriteArticlesAsync()
        {
            return await _context.Articles
                .Where(a => a.IsFavorite)
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }
        
        public async Task<Article> GetArticleByIdAsync(int id)
        {
            return await _context.Articles.FindAsync(id);
        }
        
        public async Task<Article> AddArticleAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }
        
        public async Task<bool> UpdateArticleAsync(Article article)
        {
            _context.Entry(article).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }
        
        public async Task<bool> MarkArticleAsReadAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return false;
            }
            
            article.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ToggleArticleFavoriteAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return false;
            }
            
            article.IsFavorite = !article.IsFavorite;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteOldArticlesAsync(int maxArticlesPerFeed)
        {
            var sources = await _context.Sources.ToListAsync();
            
            foreach (var source in sources)
            {
                var articlesToKeep = await _context.Articles
                    .Where(a => a.SourceId == source.Id)
                    .OrderByDescending(a => a.PublishDate)
                    .Take(maxArticlesPerFeed)
                    .Select(a => a.Id)
                    .ToListAsync();
                
                var articlesToDelete = await _context.Articles
                    .Where(a => a.SourceId == source.Id && !articlesToKeep.Contains(a.Id))
                    .ToListAsync();
                
                _context.Articles.RemoveRange(articlesToDelete);
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
