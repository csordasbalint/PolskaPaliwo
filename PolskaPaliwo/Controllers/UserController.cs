using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PolskaPaliwo.Models;

namespace PolskaPaliwo.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> FeedbackSave(int ratingScore)
        {
            var userId = _userManager.GetUserId(this.User);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.LastFeedback = ratingScore;
                    await _userManager.UpdateAsync(user);
                }
                return Ok();
            }
            return Ok();
        }

    }
}
