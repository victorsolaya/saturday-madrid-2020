using Microsoft.Xrm.Sdk;
using System;
using TechTalk.SpecFlow;
using VSanchez.Plugins;

namespace VSanchez.Plugins.UnitTests.Steps
{
    [Binding]
    public class When_Steps
    {
        XrmContext CRM { get; set; }
        IOrganizationService service { get; set; }
        public When_Steps()
        {
            CRM = Generic_Steps.CRM;
            service = Generic_Steps.service;
        }

        [When(@"I call the plugin AccountPlugin with record ""(.*)""")]

        public void WhenICallThePluginAccountPlugin(string nameEntity)
        {
            var target = (Entity)CRM.Bag[nameEntity];
            CRM.FakeXrmContextPlugin.MessageName = "Update";
            CRM.FakeXrmContextPlugin.Stage = 40;
            CRM.FakeXrmContextPlugin.InputParameters = new ParameterCollection { ["Target"] = target };
            var fakedPlugin = CRM.FakeXrmContextPlugin;
            var executedPlugin = CRM.FakeXrmContext.ExecutePluginWith<AccountPlugin>(fakedPlugin);
        }
    }
}
