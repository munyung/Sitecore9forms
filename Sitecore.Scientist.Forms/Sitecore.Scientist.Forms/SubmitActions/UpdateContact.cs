using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Analytics;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.Configuration;
using Sitecore.XConnect.Collection.Model;

namespace Sitecore.Scientist.Forms.SubmitActions
{

    /// <summary>
    /// Submit action for updating <see cref="PersonalInformation"/> and <see cref="EmailAddressList"/> facets of a <see cref="XConnect.Contact"/>.
    /// </summary>
    /// <seealso cref="Sitecore.ExperienceForms.Processing.Actions.SubmitActionBase{UpdateContactData}" />
    public class UpdateContact : SubmitActionBase<UpdateContactData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateContact"/> class.
        /// </summary>
        /// <param name="submitActionData">The submit action data.</param>
        public UpdateContact(ISubmitActionData submitActionData) : base(submitActionData)
        {
        }
        /// <summary>
        /// Gets the current tracker.
        /// </summary>
        protected virtual ITracker CurrentTracker => Tracker.Current;
        /// <summary>
        /// Executes the action with the specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="formSubmitContext">The form submit context.</param>
        /// <returns><c>true</c> if the action is executed correctly; otherwise <c>false</c></returns>
        protected override bool Execute(UpdateContactData data, FormSubmitContext formSubmitContext)
        {
            Assert.ArgumentNotNull(data, nameof(data));
            Assert.ArgumentNotNull(formSubmitContext, nameof(formSubmitContext));
            var firstNameField = GetFieldById(data.FirstNameFieldId, formSubmitContext.Fields);
            var lastNameField = GetFieldById(data.LastNameFieldId, formSubmitContext.Fields);
            var emailField = GetFieldById(data.EmailFieldId, formSubmitContext.Fields);
            if (firstNameField == null && lastNameField == null && emailField == null)
            {
                return false;
            }
            using (var client = CreateClient())
            {
                try
                {
                    var source = "Subcribe.Form";
                    var id = CurrentTracker.Contact.ContactId.ToString("N");
                    CurrentTracker.Session.IdentifyAs(source, id);
                    var trackerIdentifier = new IdentifiedContactReference(source, id);
                    var expandOptions = new ContactExpandOptions(
                        CollectionModel.FacetKeys.PersonalInformation,
                        CollectionModel.FacetKeys.EmailAddressList);
                    Contact contact = client.Get(trackerIdentifier, expandOptions);
                    SetPersonalInformation(GetValue(firstNameField), GetValue(lastNameField), contact, client);
                    SetEmail(GetValue(emailField), contact, client);
                    client.Submit();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, ex);
                    return false;
                }
            }
        }
        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <returns>The <see cref="IXdbContext"/> instance.</returns>
        protected virtual IXdbContext CreateClient()
        {
            return SitecoreXConnectClientConfiguration.GetClient();
        }
        /// <summary>
        /// Gets the field by <paramref name="id" />.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>The field with the specified <paramref name="id" />.</returns>
        private static IViewModel GetFieldById(Guid id, IList<IViewModel> fields)
        {
            return fields.FirstOrDefault(f => Guid.Parse(f.ItemId) == id);
        }
        /// <summary>
        /// Gets the <paramref name="field" /> value.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The field value.</returns>
        private static string GetValue(object field)
        {
            return field?.GetType().GetProperty("Value")?.GetValue(field, null)?.ToString() ?? string.Empty;
        }
        /// <summary>
        /// Sets the <see cref="PersonalInformation"/> facet of the specified <paramref name="contact" />.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="contact">The contact.</param>
        /// <param name="client">The client.</param>
        private static void SetPersonalInformation(string firstName, string lastName, Contact contact, IXdbContext client)
        {
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
            {
                return;
            }
            PersonalInformation personalInfoFacet = contact.Personal() ?? new PersonalInformation();
            if (personalInfoFacet.FirstName == firstName && personalInfoFacet.LastName == lastName)
            {
                return;
            }
            personalInfoFacet.FirstName = firstName;
            personalInfoFacet.LastName = lastName;
            client.SetPersonal(contact, personalInfoFacet);
        }
        /// <summary>
        /// Sets the <see cref="EmailAddressList"/> facet of the specified <paramref name="contact" />.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <param name="contact">The contact.</param>
        /// <param name="client">The client.</param>
        private static void SetEmail(string email, Contact contact, IXdbContext client)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }
            EmailAddressList emailFacet = contact.Emails();
            if (emailFacet == null)
            {
                emailFacet = new EmailAddressList(new EmailAddress(email, false), "Preferred");
            }
            else
            {
                if (emailFacet.PreferredEmail?.SmtpAddress == email)
                {
                    return;
                }
                emailFacet.PreferredEmail = new EmailAddress(email, false);
            }
            client.SetEmails(contact, emailFacet);
        }
    }
    public class UpdateContactData
    {
        public Guid FirstNameFieldId { get; set; }
        public Guid LastNameFieldId { get; set; }
        public Guid EmailFieldId { get; set; }
    }
}
