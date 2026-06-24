using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using MobileShop.ViewModels;

namespace MobileShop.Controllers;

public class ContactController : Controller
{
    // GET: /Contact
    public ActionResult Index()
    {
        return View(new ContactViewModel());
    }

    // POST: /Contact
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(ContactViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Option A: Send email via SmtpClient
        // SendEmail(model);

        // Option B: Save to database
        // SaveToDb(model);

        TempData["Success"] = "Thank you! Your message has been sent.";
        return RedirectToAction("Index");
    }

    private void SendEmail(ContactViewModel model)
    {
        var mail = new MailMessage();
        mail.To.Add("momincse13@gmail.com");
        mail.From = new MailAddress(model.Email, model.Name);
        mail.Subject = model.Subject;
        mail.Body = model.Message;

        using (var smtp = new SmtpClient())
        {
            // Configure in Web.config <system.net><mailSettings>
            smtp.Send(mail);
        }
    }
}