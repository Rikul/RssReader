using Microsoft.EntityFrameworkCore;
using RssReader.Models;
using System.Threading.Tasks;

namespace RssReader.Data
{
    public class SettingsRepository
    {
        private readonly DatabaseContext _context;
        
        public SettingsRepository(DatabaseContext context)
        {
            _context = context;
        }
        
        public async Task<Settings> GetSettingsAsync()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new Settings { Id = 1 };
                _context.Settings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }
        
        public async Task<bool> UpdateSettingsAsync(Settings settings)
        {
            _context.Entry(settings).State = EntityState.Modified;
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
    }
}