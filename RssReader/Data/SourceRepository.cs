using Microsoft.EntityFrameworkCore;
using RssReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RssReader.Data
{
    public class SourceRepository
    {
        private readonly DatabaseContext _context;
        
        public SourceRepository(DatabaseContext context)
        {
            _context = context;
        }
        
        public async Task<List<Source>> GetAllSourcesAsync()
        {
            return await _context.Sources.ToListAsync();
        }
        
        public async Task<Source> GetSourceByIdAsync(int id)
        {
            return await _context.Sources.FindAsync(id);
        }
        
        public async Task<Source> AddSourceAsync(Source source)
        {
            _context.Sources.Add(source);
            await _context.SaveChangesAsync();
            return source;
        }
        
        public async Task<bool> UpdateSourceAsync(Source source)
        {
            _context.Entry(source).State = EntityState.Modified;
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
        
        public async Task<bool> DeleteSourceAsync(int id)
        {
            var source = await _context.Sources.FindAsync(id);
            if (source == null)
            {
                return false;
            }
            
            _context.Sources.Remove(source);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
