using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VSanchez.Plugins.UnitTests
{
    public class XrmContext
    {

        public Dictionary<string, object> Bag { get; set; }
        public Dictionary<string, Entity> LastEntity { get; set; }

        public XrmFakedContext FakeXrmContext { get; set; }
        public XrmFakedPluginExecutionContext FakeXrmContextPlugin { get; set; }

        IOrganizationService service { get; set; }


        public XrmContext()
        {
            Bag = new Dictionary<string, object>();
            LastEntity = new Dictionary<string, Entity>();
            var mockCRM = new MockCRM();
            FakeXrmContext = mockCRM.xrmContext;
            FakeXrmContextPlugin = mockCRM.xrmContextPlugin;
            service = FakeXrmContext.GetOrganizationService();
        }

        public void AddEntityToMockCRM(string known, Entity entity)
        {
            if (!LastEntity.ContainsKey(entity.LogicalName)) { LastEntity.Add(entity.LogicalName, null); }
            LastEntity[entity.LogicalName] = entity;
            Guid id = service.Create(entity);
            Entity entityCreated = service.Retrieve(entity.LogicalName, id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            Bag.Add(known, entityCreated);
        }

        public Entity RetrieveEntityInMockCRM(string key)
        {
            return (Entity)Bag[key];
        }

        public Entity RetrieveRefreshedEntityInMockCRM(string key)
        {
            Entity entityInBag = (Entity)Bag[key];
            Entity toBeRetrieved = new Entity();
            try
            {
                toBeRetrieved = service.Retrieve(entityInBag.LogicalName, entityInBag.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            }
            catch (Exception)
            {
                throw;
            }
            return toBeRetrieved;

        }

        public void RefreshEntityInMockCRM(string key)
        {
            Entity entityInBag = (Entity)Bag[key];
            try
            {
                var entityRefreshed = service.Retrieve(entityInBag.LogicalName, entityInBag.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                Bag[key] = entityRefreshed;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public Entity UpdateEntityInMock(string key, Entity entity)
        {
            Entity entityInBag = (Entity)Bag[key];
            entityInBag = entity;
            try
            {
                service.Update(entityInBag);
            }
            catch (Exception)
            {
                throw;
            }
            return entityInBag;
        }

        public object CreateEntity(string entityname)
        {
            var assembly = Assembly.Load("VSanchez.Plugins");
            var typeassembly = assembly.GetExportedTypes().Where(x => x.Name != null && x.Name.ToLower() == entityname.ToLower()).Select(x => x.FullName).FirstOrDefault();
            return assembly.CreateInstance(typeassembly);
        }

        public void AddAttributeToEntity(string key, string value, string entityname, ref Entity entity)
        {
            var parsedEntity = CreateEntity(entityname);
            var attribute = parsedEntity.GetType().GetProperties().Where(x => x.Name != null && x.Name.ToLower() == key).Select(x => x).FirstOrDefault();
            string typeAttribute = string.Empty;
            int optionValue = int.MinValue;
            if (attribute.PropertyType.GenericTypeArguments != null && attribute.PropertyType.GenericTypeArguments.Length > 0)
            {
                if (attribute.PropertyType.GenericTypeArguments.First().Namespace.Contains("VSanchez"))
                {
                    var optionset = CreateEntity(attribute.PropertyType.GenericTypeArguments.First().Name);
                    optionValue = (int)Enum.Parse(optionset.GetType(), optionset.GetType().GetFields().Where(x => x.Name.ToLower() == value.ToLower().Replace(" ", "")).Select(x => x.Name).FirstOrDefault());
                    typeAttribute = optionset.GetType().BaseType.Name;
                }
                else
                {
                    typeAttribute = attribute.PropertyType.GenericTypeArguments.FirstOrDefault().Name;
                }
            }
            else
            {
                typeAttribute = attribute.PropertyType.Name;
            }
            switch (typeAttribute)
            {
                case "EntityReference":
                    var entityInTheBag = (Entity)Bag[value];
                    entity[key] = entityInTheBag.ToEntityReference();
                    break;
                case "Enum":
                    entity[key] = new OptionSetValue(optionValue);
                    break;
                case "DateTime":
                    entity[key] = DateTime.Parse(value);
                    break;
                case "Boolean":
                    entity[key] = bool.Parse(value);
                    break;
                case "Int32":
                    entity[key] = int.Parse(value);
                    break;
                default:
                    entity[key] = value;
                    break;
            }
        }

        public void AddPrimaryAttributeNameMetadataToMock(string entityLogicalName, string attributeLogicalName)
        {
            var metadata = FakeXrmContext.GetEntityMetadataByName(entityLogicalName);

            var nameAttribute = new StringAttributeMetadata()
            {
                LogicalName = attributeLogicalName,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),
            };

            metadata.SetAttributeCollection(new[] { nameAttribute });
            metadata.SetFieldValue("_primaryNameAttribute", attributeLogicalName);
            FakeXrmContext.SetEntityMetadata(metadata);
        }

    }
}
