using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using VSanchez.Plugins.EntityMetadata;

namespace VSanchez.Plugins
{
    public class AccountPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            if (service == null) { return; }
            if (!context.InputParameters.Contains("Target")) { return; }
            //Let's get the target account as entity
            var accountTarget = context.InputParameters["Target"] as Entity;

            // Check the target is not null
            if (accountTarget == null || accountTarget.Id == Guid.Empty) { return; }

            var account = new Account();
            //Transform the entity to Account
            try
            {
                account = service.Retrieve(Account.EntityLogicalName, accountTarget.Id, new ColumnSet(new string[] { "accountid", "address1_city", "address1_country" })).ToEntity<Account>();
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"There was an issue retreving the account: {ex.Message}");
            }

            //Get the contacts from the account
            var contactsFromAccount = GetContacts(service, account.Id).ToList();

            tracingService.Trace($"There were {contactsFromAccount.Count} contacts retrieved");
            //Update the contacts with the same information as the account
            try
            {
                UpdateContactsFromAccount(service, account, contactsFromAccount);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"There was an error updating the contacts: {ex.Message}");
            }
        }

        private IQueryable<Contact> GetContacts(IOrganizationService service, Guid accountid)
        {
            IQueryable<Contact> contactsQuery = null;
            using (CrmServiceContext svcContext = new CrmServiceContext(service))
            {

                contactsQuery = (from contact in svcContext.ContactSet
                                 join account in svcContext.AccountSet
                                     on contact.ParentCustomerId.Id equals account.AccountId
                                 where account.AccountId == accountid
                                 select contact);
            }
            return contactsQuery;
        }

        private void UpdateContactsFromAccount(IOrganizationService service, Account account, List<Contact> contacts)
        {
            var accountCity = account.Address1_City;
            var accountCountry = account.Address1_Country;
            foreach (var contact in contacts)
            {
                var contactToUpdate = new Contact()
                {
                    ContactId = contact.Id,
                    Address1_City = accountCity,
                    Address1_Country = accountCountry
                };
                try
                {
                    service.Update(contactToUpdate);

                }
                catch (Exception ex)
                {

                    throw new InvalidPluginExecutionException($"There was an error updating the contact: {ex.Message}");
                }
            }
        }
    }
}
