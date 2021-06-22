using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EatCleanAPI.Models;

namespace EatCleanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuDetailsController : ControllerBase
    {
        private readonly VegafoodBotContext _context;

        public MenuDetailsController(VegafoodBotContext context)
        {
            _context = context;
        }

        // GET: api/MenuDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuDetails>>> GetMenuDetails()
        {
            return await _context.MenuDetails.ToListAsync();
        }

        // GET: api/MenuDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuDetails>> GetMenuDetails(int id)
        {
            var menuDetails = await _context.MenuDetails.FindAsync(id);

            if (menuDetails == null)
            {
                return NotFound();
            }

            return menuDetails;
        }

        // PUT: api/MenuDetails/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenuDetails(int id, MenuDetails menuDetails)
        {
            if (id != menuDetails.MenuId)
            {
                return BadRequest();
            }

            _context.Entry(menuDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuDetailsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MenuDetails
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<MenuDetails>> PostMenuDetails(MenuDetails menuDetails)
        {
            _context.MenuDetails.Add(menuDetails);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MenuDetailsExists(menuDetails.MenuId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetMenuDetails", new { id = menuDetails.MenuId }, menuDetails);
        }

        // DELETE: api/MenuDetails/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MenuDetails>> DeleteMenuDetails(int id)
        {
            var menuDetails = await _context.MenuDetails.FindAsync(id);
            if (menuDetails == null)
            {
                return NotFound();
            }

            _context.MenuDetails.Remove(menuDetails);
            await _context.SaveChangesAsync();

            return menuDetails;
        }

        private bool MenuDetailsExists(int id)
        {
            return _context.MenuDetails.Any(e => e.MenuId == id);
        }
    }
}
