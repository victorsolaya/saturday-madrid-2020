using VSanchez.Plugins;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;
using System.Collections.Generic;

namespace VSanchez.Plugins.UnitTests.Steps
{
    [Binding]
    public class Generic_Steps
    {
        public static XrmContext CRM { get; set; }
        public static IOrganizationService service { get; set; }

        public Generic_Steps()
        {
            CRM = new XrmContext();
            service = CRM.FakeXrmContext.GetOrganizationService();
        }

        #region GIVEN STEPS

        [Given(@"the entity ""(.*)"" known as (.*) exists with the fields")]
        public void GivenEntityNameKnownAs(string entityname, string known, Table fields)
        {

            Entity entity = new Entity(entityname);

            foreach (var row in fields.Rows)
            {

                var key = row["Field"];
                var value = row["Value"];
                CRM.AddAttributeToEntity(key, value, entityname, ref entity);
            }
            CRM.AddEntityToMockCRM(known, entity);
        }

        [Given(@"the entity name contains primary attribute name")]
        public void GivenEntityNameWithPrimaryNameField(Table table)
        {
            foreach (var row in table.Rows)
            {
                var entity = row["Field"];
                var attribute = row["Value"];
                CRM.AddPrimaryAttributeNameMetadataToMock(entity, attribute);
            }
        }

        #endregion
        [Then(@"the service should have (.*) records of the entity (.*)")]
        public void ThenTheServiceShouldHaveXRecordsOfEntity(int records, string entityname)
        {
            string getEntityRecords = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
            <entity name='{entityname}'>
            </entity>
            </fetch>";
            var entities = service.RetrieveMultiple(new FetchExpression(getEntityRecords));
            Assert.AreEqual(records, entities.Entities.Count);
            Assert.AreEqual(entityname.ToLower(), entities.Entities[0].LogicalName.ToLower());
        }

        [Then(@"the entity (.*) should have fields")]
        public void ThenTheEntityShouldHaveFields(string entityNameInBag, Table fields)
        {
            Entity entityInBag = (Entity)CRM.Bag[entityNameInBag];
            foreach (var row in fields.Rows)
            {

                var key = row["Field"];
                var value = row["Value"];
                var attributeValue = CheckAttributeInThen(key, entityInBag);
                Assert.AreEqual(value, attributeValue);
            }
        }

        #region GENERIC FUNCTIONS

        [When(@"all the entities are refreshed")]
        public void WhenAllTheEntitiesAreRefreshed()
        {
            Dictionary<string, Entity> allEntitiesInBag = new Dictionary<string, Entity>();
            foreach (var itemInBag in CRM.Bag)
            {
                if (itemInBag.Value.GetType().BaseType.Name == "Entity")
                {
                    allEntitiesInBag.Add(itemInBag.Key, (Entity)itemInBag.Value);
                }
            }
            foreach (var entityInBag in allEntitiesInBag)
            {
                if (entityInBag.Value.GetType().BaseType.Name == "Entity")
                {
                    CRM.RefreshEntityInMockCRM(entityInBag.Key);
                }
            }
        }

        public string CheckAttribute(string field, Entity entity)
        {
            var attribute = entity.Attributes[field];
            var fieldSource = string.Empty;
            switch (attribute.GetType().Name)
            {
                case "EntityReference":
                    var fieldReference = ((EntityReference)attribute);
                    if (fieldReference != null && fieldReference.Id != Guid.Empty)
                    {
                        var mainField = GetPrimaryFieldName(fieldReference.LogicalName);
                        var fieldName = service.Retrieve(fieldReference.LogicalName, fieldReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { mainField }));
                        fieldSource = fieldName?.Attributes[mainField]?.ToString()?.ToLower();
                    }
                    else
                    {
                        fieldSource = null;
                    }
                    break;
                case "OptionSetValue":
                    fieldSource = ((OptionSetValue)attribute)?.Value.ToString()?.ToLower();
                    break;
                case "DateTime":
                    fieldSource = ((DateTime)attribute) != null ? ((DateTime)attribute).ToString()?.ToLower() : null;
                    break;
                default:
                    fieldSource = attribute?.ToString()?.ToLower();
                    break;
            }
            return fieldSource;
        }

        public static string GetPrimaryFieldName(string entityname)
        {
            RetrieveEntityRequest retrievesEntityRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Entity,
                LogicalName = entityname
            };

            //Execute Request
            RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)service.Execute(retrievesEntityRequest);
            // Gets the primaryid attribute
            // var idFieldName = retrieveEntityResponse.EntityMetadata.PrimaryIdAttribute;

            // Gets the primary field name
            return retrieveEntityResponse.EntityMetadata.PrimaryNameAttribute;
        }

        public string CheckAttributeInThen(string field, Entity entity)
        {
            var attribute = entity.Attributes[field];
            string fieldSource;
            switch (attribute.GetType().Name)
            {
                case "EntityReference":
                    var fieldReference = ((EntityReference)attribute);
                    if (fieldReference != null && fieldReference.Id != Guid.Empty)
                    {
                        var mainField = GetPrimaryFieldName(fieldReference.LogicalName);
                        var fieldName = service.Retrieve(fieldReference.LogicalName, fieldReference.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(new string[] { mainField }));
                        fieldSource = fieldName?.Attributes[mainField]?.ToString();
                    }
                    else
                    {
                        fieldSource = null;
                    }
                    break;
                case "OptionSetValue":
                    fieldSource = ((OptionSetValue)attribute)?.Value.ToString();
                    break;
                case "DateTime":
                    fieldSource = ((DateTime)attribute) != null ? ((DateTime)attribute).ToString() : null;
                    break;
                default:
                    fieldSource = attribute?.ToString();
                    break;
            }
            return fieldSource;
        }

        #endregion
    }
}
